using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Wildlife
{
    public class DataViewRowCell_Tame : DataViewRowCell_Toggle
    {
        public Pawn Pawn;
        public int Chance;

        public DataViewRowCell_Tame(Pawn Pawn)
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
            // Copied from PawnColumnWorker_Tame.HasCheckbox
            return (TameUtility.CanTame(Pawn) && Pawn.SpawnedOrAnyParentSpawned);
        }

        public override void Pull()
        {
            Button.ToggleState = (Pawn.MapHeld?.designationManager.DesignationOn(Pawn, DesignationDefOf.Tame) != null);

            // Copied from PawnColumnWorker_ManhunterOnTameFailChance.GetTextFor
            Chance = (int)(Pawn.RaceProps.manhunterOnTameFailChance * 100f);

            if (Chance > 0)
            {
                Button.Text = Chance.ToString();
                ToolTipText = "DesignatorTameDesc".Translate() + Environment.NewLine + "Stat_Race_Animal_TameFailedRevengeChance_Desc".Translate();
            }
            else
            {
                Button.Text = string.Empty;
                // Don't display ToolTip when the Cell is empty.
                ToolTipText = string.Empty;
            }
        }

        public override void Push()
        {
            if (Button.ToggleState)
            {
                // Will complain if we don't check if the Designator exist first before trying to add it.
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.Tame) == null)
                {
                    // Remove Selection from Hunting as these 2 are mutually exclusive.
                    DataViewRow.Cells.OfType<DataViewRowCell_Hunt>().FirstOrDefault()?.Floor();

                    Pawn.MapHeld.designationManager.AddDesignation(new Designation(Pawn, DesignationDefOf.Tame, null));
                    TameUtility.ShowDesignationWarnings(Pawn, true);
                }
            }
            else
            {
                if (Pawn.MapHeld.designationManager.DesignationOn(Pawn, DesignationDefOf.Tame) != null)
                {
                    Pawn.MapHeld.designationManager.TryRemoveDesignationOn(Pawn, DesignationDefOf.Tame);
                }
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            // Selected Cells goto the top.
            return ((base.CompareTo(Other) * 2) + Chance.CompareTo(((DataViewRowCell_Tame)Other).Chance));
        }
    }
}
