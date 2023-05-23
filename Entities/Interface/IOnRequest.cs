namespace BigMod.Entities.Interface
{
    /// <summary>
    /// Defines a standard mechanism for updating UI information based on based on request rate.
    /// </summary>
    public interface IOnRequest
    {
        /// <summary>
        /// Current position in request throttling.
        /// </summary>
        public int RequestCurrent { get; set; }
        /// <summary>
        /// How many update cycles between updating the ListView.
        /// </summary>
        public int RequestRate { get; set; }
        /// <summary>
        /// Called by <see cref="DoOnRequest"/> when <see cref="RequestCurrent"/> is at or suppasses <see cref="RequestCurrent"/>.
        /// </summary>
        public event EventHandler OnRequest;

        /// <summary>
        /// Handles data retrieving.
        /// </summary>
        public void DoOnRequest();
    }
}
