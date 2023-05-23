using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListView_Thoughts : ListView_Stats
    {
        public ListView_Thoughts(Vector2 Size, Vector2 Offset = default, bool ShowScrollbarVertical = false) : base(Size, Offset, ShowScrollbarVertical)
        {
        }

        public override void Pull()
        {
            // Populate and Pull are merged here as this list will charge overtime, where as the others are static.

            List<Thought> Groups = new List<Thought>();
            PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder(Pawn.needs.mood, Groups);

            List<ListViewItem_Thoughts> Current_Items = Items.OfType<ListViewItem_Thoughts>().ToList();
            IEnumerable<Thought> Current_Thoughts = Current_Items.Select((F) => F.Thought);

            // Validate Existing Items
            foreach (ListViewItem_Thoughts Item in Current_Items)
            {
                if (!Groups.Contains(Item.Thought))
                {
                    Item.RemoveFromItemParent();
                }
            }

            // Add in New Items
            foreach (Thought Thought in Groups)
            {
                if (!Current_Thoughts.Contains(Thought))
                {
                    AddItem(new ListViewItem_Thoughts(Pawn, Thought));
                }
            }

            // Update all Items
            foreach (ListViewItem_Thoughts Item in Items.OfType<ListViewItem_Thoughts>())
            {
                Item.Pull();
            }
        }
    }
}
