namespace BigMod.Entities.Interface
{
    public interface IFilterable
    {
        /// <summary>
        /// Called in <see cref="DoOnFilter(object, EventArgs)"/>.
        /// </summary>
        public event EventHandler OnFilter;

        /// <summary>
        /// RAises the filtering event and does filtering behavior.
        /// </summary>
        public void DoOnFilter(object Sender, EventArgs EventArgs);

        /// <summary>
        /// Is Entity currently Filtered out and won't be rendered?
        /// </summary>
        public bool IsFilteredOut { get; set; }
    }
}
