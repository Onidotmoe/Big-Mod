using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class Faction : Button, IPawn, IPull
    {
        public Pawn Pawn { get; set; }

        public Faction(float Width, float Height)
        {
            Size = new Vector2(Width, Height);

            AddImage();

            Style.DrawBackground = true;

            Image.InheritParentSize = false;
            Image.Size = new Vector2(Height, Height);
            Label.OffsetX = (Image.Width + 5f);
            Label.Style.TextAnchor = TextAnchor.MiddleLeft;
        }

        public void Pull()
        {
            if ((Pawn.Faction != null) && !Pawn.Faction.Hidden)
            {
                Image.Texture = Pawn.Faction.def.FactionIcon;
                Image.Style.Color = Pawn.Faction.Color;
                Text = Pawn.Faction.Name;
                ToolTipText = ("Faction".Translate() + ": " + Pawn.Faction.Name).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "FactionDesc".Translate(Pawn.Named("PAWN")).Resolve() + "\n\n" + "ClickToViewFactions".Translate().Resolve().Colorize(ColoredText.SubtleGrayColor);

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

            WindowManager.TryToggleWindowVanilla<Dialog_FactionDuringLanding>();
        }
    }
}
