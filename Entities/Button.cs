namespace BigMod.Entities
{
    public enum ButtonStyle
    {
        Image,
        Invisible,
        /// <summary>
        /// Draws the Button using its <see cref="DrawInternal"/>.
        /// </summary>
        Text,
        /// <summary>
        /// <para>Allows you to compelete customize the button.</para>
        /// <para>Requires that the button has <see cref="Button.DrawCustom"/> set.</para>
        /// </summary>
        Custom
    }

    /// <summary>
    /// Builds the Widgets Button in the Entity based system./>
    /// </summary>
    public class Button : Panel
    {
        public string Text
        {
            get
            {
                return Label.Text;
            }
            set
            {
                Label.Text = value;
            }
        }
        public Label Label;
        public Image Image;
        public virtual event EventHandler OnToggleStateChanged;

        public Color ColorToggleOff;
        private bool _CanToggle;
        /// <summary>
        /// Can this Button be Toggled?
        /// </summary>
        public bool CanToggle
        {
            get
            {
                return _CanToggle;
            }
            set
            {
                _CanToggle = value;

                if (_CanToggle)
                {
                    Style.BackgroundColor = (ToggleState ? Style.SelectedColor : ColorToggleOff);
                }
            }
        }

        private bool _ToggleState;
        /// <summary>
        /// Current Toggle state.
        /// </summary>
        public bool ToggleState
        {
            get
            {
                return _ToggleState;
            }
            set
            {
                if (_ToggleState != value)
                {
                    _ToggleState = value;
                    DoOnToggleStateChanged();
                }
            }
        }
        /// <summary>
        /// Used when <see cref="RenderStyle"/> is set to <see cref="ButtonStyle.Custom"/>, allows complete control over the rendering of the button.
        /// </summary>
        public Action DrawCustom;

        public Button(ButtonStyle RenderStyle = ButtonStyle.Text)
        {
            Selectable = true;
            Style.DrawMouseOver = true;
            this.RenderStyle = RenderStyle;
        }

        public Button(Vector2 Size, Vector2 Offset) : this(ButtonStyle.Text)
        {
            this.Size = Size;
            this.Offset = Offset;
        }

        public Button(string Text, Vector2 Size) : this(ButtonStyle.Text)
        {
            this.Text = Text;
            this.Size = Size;
        }

        public Button(string Text, bool CanToggle = false) : this(ButtonStyle.Text)
        {
            this.Text = Text;
            this.CanToggle = CanToggle;
        }

        public Button(ButtonStyle RenderStyle, string TexturePath, bool CanToggle = false) : this(RenderStyle)
        {
            Image.SetTexture(TexturePath);
            this.CanToggle = CanToggle;
        }

        public Button(ButtonStyle RenderStyle, Vector2 Size, string TexturePath, bool CanToggle = false) : this(RenderStyle, TexturePath, CanToggle)
        {
            this.Size = Size;
        }

        private ButtonStyle _RenderStyle = ButtonStyle.Text;
        public ButtonStyle RenderStyle
        {
            get
            {
                return _RenderStyle;
            }
            set
            {
                _RenderStyle = value;

                switch (_RenderStyle)
                {
                    case ButtonStyle.Image:
                        AddImage();
                        break;

                    case ButtonStyle.Text:
                        AddLabel();
                        break;

                    case ButtonStyle.Invisible:
                        if (Label != null)
                        {
                            Label.IsVisible = false;
                        }
                        if (Image != null)
                        {
                            Image.IsVisible = false;
                        }
                        break;

                    case ButtonStyle.Custom:
                        break;

                    default:
                        break;
                }
            }
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                if (RenderStyle == ButtonStyle.Custom)
                {
                    DrawCustom();
                }

                base.Draw();
            }
        }

        public virtual void AddLabel(string Text = null)
        {
            if (Label == null)
            {
                Label = new Label();
                Label.InheritParentSize = true;
                AddChild(Label);
            }

            this.Text = Text;
        }

        public virtual void AddImage(string TexturePath = null)
        {
            if (Image == null)
            {
                Image = new Image();
                Image.InheritParentSize = true;
                // Default Image Render Color is White.
                Image.Style.Color = Color.white;
                AddChild(Image);
            }

            if (!string.IsNullOrWhiteSpace(TexturePath))
            {
                Image.SetTexture(TexturePath);
            }
        }

        public virtual void DoOnToggleStateChanged()
        {
            Style.BackgroundColor = (ToggleState ? Style.SelectedColor : ColorToggleOff);

            OnToggleStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Toggle()
        {
            ToggleState = !ToggleState;
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClick(Sender, EventArgs);

            if (CanToggle)
            {
                Toggle();
            }
        }

        public override void SetStyle(string Palette)
        {
            base.SetStyle(Palette);

            // Save this Color as the base Style will be overwrite it when ToggleState changes.
            ColorToggleOff = Style.BackgroundColor;

            if (CanToggle)
            {
                // Update the Toggle Color.
                Style.BackgroundColor = (ToggleState ? Style.SelectedColor : ColorToggleOff);
            }
        }
    }
}
