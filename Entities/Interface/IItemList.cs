namespace BigMod.Entities.Interface
{
    // TODO: Change to generic, remove references to "Item", to allow it to be used for DataViews too

    /// <summary>
    /// Standardized the Items List behaviour across <see cref="ListView"/> and <see cref="ListViewItemGroup"/>.
    /// </summary>
    public interface IItemList
    {
        public event EventHandler OnItemsChanged;
        /// <summary>
        /// Items exist in <see cref="Children"/> too.
        /// </summary>
        public List<ListViewItem> Items { get; set; }

        /// <summary>
        /// Insert the given Item into the Items list at the specified Index.
        /// </summary>
        /// <param name="Index">Index in Items list to insert to.</param>
        /// <param name="Item">Item to insert.</param>
        public void InsertItem(int Index, ListViewItem Item);

        /// <summary>
        /// Appends the given Item to the bottom of the Items list.
        /// </summary>
        /// <param name="Item">Item to add.</param>
        public void AddItem(ListViewItem Item);

        /// <summary>
        /// Removes the given Item from the Items list.
        /// </summary>
        /// <param name="Item">Item to remove.</param>
        public void RemoveItem(ListViewItem Item);

        /// <summary>
        /// Removes the Item at the given Index from the Items list.
        /// </summary>
        /// <param name="Index">Index in Items list to remove.</param>
        public void RemoveItemAt(int Index);

        /// <summary>
        /// Called whenever the Items list changes.
        /// </summary>
        public void DoOnItemsChanged();

        /// <summary>
        /// Removes all Items from the Items list.
        /// </summary>
        public void Clear();

        /// <summary>
        /// Recalculates all Items' positions.
        /// </summary>
        public void UpdatePositions();

        /// <summary>
        /// Offset to be applied between Items.
        /// </summary>
        public Vector2 ItemMargin { get; set; }
        /// <summary>
        /// Internal list of Children that aren't Items, used for Updating and Rendering.
        /// </summary>
        public List<Panel> _NonItemChildren { get; set; }

        /// <summary>
        /// Add a range of Items to the List.
        /// </summary>
        /// <typeparam name="T">ListViewItem or derived class.</typeparam>
        /// <param name="Items">List of Items to add.</param>
        public void AddRange<T>(List<T> Items) where T : ListViewItem;

        /// <summary>
        /// All Items that will currently be rendered.
        /// </summary>
        public List<ListViewItem> Renderables { get; set; }
    }
}
