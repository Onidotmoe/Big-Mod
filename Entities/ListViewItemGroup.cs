using BigMod.Entities.Interface;
using Verse;

namespace BigMod.Entities
{
    /// <summary>
    /// Represents a group of <see cref="ListViewItem"/>, with a header item to collapse and expand them.
    /// </summary>
    /// <remarks>Will not expand if it doesn't have any items.</remarks>
    public class ListViewItemGroup : ListViewItem, IItemList
    {
        public virtual event EventHandler OnItemsChanged;
        public List<Panel> _NonItemChildren { get; set; } = new List<Panel>();
        public List<ListViewItem> Renderables { get; set; } = new List<ListViewItem>();

        private bool _IsExpanded;
        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                if (CanExpand())
                {
                    // Update on change
                    if (_IsExpanded != value)
                    {
                        _IsExpanded = value;

                        if (_IsExpanded)
                        {
                            DoOnExpanding();
                        }
                        else if (CanCollapse())
                        {
                            DoOnCollapsing();
                        }

                        UpdateItemParent();
                    }
                }
                else if (_IsExpanded && CanCollapse())
                {
                    _IsExpanded = false;
                    DoOnCollapsing();
                    UpdateItemParent();
                }
            }
        }
        public ListViewStyle RenderStyle = ListViewStyle.List;
        public List<ListViewItem> Items { get; set; } = new List<ListViewItem>();
        public Vector2 ItemMargin { get; set; } = new Vector2(10, 1);
        public event EventHandler OnExpanding;
        public event EventHandler OnCollapsing;
        /// <summary>
        /// Items will try to fill the entire width of the <see cref="ItemsBounds"/>.
        /// </summary>
        public bool ExtendItemsHorizontally = true;
        public override Rect Bounds
        {
            get
            {
                if (!IsExpanded)
                {
                    return _Bounds;
                }
                else
                {
                    // Get the Expanded Height
                    return new Rect(_Bounds.x, _Bounds.y, _Bounds.width, Height);
                }
            }
            set
            {
                _Bounds = value;
            }
        }

        public override float Height
        {
            get
            {
                if (IsExpanded)
                {
                    return (Renderables.Any() ? (Renderables.Max((F) => F.Bottom) - Y) : Header.Height);
                }
                else
                {
                    return _Bounds.height;
                }
            }
            set
            {
                // Same code as Panel class
                if (_Bounds.height != value)
                {
                    _OldBounds = _Bounds;
                    _Bounds.height = Mathf.Max(MinSize.y, value);

                    if (MaxSize != Vector2.zero)
                    {
                        _Bounds.height = Mathf.Min(MaxSize.y, _Bounds.height);
                    }

                    DoOnSizeChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }

        public ListViewItemGroup()
        {
            // Prevent the header from growing into its children's bounds
            Header.MaxSize = new Vector2(0, _Bounds.height);
            Header.OnClick += Header_OnClick;
            Header.OnClick += DoOnShiftClick;
        }

        public ListViewItemGroup(string Text) : this()
        {
            this.Text = Text;
        }

        public override void Draw()
        {
            foreach (Panel Child in _NonItemChildren)
            {
                Child.Draw();
            }

            if (IsExpanded)
            {
                foreach (ListViewItem Item in Renderables)
                {
                    Item.Draw();
                }
            }

            Header.DrawMouseOver();
            DrawBorder();
            DrawSelected();
            DrawMouseOver();
            DrawDisabledOverlay();
        }

        public virtual void DoOnShiftClick(object Sender, MouseEventArgs EventArgs)
        {
            if (WindowManager.IsShiftDown())
            {
                if (!IsSelected && Selectable)
                {
                    Select(true);
                }
                else
                {
                    Deselect(true);
                }
            }
        }

        /// <summary>
        /// Only clicking on the Child Header should expand the Group.
        /// </summary>
        /// <remarks>Copies Expanded setting to its Descendants.</remarks>
        /// <param name="Sender">Sender Object.</param>
        /// <param name="EventArgs">Mouse arguments.</param>
        public virtual void Header_OnClick(object Sender, MouseEventArgs EventArgs)
        {
            IsExpanded = !IsExpanded;

            if (WindowManager.IsShiftDown() && WindowManager.IsAltDown())
            {
                // Copies Expanded setting to its Descendants
                Flatten().OfType<ListViewItemGroup>().ToList().ForEach((F) => F.IsExpanded = IsExpanded);
            }
        }

        public override void DoOnSizeChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnSizeChanged(Sender, EventArgs);

            if (ExtendItemsHorizontally)
            {
                foreach (ListViewItem Item in Items)
                {
                    Item.Width = (Width - ItemMargin.x);
                }
            }

            // The Grid Style pushes items to the next row when they would overflow MaxX
            if (RenderStyle == ListViewStyle.Grid)
            {
                UpdatePositions();
            }
        }

        public override void DoOnListViewRemoved()
        {
            base.DoOnListViewRemoved();

            foreach (ListViewItem Item in Items)
            {
                Item.ListView = null;
            }
        }

        public override void DoOnListViewAdded()
        {
            base.DoOnListViewAdded();

            foreach (ListViewItem Item in Items)
            {
                Item.ListView = ListView;
            }
        }

        /// <summary>
        /// Conditional check if this Group can be expanded.
        /// </summary>
        /// <returns>True if this Group item can be expanded.</returns>
        public virtual bool CanExpand()
        {
            return true;
        }

        /// <summary>
        /// Conditional check if this Group can be collapsed.
        /// </summary>
        /// <returns>True if this Group item can be collapsed.</returns>
        public virtual bool CanCollapse()
        {
            return true;
        }

        public virtual void DoOnExpanding()
        {
            OnExpanding?.Invoke(this, EventArgs.Empty);

            DoOnItemsChanged();
        }

        public virtual void DoOnCollapsing()
        {
            OnCollapsing?.Invoke(this, EventArgs.Empty);

            DoOnItemsChanged();
        }

        #region "IItemList Manipulation"

        public void AddItem(ListViewItem Item)
        {
            Rect Last = (Items.Any() ? Items.Last().Bounds : new Rect(X, _Bounds.yMax, 0f, 0f));

            switch (RenderStyle)
            {
                case ListViewStyle.List:
                    Item.X = ItemMargin.x;
                    Item.Y = ItemMargin.y + Last.yMax;

                    Item.Width = Mathf.Max((Width - ItemMargin.x), (Header.Width - ItemMargin.x));
                    Item.Height = Mathf.Max(Item.Height, Mathf.Max(Item.Height, MinSize.y));

                    break;

                case ListViewStyle.Row:
                    Item.X = ItemMargin.x + Last.xMax;
                    Item.Y = ItemMargin.y;

                    Item.Width = Mathf.Max((Item.Width - ItemMargin.x), Mathf.Max(Item.Width, MinSize.x));
                    Item.Height = Mathf.Max(Item.Height, Mathf.Max(Item.Height, MinSize.y));

                    break;

                case ListViewStyle.Grid:
                    float X = ItemMargin.x + Last.xMax;
                    float Y = Last.y;

                    // Overflows the Group item, push it to the next row
                    if (X > Width)
                    {
                        X = ItemMargin.x;
                        Y = (ItemMargin.y + Last.yMax);
                    }

                    Item.X = X;
                    Item.Y = Y;

                    Item.Width = Mathf.Max(Item.Width, Mathf.Max(Item.Width, MinSize.x));
                    Item.Height = Mathf.Max(Item.Height, Mathf.Max(Item.Height, MinSize.y));

                    break;
            }

            Item.ListView = ListView;
            Item.Group = this;
            Items.Add(Item);
            Children.Add(Item);
            Register(Item);

            OnFilter += Item.DoOnFilter;

            if (IsExpanded && (ListView != null))
            {
                ListView.UpdateViewPortBounds();
                ListView.UpdatePositions();
            }

            DoOnItemsChanged();
        }

        public void RemoveItem(ListViewItem Item)
        {
            Item.ListView = null;
            Item.Group = null;
            Items.Remove(Item);
            Children.Remove(Item);
            UnRegister(Item);

            OnFilter -= Item.DoOnFilter;

            if (IsExpanded && (ListView != null))
            {
                ListView.UpdateViewPortBounds();
                ListView.UpdatePositions();
            }

            DoOnItemsChanged();
        }

        public void Clear()
        {
            for (int i = (Items.Count - 1); i >= 0; i--)
            {
                RemoveItem(Items[i]);
            }
        }

        public void InsertItem(int Index, ListViewItem Item)
        {
            AddItem(Item);

            // Don't bother changing its position if it's out of bounds.
            // Index is Zero based, Count is not.
            if ((Index >= 0) && (Index <= (Items.Count - 1)))
            {
                Items.Remove(Item);
                Items.Insert(Index, Item);
            }

            UpdatePositions();
        }

        public void RemoveItemAt(int Index)
        {
            RemoveItem(Items[Index]);
        }

        public virtual void DoOnItemsChanged()
        {
            // Disable self if no items are present.
            IsEnabled = Items.Any();

            Renderables.Clear();

            if (IsExpanded)
            {
                Renderables.AddRange(Items.Where((F) => (F.IsVisible && !F.IsFilteredOut)));
            }

            OnItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void DoOnFilter(object Sender, EventArgs EventArgs)
        {
            base.DoOnFilter(Sender, EventArgs);

            DoOnItemsChanged();
            UpdatePositions();

            UpdateItemParent();
        }

        public void UpdatePositions()
        {
            if (IsExpanded)
            {
                IList<ListViewItem> Presentables = Items.Where((F) => (!F.IsHidden && !F.IsFilteredOut)).ToList();

                for (int i = 0; i < Presentables.Count; i++)
                {
                    Rect Previous = (i > 0 ? Presentables[i - 1].Bounds : new Rect(X, _Bounds.yMax, 0f, 0f));

                    ListViewItem Item = Presentables[i];

                    switch (RenderStyle)
                    {
                        case ListViewStyle.List:
                            Item.X = ItemMargin.x;
                            Item.Y = (ItemMargin.y + Previous.yMax);
                            break;

                        case ListViewStyle.Row:
                            Item.X = (ItemMargin.x + Previous.xMax);
                            Item.Y = ItemMargin.y;
                            break;

                        case ListViewStyle.Grid:
                            float X = (ItemMargin.x + Previous.xMax);
                            float Y = Mathf.Max(ItemMargin.y, Previous.yMin);

                            // Overflows the X axis, push it to the next row
                            if ((X + Item.Width) > Width)
                            {
                                X = ItemMargin.x + this.X;
                                Y = (ItemMargin.y + Previous.yMax);
                            }

                            Item.X = X;
                            Item.Y = Y;
                            break;
                    }

                    (Item as ListViewItemGroup)?.UpdatePositions();
                }
            }
        }

        public void AddRange<T>(List<T> Items) where T : ListViewItem
        {
            foreach (T Item in Items)
            {
                AddItem(Item);
            }
        }

        public override void Select(bool All = false)
        {
            base.Select();

            if (All)
            {
                foreach (ListViewItem Item in Items)
                {
                    Item.Select(All);
                }
            }
        }

        public override void Deselect(bool All = false)
        {
            base.Deselect();

            if (All)
            {
                foreach (ListViewItem Item in Items)
                {
                    Item.Deselect(All);
                }
            }
        }

        public virtual void SetSelectItems(bool State = true)
        {
            if (State)
            {
                Select(true);
            }
            else
            {
                Deselect(true);
            }
        }

        /// <summary>
        /// Flattens any other <see cref="ListViewItemGroup"/> in this group and returns all items.
        /// </summary>
        /// <returns>List of all Items in this Group and any subgroup.</returns>
        public List<ListViewItem> Flatten()
        {
            List<ListViewItem> List = new List<ListViewItem>();

            List.Add(this);

            foreach (ListViewItem Item in Items)
            {
                if (Item is ListViewItemGroup Group)
                {
                    List.AddRange(Group.Flatten());
                }
                else
                {
                    List.Add(Item);
                }
            }

            return List;
        }

        #endregion "IItemList Manipulation"

        /// <summary>
        /// Insertion of non-Item children isn't respected in a ListView.
        /// </summary>
        public override void InsertChild(int Index, Panel Child)
        {
            base.InsertChild(Index, Child);

            _NonItemChildren.Add(Child);
        }

        public override void AddChild(Panel Child)
        {
            base.AddChild(Child);

            _NonItemChildren.Add(Child);
        }

        public override void RemoveChild(Panel Child)
        {
            base.RemoveChild(Child);

            _NonItemChildren.Remove(Child);
        }
    }
}
