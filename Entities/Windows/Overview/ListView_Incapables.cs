using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListView_Incapables : ListView_Stats
    {
        public ListView_Incapables(Vector2 Size, Vector2 Offset = default, bool ShowScrollbarVertical = false) : base(Size, Offset, ShowScrollbarVertical)
        {
            RenderStyle = ListViewStyle.Grid;
            ExtendItemsHorizontally = false;
        }

        public override void Pull()
        {
            Clear();

            WorkTags DisabledTags = Pawn.CombinedDisabledWorkTags;
            List<WorkTags> DisabledTagsList = DisabledTags.GetAllSelectedItems<WorkTags>().Where((F) => (F != WorkTags.None)).ToList();

            // Modified from CharacterCardUtility.DrawCharacterCard
            DisabledTagsList.Sort(delegate (WorkTags a, WorkTags b)
            {
                int A = Globals.CharacterCardUtility_GetWorkTypeDisableCauses(Pawn, a).Any((object c) => c is RoyalTitleDef) ? 1 : -1;
                int B = Globals.CharacterCardUtility_GetWorkTypeDisableCauses(Pawn, b).Any((object c) => c is RoyalTitleDef) ? 1 : -1;
                return A.CompareTo(B);
            });

            foreach (WorkTags Tag in DisabledTagsList)
            {
                ListViewItem Item = new ListViewItem(Tag.LabelTranslated().CapitalizeFirst());
                Item.SetStyle(GetType().Name + ".Item");
                Item.Header.SetStyle(GetType().Name + ".Item.Header");
                Item.Header.Label.Style.TextColor = Globals.CharacterCardUtility_GetDisabledWorkTagLabelColor(Pawn, Tag);

                Item.ToolTipText = Globals.CharacterCardUtility_GetWorkTypeDisabledCausedBy(Pawn, Tag) + "\n" + Globals.CharacterCardUtility_GetWorkTypesDisabledByWorkTag(Tag);

                Item.Size = Item.Header.Label.GetTextSize();
                Item.Width += 10f;

                AddItem(Item);
            }
        }
    }
}
