using Verse;

namespace BigMod.Entities
{
    public class ContextMenu : WindowPanel
    {
        public ListView ListView;
        /// <summary>
        /// Should the ContextMenu be closed when a option is clicked?
        /// </summary>
        public bool CloseOnOptionClick;
        /// <summary>
        /// Sets the size of the ContextMenu to the height of the <see cref="ListView"/>'s Items, only if <see cref="Root.MaxSize.y"/> is less than the ListView's Items.
        /// </summary>
        /// <remarks>Will be ignored if ListView doesn't have any Items.</remarks>
        public bool SizeToItems = true;

        public ContextMenu()
        {
            Bounds = new Rect(WindowManager.GetMousePosition(), new Vector2(200, 300));
            Root.MaxSize = Bounds.size;

            DrawBackground = true;
            DrawBorder = true;
            IsDraggable = false;
            IsResizable = false;
            CanReset = false;

            SetCloseOn(true, true, true);

            ListView = new ListView(Width, Height, false, false);
            ListView.InheritParentSize = true;
            Root.AddChild(ListView);

            Root.Style.BorderThickness = 1;
        }

        public override void PreOpen()
        {
            base.PreOpen();

            if (SizeToItems && ListView.Items.Any())
            {
                Height = Mathf.Min(Root.MaxSize.y, ((ListView.Items.Last().Bottom) + ListView.ItemMargin.y));
            }
        }

        /// <summary>
        /// Adds a Item to this Context Menu.
        /// </summary>
        /// <param name="Text">Text to be displayed on the Item's Header.</param>
        /// <param name="OnClick">Action to perform when Item is clicked.</param>
        /// <returns>Item ready to use.</returns>
        public ListViewItem AddOption(string Text, Action OnClick = null)
        {
            ListViewItem Item = new ListViewItem(Text);

            if (OnClick != null)
            {
                Item.OnClick += (obj, e) => OnClick();
            }

            ListView.AddItem(Item);

            return Item;
        }

        /// <summary>
        /// Adds a Item to this Context Menu.
        /// </summary>
        /// <param name="Text">Text to be displayed on the Item's Header.</param>
        /// <param name="Texture2D">Texture to use.</param>
        /// <param name="ToolTipText">Text to display in the ToolTip while MouseOver.</param>
        /// <param name="OnClick">Action to perform when Item is clicked.</param>
        /// <returns>Item ready to use.</returns>
        public ListViewItem AddOption(string Text, Texture2D Texture2D = null, string ToolTipText = "", Action OnClick = null)
        {
            ListViewItem Item = AddOption(Text, OnClick);

            Item.ToolTipText = ToolTipText;

            Item.Image = new Image();
            Item.Image.Size = new Vector2(Item.Height, Item.Height);
            Item.Image.Style.Color = Color.white;
            Item.Image.Texture = Texture2D;

            Item.AddChild(Item.Image);

            return Item;
        }

        /// <summary>
        /// Adds a Item to this Context Menu.
        /// </summary>
        /// <param name="Text">Text to be displayed on the Item's Header.</param>
        /// <param name="TexturePath">Path to Texture.</param>
        /// <param name="ToolTipText">Text to display in the ToolTip while MouseOver.</param>
        /// <param name="OnClick">Action to perform when Item is clicked.</param>
        /// <returns>Item ready to use.</returns>
        public ListViewItem AddOption(string Text, string TexturePath, string ToolTipText = "", Action OnClick = null)
        {
            return AddOption(Text, Globals.GetTexture(TexturePath), ToolTipText, OnClick);
        }

        public ListViewItem AddOptionToggle(string Text, Texture2D Texture2D = null, string ToolTipText = "", bool InitialState = false, Action OnToggleStateChanged = null)
        {
            ListViewItem Item = AddOption(Text, Texture2D, ToolTipText);

            Item.Header.CanToggle = true;
            Item.Header.ToggleState = InitialState;

            if (OnToggleStateChanged != null)
            {
                Item.Header.OnToggleStateChanged += (obj, e) => OnToggleStateChanged();
            }

            return Item;
        }

        /// <summary>
        /// Convert list of FloatMenuOptions to a ContextMenu.
        /// </summary>
        /// <param name="FloatMenuOptions">List of vanilla options.</param>
        public ContextMenu(IEnumerable<FloatMenuOption> FloatMenuOptions) : this()
        {
            foreach (FloatMenuOption Option in FloatMenuOptions)
            {
                // Must have a valid click action
                if (Option.action != null)
                {
                    ListViewItem Item = new ListViewItem();
                    Item.Text = Option.Label;
                    Item.ToolTipText = Option.tooltip?.ToString();

                    Item.Header.OnClick += (obj, e) =>
                    {
                        Option.action();

                        if (CloseOnOptionClick)
                        {
                            Close();
                        }
                    };

                    ListView.AddItem(Item);
                }
            }
        }
    }
}
