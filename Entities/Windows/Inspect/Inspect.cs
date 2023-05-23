using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Inspect
{
    public class Inspect : ListWindow
    {
        public override Rect DefaultBounds { get; set; } = new Rect(10f, (UI.screenHeight - 355f), 470f, 348f);
        public List<object> Selected = new List<object>();
        public List<Pawn> SelectedPawns = new List<Pawn>();
        public List<Thing> SelectedThings = new List<Thing>();
        public static Selector Selector = Find.Selector;
        private bool SelectionChanged;
        /// <summary>
        /// Used by Vanilla <see cref="ITab"/> to fill information.
        /// </summary>
        public static Thing TempTarget;
        /// <summary>
        /// How many Inspect Items will be created before combining everything else into a single "other" Items Item.
        /// </summary>
        /// <remarks>Set to -1 to disable.</remarks>
        public int Limit = 4;

        public Inspect() : base(false, true, false, true)
        {
            Identifier = "Window_Inspect";

            SetCloseOn();

            Root.MaxSize = new Vector2(Width, 0f);

            ListView.ItemMargin = new Vector2(0f, ListView.ItemMargin.y);
            ListView.IgnoreGUIScroll = true;

            ListView.Filter = (Item) =>
            {
                if (Item is ListViewItem_Inspect InspectItem)
                {
                    return InspectItem.Filter(Search.Text);
                }

                return (Item.Text?.IndexOf(Search.Text, StringComparison.InvariantCultureIgnoreCase) >= 0);
            };

            ListView.OnMouseWheel += (obj, e) =>
            {
                // Sanity check
                if (ListView.Items.Any())
                {
                    float Position = ListView.ScrollPosition.y;

                    if (e.Delta < 0f)
                    {
                        Position += DefaultBounds.height;
                    }
                    else if (e.Delta > 0f)
                    {
                        Position -= DefaultBounds.height;
                    }

                    // Find the Item that's closest to the current ScrollPosition.
                    float Y = ListView.Items.Min((F) => (Math.Abs(Position - F.Y), F.Y)).Y;
                    // Then jump to that one.
                    ListView.ScrollPosition = new Vector2(ListView.ScrollPosition.x, Y);
                }
            };

            AddButtonResize();

            Register();

            IsLocked = true;
            AddLockingToolTipIcon();

            Passthrough_EventTypes = new HashSet<EventType>() { EventType.MouseDown, EventType.ScrollWheel };

            Override_InnerWindowOnGuiCached();
        }

        public override void PreOpen()
        {
            base.PreOpen();

            // Starts invisible
            IsVisible = false;
        }

        public override void Update()
        {
            if (SelectionChanged)
            {
                SelectionChanged = false;
                OnSelectionChanged();
            }

            base.Update();
        }

        public void OnSelectionChanged()
        {
            SelectedPawns = Selected.OfType<Pawn>().ToList();
            // Pawns also appear as Things
            SelectedThings = Selected.Except(SelectedPawns).OfType<Thing>().ToList();

            IEnumerable<ListViewItem_Inspect_Pawn> Existing_Pawns = ListView.Items.OfType<ListViewItem_Inspect_Pawn>().ToList().Where((F) => F.Validate());
            List<ListViewItem_Inspect_Thing> Existing_Things = ListView.Items.OfType<ListViewItem_Inspect_Thing>().ToList().Where((F) => F.Validate()).ToList();
            ListViewItem_Inspect_Placeholder DeferItem = ListView.Items.OfType<ListViewItem_Inspect_Placeholder>().FirstOrDefault();
            DeferItem?.Validate();

            List<Thing> Defer = new List<Thing>();

            foreach (Pawn Pawn in SelectedPawns.Except(Existing_Pawns.Select((F) => F.Pawn)))
            {
                if ((Limit != -1) && (ListView.Items.Count >= Limit))
                {
                    Defer.Add(Pawn);
                }
                else
                {
                    AddItem(new ListViewItem_Inspect_Pawn(Pawn));
                }
            }

            foreach (Thing Thing in SelectedThings)
            {
                if (!Existing_Things.Any((F) => F.TryMerge(Thing)))
                {
                    if ((Limit != -1) && (ListView.Items.Count >= Limit))
                    {
                        Defer.Add(Thing);
                    }
                    else
                    {
                        ListViewItem_Inspect_Thing Item = new ListViewItem_Inspect_Thing(Thing);
                        AddItem(Item);
                        Existing_Things.Add(Item);
                    }
                }
            }

            if (Defer.Any() && (DeferItem?.TryMerge(Defer) != true))
            {
                // Defer always goes to the bottom.
                ListView.AddItem(new ListViewItem_Inspect_Placeholder(Defer));
            }

            IsVisible = ListView.Items.Any();
            Passthrough_Active = IsVisible;

            ListView.Items.OfType<ListViewItem_Inspect>().ToList().ForEach((F) => F.Pull());
        }

        public void AddItem(ListViewItem Item)
        {
            // Add items to the top and have the ListView grow upwards.
            ListView.InsertItem(0, Item);
            ListView.ScrollPosition = new Vector2(ListView.ScrollPosition.x, (ListView.ScrollPosition.y + Item.Height + ListView.ItemMargin.y));
        }

        public void Register()
        {
            BigMod.Selector.Event_Select += Select;
            BigMod.Selector.Event_Deselect += Deselect;
            BigMod.Selector.Event_ClearSelection += ClearSelection;
        }

        public void UnRegister()
        {
            BigMod.Selector.Event_Select -= Select;
            BigMod.Selector.Event_Deselect -= Deselect;
            BigMod.Selector.Event_ClearSelection -= ClearSelection;
        }

        public void Select(object Sender, EventArgs EventArgs)
        {
            // Check if it passed through Selector.SelectInternal successfully
            if (Selector.IsSelected(Sender))
            {
                Selected.Add(Sender);
                SelectionChanged = true;
            }
        }

        public void Deselect(object Sender, EventArgs EventArgs)
        {
            // Check if it was removed by Selector.DeselectInternal successfully
            if (!Selector.IsSelected(Sender))
            {
                Selected.Remove(Sender);

                SelectionChanged = true;
            }
        }

        public void ClearSelection(object Sender, EventArgs EventArgs)
        {
            Selected.Clear();

            SelectionChanged = true;
        }
    }
}
