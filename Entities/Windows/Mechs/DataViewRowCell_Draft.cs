using Verse;

namespace BigMod.Entities.Windows.Mechs
{
    public class DataViewRowCell_Draft : DataViewRowCell_Toggle
    {
        public Pawn Pawn;

        public DataViewRowCell_Draft(Pawn Pawn)
        {
            this.Pawn = Pawn;

            // Called here as Pawn isn't set in Base Constructor.
            Button.CanToggle = CanToggle();
        }

        public override bool CanToggle()
        {
            AcceptanceReport Report = MechanitorUtility.CanDraftMech(Pawn);

            if (!Report && !string.IsNullOrWhiteSpace(Report.Reason))
            {
                ToolTipText = Report.Reason.CapitalizeFirst();
                Button.IsVisible = false;
            }
            else
            {
                ToolTipText = string.Empty;
                Button.IsVisible = true;
            }

            return Report;
        }

        public override void Pull()
        {
            Button.ToggleState = Pawn.drafter.Drafted;
        }

        public override void Push()
        {
            // Ensure it is still valid to be drafted.
            if (MechanitorUtility.CanDraftMech(Pawn))
            {
                Pawn.drafter.Drafted = Button.ToggleState;
            }
        }
    }
}
