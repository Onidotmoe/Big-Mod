using BigMod.Entities.Interface;

namespace BigMod.Entities
{
    /// <summary>
    /// A clickable item for the <see cref="ListView"/> control.
    /// </summary>
    public class ListViewItem : Panel, IFilterable
    {
        public override Vector2 MinSize { get; set; } = new Vector2(20, 20);
        private bool _IsSelected;
        /// <summary>
        /// Is this Item currently Selected in its parent ListView?
        /// </summary>
        public override bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (_IsSelected != value)
                {
                    // Allow deselection.
                    if (Selectable || !value)
                    {
                        _IsSelected = value;
                        DoOnSelectionChanged();
                    }
                }
            }
        }
        public string Text
        {
            get
            {
                return Header.Text;
            }
            set
            {
                Header.Text = value;
            }
        }
        /// <summary>
        /// Raised whenever <see cref="IsSelected"/> changes.
        /// </summary>
        public event EventHandler OnSelectionChanged;
        /// <summary>
        /// Can this Item be Selected?
        /// </summary>
        /// <remarks>Non-Selectable Items can still be deselected.</remarks>
        public override bool Selectable { get; set; } = true;
        /// <summary>
        /// Optional Image
        /// </summary>
        public Image Image;
        /// <summary>
        /// Main button header.
        /// </summary>
        public Button Header;
        private ListView _ListView;
        /// <summary>
        /// Parent ListView.
        /// </summary>
        public ListView ListView
        {
            get
            {
                return _ListView;
            }
            set
            {
                // Remove the existing ListView if values differ.
                if ((_ListView != null) && (_ListView != value))
                {
                    DoOnListViewRemoved();
                    _ListView = null;
                }
                // Then add the new value, this way both Removed and Added gets triggered.
                if ((_ListView == null) && (value != null))
                {
                    _ListView = value;
                    DoOnListViewAdded();
                }
            }
        }
        private bool _IsHidden;
        /// <summary>
        /// Toggle if this Item is Hidden, Hidden items will not have their position updated in the ListView.
        /// </summary>
        /// <remarks>Overrides <see cref="IsVisible"/>.</remarks>
        public bool IsHidden
        {
            get
            {
                return _IsHidden;
            }
            set
            {
                if (_IsHidden != value)
                {
                    _IsHidden = value;

                    UpdateItemParent();
                }
            }
        }
        private bool _IsVisible = true;
        public override bool IsVisible
        {
            get
            {
                return (_IsVisible && !IsHidden && ((Group == null) || Group.IsExpanded));
            }
            set
            {
                // Same as the setter in Panel
                if (_IsVisible != value)
                {
                    _IsVisible = value;
                    DoOnVisibilityChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Group parent if inside a Group.
        /// </summary>
        public ListViewItemGroup Group;
        /// <summary>
        /// Called before ListView is removed.
        /// </summary>
        public event EventHandler OnListViewRemoved;
        /// <summary>
        /// Called after ListView is added.
        /// </summary>
        public event EventHandler OnListViewAdded;
        public event EventHandler OnFilter;
        public bool CanFilter = true;

        public ListViewItem()
        {
            Style.DrawBackground = true;
            Style.DrawMouseOver = true;
            UseAnchoring = false;

            Header = new Button();
            Header.InheritParentSize = true;
            Header.SetStyle(GetType().Name + ".Header");
            Header.Style.DrawBackground = true;
            AddChild(Header);
        }

        public ListViewItem(string Text) : this()
        {
            this.Text = Text;
        }

        public virtual void DoOnFilter(object Sender, EventArgs EventArgs)
        {
            if (!CanFilter)
            {
                return;
            }

            if ((ListView != null) && ListView.IsFilterActive && (ListView.Filter != null))
            {
                IsFilteredOut = !ListView.Filter(this);

                if (!IsFilteredOut && !IsHidden)
                {
                    foreach (ListViewItemGroup Group in GetGroupTree())
                    {
                        Group.IsFilteredOut = false;
                        Group.IsExpanded = true;
                    }
                }
            }
            else
            {
                IsFilteredOut = false;
            }

            OnFilter?.Invoke(this, EventArgs.Empty);
        }

        public bool IsFilteredOut { get; set; }

        public virtual void DoOnSelectionChanged()
        {
            OnSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called before ListView is removed.
        /// </summary>
        public virtual void DoOnListViewRemoved()
        {
            OnListViewRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called after ListView is added.
        /// </summary>
        public virtual void DoOnListViewAdded()
        {
            OnListViewAdded?.Invoke(this, EventArgs.Empty);
        }


        public override Vector2 GetAbsolutePosition()
        {
            return (Position + ParentWindow.Position + ((ListView != null) ? (ListView.Position - ListView.ScrollPosition) : Vector2.zero));
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            Toggle();

            base.DoOnClick(Sender, EventArgs);
        }

        public virtual void Select(bool All = false)
        {
            ListView?.SelectItem(this);
        }

        public virtual void Toggle()
        {
            ListView?.Toggle(this);
        }

        public virtual void Deselect(bool All = false)
        {
            ListView?.DeselectItem(this);
        }

        /// <summary>
        /// Removes this Item from its Group parent and its ListView.
        /// </summary>
        public virtual void RemoveFromItemParent()
        {
            if (Group != null)
            {
                Group.RemoveItem(this);
            }
            if (ListView != null)
            {
                ListView.RemoveItem(this);
            }
        }

        /// <summary>
        /// Updates the Position & Viewport of either a Parent ListViewItemGroup or a Parent ListView.
        /// </summary>
        public void UpdateItemParent()
        {
            if (Group != null)
            {
                Group.UpdatePositions();
            }
            // ListView can be null when initializing sub Items in this group, before adding the group to the ListView
            if (ListView != null)
            {
                // Update the ListView to correctly reflect if this item should be visible or not.
                ListView.UpdatePositions();
                ListView.UpdateViewPortBounds();
            }
        }

        /// <summary>
        /// Expands the Group this Item is inside of, with Optional Bubble up to expand all parent Groups.
        /// </summary>
        /// <param name="Bubble">Should the expansion continue until nomore Groups are available?</param>
        public virtual void ExpandParentGroup(bool Bubble = false)
        {
            if (Group != null)
            {
                Group.IsExpanded = true;
                Group.ExpandParentGroup(Bubble);
            }
        }

        /// <summary>
        /// Reclusively gets all Groups that are ancestors of this Item.
        /// </summary>
        /// <param name="List">Of Group Items that are ancestors of this Item.</param>
        /// <returns>List of Group Items.</returns>
        public List<ListViewItem> GetGroupTree(List<ListViewItem> List = null)
        {
            if (List == null)
            {
                List = new List<ListViewItem>();
            }

            if (Group != null)
            {
                List.Add(Group);

                return Group.GetGroupTree(List);
            }

            return List;
        }
    }
}
