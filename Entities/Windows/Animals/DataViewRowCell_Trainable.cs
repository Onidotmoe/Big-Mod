using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_Trainable : DataViewRowCell_Toggle
    {
        public Pawn Pawn;
        public TrainableDef Def;

        public DataViewRowCell_Trainable(Pawn Pawn, TrainableDef Def)
        {
            this.Pawn = Pawn;
            this.Def = Def;

            Button.CanToggle = CanToggle();

            if (!Button.CanToggle)
            {
                AddToolTipIcon();
            }
        }

        public override bool CanToggle()
        {
            if (Pawn.training == null)
            {
                return false;
            }

            AcceptanceReport Report = Pawn.training.CanAssignToTrain(Def, out bool State);

            if (!State || !Report.Accepted)
            {
                ToolTipText = Report.Reason.CapitalizeFirst();
                Button.IsVisible = false;

                return false;
            }

            ToolTipText = string.Empty;
            Button.IsVisible = true;

            return true;
        }

        public override void Pull()
        {
            Button.ToggleState = Pawn.training.GetWanted(Def);
        }

        public override void Push()
        {
            if (CanToggle())
            {
                Pawn.training.SetWantedRecursive(Def, Button.ToggleState);
            }
        }
    }
}
