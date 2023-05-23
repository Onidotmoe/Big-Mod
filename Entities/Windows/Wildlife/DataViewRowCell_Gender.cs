using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Wildlife
{
    public class DataViewRowCell_Gender : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public Gender Gender
        {
            get
            {
                return Pawn.gender;
            }
        }
        public Image Image;

        public DataViewRowCell_Gender(Pawn Pawn)
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
            Image.IsVisible = (Gender != Gender.None);

            if (Image.IsVisible)
            {
                // Copied from PawnColumnWorker_Gender.GetIconFor
                Image.Texture = Gender.GetIcon();
                Image.ToolTipText = Pawn.GetGenderLabel().CapitalizeFirst();
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return Gender.CompareTo(((DataViewRowCell_Gender)Other).Gender);
        }
    }
}
