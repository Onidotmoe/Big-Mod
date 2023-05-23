using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_ReleaseToWild : DataViewRowCell_Toggle
    {
        public Pawn Pawn;

        public DataViewRowCell_ReleaseToWild(Pawn Pawn)
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
            Button.ToggleState = (Pawn.MapHeld?.designationManager.DesignationOn(Pawn, DesignationDefOf.ReleaseAnimalToWild) != null);

            ToolTipText = (Button.ToggleState ? "DesignatorReleaseAnimalToWildDesc".Translate() : string.Empty);
        }

        public override void Push()
        {
            if (Button.ToggleState)
            {
                // Will complain if we don't check if the Designator exist first before trying to add it.
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.ReleaseAnimalToWild) == null)
                {
                    Pawn.MapHeld.designationManager.AddDesignation(new Designation(Pawn, DesignationDefOf.ReleaseAnimalToWild, null));
                    SlaughterDesignatorUtility.CheckWarnAboutBondedAnimal(Pawn);
                }
            }
            else
            {
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.ReleaseAnimalToWild) != null)
                {
                    Pawn.MapHeld.designationManager.TryRemoveDesignationOn(Pawn, DesignationDefOf.ReleaseAnimalToWild);
                }
            }
        }
    }
}
