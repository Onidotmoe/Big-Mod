using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListView_Skills : ListView_Stats
    {
        public ListView_Skills(Vector2 Size, Vector2 Offset = default, bool ShowScrollbarVertical = false) : base(Size, Offset, ShowScrollbarVertical)
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

            IEnumerable<SkillDef> SkillDefs = (from Def in DefDatabase<SkillDef>.AllDefs orderby Def.listOrder descending select Def);

            foreach (SkillDef Def in SkillDefs)
            {
                AddItem(new ListViewItem_Skills(Pawn, Def));
            }
        }

        public override void Pull()
        {
            foreach (ListViewItem_Skills Item in Items.OfType<ListViewItem_Skills>())
            {
                Item.Pull();
            }
        }
    }
}
