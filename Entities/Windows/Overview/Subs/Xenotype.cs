using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class Xenotype : Button, IPawn, IPull
    {
        public Pawn Pawn { get; set; }

        public Xenotype(float Width, float Height) : base(ButtonStyle.Image)
        {
            Size = new Vector2(Width, Height);

            Style.DrawBackground = true;

            Image.Style.Color = XenotypeDef.IconColor;
        }

        public void Pull()
        {
            if (Pawn.genes != null)
            {
                Image.Texture = Pawn.genes.XenotypeIcon;
                ToolTipText = (("Xenotype".Translate() + ": " + Pawn.genes.XenotypeLabelCap).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + Pawn.genes.XenotypeDescShort) + "\n\n" + "ViewGenesDesc".Translate(Pawn.Named("PAWN")).ToString().StripTags().Colorize(ColoredText.SubtleGrayColor);

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

            WindowManager.TryToggleWindowVanilla<Dialog_ViewGenes>(Pawn);
        }
    }
}
