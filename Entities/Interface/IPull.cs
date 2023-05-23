namespace BigMod.Entities.Interface
{
    /// <summary>
    /// A class with this Interface is capable of updating Data from other external sources.
    /// </summary>
    public interface IPull
    {
        /// <summary>
        /// Pulls data to Update this Entity.
        /// </summary>
        public void Pull();
    }
}
