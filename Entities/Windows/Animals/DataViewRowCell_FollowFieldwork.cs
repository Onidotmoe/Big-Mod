using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_FollowFieldwork : DataViewRowCell_Toggle
    {
        public Pawn Pawn;

        public DataViewRowCell_FollowFieldwork(Pawn Pawn)
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
            // Copied from PawnColumnWorker_FollowFieldwork.HasCheckbox
            return (Pawn.RaceProps.Animal && (Pawn.Faction == Faction.OfPlayer) && Pawn.training.HasLearned(TrainableDefOf.Obedience));
        }

        public override void Pull()
        {
            Button.ToggleState = Pawn.playerSettings.followFieldwork;

            Button.IsVisible = (Pawn.playerSettings.Master != null);
        }

        public override void Push()
        {
            if (CanToggle())
            {
                Pawn.playerSettings.followFieldwork = Button.ToggleState;
            }
        }
    }
}
