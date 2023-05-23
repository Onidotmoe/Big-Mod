using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows
{
    // TODO:
    // Needs to implement settings saving across savegames
    // https://spdskatr.github.io/RWModdingResources/saving-guide.html
    // Has to allow pets to be displayed
    public class Pawns : ListWindow, IOnRequest, IExposable
    {
        public Settings Settings;

        /// <summary>
        /// Current position in request throttling.
        /// </summary>
        public int RequestCurrent { get; set; }
        /// <summary>
        /// How many update cycles between updating the ListView.
        /// </summary>
        public virtual int RequestRate { get; set; } = 240;
        /// <summary>
        /// Called by <see cref="DoOnRequest"/> when <see cref="RequestCurrent"/> is at or suppasses <see cref="RequestCurrent"/>.
        /// </summary>
        public event EventHandler OnRequest;

        public override Rect DefaultBounds { get; set; } = new Rect(5, 60, 290, 170);

        private void Init()
        {
            Identifier = "Window_Pawns";

            ButtonCloseX.OnClick += (obj, e) =>
            {
                List<Pawns> PawnsWindows = WindowManager.GetWindows<Pawns>();

                // The first Window updated will absorb this Pawn item, bias towards ListViewStyle.List windows.
                Pawns PawnsWindow = PawnsWindows.FirstOrFallback((F) => (F.ListView.RenderStyle == ListViewStyle.List), PawnsWindows.FirstOrDefault());

                PawnsWindow?.DoOnRequest();
            };

            ListView.AllowSelection = true;
            ListView.IsMultiSelect = true;

            ListView.OnItemsChanged += (obj, e) =>
            {
                if (!ListView.Items.Any())
                {
                    Close();
                }
            };

            ListView.Filter = (Item) =>
            {
                if (Item is ListViewItem_Pawn Pawn)
                {
                    // Try to only use visible text.
                    string Query = (Pawn.Nickname.IsVisible ? Pawn.Nickname.Text : (Pawn.Name.IsVisible ? Pawn.Name.Text : (Pawn.Title.IsVisible ? Pawn.Title.Text : string.Empty)));

                    return (Query.IndexOf(Search.Text, StringComparison.InvariantCultureIgnoreCase) >= 0);
                }

                return (Item.Text?.IndexOf(Search.Text, StringComparison.InvariantCultureIgnoreCase) >= 0);
            };

            Search.TextInput.OnTypingFinished += (obj, e) =>
            {
                // Have to update Item Bounds as UnitStyle might have been changed meanwhile.
                ListView.UpdateViewPortBounds();
                ListView.UpdatePositions();
            };

            if (Settings == null)
            {
                Settings = new Settings();
                Settings.Bounds = Bounds;
            }
        }

        public Pawns() : base(Should_AddSimpleTextFilter: false)
        {
            Init();
            // Retrieve data immediately
            DoOnRequest();
        }

        public Pawns(ListViewItem_Pawn Item) : base(Should_AddSimpleTextFilter: false)
        {
            Init();

            bool Square = (Item.RenderStyle == UnitStyle.Square);

            ListView.MinSize = (Square ? Item.SquareSize : ListView.MinSize);
            ListView.RenderStyle = (Square ? ListViewStyle.Grid : ListViewStyle.List);
            ListView.ExtendItemsHorizontally = !Square;

            List<ListViewItem> Items = ListView.GetItemsReclusively();

            Items.OfType<ListViewItem_Pawn>().ToList().ForEach((F) => F.RenderStyle = Item.RenderStyle);
            Items.OfType<ListViewItemGroup>().ToList().ForEach((F) =>
            {
                F.RenderStyle = ListView.RenderStyle;
                F.ExtendItemsHorizontally = ListView.ExtendItemsHorizontally;
                F.DoOnSizeChanged(F, EventArgs.Empty);
            });

            ListView.UpdateViewPortBounds();
            ListView.UpdatePositions();

            if (WindowManager.IsAltDown())
            {
                AddItem(Item);
            }
            else
            {
                ListView.AddItem(Item);
            }

            if (Item.IsSelected)
            {
                // Popouts might have been Selected.
                ListView.SelectItem(Item);
            }
        }

        public override void Update()
        {
            base.Update();

            if (RequestCurrent >= RequestRate)
            {
                RequestCurrent = 0;

                if (IsVisible)
                {
                    DoOnRequest();
                }
            }
            else
            {
                RequestCurrent++;
            }
        }

        public IEnumerable<Pawn> AllPawns
        {
            get
            {
                return from Pawn in PawnsFinder.AllMaps_Spawned where ((!Pawn.RaceProps.Animal && !Pawn.RaceProps.Insect && !Pawn.IsColonyMechPlayerControlled) || Pawn.HostileTo(Faction.OfPlayer)) && !Pawn.Position.Fogged(Pawn.Map) select Pawn;
            }
        }

        /// <summary>
        /// Handles data retrieving.
        /// </summary>
        public virtual void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            IEnumerable<Pawn> Alive = AllPawns;

            // Get all Pawns not in this Window
            List<Pawn> OuterPawns = WindowManager.GetWindows<Pawns>().Except(this).SelectMany((F) => F.ListView.GetItemsReclusively()).OfType<ListViewItem_Pawn>().Select((F) => F.Pawn).ToList();

            Alive = Alive.Except(OuterPawns).ToList();

            ToolTip ToolTip = WindowManager.Instance.GetToolTip("PopOut_Preview");

            // Don't Handle any Items currently being dragged around inside a ToolTip Window.
            if (ToolTip != null)
            {
                List<Pawn> ToolTip_Pawns = ToolTip.Root.GetChildren<ListView>().SelectMany((F) => F.GetItemsReclusively()).OfType<ListViewItem_Pawn>().Select((F) => F.Pawn).ToList();
                Alive = Alive.Except(ToolTip_Pawns).ToList();
            }

            List<ListViewItem_Pawn> Items = ListView.GetItemsReclusively().OfType<ListViewItem_Pawn>().ToList();

            foreach (Pawn Pawn in Alive)
            {
                if (!Items.Exists((F) => (F.Pawn == Pawn)))
                {
                    ListViewItem_Pawn Item = new ListViewItem_Pawn(Pawn);
                    Item.RenderStyle = ((ListView.RenderStyle == ListViewStyle.Grid) ? UnitStyle.Square : UnitStyle.Line);
                    Item.Pawns = this;
                    AddItem(Item);
                }
            }

            for (int i = (Items.Count - 1); i >= 0; i--)
            {
                ListViewItem_Pawn Item = Items[i];

                if (Alive.Contains(Item.Pawn))
                {
                    Item.DoOnRequest();
                }
                else
                {
                    // Pawn isn't alive, remove it from the list
                    Item.RemoveFromItemParent();
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<Settings>(ref this.Settings, nameof(Windows.Settings), default(Settings), false);
        }

        public enum PawnGroup
        {
            None,
            Quest,
            Slaves,
            Colonists,
            Hostiles,
            Neutrals,
            Traders,
            Prisoners,
            Others
        }

        public virtual void AddItem(ListViewItem_Pawn Item)
        {
            PawnGroup ID = GetGroupID(Item.Pawn);

            ListViewItemGroup Group = GetGroup(ID);

            if (Group == null)
            {
                Group = CreateGroup(ID);

                if (PawnGroup.Colonists == ID)
                {
                    Group.Text = Item.Pawn.Faction.Name.CapitalizeFirst();
                    Group.Header.Image.SetTexture(Item.Pawn.Faction.def.factionIconPath);
                    Group.Header.Image.Style.Color = Item.Pawn.Faction.Color;
                    Group.Header.Label.Style.TextColor = Item.Pawn.Faction.Color;
                }

                ListView.AddItem(Group);
            }

            Group.AddItem(Item);
        }

        public virtual PawnGroup GetGroupID(Pawn Pawn)
        {
            PawnGroup Group = PawnGroup.None;

            if (Pawn.Faction == Faction.OfPlayer)
            {
                if (Pawn.IsSlave)
                {
                    Group = PawnGroup.Slaves;
                }
                else
                {
                    Group = PawnGroup.Colonists;
                }
            }
            else if (QuestUtility.IsReservedByQuestOrQuestBeingGenerated(Pawn))
            {
                Group = PawnGroup.Quest;
            }
            else if (Pawn.IsPrisonerOfColony)
            {
                Group = PawnGroup.Prisoners;
            }
            else if (Pawn.Faction != null)
            {
                switch (Pawn.Faction.PlayerRelationKind)
                {
                    case FactionRelationKind.Hostile:
                        Group = PawnGroup.Hostiles;
                        break;

                    case FactionRelationKind.Neutral:
                        Group = PawnGroup.Neutrals;
                        break;

                    default:
                        Group = PawnGroup.Others;
                        break;
                }

                if ((Pawn.Faction.PlayerRelationKind != FactionRelationKind.Hostile) && (Pawn.trader != null))
                {
                    Group = PawnGroup.Traders;
                }
            }

            return Group;
        }

        public virtual ListViewItemGroup GetGroup(PawnGroup ID)
        {
            return ListView.GetItemsReclusively().OfType<ListViewItemGroup>().FirstOrDefault((F) => F.ID == ID.ToString());
        }

        public virtual ListViewItemGroup CreateGroup(PawnGroup ID)
        {
            string StringID = ID.ToString();

            ListViewItemGroup Group = new ListViewItemGroup(StringID.Translate());
            Group.SetStyle(GetType().Name + ".ListViewItemGroup");
            Group.Header.SetStyle(GetType().Name + ".ListViewItemGroup.Header");
            Group.Header.Style.TextColor = Globals.GetColor(GetType().Name + "." + StringID);
            Group.Header.Style.DrawBackground = false;
            Group.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Group.Header.Label.OffsetX = (Group.MinSize.y + 2f);
            Group.Header.CanToggle = false;
            Group.Style.DrawBackground = false;
            Group.Style.DrawMouseOver = false;
            Group.ID = StringID;
            Group.IsExpanded = true;
            Group.InheritParentSizeWidth = true;
            Group.InheritParentSize_Modifier = new Vector2(-ListView.ItemMargin.x - ListView.ScrollbarSize, 0f);

            Group.Header.Image = new Image(Globals.TryGetTexturePathFromAlias(StringID));
            Group.Header.Image.Size = new Vector2(Group.MinSize.y, Group.MinSize.y);
            Group.Header.Image.Style.Color = Color.white;
            Group.Header.AddChild(Group.Header.Image);

            Group.OnItemsChanged += (obj, e) =>
            {
                if (!Group.Items.Any())
                {
                    // Remove when there's no more children in it.
                    Group.RemoveFromItemParent();
                }
            };

            return Group;
        }
    }

    public class Settings
    {
        public Rect Bounds;
        public bool Visible_Nickname = true;
        public bool Visible_Title;
        public bool Visible_Name;
        public bool Visible_Weapon_Pacifist = true;
        public bool Portrait_Headshot;
    }
}
