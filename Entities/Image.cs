using Verse;

namespace BigMod.Entities
{
    public enum ImageStyle
    {
        /// <summary>
        /// Draws a monocolored texture using <see cref="Style.Color"/>.
        /// </summary>
        Color,
        None,
        Fitted,
        Rotated,
        /// <summary>
        /// This has clipping applied to the rendered texture.
        /// </summary>
        Part,
        /// <summary>
        /// Uses <see cref="GUI.DrawTexture(Rect, Texture, ScaleMode, bool, float, Color, Vector4, Vector4)"/> instead of <see cref="Widgets"/>.
        /// </summary>
        GUI
    }

    public class Image : Panel
    {
        public ImageStyle RenderStyle = ImageStyle.GUI;
        /// <summary>
        /// Angle to apply when rendering with <see cref="ImageStyle.Rotated"/>.
        /// </summary>
        public float Angle;
        /// <summary>
        /// UV to apply for clipping when rendering with <see cref="ImageStyle.Part"/>
        /// </summary>
        public Rect TextureUV;
        public ScaleMode ScaleMode = ScaleMode.StretchToFill;
        public bool AlphaBlend = true;
        public float ImageAspect = 1f;

        public Image(Rect Bounds = default) : base(Bounds)
        {
        }

        public Image(Texture2D Texture, Rect Bounds = default) : base(Bounds)
        {
            this.Texture = Texture;
        }

        public Image(string TexturePath)
        {
            SetTexture(TexturePath);
        }

        public Image(string TexturePath, float Width = 0f, float Height = 0f) : this(TexturePath)
        {
            Size = new Vector2(Width, Height);
        }

        public Image(ImageStyle Style)
        {
            RenderStyle = Style;
        }

        public Image(ImageStyle Style, float Width = 0f, float Height = 0f) : this(Style)
        {
            Size = new Vector2(Width, Height);
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                base.Draw();

                if ((Texture != null) || (RenderStyle == ImageStyle.Color))
                {
                    switch (RenderStyle)
                    {
                        case ImageStyle.Color:
                            GUI.DrawTexture(Bounds, BaseContent.WhiteTex, ScaleMode, AlphaBlend, ImageAspect, Style.Color, Style.BorderThickness, Style.BorderThickness);
                            break;

                        case ImageStyle.None:
                            GUI.DrawTexture(Bounds, Texture);
                            break;

                        case ImageStyle.Fitted:
                            GUI.color = Style.Color;
                            Widgets.DrawTextureFitted(Bounds, Texture, TextureScale);
                            GUI.color = Color.white;
                            break;

                        case ImageStyle.Rotated:
                            Widgets.DrawTextureRotated(Bounds, Texture, Angle);
                            break;

                        case ImageStyle.Part:
                            Widgets.DrawTexturePart(Bounds, TextureUV, Texture);
                            break;

                        case ImageStyle.GUI:
                            GUI.DrawTexture(Bounds, Texture, ScaleMode, AlphaBlend, ImageAspect, Style.Color, Style.BorderThickness, Style.BorderThickness);
                            break;
                    }
                }
            }
        }
    }
}
