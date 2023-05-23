using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_Pregnant : DataViewRowCell_Data
    {
        public Label Label;
        public Pawn Pawn;
        public float Progress
        {
            get
            {
                Hediff Hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant, true);

                if (Hediff == null)
                {
                    return 0f;
                }

                return ((Hediff_Pregnant)Hediff).GestationProgress;
            }
        }

        public DataViewRowCell_Pregnant(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Label = new Label();
            Label.SetStyle("DataViewRowCell_Pregnant.Label");
            Label.InheritParentSize = true;
            AddChild(Label);
        }

        public override void Pull()
        {
            if (Progress > 0)
            {
                Label.Text = Progress.ToStringPercentEmptyZero();
                // Copied from PawnColumnWorker_Pregnant.GetTooltipText
                ToolTipText = "PregnantIconDesc".Translate(((int)(Progress * (Pawn.RaceProps.gestationPeriodDays * 60000f))).ToStringTicksToDays("F0"), ((int)(Pawn.RaceProps.gestationPeriodDays * 60000f)).ToStringTicksToDays("F0"));
            }
            else
            {
                Label.Text = string.Empty;
                ToolTipText = string.Empty;
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return Progress.CompareTo(((DataViewRowCell_Pregnant)Other).Progress);
        }
    }
}
