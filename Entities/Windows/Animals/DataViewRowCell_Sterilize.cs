using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_Sterilize : DataViewRowCell_Toggle
    {
        public Pawn Pawn;
        public bool IsSterile
        {
            get
            {
                // Copied from PawnColumnWorker_Sterilize.AnimalSterile
                return Pawn.health.hediffSet.HasHediff(HediffDefOf.Sterilized, false);
            }
        }
        public List<Bill> SterilizeOperations
        {
            get
            {
                return (from Bill in Pawn.BillStack.Bills where Bill.recipe == RecipeDefOf.Sterilize select Bill).ToList<Bill>();
            }
        }
        public bool SterilizationInProgress
        {
            get
            {
                return SterilizeOperations.Any<Bill>();
            }
        }

        public DataViewRowCell_Sterilize(Pawn Pawn)
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
            return !IsSterile;
        }

        public override void Pull()
        {
            ToolTipText = (IsSterile ? "AnimalAlreadySterile".Translate() : "SterilizeAnimal".Translate());

            Button.ToggleState = SterilizationInProgress;
        }

        public override void Push()
        {
            bool Active = SterilizationInProgress;

            if (Button.ToggleState && !Active)
            {
                HealthCardUtility.CreateSurgeryBill(Pawn, RecipeDefOf.Sterilize, null, null, true);
            }
            else if (!Button.ToggleState && Active)
            {
                foreach (Bill Bill in SterilizeOperations)
                {
                    Pawn.BillStack.Delete(Bill);
                }
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            // Selected Cells goto the top.
            return ((base.CompareTo(Other) * 2) + IsSterile.CompareTo(((DataViewRowCell_Sterilize)Other).IsSterile));
        }
    }
}
