using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_MedicalCare : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public Button Button;
        public MedicalCareCategory MedicalCare
        {
            get
            {
                return Pawn.playerSettings.medCare;
            }
            set
            {
                if (Pawn.playerSettings.medCare != value)
                {
                    Pawn.playerSettings.medCare = value;
                    DoOnDataChanged();
                }
            }
        }

        public DataViewRowCell_MedicalCare(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Button = new Button();
            // MouseOver is drawn by the Cell instead.
            Button.Style.DrawMouseOver = false;
            Button.InheritParentSize = true;
            Button.AddImage();
            Button.Image.ScaleMode = ScaleMode.ScaleToFit;
            AddChild(Button);
        }

        public override void Pull()
        {
            Button.Image.Texture = Globals.MedicalCareUtility_MedicalCareIcon(MedicalCare);

            ToolTipText = (nameof(MedicalCareCategory) + "_" + MedicalCare).Translate().CapitalizeFirst();
        }

        public override void Floor()
        {
            MedicalCare = MedicalCareCategory.NoCare;
        }

        public override void Ceiling()
        {
            MedicalCare = MedicalCareCategory.Best;
        }

        public override void Copy(DataViewRowCell_Data Cell)
        {
            MedicalCare = ((DataViewRowCell_MedicalCare)Cell).MedicalCare;
        }

        public override void AddDelta(int Delta, bool Cycling = true)
        {
            Delta = (((int)MedicalCare) + Delta);

            if (Cycling)
            {
                if (Delta < (int)MedicalCareCategory.NoCare)
                {
                    Delta = (int)MedicalCareCategory.Best;
                }
                else if (Delta > (int)MedicalCareCategory.Best)
                {
                    Delta = (int)MedicalCareCategory.NoCare;
                }
            }
            else
            {
                Delta = Mathf.Clamp(Delta, (int)MedicalCareCategory.NoCare, (int)MedicalCareCategory.Best);
            }

            MedicalCare = (MedicalCareCategory)Delta;
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return MedicalCare.CompareTo(((DataViewRowCell_MedicalCare)Other).MedicalCare);
        }

        public override void DoOnDataChanged()
        {
            base.DoOnDataChanged();

            Pull();
        }
    }
}
