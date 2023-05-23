using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Wildlife
{
    public class DataViewRowCell_Predator : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public bool Predator
        {
            get
            {
                return Pawn.RaceProps.predator;
            }
        }
        public Image Image;

        public DataViewRowCell_Predator(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Image = new Image(Globals.GetTextureFromAlias("Predator"));
            Image.InheritParentSize = true;
            Image.ScaleMode = ScaleMode.ScaleToFit;
            Image.Style.Color = Color.white;
            AddChild(Image);
        }

        public override void Pull()
        {
            Image.IsVisible = Predator;

            if (Predator)
            {
                ToolTipText = "IsPredator".Translate();
            }
            else
            {
                ToolTipText = string.Empty;
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return Predator.CompareTo(((DataViewRowCell_Predator)Other).Predator);
        }
    }
}
