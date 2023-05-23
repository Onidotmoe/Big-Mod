using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Inspect
{
    /// <summary>
    /// A Info Card to display in the <see cref="Inspect"/> Window.
    /// </summary>
    public class ListViewItem_Inspect : ListViewItem, IPull
    {
        public Thing Target;
        public List<Inspect_TabButton> Tabs;
        /// <summary>
        /// Is <see cref="Target"/> currently selected by the Player?
        /// </summary>
        public bool IsTarget
        {
            get
            {
                return Find.Selector.SelectedObjects.Contains(Target);
            }
        }

        public ListViewItem_Inspect()
        {
            Selectable = false;
        }

        public virtual void Pull()
        {
        }
        public virtual bool TryMerge(Thing Other)
        {
            return false;
        }

        public virtual bool Filter(string Search)
        {
            return true;
        }
        /// <summary>
        /// Removes this Item from it's Parent's ListView, if it's not currently Selected.
        /// </summary>
        /// <returns>True if is still Valid.</returns>
        public virtual bool Validate()
        {
            if (!IsTarget)
            {
                RemoveFromItemParent();
                return false;
            }

            return true;
        }

        public virtual void AddTabs(Thing Target)
        {
            this.Target = Target;

            IEnumerable<InspectTabBase> TabBases = Target.GetInspectTabs();

            // Not all Things have Tabs
            if (TabBases != null)
            {
                Tabs = new List<Inspect_TabButton>();

                foreach (InspectTabBase Tab in TabBases)
                {
                    if (Tab.IsVisible && !Tab.Hidden)
                    {
                        Inspect_TabButton Button = new Inspect_TabButton(Tab);
                        Button.Offset = new Vector2((Tabs.LastOrDefault()?.OffsetRight + 2f ?? 2f), 2f);
                        Button.Target = Target;
                        Tabs.Add(Button);
                        AddChild(Button);
                    }
                }
            }
        }
    }
}
