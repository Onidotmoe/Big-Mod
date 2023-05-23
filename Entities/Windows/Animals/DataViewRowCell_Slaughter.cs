using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_Slaughter : DataViewRowCell_Toggle
    {
        public Pawn Pawn;

        public DataViewRowCell_Slaughter(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Button.CanToggle = CanToggle();

            if (!Button.CanToggle)
            {
                AddToolTipIcon();
            }
        }

        public override bool CanToggle()
        {
            return (Pawn.RaceProps.Animal && Pawn.RaceProps.IsFlesh && (Pawn.Faction == Faction.OfPlayer) && Pawn.SpawnedOrAnyParentSpawned);
        }

        public override void Pull()
        {
            Button.ToggleState = (Pawn.MapHeld?.designationManager.DesignationOn(Pawn, DesignationDefOf.Slaughter) != null);

            ToolTipText = (Button.ToggleState ? "DesignatorSlaughterDesc".Translate() : string.Empty);
        }

        public override void Push()
        {
            if (Button.ToggleState)
            {
                // Will complain if we don't check if the Designator exist first before trying to add it.
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.Slaughter) == null)
                {
                    Pawn.MapHeld.designationManager.AddDesignation(new Designation(Pawn, DesignationDefOf.Slaughter, null));
                    SlaughterDesignatorUtility.CheckWarnAboutBondedAnimal(Pawn);
                }
            }
            else
            {
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.Slaughter) != null)
                {
                    Pawn.MapHeld.designationManager.TryRemoveDesignationOn(Pawn, DesignationDefOf.Slaughter);
                }
            }
        }
    }
}
