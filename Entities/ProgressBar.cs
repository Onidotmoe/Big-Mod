using Verse;

namespace BigMod.Entities
{
    public enum ProgressBarStyle
    {
        /// <summary>
        /// No texture just a color based on <see cref="Style.BackgroundColor"/>.
        /// </summary>
        None,
        /// <summary>
        /// Fill texture.
        /// </summary>
        Texture,
        /// <summary>
        /// Fill texture with a background.
        /// </summary>
        TextureWithBackground,
        /// <summary>
        /// Text in the foreground.
        /// </summary>
        Label,
    }

    public class ProgressBar : Panel
    {
        /// <summary>
        /// <para>Thresholds are Colors that are used inbetween ranges. Instead of gradual change between Min and Max values, Thresholds allow hardlines between Color changes.</para>
        /// <para>Will prevent <see cref="ColorOverride"/> from being used.</para>
        /// </summary>
        public bool UseThresholds;
        /// <summary>
        /// When <see cref="Percentage"/> has surpassed a value inside this Dictionary, it'll try to change the Color to the next available value.
        /// </summary>
        public Dictionary<float, Color> Thresholds;
        /// <summary>
        /// Toggle the request data pulling if not needed.
        /// </summary>
        public virtual bool DoRequest { get; set; } = true;
        /// <summary>
        /// Current position in request throttling.
        /// </summary>
        private int RequestCurrent;
        /// <summary>
        /// How many update cycles between updating the bar.
        /// </summary>
        public virtual int RequestRate { get; set; } = 120;
        /// <summary>
        /// Use to draw indicator arros when <see cref="RenderStyle"/> is set to <see cref="ProgressBarStyle.Arrows"/>.
        /// </summary>
        public float Delta;
        public ProgressBarStyle RenderStyle = ProgressBarStyle.None;
        /// <summary>
        /// Called by <see cref="DoOnRequest"/> when <see cref="RequestCurrent"/> is at or suppasses <see cref="RequestCurrent"/>.
        /// </summary>
        public event EventHandler OnRequest;
        /// <summary>
        /// Called when the bar becomes full.
        /// </summary>
        public event EventHandler OnFull;
        /// <summary>
        /// Called when the bar percentage has been changed.
        /// </summary>
        public event EventHandler OnChange;
        /// <summary>
        /// Called when the bar becomes empty.
        /// </summary>
        public event EventHandler OnEmpty;
        public bool WordWrap;
        private int _Cache_LabelWidth;
        /// <summary>
        /// Bounds of the texture, updated On Change.
        /// </summary>
        private Rect _Cache_TextureBounds;
        private bool _IsVertical;
        private bool _DoArrows;
        /// <summary>
        /// Shows a series of Arrows indicating if the number has been changed, will not show anything if it hasn't changed.
        /// </summary>
        public bool DoArrows
        {
            get
            {
                return _DoArrows;
            }
            set
            {
                if (_DoArrows != value)
                {
                    _DoArrows = value;

                    if (_DoArrows)
                    {
                        AddArrows();
                    }
                    else
                    {
                        RemoveArrows();
                    }
                }
            }
        }
        /// <summary>
        /// The current amount of visible arrows, negative means that delta sum is negative.
        /// </summary>
        public int ArrowsCurrent;
        /// <summary>
        /// How many Arrows can be active at any given time.
        /// </summary>
        public int ArrowsMax = 3;
        /// <summary>
        /// Size of each Arrow.
        /// </summary>
        public Vector2 ArrowSize = new Vector2(12f, 12f);
        /// <summary>
        /// Offset of the Arrows.
        /// </summary>
        public Vector2 ArrowOffset = new Vector2(-30f, 0f);
        public Dictionary<int, Image> Arrows;
        /// <summary>
        /// Should the bar be filled Bottom to Top instead of Left to Right?
        /// </summary>
        public bool IsVertical
        {
            get
            {
                return _IsVertical;
            }
            set
            {
                _IsVertical = value;
                // Update the appearance of the ProgressBar
                DoOnChange();
            }
        }
        private string _Text;
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                _Cache_LabelWidth = (int)Verse.Text.CalcSize(_Text).x;
            }
        }
        /// <summary>
        /// Background texture to draw when <see cref="RenderStyle"/> is set to <see cref="ProgressBarStyle.TextureWithBackground"/>.
        /// </summary>
        public Texture2D TextureBackground;
        private Color _ColorMax = Globals.GetColor("ProgressBar.ColorMax");
        /// <summary>
        /// Color to draw with when <see cref="Percentage"/> is 1.
        /// </summary>
        public Color ColorMax
        {
            get
            {
                return _ColorMax;
            }
            set
            {
                if (_ColorMax != value)
                {
                    _ColorMax = value;
                    // Current Color has to be updated everytime any color is changed.
                    UpdateColor();
                }
            }
        }
        private Color _ColorMin = Globals.GetColor("ProgressBar.ColorMin");
        /// <summary>
        /// Color to draw with when <see cref="Percentage"/> is 0.
        /// </summary>
        public Color ColorMin
        {
            get
            {
                return _ColorMin;
            }
            set
            {
                if (_ColorMin != value)
                {
                    _ColorMin = value;
                    UpdateColor();
                }
            }
        }
        private Color _ColorOverride;
        /// <summary>
        /// If set will be used instead of <see cref="ColorMin"/> and <see cref="ColorMax"/>. Won't be used if <see cref="UseThresholds"/> is true.
        /// </summary>
        public Color ColorOverride
        {
            get
            {
                return _ColorOverride;
            }
            set
            {
                if (_ColorOverride != value)
                {
                    _ColorOverride = value;
                    UpdateColor();
                }
            }
        }
        /// <summary>
        /// Current Color to draw with. Updated by <see cref="DoOnChange"/>.
        /// </summary>
        public Color ColorCurrent;
        public ScaleMode ScaleMode = ScaleMode.StretchToFill;
        public bool AlphaBlend = true;
        public float ImageAspect = 1;
        private float _Percentage;
        public virtual float Percentage
        {
            get
            {
                return _Percentage;
            }
            set
            {
                if (_Percentage != value)
                {
                    float Old_Percentage = _Percentage;
                    _Percentage = value;

                    Delta = (_Percentage - Old_Percentage);

                    if ((Old_Percentage + Delta) >= 1f)
                    {
                        DoOnFull();
                    }
                    else if ((Old_Percentage + Delta) <= 0f)
                    {
                        DoOnEmpty();
                    }

                    DoOnChange();
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (DoRequest)
            {
                if (RequestCurrent >= RequestRate)
                {
                    RequestCurrent = 0;
                    DoOnRequest();
                }
                else
                {
                    RequestCurrent++;
                }
            }
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                DrawUnderlays();

                Verse.Text.Font = Style.FontType;
                Verse.Text.Anchor = Style.TextAnchor;
                Verse.Text.WordWrap = WordWrap;

                switch (RenderStyle)
                {
                    case ProgressBarStyle.None:
                        GUI.DrawTexture(_Cache_TextureBounds, BaseContent.WhiteTex, ScaleMode, AlphaBlend, ImageAspect, ColorCurrent, Style.BorderThickness, Style.BorderThickness);
                        break;

                    case ProgressBarStyle.Texture:
                        Widgets.FillableBar(Bounds, Percentage, Texture);
                        break;

                    case ProgressBarStyle.TextureWithBackground:
                        Widgets.FillableBar(Bounds, Percentage, Texture, TextureBackground, (Style.BorderThickness > 0));
                        break;

                    case ProgressBarStyle.Label:
                        Widgets.FillableBarLabeled(Bounds, Percentage, _Cache_LabelWidth, Text);
                        break;
                }

                Verse.Text.Anchor = TextAnchor.UpperLeft;
                Verse.Text.WordWrap = true;

                DrawChildren();
            }
        }

        public virtual void UpdateArrows()
        {
            if (DoArrows && (Delta != 0f))
            {
                if (Delta > 0f)
                {
                    // Don't add more arrows if already at Max.
                    if (ArrowsCurrent < ArrowsMax)
                    {
                        ArrowsCurrent += 1;
                    }
                }
                else if (Delta < 0f)
                {
                    if (ArrowsCurrent > -ArrowsMax)
                    {
                        ArrowsCurrent -= 1;
                    }
                }

                foreach (var KeyValue in Arrows)
                {
                    if (((ArrowsCurrent > 0f) && (KeyValue.Key > 0f)) || ((ArrowsCurrent < 0f) && (KeyValue.Key < 0f)))
                    {
                        KeyValue.Value.IsVisible = true;
                    }
                    else
                    {
                        KeyValue.Value.IsVisible = false;
                    }
                }
            }
        }

        public virtual void AddArrows()
        {
            Arrows = new Dictionary<int, Image>();

            for (int i = -ArrowsMax; i < ArrowsMax; i++)
            {
                Image Arrow = new Image(Globals.TryGetTexturePathFromAlias(((i < 0) ? "DeltaArrowLeft" : "DeltaArrowRight")), ArrowSize.x, ArrowSize.y);
                Arrow.Style.Color = Globals.GetColor((i < 0) ? "ProgressBar.Arrows.Negative" : "ProgressBar.Arrows.Positive");
                Arrow.ID = i.ToString();
                Arrow.Anchor = Anchor.CenterRight;
                Arrow.Offset = new Vector2((-(((ArrowSize.y / 2) * Math.Abs(i)) + 5f) + ArrowOffset.x), ArrowOffset.y);
                Arrow.IsVisible = false;

                Arrows.Add(i, Arrow);
                AddChild(Arrow);
            }

            UpdateArrows();
        }

        public virtual void RemoveArrows()
        {
            if (Arrows != null)
            {
                foreach (Image Arrow in Arrows.Values)
                {
                    Arrow.RemoveFromParent();
                }

                Arrows = null;
            }
        }

        /// <summary>
        /// Handles data retrieving.
        /// </summary>
        public virtual void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the bar becomes full.
        /// </summary>
        public virtual void DoOnFull()
        {
            OnFull?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the bar percentage has been changed.
        /// </summary>
        public virtual void DoOnChange()
        {
            UpdateColor();
            UpdateTextureBounds();
            UpdateArrows();

            OnChange?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the bar becomes empty.
        /// </summary>
        public virtual void DoOnEmpty()
        {
            OnEmpty?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateTextureBounds()
        {
            if (RenderStyle == ProgressBarStyle.None)
            {
                if (!IsVertical)
                {
                    _Cache_TextureBounds = new Rect(X, Y, (Width * Percentage), Height);
                }
                else
                {
                    // Changes from Top-to-Bottom, to Bottom-to-Top
                    _Cache_TextureBounds = new Rect(X, (Y + (Height * (1f - Percentage))), Width, (Height * Percentage));
                }
            }
        }

        public override void DoOnBoundsChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnBoundsChanged(Sender, EventArgs);

            UpdateTextureBounds();
        }

        /// <summary>
        /// Update the CurrentColor to reflect the fill Percentage of the ProgressBar.
        /// </summary>
        public virtual void UpdateColor()
        {
            if (!UseThresholds)
            {
                ColorCurrent = ((ColorOverride != Color.clear) ? ColorOverride : Color.Lerp(ColorMin, ColorMax, Percentage));
            }
            else
            {
                float Closest = Thresholds.Keys.Aggregate((F, F2) => ((Mathf.Abs(F - Percentage) < Mathf.Abs(F2 - Percentage)) ? F : F2));
                Thresholds.TryGetValue(Closest, out ColorCurrent);
            }
        }

        /// <summary>
        /// Adds all Aliases that start with the <paramref name="AliasParent"/> and uses their suffix as the Threshold value.
        /// </summary>
        /// <param name="AliasParent">Base string of Aliases to add.</param>
        public virtual void AddThresholds(string AliasParent)
        {
            IEnumerable<string> Keys = Globals.Assigns.Keys.Where((F) => F.StartsWith(AliasParent));

            foreach (string Key in Keys)
            {
                float Threshold = float.Parse(Key.Replace(AliasParent, string.Empty));

                Thresholds.Add((Threshold / 100), Globals.GetColor(Key));
            }
        }

        /// <summary>
        /// The Threshold Ticks that divide a bar into segments. Use this for divisions too.
        /// </summary>
        /// <param name="Threshold">Where the Tick should be located.</param>
        /// <param name="Percentage">Applies either of 2 colors based on if Threshold has based Percentage or not.</param>
        /// <param name="Width">Width of the ProgressBar.</param>
        /// <param name="Tick_Width">Width of the Tick.</param>
        /// <param name="Tick_Height">Height of the Tick.</param>
        /// <returns>Image with correct positioning relative to the supplied dimensions, will be Anchored to BottomLeft.</returns>
        public static Image Tick(float Threshold, float Percentage, float Width, float Tick_Width, float Tick_Height)
        {
            Image Image = new Image(ImageStyle.Color, Tick_Width, Tick_Height);
            Image.ID = "Tick";
            Image.Anchor = Anchor.BottomLeft;
            Image.Offset = new Vector2(((Width * Threshold) - (Image.Width / 2)), 0f);
            Image.Style.Color = ((Threshold < Percentage) ? Globals.GetColor("ProgressBar.Tick.Below") : Globals.GetColor("ProgressBar.Tick.Above"));

            return Image;
        }

        /// <summary>
        /// A Marker that indicates the Instant Level Percentage of a Need.
        /// </summary>
        /// <param name="Percentage">Where the Marker should be located.</param>
        /// <param name="Width">Width of the ProgressBar.</param>
        /// <param name="Height">Height of the ProgressBar.</param>
        /// <returns>Image with correct positioning relative to the supplied dimensions, will be Anchored to BottomLeft.</returns>
        public static Image Marker(float Percentage, float Width, float Height)
        {
            Image Image = new Image(Globals.TryGetTexturePathFromAlias("Marker"), Height, Height);
            Image.ID = "Marker";
            Image.Anchor = Anchor.BottomLeft;
            Image.Style.Color = Color.white;

            Image.Offset = new Vector2(((Width * Percentage) - (Image.Width / 2)), 0f);

            return Image;
        }
    }
}
