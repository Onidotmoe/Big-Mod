using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Wildlife
{
    public class DataViewRowCell_Hunt : DataViewRowCell_Toggle
    {
        public Pawn Pawn;
        public int Chance;

        public DataViewRowCell_Hunt(Pawn Pawn)
        {
            this.Pawn = Pawn;

            // Called here as Pawn isn't set in Base Constructor.
            Button.CanToggle = CanToggle();

            if (!Button.CanToggle)
            {
                AddToolTipIcon();
            }
        }

        public override bool CanToggle()
        {
            // Copied from PawnColumnWorker_Hunt.HasCheckbox
            return (Pawn.AnimalOrWildMan() && Pawn.RaceProps.IsFlesh && (Pawn.Faction == null || !Pawn.Faction.def.humanlikeFaction) && Pawn.SpawnedOrAnyParentSpawned);
        }

        public override void Pull()
        {
            Button.ToggleState = (Pawn.MapHeld?.designationManager.DesignationOn(Pawn, DesignationDefOf.Hunt) != null);

            // Copied from PawnColumnWorker_ManhunterOnDamageChance.GetTextFor
            Chance = (int)(PawnUtility.GetManhunterOnDamageChance(Pawn, null) * 100f);

            if (Chance > 0)
            {
                Button.Text = Chance.ToString();
                ToolTipText = "DesignatorHuntDesc".Translate() + Environment.NewLine + "HarmedRevengeChanceExplanation".Translate();
            }
            else
            {
                Button.Text = string.Empty;
                ToolTipText = string.Empty;
            }
        }

        public override void Push()
        {
            if (Button.ToggleState)
            {
                // Will complain if we don't check if the Designator exist first before trying to add it.
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.Hunt) == null)
                {
                    // Remove Selection from Taming as these 2 are mutually exclusive.
                    DataViewRow.Cells.OfType<DataViewRowCell_Tame>().FirstOrDefault()?.Floor();

                    Pawn.MapHeld.designationManager.AddDesignation(new Designation(Pawn, DesignationDefOf.Hunt, null));
                    Designator_Hunt.ShowDesignationWarnings(Pawn);
                }
            }
            else
            {
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.Hunt) != null)
                {
                    Pawn.MapHeld.designationManager.TryRemoveDesignationOn(Pawn, DesignationDefOf.Hunt);
                }
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            // Selected Cells goto the top.
            return ((base.CompareTo(Other) * 2) + Chance.CompareTo(((DataViewRowCell_Hunt)Other).Chance));
        }
    }
}
