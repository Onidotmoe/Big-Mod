using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Orders
{
    public class Orders : WindowPanel, IOnRequest
    {
        public int RequestCurrent { get; set; }
        public int RequestRate { get; set; } = 30;

        public event EventHandler OnRequest;
        public Vector2 ItemMinSize = new Vector2(80f, 85f);
        public ListView ListView;
        public List<object> Selected = new List<object>();
        public static Selector Selector = Find.Selector;
        public bool SelectionChanged;

        public Orders()
        {
            Identifier = "Window_Orders";

            DrawBorder = false;

            // Prevents this window from getting closed
            SetCloseOn();

            VisibleOnMouseOverOnly_ButtonCloseX = true;
            VisibleOnMouseOverOnly_ButtonResize = true;

            ListView = new ListView(Width, Height, false, false);
            ListView.SetStyle("Orders.ListView");
            ListView.RenderStyle = ListViewStyle.Grid;
            ListView.InheritParentSize = true;
            ListView.ExtendItemsHorizontally = false;
            ListView.IgnoreGUIScroll = true;
            ListView.ItemMinSize = ItemMinSize;
            ListView.OnMouseWheel += (obj, e) =>
            {
                // Sanity check
                if (ListView.Items.Any())
                {
                    float Position = ListView.ScrollPosition.y;

                    if (e.Delta < 0f)
                    {
                        Position += ItemMinSize.y;
                    }
                    else if (e.Delta > 0f)
                    {
                        Position -= ItemMinSize.y;
                    }

                    // Find the Item that's closest to the current ScrollPosition.
                    float Y = ListView.Items.Min((F) => (Math.Abs(Position - F.Y), F.Y)).Y;
                    // Then jump to that one.
                    ListView.ScrollPosition = new Vector2(ListView.ScrollPosition.x, Y);
                }
            };
            Root.AddChild(ListView);
            ListView.OnItemsChanged += Update_IgnoreGUIScroll;
            Root.OnSizeChanged += Update_IgnoreGUIScroll;

            AddButtonResize();
            Size = new Vector2((((ItemMinSize.x + (ListView.ItemMargin.x * 2f)) * 8f) + ButtonResize.Width + 5f), (ItemMinSize.y + 20f));
            Position = new Vector2(((UI.screenWidth / 2) - (Width / 2)), (UI.screenHeight - Height - (ItemMinSize.x * 0.8f)));
            DefaultBounds = Bounds;

            Register();

            ListView.InheritParentSize_Modifier = new Vector2((((ButtonResize != null) ? -ButtonResize.Width : 0f) - 5f), 0f);

            // All this is to allow MouseWheel to go through when the ListView Items are all visible and to block Passthrough of Inputs while over the Resize and Close buttons.
            Passthrough_EventTypes = new HashSet<EventType> { EventType.ScrollWheel };

            ButtonResize.OnMouseEnter += (obj, e) => { Passthrough_EventTypes.Add(EventType.MouseDown); Passthrough_Active = false; };
            ButtonResize.OnWhileMouseOver += (obj, e) => Passthrough_Active = true;
            ButtonResize.OnMouseLeave += (obj, e) => { Passthrough_EventTypes.Remove(EventType.MouseDown); Passthrough_Active = false; };

            Override_InnerWindowOnGuiCached();

            IsLocked = true;
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

            if (IsVisible)
            {
                if (RequestCurrent >= RequestRate)
                {
                    RequestCurrent = 0;
                    DoOnRequest();
                }
                else
                {
                    RequestCurrent++;
                }
            }

            base.Update();
        }

        public void DoOnRequest()
        {
            // Some mods like "Simple Sidearms" change the gizmo when the pawn pickups something, because of this we need to update and validate gizmos outside the Selection process.
            SelectionChanged = true;
        }

        public void OnSelectionChanged()
        {
            // TODO: this method could be more efficient
            ListView.Clear();

            List<Gizmo> Gizmos = new List<Gizmo>();

            Gizmos.AddRange(Selected.OfType<ISelectable>().SelectMany((F) => F.GetGizmos()));

            // Pawns are also Things.
            // Modified from InspectGizmoGrid.DrawInspectGizmoGridFor
            foreach (Thing Thing in Selected.OfType<Thing>())
            {
                foreach (Designator Designator in Find.ReverseDesignatorDatabase.AllDesignators)
                {
                    Command_Action Command = Designator.CreateReverseDesignationGizmo(Thing);

                    if (Command != null)
                    {
                        Action OldAction = Command.action;

                        Command.action = () =>
                        {
                            OldAction();
                            SelectionChanged = true;
                        };

                        Gizmos.Add(Command);
                    }

                    // TODO: replace vanilla gizmo buttons with this instead
                    //AcceptanceReport AcceptanceReport = Designator.CanDesignateThing(Thing);

                    //if (AcceptanceReport.Accepted || ((Designator is Designator_Deconstruct) && !AcceptanceReport.Reason.NullOrEmpty()))
                    //{
                    //    //Command_Action Command_Action = new Command_Action();
                    //    //Command_Action.defaultLabel = Designator.LabelCapReverseDesignating(Thing);
                    //    //Command_Action.icon = Designator.IconReverseDesignating(Thing, out float iconAngle, out Vector2 iconOffset);
                    //    //Command_Action.iconAngle = iconAngle;
                    //    //Command_Action.iconOffset = iconOffset;
                    //    //Command_Action.defaultDesc = (AcceptanceReport.Reason ?? Designator.DescReverseDesignating(Thing));
                    //    //Command_Action.Order = ((Designator is Designator_Uninstall) ? -11f : -20f);
                    //    //Command_Action.disabled = !AcceptanceReport.Accepted;
                    //    //Command_Action.hotKey = Designator.hotKey;
                    //    //Command_Action.groupKey = Designator.groupKey;
                    //    //Command_Action.groupKeyIgnoreContent = Designator.groupKeyIgnoreContent;

                    //    //Command_Action.action = () =>
                    //    //{
                    //    //    Designator.DesignateThing(Thing);
                    //    //    Designator.Finalize(true);
                    //    //    SelectionChanged = true;
                    //    //};

                    //    //Gizmos.Add(Command_Action);

                    //}
                }
            }

            Gizmos.SortStable(SortByOrder);

            List<ListViewItem_Order> Existing_Orders = ListView.Items.OfType<ListViewItem_Order>().ToList();

            foreach (Gizmo Gizmo in Gizmos)
            {
                if (!Existing_Orders.Any((F) => F.TryMerge(Gizmo)))
                {
                    ListViewItem_Order Item = new ListViewItem_Order(this, Gizmo);
                    ListView.AddItem(Item);
                    Existing_Orders.Add(Item);
                }
            }

            IsVisible = ListView.Items.Any();
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

        public void Update_IgnoreGUIScroll(object Sender, EventArgs EventArgs)
        {
            // Don't allow the input to go through the window, if there are items to scroll through.
            Passthrough_Active = ((ListView.Items.LastOrDefault()?.Bottom ?? 0f) > Height);
        }

        /// <summary>
        /// Copied from GizmoGridDrawer.SortByOrder
        /// </summary>
        public static readonly Func<Gizmo, Gizmo, int> SortByOrder = (Gizmo A, Gizmo B) => A.Order.CompareTo(B.Order);
    }
}
