using BigMod.Entities.Windows;

namespace BigMod.Entities
{
    public class DropDown : Button
    {
        public ListView ListView;
        public ToolTip ToolTip;
        public Vector2 SizeExpanded = new Vector2(200f, 150f);
        public virtual event EventHandler OnClickedItem;
        public bool Collapse_OnClickedItem = true;
        /// <summary>
        /// Sets the size of the <see cref="ToolTip"/> to the height of the <see cref="ListView"/>'s Items, only if <see cref="SizeExpanded.y"/> is less than the ListView's Items.
        /// </summary>
        /// <remarks>Will be ignored if ListView doesn't have any Items.</remarks>
        public bool ToolTip_SizeToItems = true;
        /// <summary>
        /// Additional Offset applied to the <see cref="ToolTip"/> when it opens.
        /// </summary>
        public Vector2 ToolTip_Offset = Vector2.zero;

        public DropDown()
        {
            CanToggle = true;

            ToolTip = new ToolTip();
            ToolTip.AttachedToMouse = false;
            ToolTip.ManualDisposal = true;
            ToolTip.IgnoreMouseInput = false;
            ToolTip.ToolTipHost = this;
            ToolTip.layer = Verse.WindowLayer.GameUI;
            ToolTip.CloseOnMouseOutsideClick = true;
            ToolTip.SetCloseOn(true, true, true);

            ListView = new ListView(Width, Height, false);
            ListView.OffsetY = Height;
            ListView.InheritParentSize = true;
            ToolTip.Root.AddChild(ListView);

            BigMod.WindowStack.OnWindowClosed += OnWindowClosed;
        }

        public DropDown(string Text, float Width, float Height) : this()
        {
            this.Text = Text;

            Size = new Vector2(Width, Height);
        }

        public DropDown(ButtonStyle RenderStyle, string TexturePath) : this()
        {
            this.RenderStyle = RenderStyle;

            SetTexture(TexturePath);
        }

        public void OnWindowClosed(object Sender, EventArgs EventArgs)
        {
            if (Sender == ParentWindow)
            {
                WindowManager.CloseWindow(ToolTip);
                ToggleState = false;
            }
            else if (Sender == ToolTip)
            {
                ToggleState = false;
            }
        }

        public override void DoOnSizeChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnSizeChanged(Sender, EventArgs);

            if (ToolTip != null)
            {
                ToolTip.Size = ((!ToolTip_SizeToItems || !ListView.Items.Any()) ? SizeExpanded : new Vector2(SizeExpanded.x, Mathf.Min(SizeExpanded.y, (ListView.Items.Last().Bottom) + ListView.ItemMargin.y)));
            }
        }

        public override void DoOnBoundsChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnBoundsChanged(Sender, EventArgs);

            // Don't Update Position until fully initialized.
            if ((ToolTip != null) && (Parent != null) && (ParentWindow != null))
            {
                if (ToolTip.IsVisible)
                {
                    ToolTip.Position = (GetAbsolutePosition() + ToolTip_Offset);
                    ToolTip.Y += Height;
                }
            }
        }

        public override void DoOnToggleStateChanged()
        {
            base.DoOnToggleStateChanged();

            if (ToggleState)
            {
                ToolTip.Position = (GetAbsolutePosition() + ToolTip_Offset);
                ToolTip.Y += Height;
                ToolTip.Size = ((!ToolTip_SizeToItems || !ListView.Items.Any()) ? SizeExpanded : new Vector2(SizeExpanded.x, Mathf.Min(SizeExpanded.y, (ListView.Items.Last().Bottom) + ListView.ItemMargin.y)));
                WindowManager.OpenWindow(ToolTip);
            }
            else
            {
                WindowManager.CloseWindow(ToolTip);
            }
        }

        public void AddItem(ListViewItem Item)
        {
            Item.OnClick += DoOnClickedItem;
            ListView.AddItem(Item);
        }

        public void InsertItem(int Index, ListViewItem Item)
        {
            Item.OnClick += DoOnClickedItem;
            ListView.InsertItem(Index, Item);
        }

        public virtual void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            OnClickedItem?.Invoke(Sender, EventArgs.Empty);

            if (Collapse_OnClickedItem)
            {
                WindowManager.CloseWindow(ToolTip);
            }
        }
    }
}
