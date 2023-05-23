using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Inspect
{
    public class ListViewItem_Inspect_Placeholder : ListViewItem_Inspect
    {
        public override Vector2 MinSize { get; set; } = new Vector2(0f, 30f);
        public List<Thing> Things;

        public ListViewItem_Inspect_Placeholder(List<Thing> Things)
        {
            this.Things = Things;

            IgnoreMouse = true;

            Pull();
        }

        public override bool Validate()
        {
            // Go backwards and remove all non-selected items.
            for (int i = (Things.Count - 1); i >= 0; i--)
            {
                if (!Find.Selector.SelectedObjects.Contains(Things[i]))
                {
                    Things.RemoveAt(i);
                }
            }

            if (!Things.Any())
            {
                RemoveFromItemParent();
                return false;
            }

            return true;
        }

        public override void Pull()
        {
            Text = $"{"Various".Translate(Things.Count)} {"Variety".Translate(Things.GroupBy((F) => F.def).Count())}";
        }

        public virtual bool TryMerge(List<Thing> Things)
        {
            this.Things.AddRange(Things);

            return true;
        }

        public override bool Filter(string Search)
        {
            return Things.Any((F) => F.Label.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
    }
}
