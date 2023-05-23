using Verse;

namespace BigMod.Entities
{
    /// <summary>
    /// Searchbar with a clickable icon to clear it.
    /// </summary>
    public class Search : Panel
    {
        public TextInput TextInput = new TextInput();
        public Button Button = new Button(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("SearchClear"));
        public string Text
        {
            get
            {
                return TextInput.Text;
            }
        }

        public Search(float Width, float Height = 20f, bool InheritParentSizeWidth = true) : base(new Vector2(Width, Height))
        {
            this.InheritParentSizeWidth = InheritParentSizeWidth;

            Button.SetStyle("Search.Button");
            Button.ToolTipText = "SearchClear".Translate();
            Button.Size = new Vector2(20f, 20f);
            Button.OnClick += Clear;
            AddChild(Button);

            TextInput.SetStyle("Search.TextInput");
            TextInput.InheritParentSize = true;
            TextInput.InheritParentSize_Modifier = new Vector2((-Button.Width - 5f), 0f);
            TextInput.Offset = new Vector2((Button.Width + 2f), 0f);
            // Set a initial size so it doesn't look weird when it first renders.
            TextInput.Size = new Vector2((Width + TextInput.InheritParentSize_Modifier.x), MaxSize.y);
            AddChild(TextInput);
        }

        public void Clear(object Sender, MouseEventArgs EventArgs)
        {
            TextInput.Clear();
        }
    }
}
