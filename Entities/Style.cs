using Verse;

namespace BigMod.Entities
{
    /// <summary>
    /// Holds all styling information, like Colors.
    /// </summary>
    public class Style
    {
        /// <summary>
        /// General Entity Color.
        /// </summary>
        public Color Color;
        /// <summary>
        /// Color of any text.
        /// </summary>
        public Color TextColor;
        /// <summary>
        /// Color of any text outline.
        /// </summary>
        public Color TextOutlineColor;
        /// <summary>
        /// Thickness of the Text Outline, disabled if 0.
        /// </summary>
        public int OutlineWidth;
        /// <summary>
        /// If Entity can be selected this is the overlay color when selected.
        /// </summary>
        public Color SelectedColor;
        /// <summary>
        /// Color of Border.
        /// </summary>
        public Color BorderColor;
        /// <summary>
        /// Color of background box.
        /// </summary>
        public Color BackgroundColor;
        /// <summary>
        /// Color overlay applied when disabled.
        /// </summary>
        public Color DisabledColor;
        /// <summary>
        /// Color when Mouse is over.
        /// </summary>
        public Color MouseOverColor;
        /// <summary>
        /// Color when Mouse is down.
        /// </summary>
        public Color MouseDownColor;
        /// <summary>
        /// Color of any Text when Mouse is over.
        /// </summary>
        public Color MouseOverTextColor;
        /// <summary>
        /// Toggles the Background.
        /// </summary>
        public bool DrawBackground;
        /// <summary>
        /// Toggles the Border.
        /// </summary>
        public bool DrawBorder;
        /// <summary>
        /// Toggles MouseOver overlay.
        /// </summary>
        public bool DrawMouseOver;
        /// <summary>
        /// Thickness of this Entity's surrounding border, disabled if 0.
        /// </summary>
        public int BorderThickness;
        /// <summary>
        /// Should strings that exceed their parent bounds wrap?
        /// </summary>
        public bool WordWrap;
        private GameFont _FontType = GameFont.Small;
        /// <summary>
        /// Font to be used when rendering text.
        /// </summary>
        public GameFont FontType
        {
            get
            {
                return _FontType;
            }
            set
            {
                _FontType = value;
                Font = Verse.Text.fontStyles[(int)value].font;
            }
        }
        /// <summary>
        /// Font to be used when rendering text.
        /// </summary>
        public Font Font = Verse.Text.fontStyles[(int)GameFont.Small].font;
        /// <summary>
        /// Anchor to align text when rendering.
        /// </summary>
        public TextAnchor TextAnchor = TextAnchor.MiddleCenter;
        /// <summary>
        /// Offset to apply to rendered Text.
        /// </summary>
        public Vector2 TextOffset = Vector2.zero;
        /// <summary>
        /// Used by <see cref="ResetColors(string)"/> to fill colors when <see cref="Palette"/> doesn't have them.
        /// </summary>
        public string PaletteFallback = nameof(Panel);
        /// <summary>
        /// Current color palette.
        /// </summary>
        public string Palette;

        public Style()
        {
        }

        public Style(string Palette) : this()
        {
            this.Palette = Palette;

            if (Globals.Palettes.ContainsKey(Palette))
            {
                SetPalette(Palette);
            }
        }

        /// <summary>
        /// Creates a Shallow Copy of this Style.
        /// </summary>
        /// <returns>Shallow Copy of this object.</returns>
        public Style Clone()
        {
            return (Style)MemberwiseClone();
        }

        /// <summary>
        /// Advances the Hierarchy by setting the current <see cref="PaletteFallback"/> to <see cref="Palette"/> and Importing the new Palette onto the existing colors.
        /// </summary>
        /// <param name="Hierarch"></param>
        public void SetHierarchy(string Hierarch)
        {
            PaletteFallback = Palette;
            Palette = Hierarch;

            if (Globals.Palettes.ContainsKey(Palette))
            {
                SetPalette(Palette);
            }
        }

        /// <summary>
        /// Sets colors from the given Palette. Will use <see cref="PaletteFallback"/>, if Palette doesn't exist.
        /// </summary>
        /// <remarks>Unlike <see cref="ImportPalette(string)"/> this will override all colors and <see cref="Palette"/>.</remarks>
        public void SetPalette(string Palette)
        {
            this.Palette = Palette;

            if (!Globals.Palettes.TryGetValue(Palette, out Globals.Sheet.Style.Proxy Proxy))
            {
                if (!Globals.Palettes.TryGetValue(PaletteFallback, out Proxy))
                {
                    Globals.WriteLineError($"Style.SetPalette : Tried to get Palette '{Palette}' and failed. Tried to get PaletteFallback '{PaletteFallback}' but it didn't exist.");
                    return;
                }
            }

            Proxy.Apply(this);
        }
    }
}
