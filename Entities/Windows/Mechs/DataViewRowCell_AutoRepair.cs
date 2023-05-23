using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Mechs
{
    public class DataViewRowCell_AutoRepair : DataViewRowCell_Toggle
    {
        public Pawn Pawn;

        public DataViewRowCell_AutoRepair(Pawn Pawn)
        {
            this.Pawn = Pawn;

            // Called here as Pawn isn't set in Base Constructor.
            Button.CanToggle = CanToggle();
        }

        public override bool CanToggle()
        {
            return (Pawn.GetComp<CompMechRepairable>() != null);
        }

        public override void Pull()
        {
            CompMechRepairable Component = Pawn.GetComp<CompMechRepairable>();

            if (Component != null)
            {
                Button.IsVisible = true;
                Button.ToggleState = Component.autoRepair;
            }
            else
            {
                Button.IsVisible = false;
            }
        }

        public override void Push()
        {
            CompMechRepairable Component = Pawn.GetComp<CompMechRepairable>();

            if (Component != null)
            {
                Component.autoRepair = Button.ToggleState;
            }
        }
    }
}
