using BigMod.Entities.Interface;
using BigMod.Entities.Windows.Overview.Subs;
using RimWorld;
using System.Text;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public enum DisplayStyle
    {
        /// <summary>
        /// Will arrange and draw stuff as if it's going to a <see cref="Overview"/> window. Will include <see cref="OverviewPawn.Inventory"/> and <see cref="OverviewPawn.Storage"/>.
        /// </summary>
        Window,
        /// <summary>
        /// Will arrange and draw stuff as if it's going to a <see cref="Inspect.ListViewItem_Inspect"/> ListViewItem. Will not include <see cref="OverviewPawn.Inventory"/> and <see cref="OverviewPawn.Storage"/>.
        /// </summary>
        Inspect
    }

    public class OverviewPawn : Panel, IOnRequest, IPawn
    {
        public event EventHandler OnRequest;
        private DisplayStyle _RenderStyle = DisplayStyle.Window;
        public DisplayStyle RenderStyle
        {
            get
            {
                return _RenderStyle;
            }
            set
            {
                if (_RenderStyle != value)
                {
                    _RenderStyle = value;

                    switch (value)
                    {
                        case DisplayStyle.Window:
                            DoStyleWindow();
                            break;

                        case DisplayStyle.Inspect:
                            DoStyleInspect();
                            break;
                    }
                }
            }
        }
        public Pawn Pawn { get; set; }
        public int RequestCurrent { get; set; }
        public int RequestRate { get; set; } = 400;
        public ListViewItem_Pawn Portrait;
        /// <summary>
        /// All non-equipped items on the Pawn.
        /// </summary>
        public InventoryPanel Inventory;
        /// <summary>
        /// All equippable items.
        /// </summary>
        public InventoryPanel Storage;

        public Outfits Current_Outfits = new Outfits(100f, 25f) {Anchor = Anchor.BottomLeft};
        public Foods Current_Foods = new Foods(100f, 25f) {Anchor = Anchor.BottomLeft};
        public Drugs Current_Drugs = new Drugs(100f, 25f) {Anchor = Anchor.BottomLeft};

        public Button Edit_Current_Outfits = new Button(ButtonStyle.Image, new Vector2(25f, 25f), Globals.TryGetTexturePathFromAlias("Edit")) {ToolTipText = "AssignTabEdit".Translate(), Anchor = Anchor.BottomLeft};
        public Button Edit_Current_Foods = new Button(ButtonStyle.Image, new Vector2(25f, 25f), Globals.TryGetTexturePathFromAlias("Edit")) {ToolTipText = "AssignTabEdit".Translate(), Anchor = Anchor.BottomLeft};
        public Button Edit_Current_Drugs = new Button(ButtonStyle.Image, new Vector2(25f, 25f), Globals.TryGetTexturePathFromAlias("Edit")) {ToolTipText = "AssignTabEdit".Translate(), Anchor = Anchor.BottomLeft};
        public Button Edit_Default_Medicine = new Button(ButtonStyle.Image, new Vector2(25f, 25f), Globals.TryGetTexturePathFromAlias("Edit")) {ToolTipText = "DefaultMedicineSettings".Translate(), Anchor = Anchor.BottomLeft};

        public Button Open_Outfits = new Button(ButtonStyle.Image, new Vector2(25f, 25f), Globals.TryGetTexturePathFromAlias("Outfits")) {ToolTipText = "ManageOutfits".Translate(), Anchor = Anchor.BottomLeft};
        public Button Open_Foods = new Button(ButtonStyle.Image, new Vector2(25f, 25f), Globals.TryGetTexturePathFromAlias("Foods")) {ToolTipText = "ManageFoodRestrictions".Translate(), Anchor = Anchor.BottomLeft};
        public Button Open_Drugs = new Button(ButtonStyle.Image, new Vector2(25f, 25f), Globals.TryGetTexturePathFromAlias("Drugs")) {ToolTipText = "ManageDrugPolicies".Translate(), Anchor = Anchor.BottomLeft};

        public HostilityResponse HostilityResponse = new HostilityResponse(25f, 25f) {Anchor = Anchor.BottomLeft};

        public SelfTend SelfTend = new SelfTend(25f, 25f) {Anchor = Anchor.BottomLeft};
        public MedicineTreatmentQuality Medicine_TreatmentQuality = new MedicineTreatmentQuality(25f, 25f) {Anchor = Anchor.BottomLeft};
        public MedicineCarry Medicine_Carry = new MedicineCarry(50f, 25f) {Anchor = Anchor.BottomLeft};
        public MedicineCarryType Medicine_CarryType = new MedicineCarryType(25f, 25f) {Anchor = Anchor.BottomLeft};

        public ListView_Needs Needs;
        public ListView_Thoughts Thoughts;
        public ListView_Skills Skills;
        public ListView_Incapables Incapables;
        public ListView_Traits Traits;
        public Equipment Equipment;

        public Label Header_Nickname = new Label(true, true){ IgnoreMouse = false, ToolTipText = "Nickname".Translate()};
        public Label Header_Name = new Label(true, true){ IgnoreMouse = false, ToolTipText = "Name".Translate()};
        public Label Header_Title = new Label(true, true){ IgnoreMouse = false, ToolTipText = "Title".Translate()};
        public Label Header_Miscellaneous = new Label(true, true) { IgnoreMouse = false };
        public Label Header_Childhood = new Label(true, true) { IgnoreMouse = false };
        public Label Header_Adulthood = new Label(true, true) { IgnoreMouse = false };
        public Label Header_Jobs = new Label() { IgnoreMouse = false, RenderStyle = LabelStyle.Scrollable };
        // These don't auto resize as that would cause problems in Inspect Mode.
        public Label Header_Incapables = new Label("Incapables".Translate(), new Vector2(100f, 20f));
        public Label Header_Traits = new Label("Traits".Translate(), new Vector2(100f, 20f));
        public Xenotype Xenotype = new Xenotype(25f, 25f);
        public Subs.Faction Faction = new Subs.Faction(140f, 25f);
        public Religion Religion = new Religion(140f, 25f);
        // Letting it take up the space beneath FavoriteColor
        public InventorySummary InventorySummary = new InventorySummary(140f + 25f + 2f, 25f);
        public Image FavoriteColor = new Image(ImageStyle.Color, 25f, 25f) { IgnoreMouse = false };
        private List<IPawn> IPawns = new List<IPawn>();
        private List<IPull> IPulls = new List<IPull>();

        public Color Jobs_Current = Globals.GetColor("OverviewPawn.Header_Jobs.Current.Color");
        public Color Jobs_Queued = Globals.GetColor("OverviewPawn.Header_Jobs.Queued.Color");

        // TODO: Animals need bonded parent and Mechs need overseers pawnitems
        public OverviewPawn(float Width, float Height, DisplayStyle RenderStyle = DisplayStyle.Window)
        {
            Size = new Vector2(Width, Height);

            ID = "OverviewPawn";

            Header_Nickname.Style.FontType = GameFont.Medium;
            Header_Name.Style.FontType = GameFont.Tiny;
            Header_Title.Style.FontType = GameFont.Tiny;
            Header_Miscellaneous.Style.FontType = GameFont.Tiny;

            Header_Nickname.Style.TextColor = Color.white;
            Header_Name.Style.TextColor = Globals.GetColor("OverviewPawn.Header.Subs.TextColor");
            Header_Title.Style.TextColor = Globals.GetColor("OverviewPawn.Header.Subs.TextColor");
            Header_Miscellaneous.Style.TextColor = Globals.GetColor("OverviewPawn.Header.Subs.TextColor");

            Edit_Current_Outfits.OnClick += (obj, e) => WindowManager.TryToggleWindowVanilla<Dialog_ManageOutfits>(Pawn.outfits.CurrentOutfit);
            Edit_Current_Foods.OnClick += (obj, e) => WindowManager.TryToggleWindowVanilla<Dialog_ManageFoodRestrictions>(Pawn.foodRestriction.CurrentFoodRestriction);
            Edit_Current_Drugs.OnClick += (obj, e) => WindowManager.TryToggleWindowVanilla<Dialog_ManageDrugPolicies>(Pawn.drugs.CurrentPolicy);
            Edit_Default_Medicine.OnClick += (obj, e) => WindowManager.TryToggleWindowVanilla<Dialog_MedicalDefaults>();
            Open_Outfits.OnClick += (obj, e) => WindowManager.TryToggleWindowVanilla<Dialog_ManageOutfits>(default(Outfit));
            Open_Foods.OnClick += (obj, e) => WindowManager.TryToggleWindowVanilla<Dialog_ManageFoodRestrictions>(default(FoodRestriction));
            Open_Drugs.OnClick += (obj, e) => WindowManager.TryToggleWindowVanilla<Dialog_ManageDrugPolicies>(default(DrugPolicy));

            Header_Childhood.SetStyle("OverviewPawn.Header.Childhood");
            Header_Adulthood.SetStyle("OverviewPawn.Header.Adulthood");
            Header_Childhood.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header_Adulthood.Style.TextAnchor = TextAnchor.MiddleLeft;

            Header_Jobs.SetStyle("OverviewPawn.Header.Jobs");
            Header_Jobs.Style.TextAnchor = TextAnchor.MiddleLeft;

            Current_Outfits.Label.RenderStyle = LabelStyle.Fit;
            Current_Foods.Label.RenderStyle = LabelStyle.Fit;
            // One of the default entries is slightly too long.
            Current_Drugs.Label.RenderStyle = LabelStyle.Fit;

            Incapables = new ListView_Incapables(new Vector2(190f, 60f));
            Traits = new ListView_Traits(new Vector2(220f, 75f));
            Equipment = new Equipment(new Vector2(320f, 300f));

            Needs = new ListView_Needs(new Vector2(220f, 280f));
            Thoughts = new ListView_Thoughts(new Vector2(220f, 280f));
            Skills = new ListView_Skills(new Vector2(200f, 280f));

            AddRange(Incapables, Equipment, Needs, Thoughts, Skills, Traits, Faction, Xenotype, Religion, InventorySummary, FavoriteColor, Header_Nickname, Header_Name, Header_Title, Header_Miscellaneous, Header_Incapables, Header_Traits, Header_Childhood, Header_Adulthood, Header_Jobs, Current_Outfits, Edit_Current_Outfits, Edit_Current_Foods, Current_Foods, Current_Drugs, Edit_Current_Drugs, Edit_Default_Medicine, HostilityResponse, Medicine_TreatmentQuality, Medicine_Carry, Medicine_CarryType, SelfTend, Open_Outfits, Open_Foods, Open_Drugs);
            // Hide these, their visiblity will be toggled in Pull().
            (new List<Panel> { Incapables, Equipment, Needs, Thoughts, Skills, Traits, Faction, Xenotype, Religion, InventorySummary, Current_Outfits, Edit_Current_Outfits, Edit_Current_Foods, Current_Foods, Current_Drugs, HostilityResponse, Medicine_TreatmentQuality, Medicine_CarryType, SelfTend }).ForEach((F) => F.IsVisible = false);

            this.RenderStyle = RenderStyle;

            // Window is the default Style and has to be updated manually.
            if (RenderStyle == DisplayStyle.Window)
            {
                DoStyleWindow();
            }
        }

        private void DoStyleWindow()
        {
            if (Storage == null)
            {
                Storage = new InventoryPanel(new Vector2(Mathf.Min((Width * 0.3f), 200f), Height));
                Storage.Anchor = Anchor.TopRight;
                Storage.InheritParentSizeHeight = true;
                Storage.OnEmbed += DoOnEmbed_Storage;
                AddChild(Storage);
            }
            if (Inventory == null)
            {
                Inventory = new InventoryPanel(Storage.Size, new Vector2(-Storage.Width - 5f, 0f));
                Inventory.Anchor = Anchor.TopRight;
                Inventory.InheritParentSizeHeight = true;
                Inventory.LimitStackAmount = true;
                Inventory.OnEmbed += DoOnEmbed_Inventory;
                AddChild(Inventory);

                BigMod.ThingOwner.Added += TrackerInventory;
                BigMod.ThingOwner.Removed += TrackerInventory;
            }

            Needs.IncludeMood = true;
            // Needs Progress bar Tick sizes and positions have to be redone.
            if (Pawn != null)
            {
                Needs.Populate();
            }

            Equipment.Size = new Vector2(160f, 220f);
            Equipment.RenderStyle = RenderStyle;
            Equipment.Slots.ForEach((F) => F.CanDragRemoveItem = true);

            Header_Childhood.IsVisible = true;
            Header_Adulthood.IsVisible = true;

            FavoriteColor.IsVisible = true;

            Edit_Current_Outfits.IsVisible = true;
            Edit_Current_Foods.IsVisible = true;
            Edit_Current_Drugs.IsVisible = true;
            Edit_Default_Medicine.IsVisible = true;
            Open_Outfits.IsVisible = true;
            Open_Foods.IsVisible = true;
            Open_Drugs.IsVisible = true;

            Medicine_Carry.IsVisible = true;

            Incapables.IsVisible = true;
            Traits.IsVisible = true;

            Religion.Size = new Vector2(140f, 25f);
            Religion.Image.OffsetX = 0f;
            // Reset ProgressBar Size
            Religion.ProgressBar.Size = new Vector2(Religion.Width, (Religion.Height / 10f));
            Religion.ProgressBar.IsVertical = false;
            Religion.Label.IsVisible = true;
            
            InventorySummary.IsVisible = true;

            Faction.Size = new Vector2(140f, 25f);
            Faction.Label.IsVisible = true;

            Header_Incapables.IgnoreMouse = true;
            Header_Traits.IgnoreMouse = true;

            Current_Outfits.Anchor = Anchor.BottomLeft;
            Current_Foods.Anchor = Anchor.BottomLeft;
            Current_Drugs.Anchor = Anchor.BottomLeft;

            Xenotype.Anchor = Anchor.TopLeft;
            Faction.Anchor = Anchor.TopLeft;
            Religion.Anchor = Anchor.TopLeft;
            InventorySummary.Anchor = Anchor.TopLeft;
            FavoriteColor.Anchor = Anchor.TopLeft;
            Equipment.Anchor = Anchor.TopLeft;
            Header_Jobs.Anchor = Anchor.TopLeft;

            Header_Incapables.Anchor = Anchor.TopLeft;
            Header_Traits.Anchor = Anchor.TopLeft;
            Header_Incapables.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header_Traits.Style.TextAnchor = TextAnchor.MiddleLeft;

            HostilityResponse.Anchor = Anchor.BottomLeft;
            Medicine_TreatmentQuality.Anchor = Anchor.BottomLeft;
            Medicine_CarryType.Anchor = Anchor.BottomLeft;
            SelfTend.Anchor = Anchor.BottomLeft;

            Current_Outfits.Width = 100f;
            Current_Foods.Width = 100f;
            Current_Drugs.Width = 100f;

            Header_Nickname.Offset = new Vector2(10f, 5f);
            Header_Name.Offset = new Vector2(10f, 29f);
            Header_Title.Offset = new Vector2(10f, 42f);
            Header_Miscellaneous.Offset = new Vector2(10f, 56f);

            Header_Incapables.Offset = new Vector2(10f, 184f);
            Incapables.Offset = new Vector2(Header_Incapables.OffsetX, Header_Incapables.Bottom);

            Xenotype.Offset = new Vector2(320f, 10f);
            Faction.Offset = new Vector2((Xenotype.OffsetRight + 2f), Xenotype.Y);
            Religion.Offset = new Vector2((Faction.OffsetRight + 2f), Xenotype.Y);
            InventorySummary.Offset = new Vector2(Religion.OffsetX, Religion.Bottom + 2f);
            FavoriteColor.Offset = new Vector2((Religion.OffsetRight + 2f), Xenotype.Y);
            Current_Outfits.Offset = new Vector2(5f, -35f);
            Edit_Current_Outfits.Offset = new Vector2((Current_Outfits.Right), Current_Outfits.OffsetY);
            Current_Foods.Offset = new Vector2((Edit_Current_Outfits.Right + 3f), Current_Outfits.OffsetY);
            Edit_Current_Foods.Offset = new Vector2((Current_Foods.Right), Current_Outfits.OffsetY);
            Current_Drugs.Offset = new Vector2((Edit_Current_Foods.Right + 3f), Current_Outfits.OffsetY);
            Edit_Current_Drugs.Offset = new Vector2((Current_Drugs.Right), Current_Outfits.OffsetY);
            Open_Outfits.Offset = new Vector2(Current_Outfits.OffsetX, -5f);
            Open_Foods.Offset = new Vector2((Open_Outfits.Right + 5f), Open_Outfits.OffsetY);
            Open_Drugs.Offset = new Vector2((Open_Foods.Right + 5f), Open_Foods.OffsetY);

            HostilityResponse.Offset = new Vector2((Open_Drugs.Right + 25f), Open_Drugs.OffsetY);
            Medicine_TreatmentQuality.Offset = new Vector2((HostilityResponse.OffsetRight + 5f), Open_Drugs.OffsetY);
            Medicine_Carry.Offset = new Vector2((Medicine_TreatmentQuality.OffsetRight + 5f), Open_Drugs.OffsetY);
            Medicine_CarryType.Offset = new Vector2((Medicine_Carry.OffsetRight + 5f), Open_Drugs.OffsetY);
            Edit_Default_Medicine.Offset = new Vector2((Medicine_CarryType.OffsetRight + 5f), Open_Drugs.OffsetY);
            SelfTend.Offset = new Vector2((Edit_Default_Medicine.OffsetRight + 5f), Open_Drugs.OffsetY);

            Header_Childhood.Offset = new Vector2(108f, 80f);
            Header_Adulthood.Offset = new Vector2(Header_Childhood.OffsetX, (Header_Childhood.Bottom + 13f));

            Header_Traits.Offset = new Vector2(Header_Incapables.OffsetX, Incapables.Bottom);
            Traits.Offset = new Vector2(Header_Traits.OffsetX, Header_Traits.Bottom);

            Equipment.Offset = new Vector2(250f, 50f);

            Needs.Size = new Vector2(220f, 280f);
            Thoughts.Size = Needs.Size;
            Skills.Size = new Vector2(200f, Thoughts.Height);

            Needs.Offset = new Vector2(10f, Equipment.OffsetBottom + 100f);
            Thoughts.Offset = new Vector2(Needs.OffsetRight, Needs.OffsetY);
            Skills.Offset = new Vector2(Thoughts.OffsetRight, Needs.OffsetY);

            Header_Jobs.Style.FontType = GameFont.Small;
            Header_Jobs.InheritParentSizeWidth = false;
            Header_Jobs.Size = new Vector2((Thoughts.Width + Skills.Width - 20f), (Traits.Height + 5f));
            Header_Jobs.Offset = new Vector2((Traits.OffsetRight + 10f), (Equipment.OffsetBottom + 5f));
        }

        private void DoStyleInspect()
        {
            if (Storage != null)
            {
                Storage.OnEmbed -= DoOnEmbed_Storage;
                RemoveChild(Storage);
                Storage = null;
            }
            if (Inventory != null)
            {
                Inventory.OnEmbed -= DoOnEmbed_Inventory;
                RemoveChild(Inventory);
                Inventory = null;

                BigMod.ThingOwner.Added -= TrackerInventory;
                BigMod.ThingOwner.Removed -= TrackerInventory;
            }

            Needs.IncludeMood = false;
            // Needs Progress bar Tick sizes and positions have to be redone.
            if (Pawn != null)
            {
                Needs.Populate();
            }

            Equipment.RenderStyle = RenderStyle;
            Equipment.Size = new Vector2(Equipment.Slots.Max((F) => F.Right), Equipment.Slots.Max((F) => F.Bottom));
            Equipment.Slots.ForEach((F) => F.CanDragRemoveItem = false);

            Header_Childhood.IsVisible = false;
            Header_Adulthood.IsVisible = false;

            FavoriteColor.IsVisible = false;

            Edit_Current_Outfits.IsVisible = false;
            Edit_Current_Foods.IsVisible = false;
            Edit_Current_Drugs.IsVisible = false;
            Edit_Default_Medicine.IsVisible = false;
            Open_Outfits.IsVisible = false;
            Open_Foods.IsVisible = false;
            Open_Drugs.IsVisible = false;

            Medicine_Carry.IsVisible = false;

            Incapables.IsVisible = false;
            Traits.IsVisible = false;

            Header_Incapables.IgnoreMouse = false;
            Header_Traits.IgnoreMouse = false;

            Religion.Size = new Vector2(28f, 25f);
            Religion.Image.OffsetX = 3f;
            Religion.ProgressBar.Size = new Vector2(3f, 25f);
            Religion.ProgressBar.IsVertical = true;
            Religion.Label.IsVisible = false;

            Faction.Size = new Vector2(25f, 25f);
            Faction.Label.IsVisible = false;

            Current_Outfits.Anchor = Anchor.TopRight;
            Current_Foods.Anchor = Anchor.TopRight;
            Current_Drugs.Anchor = Anchor.TopRight;

            Xenotype.Anchor = Anchor.TopRight;
            Faction.Anchor = Anchor.TopRight;
            Religion.Anchor = Anchor.TopRight;
            InventorySummary.Anchor = Anchor.TopRight;
            FavoriteColor.Anchor = Anchor.TopRight;
            Equipment.Anchor = Anchor.BottomLeft;

            Header_Incapables.Anchor = Anchor.TopRight;
            Header_Traits.Anchor = Anchor.TopRight;
            Header_Incapables.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header_Traits.Style.TextAnchor = TextAnchor.MiddleLeft;

            HostilityResponse.Anchor = Anchor.TopRight;
            Medicine_TreatmentQuality.Anchor = Anchor.TopRight;
            Medicine_CarryType.Anchor = Anchor.TopRight;
            SelfTend.Anchor = Anchor.TopRight;
            Header_Jobs.Anchor = Anchor.BottomLeft;

            Current_Outfits.Width = 80f;
            Current_Foods.Width = 80f;
            Current_Drugs.Width = 90f;

            Header_Nickname.Offset = new Vector2(90f, 5f);
            Header_Name.Offset = new Vector2(Header_Nickname.X, 29f);
            Header_Title.Offset = new Vector2(Header_Nickname.X, 42f);
            Header_Miscellaneous.Offset = new Vector2(Header_Nickname.X, 56f);

            Header_Jobs.Style.FontType = GameFont.Tiny;
            Header_Jobs.InheritParentSizeWidth = true;
            Header_Jobs.InheritParentSize_Modifier = new Vector2(-10f, 0f);
            Header_Jobs.Height = 20f;
            Header_Jobs.Offset = new Vector2(5f, -5f);

            // Anchor.Right adds its own Width to the final position, Offset has to take that into account.
            Xenotype.Offset = new Vector2((-Faction.Width - Religion.Width - 10f), 5f);
            Faction.Offset = new Vector2((Xenotype.OffsetX + Faction.Width + 2f), Xenotype.Y);
            Religion.Offset = new Vector2((Faction.OffsetX + Religion.Width + 2f), Xenotype.Y);

            Current_Outfits.Offset = new Vector2((-Current_Foods.Width - Current_Drugs.Width - 10f), 65f);
            Current_Foods.Offset = new Vector2((Current_Outfits.OffsetX + Current_Foods.Width + 3f), Current_Outfits.OffsetY);
            Current_Drugs.Offset = new Vector2((Current_Foods.OffsetX + Current_Drugs.Width + 3f), Current_Outfits.OffsetY);

            HostilityResponse.Offset = new Vector2((-Medicine_TreatmentQuality.Width - Medicine_CarryType.Width - SelfTend.Width - 10f), (Xenotype.OffsetBottom + 2f));
            Medicine_TreatmentQuality.Offset = new Vector2((HostilityResponse.OffsetX + Medicine_TreatmentQuality.Width + 2f), HostilityResponse.Y);
            Medicine_CarryType.Offset = new Vector2((Medicine_TreatmentQuality.OffsetX + Medicine_CarryType.Width + 2f), HostilityResponse.Y);
            SelfTend.Offset = new Vector2((Medicine_CarryType.OffsetX + SelfTend.Width + 2f), HostilityResponse.Y);

            Header_Incapables.Offset = new Vector2((HostilityResponse.OffsetX - HostilityResponse.Width - 3f), 5f);
            Header_Traits.Offset = new Vector2(Header_Incapables.OffsetX, (Header_Incapables.Bottom - 2f));

            InventorySummary.Offset = new Vector2(Header_Incapables.OffsetX, Header_Traits.OffsetBottom);
            InventorySummary.Size = new Vector2(100f, 20f);

            Equipment.Offset = new Vector2(5f, -5f);

            Needs.Size = new Vector2(160f, 132f);
            Thoughts.Size = new Vector2(170f, Needs.Height);
            Skills.Size = new Vector2(120f, Needs.Height);

            Needs.Offset = new Vector2(5f, 100f);
            Thoughts.Offset = new Vector2(Needs.OffsetRight, Needs.Y);
            Skills.Offset = new Vector2(Thoughts.OffsetRight, Needs.Y);
        }

        #region "Updates"

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

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;

            if (Portrait != null)
            {
                // Remove existing one before adding a new one.
                Portrait.RemoveFromParent();
            }

            Portrait = new ListViewItem_Pawn(Pawn);
            Portrait.UseAnchoring = true;

            // Have to handle Portrait Offset here as it can be null when initiating this Class.
            if (RenderStyle == DisplayStyle.Window)
            {
                Portrait.SquareSize = new Vector2(90f, 90f);
                Portrait.Offset = new Vector2(10f, 80f);
            }
            else if (RenderStyle == DisplayStyle.Inspect)
            {
                Portrait.SquareSize = new Vector2(80f, 92f);
                Portrait.Offset = new Vector2(5f, 5f);
            }

            AddChild(Portrait);
            Portrait.RenderStyle = UnitStyle.Square;
            Portrait.Display = true;

            IPawns.Cast<Panel>().ToList().ForEach((F) => F.IsVisible = false);
            IPulls.Cast<Panel>().ToList().ForEach((F) => F.IsVisible = false);

            IPawns.Clear();
            IPulls.Clear();

            if (Pawn.needs != null)
            {
                IPawns.Add(Needs);
                IPulls.Add(Needs);
            }
            if (Pawn.CombinedDisabledWorkTags != WorkTags.None)
            {
                // Even if the Incapables and Traits ListViews aren't visible in DisplayStyle.Inspect mode, they still have to Pull their data for their Headers to Update correctly.
                IPawns.Add(Incapables);
                IPulls.Add(Incapables);
            }
            if (Portrait.IsHuman)
            {
                IPawns.AddRange(new IPawn[] { Equipment, Skills, Thoughts, Xenotype, Faction, Religion, Traits });
                IPulls.AddRange(new IPull[] { Equipment, Skills, Thoughts, Xenotype, Faction, Religion, Traits });

                if (Portrait.IsPlayerFaction)
                {
                    IPawns.AddRange(new List<IPawn> { Current_Outfits, Current_Foods, Current_Drugs, Medicine_CarryType, SelfTend, HostilityResponse });
                    IPulls.AddRange(new List<IPull> { Current_Outfits, Current_Foods, Current_Drugs, Medicine_CarryType, SelfTend, HostilityResponse });

                    if (RenderStyle == DisplayStyle.Window)
                    {
                        IPawns.Add(Medicine_Carry);
                        IPulls.Add(Medicine_Carry);
                    }
                }
            }
            if (Portrait.IsMech)
            {
                IPawns.Add(Equipment);
                IPulls.Add(Equipment);
            }
            else if (Portrait.IsPlayerFaction)
            {
                IPawns.Add(Medicine_TreatmentQuality);
                IPulls.Add(Medicine_TreatmentQuality);
            }
            if (Portrait.IsPlayerFaction)
            {
                IPawns.Add(InventorySummary);
                IPulls.Add(InventorySummary);
            }

            IPawns.Cast<Panel>().ToList().ForEach((F) => F.IsVisible = true);
            IPulls.Cast<Panel>().ToList().ForEach((F) => F.IsVisible = true);

            IPawns.ForEach((F) => F.SetPawn(Pawn));

            DoOnRequest();
        }

        public void Pull()
        {
            Portrait.DoOnRequest();

            // Grab these values from the Potrait
            Header_Nickname.Text = Portrait.Nickname.Text;
            Header_Name.Text = Portrait.Name.Text;
            Header_Title.Text = Portrait.Title.Text;
            Header_Miscellaneous.Text = Globals.GetGenderUnicodeSymbol(Pawn.gender) + " " + "PeriodYears".Translate(Pawn.ageTracker.AgeNumberString);
            Header_Miscellaneous.ToolTipText = Pawn.ageTracker.AgeTooltipString;

            Edit_Default_Medicine.ToolTipText = "DefaultMedicineSettings".Translate();

            if (Portrait.IsHuman)
            {
                Header_Childhood.Text = "Childhood".Translate() + ": \n" + Pawn.story.GetBackstory(BackstorySlot.Childhood)?.TitleCapFor(Pawn.gender);
                Header_Adulthood.Text = "Adulthood".Translate() + ": \n" + Pawn.story.GetBackstory(BackstorySlot.Adulthood)?.TitleCapFor(Pawn.gender);
                Header_Childhood.ToolTipText = Pawn.story.GetBackstory(BackstorySlot.Childhood)?.FullDescriptionFor(Pawn).Resolve();
                Header_Adulthood.ToolTipText = Pawn.story.GetBackstory(BackstorySlot.Adulthood)?.FullDescriptionFor(Pawn).Resolve();

                FavoriteColor.Style.Color = Pawn.story.favoriteColor.Value;

                string IdeoColor = string.Empty;

                if ((Pawn.Ideo != null) && !Pawn.Ideo.hiddenIdeoMode)
                {
                    IdeoColor = "OrIdeoColor".Translate(Pawn.Named("PAWN"));
                }

                FavoriteColor.ToolTipText = "FavoriteColorTooltip".Translate(Pawn.Named("PAWN"), 0.6f.ToStringPercent().Named("PERCENTAGE"), IdeoColor.Named("ORIDEO")).Resolve();

                // Add a additional description for Colorblind folks.
                FavoriteColor.ToolTipText += "\n\n" + (Globals.GetClosestColorDef(FavoriteColor.Style.Color)?.LabelCap ?? string.Empty);

                Edit_Default_Medicine.ToolTipText += "\n\n" + Globals.GetMedicalGroup(Pawn).Translate();
            }

            IPulls.ForEach((F) => F.Pull());

            if (!Incapables.Items.Any())
            {
                Header_Incapables.Text = "NoIncapables".Translate();
                Incapables.IsVisible = false;
            }
            else if (RenderStyle == DisplayStyle.Inspect)
            {
                Header_Incapables.Text = $"{"Incapables".Translate()} : {Incapables.Items.Count}";
                Header_Incapables.ToolTipText = string.Join("\n", Incapables.Items.Select((F) => F.Text).ToArray());
                Incapables.IsVisible = false;
            }
            if (!Traits.Items.Any())
            {
                Header_Traits.Text = "NoTraits".Translate();
                Traits.IsVisible = false;
            }
            else if (RenderStyle == DisplayStyle.Inspect)
            {
                Header_Traits.Text = $"{"Traits".Translate()} : {Traits.Items.Count}";
                Header_Traits.ToolTipText = string.Join("\n", Traits.Items.Select((F) => F.Text).ToArray());
                Traits.IsVisible = false;
            }

            UpdateInventory();
            UpdateStorage();
            UpdateJobs();
        }
        public void TrackerInventory(object Sender, BigMod.ThingOwner.ItemEventArgs ItemEventArgs)
        {
            if (Inventory.IsVisible && ((Sender is Pawn PawnSender) && (PawnSender == Pawn)))
            {
                UpdateInventory();
            }
        }
        public void UpdateInventory()
        {
            if (Inventory != null)
            {
                Inventory.ListView.Clear();

                // Add Items Reserved by this Pawn.
                Verse.AI.ReservationManager ReservationManager = Pawn.MapHeld.reservationManager;
                IEnumerable<Thing> ReservedThings = ReservationManager.AllReservedThings().Where((F) => ReservationManager.ReservedBy(F, Pawn));

                // Check if there's any item currently being held as a ToolTip.
                ListViewItem_Inventory.GetToolTipItem(out ListViewItem_Inventory ToolTipItem);

                List<Thing> ToolTipThings = ((ToolTipItem != null) ? (ToolTipItem.Things ?? new List<Thing>(){ ToolTipItem.Thing }) : new List<Thing>());

                foreach (Thing Thing in Pawn.inventory.innerContainer.Concat(ReservedThings))
                {
                    if (!Equipment.Exists(Thing) && !ToolTipThings.Contains(Thing))
                    {
                        ListViewItem_Inventory Item = new ListViewItem_Inventory(Thing);
                        Item.Fallback = Inventory;
                        Item.Count = Thing.stackCount;
                        Item.Amount.Text = Thing.stackCount.ToString() + "x";

                        Inventory.ListView.AddItem(Item);
                    }
                }

                // Triggers Filtering and Sorting.
                Inventory.OnTypingFinished(Inventory, EventArgs.Empty);
            }
        }

        public void UpdateStorage()
        {
            if (Storage != null)
            {
                Storage.ListView.Clear();

                Dictionary<ThingDef, (List<Thing> Things, int Count)> ListableCommodities = Globals.GetAllCommodities();
                List<Thing> InventoryThings = Inventory.ListView.Items.OfType<ListViewItem_Inventory>().SelectMany((F) => F.Things ?? new List<Thing>(){ F.Thing }).ToList();

                // Check if there's any item currently being held as a ToolTip.
                ListViewItem_Inventory.GetToolTipItem(out ListViewItem_Inventory ToolTipItem);

                // Add the ToolTipItem's Things to the exemption list.
                if (ToolTipItem != null)
                {
                    if (ToolTipItem.Things != null)
                    {
                        InventoryThings.AddRange(ToolTipItem.Things);
                    }
                    else
                    {
                        InventoryThings.Add(ToolTipItem.Thing);
                    }
                }

                foreach (var Commodity in ListableCommodities)
                {
                    List<Thing> Things = Commodity.Value.Things;
                    int Count = Commodity.Value.Count;

                    for (int i = (Things.Count - 1); i >= 0; i--)
                    {
                        Thing Thing = Things[i];

                        // Remove stuff that is in the Equipment or the Inventory panel.
                        if (Equipment.Exists(Thing) || InventoryThings.Contains(Thing))
                        {
                            Count -= Thing.stackCount;
                            Things.RemoveAt(i);
                        }
                    }

                    if (Things.Any())
                    {
                        ListViewItem_Inventory Item = new ListViewItem_Inventory(Things.First(), false);
                        Item.Things = Things;
                        Item.Fallback = Storage;
                        Item.Count = Count;
                        Item.Amount.Text = Count.ToString() + "x";

                        Storage.ListView.AddItem(Item);
                    }
                }

                // Triggers Filtering and Sorting.
                Storage.OnTypingFinished(Storage, EventArgs.Empty);
            }
        }
        public void UpdateJobs()
        {
            StringBuilder StringBuilder = new StringBuilder();

            foreach (Verse.AI.Job Job in Pawn.jobs.AllJobs())
            {               
                StringBuilder.AppendLine((Pawn.CurJob == Job) ? Job.GetReport(Pawn).CapitalizeFirst().Colorize(Jobs_Current) : Job.GetReport(Pawn).CapitalizeFirst().Colorize(Jobs_Queued));
            }

            Header_Jobs.Text = StringBuilder.ToString().TrimEndNewlines();
            // Hide if no Jobs
            Header_Jobs.IsVisible = !string.IsNullOrWhiteSpace(Header_Jobs.Text);
        }

        // TODO: Fallback shouldn't trigger these 2 :
        // TODO: Equipment slots SHOULD trigger these 2 :
        public void DoOnEmbed_Inventory(object Sender, EventArgs EventArgs)
        {
            ListViewItem_Inventory Item = (ListViewItem_Inventory)Sender;

            // Clear reservations and Cancel Jobs.
            Item.ReleaseReservation();

            ListViewItem_Inventory.InterfacePickup(Pawn, Item.Thing, Item.Count);
        }

        public void DoOnEmbed_Storage(object Sender, EventArgs EventArgs)
        {
            ListViewItem_Inventory Item = (ListViewItem_Inventory)Sender;

            // Clear reservations and Cancel Jobs.
            Item.ReleaseReservation();

            ListViewItem_Inventory.InterfaceHaulToStorage(Pawn, Item.Thing, Item.Count);
        }

        public void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            Pull();
        }

        #endregion "Updates"


        /// <summary>
        /// Get a Instance of <see cref="OverviewPawn"/> class from a currently Open Window.
        /// </summary>
        /// <param name="OverviewPawn">The first Instance of <see cref="OverviewPawn"/> class.</param>
        public static bool GetInstance(out OverviewPawn OverviewPawn)
        {
            OverviewPawn = WindowManager.Instance.Windows.SelectMany((F) => F.Root.GetChildrenFlatten().OfType<OverviewPawn>()).FirstOrDefault();

            return (OverviewPawn != null);
        }
    }
}
