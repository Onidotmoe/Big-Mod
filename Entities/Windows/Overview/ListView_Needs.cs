using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListView_Needs : ListView_Stats
    {
        /// <summary>
        /// If the Mood Need should be included in <see cref="Pull"/>.
        /// </summary>
        public bool IncludeMood = true;

        public ListView_Needs(Vector2 Size, Vector2 Offset = default, bool ShowScrollbarVertical = false) : base(Size, Offset, ShowScrollbarVertical)
        {
        }

        public override void SetPawn(Pawn Pawn)
        {
            base.SetPawn(Pawn);

            // Can't populate without Pawn being set first.
            Populate();
        }

        public void Populate()
        {
            Clear();

            List<Need> AllNeeds = Pawn.needs.AllNeeds.Where((F) => F.ShowOnNeedList).ToList();

            if (IncludeMood)
            {
                AllNeeds.Add(Pawn.needs.mood);

                // This is how the game does it in Need_Mood.DrawOnGUI
                List<float> threshPercents = new List<float>();
                threshPercents.Add(Pawn.mindState.mentalBreaker.BreakThresholdExtreme);
                threshPercents.Add(Pawn.mindState.mentalBreaker.BreakThresholdMajor);
                threshPercents.Add(Pawn.mindState.mentalBreaker.BreakThresholdMinor);

                Globals.Set_Need_threshPercents(Pawn.needs.mood, threshPercents);
            }

            // Also has threshPercents defined inside its DrawOnGUI
            Need_Food Need_Food = AllNeeds.OfType<Need_Food>().FirstOrDefault();

            if (Need_Food != null)
            {
                List<float> threshPercents = new List<float>();

                threshPercents.Add(Need_Food.PercentageThreshHungry);
                threshPercents.Add(Need_Food.PercentageThreshUrgentlyHungry);

                Globals.Set_Need_threshPercents(Need_Food, threshPercents);
            }

            PawnNeedsUIUtility.SortInDisplayOrder(AllNeeds);

            foreach (Need Need in AllNeeds)
            {
                AddItem(new ListViewItem_Needs(Width, Need));
            }
        }

        public override void Pull()
        {
            foreach (ListViewItem_Needs Item in Items.OfType<ListViewItem_Needs>())
            {
                Item.Pull();
            }
        }
    }
}
