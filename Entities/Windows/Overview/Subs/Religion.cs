using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class Religion : Button, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public ProgressBar ProgressBar = new ProgressBar() {Anchor = Anchor.BottomLeft, DoRequest = false};

        public Religion(float Width, float Height)
        {
            Size = new Vector2(Width, Height);

            AddImage();

            Style.DrawBackground = true;

            Image.InheritParentSize = false;
            Image.Size = new Vector2(Height, Height);
            Label.OffsetX = (Image.Width + 5f);
            Label.Style.TextAnchor = TextAnchor.MiddleLeft;

            ProgressBar.Size = new Vector2(Width, (Height / 10f));

            AddChild(ProgressBar);
        }

        public void Pull()
        {
            if (Pawn.ideo != null)
            {
                Image.Texture = Pawn.Ideo.Icon;
                Image.Style.Color = Pawn.Ideo.Color;
                Text = Pawn.ideo.Ideo.name;

                Precept_Role Role = Pawn.Ideo.GetRole(Pawn);

                ToolTipText = Pawn.Ideo.name.Colorize(ColoredText.TipSectionTitleColor);
                ToolTipText += "\n\n" + "Certainty".Translate().CapitalizeFirst().Resolve() + " : " + Pawn.ideo.Certainty.ToStringPercent();

                if (Pawn.ideo.PreviousIdeos.Any())
                {
                    ToolTipText += "\n\n" + "Formerly".Translate().CapitalizeFirst() + ": \n" + (from Ideo in Pawn.ideo.PreviousIdeos select Ideo.name).ToLineList("  - ", false);
                }

                if (Role != null)
                {
                    ToolTipText += "\n\n" + Role.GetTip();
                }

                ToolTipText += "\n\n" + "ClickForMoreInfo".Translate().Resolve().Colorize(ColoredText.SubtleGrayColor);

                ProgressBar.ColorOverride = Pawn.ideo.Ideo.Color;
                ProgressBar.Percentage = Pawn.ideo.Certainty;

                IsVisible = true;
            }
            else
            {
                IsVisible = false;
            }
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClick(Sender, EventArgs);

            WindowManager.TryToggleWindowVanilla<Dialog_IdeosDuringLanding>();
        }
    }
}
