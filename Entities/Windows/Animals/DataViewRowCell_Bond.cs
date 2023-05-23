using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_Bond : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public Image Image;
        public int Bond;

        public DataViewRowCell_Bond(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Image = new Image();
            Image.InheritParentSize = true;
            Image.ScaleMode = ScaleMode.ScaleToFit;
            Image.Style.Color = Color.white;
            AddChild(Image);
        }

        public override void Pull()
        {
            // Copied from PawnColumnWorker_Bond.GetIconTip
            ToolTipText = TrainableUtility.GetIconTooltipText(Pawn);

            IEnumerable<Pawn> AllColonistBondsFor = TrainableUtility.GetAllColonistBondsFor(Pawn);

            if (!AllColonistBondsFor.Any())
            {
                Image.Texture = null;
                Image.IsVisible = false;
                Bond = 0;
            }
            else if (AllColonistBondsFor.Any((Pawn Bond) => (Bond == Pawn.playerSettings.Master)))
            {
                Image.Texture = Globals.GetTextureFromAlias("Bond");
                Image.IsVisible = true;
                Bond = 2;
            }
            else
            {
                Image.Texture = Globals.GetTextureFromAlias("BondBroken");
                Image.IsVisible = true;
                Bond = 1;
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return Bond.CompareTo(((DataViewRowCell_Bond)Other).Bond);
        }
    }
}
