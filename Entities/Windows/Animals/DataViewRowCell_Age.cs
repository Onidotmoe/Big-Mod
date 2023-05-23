using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_Age : DataViewRowCell_Data
    {
        public Label Label;
        public Pawn Pawn;
        public float Age
        {
            get
            {
                return Pawn.ageTracker.BiologicalTicksPerTick;
            }
        }

        public DataViewRowCell_Age(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Label = new Label();
            Label.InheritParentSize = true;
            AddChild(Label);
        }

        public override void Pull()
        {
            Label.Text = Pawn.ageTracker.AgeBiologicalYears.ToString();
            ToolTipText = Globals.PawnColumnWorker_LifeStage_GetIconTip(Pawn);
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return Age.CompareTo(((DataViewRowCell_Age)Other).Age);
        }
    }
}
