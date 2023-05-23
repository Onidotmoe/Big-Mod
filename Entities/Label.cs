using Verse;

namespace BigMod.Entities
{
    public enum LabelStyle
    {
        /// <summary>
        /// Draws the Label without any extra processing.
        /// </summary>
        None,
        /// <summary>
        /// Prevents the Label's Text from exceeding its Bounds.
        /// </summary>
        Fit,
        /// <summary>
        /// Adds Scrollbars if Label's Text exceeds its Bounds.
        /// </summary>
        Scrollable,
        /// <summary>
        /// Uses <see cref="GUI.Label(Rect, string)"/> instead of <see cref="Widgets"/>.
        /// </summary>
        GUI
    }

    public class Label : Panel
    {
        public LabelStyle RenderStyle = LabelStyle.None;
        private string _Text;
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    DoSizeToText();
                }
            }
        }
        public Vector2 ScrollbarPosition = Vector2.zero;
        public bool DontConsumeScrollEventsIfNoScrollbar;
        public bool TakeScrollbarSpaceEvenIfNoScrollbar = true;
        public bool LongLabel;
        public bool SizeToText_Horizontal;
        public bool SizeToText_Vertical;
        /// <summary>
        /// A buffer added when using <see cref="SizeToText"/>.
        /// </summary>
        public Vector2 SizeToTextBuffer = new Vector2(2f, 2f);
        public override bool IgnoreMouse { get; set; } = true;

        public Label(bool SizeToText_Horizontal = false, bool SizeToText_Vertical = false)
        {
            if (SizeToText_Horizontal || SizeToText_Vertical)
            {
                SizeToText(SizeToText_Horizontal, SizeToText_Vertical);
            }
        }

        public Label(string Text) : this()
        {
            this.Text = Text;
        }

        public Label(string Text, Vector2 Size) : this(Text)
        {
            this.Size = Size;
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                base.Draw();

                Verse.Text.Font = Style.FontType;
                Verse.Text.Anchor = Style.TextAnchor;
                Verse.Text.WordWrap = Style.WordWrap;

                if (Style.OutlineWidth > 0)
                {
                    DrawOutline();
                }

                GUI.color = GetActiveTextColor();

                switch (RenderStyle)
                {
                    case LabelStyle.None:
                        Widgets.Label(Bounds, Text);
                        break;

                    case LabelStyle.Fit:
                        Widgets.LabelFit(Bounds, Text);
                        break;

                    case LabelStyle.Scrollable:
                        Widgets.LabelScrollable(Bounds, Text, ref ScrollbarPosition, DontConsumeScrollEventsIfNoScrollbar, TakeScrollbarSpaceEvenIfNoScrollbar, LongLabel);
                        break;

                    case LabelStyle.GUI:
                        // TODO: Issues related with Widgets.Label implementation vs GUI.Label
                        GUI.skin.label.font = Style.Font;
                        GUI.skin.label.alignment = Style.TextAnchor;
                        GUI.skin.label.wordWrap = Style.WordWrap;
                        GUI.skin.label.contentOffset = Style.TextOffset;
                        GUI.skin.label.normal.textColor = GetActiveTextColor();

                        GUI.Label(Bounds, Text);
                        break;
                }

                // Text Anchoring always has to be reset manually to UpperLeft at the end of the frame or the game will throw a exception
                Verse.Text.Anchor = TextAnchor.UpperLeft;
                Verse.Text.WordWrap = true;
                GUI.color = Color.white;
            }
        }

        public void DrawOutline()
        {
            GUI.color = Style.TextOutlineColor;
            int i;

            for (i = -Style.OutlineWidth; i <= Style.OutlineWidth; i++)
            {
                GUI.Label(new Rect((X - Style.OutlineWidth), (Y + i), Width, Height), Text, GUIStyle);
                GUI.Label(new Rect((X + Style.OutlineWidth), (Y + i), Width, Height), Text, GUIStyle);
            }
            for (i = -Style.OutlineWidth + 1; i <= Style.OutlineWidth - 1; i++)
            {
                GUI.Label(new Rect((X + i), (Y - Style.OutlineWidth), Width, Height), Text, GUIStyle);
                GUI.Label(new Rect((X + i), (Y + Style.OutlineWidth), Width, Height), Text, GUIStyle);
            }

            GUI.color = Style.TextColor;
        }

        public override void DoOnSizeChanged(object Sender, EventArgs EventArgs)
        {
            // Text has to be sized first to allow parents to inherit the new size.
            DoSizeToText();

            base.DoOnSizeChanged(Sender, EventArgs);
        }

        public void SizeToText(bool Horizontol = true, bool Vertical = false)
        {
            SizeToText_Horizontal = Horizontol;
            SizeToText_Vertical = Vertical;

            // Update size immediately
            DoOnSizeChanged(this, EventArgs.Empty);
        }

        public void DoSizeToText()
        {
            if (SizeToText_Horizontal || SizeToText_Vertical)
            {
                Vector2 TextSize = GetTextSize();

                if (SizeToText_Horizontal)
                {
                    _Bounds.width = (TextSize.x + SizeToTextBuffer.x);
                }
                if (SizeToText_Vertical)
                {
                    _Bounds.height = (TextSize.y + SizeToTextBuffer.y);
                }
            }
        }

        /// <summary>
        /// Gets the rendered size of the <see cref="Text"/>.
        /// </summary>
        /// <returns>Dimensions of rendered text.</returns>
        public Vector2 GetTextSize()
        {
            // Has to apply all this to get the real size.
            Verse.Text.Font = Style.FontType;
            Verse.Text.Anchor = Style.TextAnchor;
            Verse.Text.WordWrap = Style.WordWrap;

            Vector2 Size = Verse.Text.CalcSize(_Text);

            Verse.Text.Anchor = TextAnchor.UpperLeft;
            Verse.Text.WordWrap = true;

            return Size;
        }

        /// <summary>
        /// Gets the rendered Height of the <see cref="Text"/>.
        /// </summary>
        /// <param name="Width">Optional Width override, otherwise will use the Label's Width.</param>
        /// <returns>Height of rendered text.</returns>
        public float GetTextHeight(float Width = -1f)
        {
            // Has to apply all this to get the real size.
            Verse.Text.Font = Style.FontType;
            Verse.Text.Anchor = Style.TextAnchor;
            Verse.Text.WordWrap = Style.WordWrap;

            float Height = Verse.Text.CalcHeight(Text, ((Width != -1f) ? Width : this.Width));

            Verse.Text.Anchor = TextAnchor.UpperLeft;
            Verse.Text.WordWrap = true;

            return Height;
        }
    }
}
