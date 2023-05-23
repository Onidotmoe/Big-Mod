using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_FollowDrafted : DataViewRowCell_Toggle
    {
        public Pawn Pawn;

        public DataViewRowCell_FollowDrafted(Pawn Pawn)
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
            // Copied from PawnColumnWorker_FollowDrafted.HasCheckbox
            return (Pawn.RaceProps.Animal && (Pawn.Faction == Faction.OfPlayer) && Pawn.training.HasLearned(TrainableDefOf.Obedience));
        }

        public override void Pull()
        {
            Button.ToggleState = Pawn.playerSettings.followDrafted;

            Button.IsVisible = (Pawn.playerSettings.Master != null);
        }

        public override void Push()
        {
            if (CanToggle())
            {
                Pawn.playerSettings.followDrafted = Button.ToggleState;
            }
        }
    }
}
