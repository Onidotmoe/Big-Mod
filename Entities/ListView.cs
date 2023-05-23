using BigMod.Entities.Interface;
using Verse;

namespace BigMod.Entities
{
    public enum ListViewStyle
    {
        /// <summary>
        /// Adds Items from Top to Bottom
        /// </summary>
        List,

        /// <summary>
        /// Adds Items from Left to Right.
        /// </summary>
        Row,

        /// <summary>
        /// Adds Items from TopLeft to BottomRight
        /// </summary>
        Grid
    }

    public class ListView : Panel, IItemList
    {
        public List<ListViewItem> Items { get; set; } = new List<ListViewItem>();
        public List<ListViewItem> Renderables { get; set; } = new List<ListViewItem>();

        public List<Panel> _NonItemChildren { get; set; } = new List<Panel>();
        public bool ShowScrollbarHorizontal;
        public bool ShowScrollbarVertical;
        public Vector2 ItemMargin { get; set; } = new Vector2(5, 2);

        public event EventHandler OnItemsChanged;
        public Func<ListViewItem, bool> Filter;
        /// <summary>
        /// <para>Raised whenever the Selection list is about to be changed or has changed.</para>
        /// <para>The EventHandler object is the Item after it has been Added or Removed from the List.</para>
        /// </summary>
        public event EventHandler OnSelectionChanged;
        public event EventHandler OnFilter;
        public event EventHandler OnScrollPositionChanged;

        public bool IsFilterActive;

        /// <summary>
        /// Sets the MinSize for Items when they are added, 0 will be ignored.
        /// </summary>
        public Vector2 ItemMinSize;
        /// <summary>
        /// Sets the MaxSize for Items when they are added, 0 will be ignored.
        /// </summary>
        public Vector2 ItemMaxSize;

        /// <summary>
        /// Alternates the background color of the Items using <see cref="ColorEven"/> and <see cref=" ColorOdd"/>.
        /// </summary>
        public bool AlternateColors;

        /// <summary>
        /// Every Even item will have this color applied to help differentiate between items.
        /// </summary
        public Color ColorEven = Globals.GetColor("ListView.ColorEven");

        /// <summary>
        /// Every odd item will have this color applied to help differentiate between items.
        /// </summary>
        public Color ColorOdd = Globals.GetColor("ListView.ColorOdd");

        /// <summary>
        /// Width of the Horizontal Scrollbar and Height of the Vertical Scrollbar.
        /// </summary>
        public float ScrollbarSize = 16f;

        /// <summary>
        /// Should the ListView grow with its children Horizontally?
        /// </summary>
        public bool ExpandHorizontally;

        /// <summary>
        /// Should the ListView grow with its children Vertically?
        /// </summary>
        public bool ExpandVertically;

        /// <summary>
        /// Items that are not visible through the <see cref="ViewPortBounds_Visible"/> rectangle will not be updated or drawn.
        /// </summary>
        public bool VirtualizeItems = true;

        /// <summary>
        /// Items will try to fill the entire width of the <see cref="ViewPortBounds"/>.
        /// </summary>
        public bool ExtendItemsHorizontally = true;

        public ListViewStyle RenderStyle = ListViewStyle.List;
        public Rect ViewPortBounds_Visible;

        /// <summary>
        /// Currently Selected Items.
        /// </summary>
        /// <remarks>Modifying the list directly from outside this class is not allowed.</remarks>
        private List<ListViewItem> _SelectedItems = new List<ListViewItem>();

        /// <summary>
        /// Currently Selected Items.
        /// </summary>
        public IList<ListViewItem> SelectedItems
        {
            get
            {
                return _SelectedItems.AsReadOnly();
            }
        }

        /// <summary>
        /// Can Items be selected in this ListView?
        /// </summary>
        public bool AllowSelection;
        /// <summary>
        /// There always has to be atleast 1 Item Selected.
        /// </summary>
        public bool StickySelection;
        /// <summary>
        /// Can multiple items be selected at the same time?
        /// </summary>
        public bool IsMultiSelect;

        private Vector2 _ScrollPosition;

        public Vector2 ScrollPosition
        {
            get
            {
                return _ScrollPosition;
            }
            set
            {
                if (_ScrollPosition != value)
                {
                    _ScrollPosition = value;
                    Update_ViewPortBounds_Visible();
                    DoOnScrollPositionChanged();
                }
            }
        }
        /// <summary>
        /// Should GUI Scrolling be used for <see cref="ScrollPosition"/> with MouseWheel?
        /// </summary>
        public bool IgnoreGUIScroll;
        public bool VisibleOnMouseOverOnly_Scrollbars;

        private void Update_ViewPortBounds_Visible()
        {
            Vector2 ScrollPositionSize = (_ViewPortBounds.size - ScrollPosition);

            ViewPortBounds_Visible = new Rect(ScrollPosition, new Vector2(Mathf.Min(Width, (ScrollPositionSize.x + Width)), Mathf.Min(Height, (ScrollPosition.y + Height))));

            UpdateVirtualization();
        }

        private Rect _ViewPortBounds;

        /// <summary>
        /// Used for Clipping Items when they overflow the ListView's Bounds.
        /// </summary>
        public Rect ViewPortBounds
        {
            get
            {
                return _ViewPortBounds;
            }
            set
            {
                if (_ViewPortBounds != value)
                {
                    // ViewPort can never be smaller than the ListView
                    _ViewPortBounds.size = Vector2.Max(Bounds.size, value.size);
                    _ViewPortBounds.position = value.position;
                    _ViewPortBounds.size = new Vector2((_ViewPortBounds.size.x - ScrollbarSize), _ViewPortBounds.size.y);

                    Update_ViewPortBounds_Visible();
                }
            }
        }

        public ListView(float Width, float Height, bool ShowScrollbarVertical = true, bool ShowScrollbarHorizontal = false)
        {
            Size = new Vector2(Width, Height);

            this.ShowScrollbarVertical = ShowScrollbarVertical;
            this.ShowScrollbarHorizontal = ShowScrollbarHorizontal;

            if (!ShowScrollbarVertical && !ShowScrollbarHorizontal)
            {
                ScrollbarSize = 0f;
            }

            Style.DrawBackground = true;
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                DrawBackground();

                foreach (Panel Child in _NonItemChildren)
                {
                    Child.Draw();
                }

                float Old_fixedHeight = GUI.skin.verticalScrollbar.fixedHeight;
                float Old_fixedWidth = GUI.skin.verticalScrollbar.fixedWidth;

                if (VisibleOnMouseOverOnly_Scrollbars && !IsMouseOver)
                {
                    GUI.skin.verticalScrollbar.fixedHeight = 0f;
                    GUI.skin.verticalScrollbar.fixedWidth = 0f;
                }

                ScrollPosition = GUI.BeginScrollView(Bounds, ScrollPosition, ViewPortBounds, ShowScrollbarHorizontal, ShowScrollbarVertical, (ShowScrollbarHorizontal ? GUI.skin.horizontalScrollbar : GUIStyle.none), (ShowScrollbarVertical ? GUI.skin.verticalScrollbar : GUIStyle.none));

                if (VisibleOnMouseOverOnly_Scrollbars)
                {
                    GUI.skin.verticalScrollbar.fixedHeight = Old_fixedHeight;
                    GUI.skin.verticalScrollbar.fixedWidth = Old_fixedWidth;
                }

                for (int i = 0; i < Renderables.Count; i++)
                {
                    ListViewItem Item = Renderables[i];

                    // TODO: this shouldn't be in here
                    if (AlternateColors)
                    {
                        Item.Header.Style.BackgroundColor = (((i % 2) == 0) ? ColorEven : ColorOdd);
                    }

                    Item.Draw();
                }

                GUI.EndScrollView(!IgnoreGUIScroll);
            }
        }

        public override void Update()
        {
            foreach (Panel Child in _NonItemChildren)
            {
                Child.Update();
            }

            // The Renderables list is rather sensitive to external changes, so use the Items list instead.
            foreach (ListViewItem Item in Items)
            {
                // Skip the Item if it isn't visible in the viewport.
                if (Item.IsVisible && !Item.IsFilteredOut)
                {
                    Item.Update();
                }
            }
        }

        /// <summary>
        /// Updates the Visibility of Items relative to their position in the visible ViewPort.
        /// </summary>
        public void UpdateVirtualization()
        {
            if (VirtualizeItems)
            {
                foreach (ListViewItem Item in Items)
                {
                    Item.IsVisible = !ShouldVirtualizeItem(Item);
                }

                UpdateRenderables();
            }
        }

        public bool ShouldVirtualizeItem(ListViewItem Item)
        {
            return !ViewPortBounds_Visible.Overlaps(Item.Bounds);
        }

        public override bool GetMouseOver(Vector2 MousePosition, Vector2 ChildOffset = default)
        {
            // Modified from Panel.GetMouseOver
            if (!IgnoreMouse && Bounds.Contains(MousePosition))
            {
                IsMouseOver = true;

                if (CanCascadeMouseInput())
                {
                    MousePosition += ChildOffset;

                    foreach (Panel Child in _NonItemChildren)
                    {
                        Child.GetMouseOver(MousePosition);
                    }

                    // Only Items are affected by ScrollPosition, ListView position has to be subtracted
                    MousePosition += ScrollPosition - Position;

                    // The Renderables list is rather sensitive to external changes, so use the Items list instead.
                    foreach (ListViewItem Item in Items)
                    {
                        // Only Items visible in the ViewPort can have MouseOver
                        if (Item.IsVisible && !Item.IsFilteredOut)
                        {
                            Item.GetMouseOver(MousePosition);
                        }
                    }
                }
            }
            else
            {
                IsMouseOver = false;
            }

            return IsMouseOver;
        }

        public void DoOnFilter(object Sender, EventArgs EventArgs)
        {
            OnFilter?.Invoke(this, EventArgs.Empty);

            UpdatePositions();
            UpdateVirtualization();
        }

        #region "IItemList Manipulation"

        public void AddItem(ListViewItem Item)
        {
            Rect Last = (Items.Any() ? Items.Last().Bounds : Rect.zero);

            Item.MinSize = new Vector2(((ItemMinSize.x != 0) ? ItemMinSize.x : Item.MinSize.x), ((ItemMinSize.y != 0) ? ItemMinSize.y : Item.MinSize.y));
            Item.MaxSize = new Vector2(((ItemMaxSize.x != 0) ? ItemMaxSize.x : Item.MaxSize.x), ((ItemMaxSize.y != 0) ? ItemMaxSize.y : Item.MaxSize.y));

            switch (RenderStyle)
            {
                case ListViewStyle.List:
                    Item.X = ItemMargin.x;
                    Item.Y = ItemMargin.y + Last.yMax;

                    Item.Width = (Width - ItemMargin.x);

                    break;

                case ListViewStyle.Row:
                    Item.X = ItemMargin.x + Last.xMax;
                    Item.Y = ItemMargin.y;

                    Item.Width = (Item.Width - ItemMargin.x);

                    break;

                case ListViewStyle.Grid:
                    float X = ItemMargin.x + Last.xMax;
                    float Y = Mathf.Max(ItemMargin.y, Last.yMin);

                    // Overflows the viewport, push it to the next row
                    if ((X + Item.Width) > ViewPortBounds.xMax)
                    {
                        X = ItemMargin.x;
                        Y = (ItemMargin.y + Last.yMax);
                    }

                    ListViewItem Sibling = Items.LastOrDefault((F) => ((int)F.X == (int)X));

                    if (Sibling != null)
                    {
                        // When in a Grid, items can have different sizes, make sure it ends up underneath its Sibling in the same X coordinate.
                        Y = (ItemMargin.y + Sibling.Bounds.yMax);
                    }

                    Item.X = X;
                    Item.Y = Y;

                    break;
            }

            Item.ListView = this;
            Items.Add(Item);
            Children.Add(Item);
            Register(Item);

            OnFilter += Item.DoOnFilter;

            DoOnItemsChanged();
        }

        public void AddRange<T>(List<T> Items) where T : ListViewItem
        {
            foreach (T Item in Items)
            {
                AddItem(Item);
            }
        }

        public void RemoveItem(ListViewItem Item)
        {
            DeselectItem(Item);
            Item.ListView = null;
            Items.Remove(Item);
            Children.Remove(Item);
            UnRegister(Item);

            OnFilter -= Item.DoOnFilter;

            UpdatePositions();

            DoOnItemsChanged();
        }

        public void RemoveItemAt(int Index)
        {
            RemoveItem(Items[Index]);
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

        public void DoOnItemsChanged()
        {
            if (Items.Any())
            {
                if (ExpandHorizontally)
                {
                    float MaxWidth = Items.Max((F) => F.Right);

                    Width = (MaxWidth + ItemMargin.x);
                }
                if (ExpandVertically)
                {
                    float MaxHeight = Items.Max((F) => F.Bottom);

                    Height = (MaxHeight + ItemMargin.y);
                }
            }

            UpdateViewPortBounds();
            UpdateRenderables();

            OnItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateRenderables()
        {
            Renderables.Clear();
            Renderables.AddRange(Items.Where((F) => (F.IsVisible && !F.IsFilteredOut)));
        }

        /// <summary>
        /// Removes all Items.
        /// </summary>
        public void Clear()
        {
            for (int i = (Items.Count - 1); i >= 0; i--)
            {
                RemoveItem(Items[i]);
            }
        }

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

        public void SelectItem(ListViewItem Item)
        {
            if (!AllowSelection)
            {
                return;
            }

            if (StickySelection && (_SelectedItems.Count == 1) && (_SelectedItems.First() == Item))
            {
                return;
            }

            if (!IsMultiSelect)
            {
                DeselectAll();
            }

            if (!_SelectedItems.Contains(Item) && Item.Selectable)
            {
                Item.IsSelected = true;
                _SelectedItems.Add(Item);
                DoOnSelectionChanged(Item);
            }
        }

        public void DeselectItem(ListViewItem Item)
        {
            if (!AllowSelection)
            {
                return;
            }

            if (_SelectedItems.Contains(Item))
            {
                _SelectedItems.Remove(Item);
                Item.IsSelected = false;
                DoOnSelectionChanged(Item);
            }
        }

        public void DeselectAll()
        {
            if (!AllowSelection)
            {
                return;
            }

            foreach (ListViewItem Item in _SelectedItems)
            {
                Item.IsSelected = false;
                DoOnSelectionChanged(Item);
            }

            _SelectedItems.Clear();
        }

        public void SelectAll()
        {
            if (!AllowSelection)
            {
                return;
            }

            foreach (ListViewItem Item in Items)
            {
                if (Item.Selectable)
                {
                    Item.Select(true);
                    Item.IsSelected = true;
                    DoOnSelectionChanged(Item);
                }
            }
        }

        public void Toggle(ListViewItem Item)
        {
            if (!AllowSelection)
            {
                return;
            }

            if (StickySelection && (_SelectedItems.Count == 1) && (_SelectedItems.First() == Item))
            {
                return;
            }

            if (_SelectedItems.Contains(Item))
            {
                DeselectItem(Item);
            }
            else if (Item.Selectable)
            {
                SelectItem(Item);
            }
        }

        public void DoOnSelectionChanged(ListViewItem Item = null)
        {
            OnSelectionChanged?.Invoke(Item, EventArgs.Empty);
        }
        public void DoOnScrollPositionChanged()
        {
            OnScrollPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets all Items in the ListView and in any <see cref="ListViewItemGroup"/>.
        /// </summary>
        /// <returns>List of all available items in the entire ListView tree.</returns>
        public List<ListViewItem> GetItemsReclusively()
        {
            List<ListViewItem> List = new List<ListViewItem>();

            foreach (ListViewItem Item in Items)
            {
                if (Item is ListViewItemGroup Group)
                {
                    // Flatten returns itself too
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

        #region "Positioning & Sizing"

        public void UpdatePositions()
        {
            IList<ListViewItem> Presentables = Items.Where((F) => (!F.IsHidden && !F.IsFilteredOut)).ToList();

            for (int i = 0; i < Presentables.Count; i++)
            {
                Rect Previous = (i > 0 ? Presentables[i - 1].Bounds : Rect.zero);
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
                            X = ItemMargin.x;
                            Y = (ItemMargin.y + Previous.yMax);
                        }

                        // Go backwards from our current position through the list.
                        for (int Sibling = (i - 1); Sibling >= 0; Sibling--)
                        {
                            if (((int)Presentables[Sibling].X == (int)X))
                            {
                                // When in a Grid, items can have different sizes, make sure it ends up underneath its OlderSibling in the same X coordinate.
                                Y = (ItemMargin.y + Presentables[Sibling].Bounds.yMax);
                                break;
                            }
                        }

                        Item.X = X;
                        Item.Y = Y;
                        break;
                }

                (Item as ListViewItemGroup)?.UpdatePositions();
            }
        }

        /// <summary>
        /// Updates the size of the ListView's ViewPort when Groups have been expanded or collapsed.
        /// </summary>
        public void UpdateViewPortBounds()
        {
            IEnumerable<ListViewItem> Presentables = Items.Where((F) => !F.IsHidden && !F.IsFilteredOut);

            if (Presentables.Any())
            {
                ListViewItem Item = Presentables.Last();

                // Retrieve the last item inside nested Groups.
                while (Item is ListViewItemGroup Group)
                {
                    if (Group.IsExpanded && Group.Items.Any())
                    {
                        Item = Group.Items.Last();
                    }
                    else
                    {
                        break;
                    }
                }

                // ViewPort can never be smaller than the ListView
                float MaxWidth = Mathf.Max(Width, Items.Max((F) => F.Right));

                // The bottom of the last Item is the height of the ListView's expanded Items.
                ViewPortBounds = new Rect(ViewPortBounds.position.x, ViewPortBounds.position.y, (MaxWidth + ItemMargin.x), (Item.Bottom + ItemMargin.y));
            }

            if (ExtendItemsHorizontally)
            {
                // ItemMargin.x has to be removed twice, because it's affecting both left and right side of the Item.
                float ItemWidth = (Width - (ItemMargin.x * 2) - ScrollbarSize);

                foreach (ListViewItem Item in Presentables)
                {
                    Item.Width = ItemWidth;
                }
            }
        }

        public override void DoOnSizeChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnSizeChanged(Sender, EventArgs);

            UpdateViewPortBounds();

            // The Grid Style pushes items to the next row when they would overflow MaxX
            if (RenderStyle == ListViewStyle.Grid)
            {
                UpdatePositions();
            }
        }

        #endregion "Positioning & Sizing"
    }
}
