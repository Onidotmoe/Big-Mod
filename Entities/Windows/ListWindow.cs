namespace BigMod.Entities.Windows
{
    /// <summary>
    /// Generic Window with a ListView and a Search bar with preset settings for displaying Items.
    /// </summary>
    public class ListWindow : WindowPanel
    {
        /// <summary>
        /// List of Pawns.
        /// </summary>
        public ListView ListView;
        public bool VisibleOnMouseOverOnly_Search = true;
        public Search Search;

        public ListWindow(bool Should_AddButtonCloseX = true, bool Should_AddButtonResize = true, bool Should_AddSimpleTextFilter = true, bool Should_MouseOverToggleVisibility = true)
        {
            Identifier = "ListWindow";
            AllowMultipleInstance = true;
            // Can only be closed using the close button or rightclick with modifier. Prevents it from being closed with escape.
            SetCloseOn();
            CanCloseRightClick = true;

            CameraMouseOverZooming = false;

            DrawBackground = false;
            DrawBorder = false;

            ListView = new ListView(Width, Height, true, false);
            ListView.SetStyle(GetType().Name + ".ListView");
            ListView.InheritParentSize = true;
            ListView.VisibleOnMouseOverOnly_Scrollbars = Should_MouseOverToggleVisibility;
            ListView.AllowSelection = true;
            ListView.IsMultiSelect = true;
            Root.AddChild(ListView);

            if (Should_AddButtonCloseX)
            {
                if (Should_MouseOverToggleVisibility)
                {
                    VisibleOnMouseOverOnly_ButtonCloseX = true;
                }

                AddButtonCloseX();
            }
            if (Should_AddButtonResize)
            {
                if (Should_MouseOverToggleVisibility)
                {
                    VisibleOnMouseOverOnly_ButtonResize = true;
                }

                AddButtonResize();
            }

            Search = new Search(Width + (((ButtonResize != null) ? -ButtonResize.Width : 0f) - 5f));
            Search.Anchor = Anchor.BottomLeft;
            Search.IsVisible = !Should_MouseOverToggleVisibility;
            Search.InheritParentSize_Modifier = new Vector2((((ButtonResize != null) ? -ButtonResize.Width : 0f) - 5f), 0f);
            Search.Offset = new Vector2(3f, -4f);
            Root.AddChild(Search);

            ListView.InheritParentSize_Modifier = new Vector2(0f, ((ButtonResize != null) ? -ButtonResize.Height : 0f) - 5f);

            if (Should_MouseOverToggleVisibility)
            {
                Root.OnMouseEnter += (obj, e) =>
                {
                    // Show the Scrollbars when entering the Window
                    ListView.VisibleOnMouseOverOnly_Scrollbars = false;

                    if (VisibleOnMouseOverOnly_Search)
                    {
                        Search.IsVisible = true;
                    }
                };

                Root.OnMouseLeave += (obj, e) =>
                {
                    // Hide them when exiting the Window
                    ListView.VisibleOnMouseOverOnly_Scrollbars = true;

                    if (VisibleOnMouseOverOnly_Search)
                    {
                        Search.IsVisible = false;
                    }
                };
            }

            Search.TextInput.OnTypingFinished += (obj, e) =>
            {
                // Whitespace should be a valid Search query.
                ListView.IsFilterActive = !string.IsNullOrEmpty(Search.Text);

                ListView.DoOnFilter(obj, e);
            };

            if (Should_AddSimpleTextFilter)
            {
                ListView.Filter = (Item) =>
                {
                    // string.Contains(string, StringComparison.InvariantCultureIgnoreCase) doesn't seem to be available to us.
                    return (Item.Text?.IndexOf(Search.Text, StringComparison.InvariantCultureIgnoreCase) >= 0);
                };
            }
        }
    }
}
