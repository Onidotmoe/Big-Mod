using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListView_Traits : ListView_Stats
    {
        public ListView_Traits(Vector2 Size, Vector2 Offset = default, bool ShowScrollbarVertical = false) : base(Size, Offset, ShowScrollbarVertical)
        {
            RenderStyle = ListViewStyle.Grid;
            ExtendItemsHorizontally = false;
        }

        public override void Pull()
        {
            Clear();

            foreach (Trait Trait in Pawn.story.traits.allTraits)
            {
                ListViewItem Item = new ListViewItem(Trait.LabelCap);
                Item.SetStyle(GetType().Name + ".Item");
                Item.Header.SetStyle(GetType().Name + ".Item.Header");
                Item.ToolTipText = Trait.TipString(Pawn);

                Item.Size = Item.Header.Label.GetTextSize();
                Item.Width += 10f;

                AddItem(Item);
            }
        }
    }
}
