using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Mechs
{
    /// <summary>
    /// Replaces the MainTabWindow_Mechs class when its button is being pressed in the ingame HUD.
    /// </summary>
    public class Mechs : DataPawnWindow_Area
    {
        public override Rect DefaultBounds { get; set; } = new Rect(10, (UI.screenHeight - 700), 705, 600);
        public Button ChooseColors;
        // Copied from MainTabWindow_Mechs.Pawns
        public override IEnumerable<Pawn> Pawns
        {
            get
            {
                return from Pawn in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer) where (Pawn.RaceProps.IsMechanoid && (Pawn.TryGetComp<CompOverseerSubject>() != null)) select Pawn;
            }
        }

        public Mechs()
        {
            Identifier = "Mechs";

            ChooseColors = new Button("Colors".Translate());
            ChooseColors.SetStyle("Mechs.Button.ChooseColors");
            ChooseColors.Style.DrawBackground = true;
            ChooseColors.Style.BackgroundColor = Find.FactionManager.OfPlayer.MechColor;
            ChooseColors.Size = new Vector2(120f, (DataView.CellHeight - 2f));
            ChooseColors.Anchor = Anchor.BottomLeft;
            ChooseColors.OffsetX = AreaManager.OffsetRight;
            ChooseColors.OnClick += (obj, e) =>
            {
                // Copied from MainTabWindow_Mechs.PostOpen
                List<Color> CachedColors = (from Def in DefDatabase<ColorDef>.AllDefsListForReading select Def.color).ToList();
                CachedColors.AddRange(from F in Find.FactionManager.AllFactionsVisible select F.Color);
                CachedColors.SortByColor((Color Color) => Color);

                WindowManager.TryToggleWindowVanilla(typeof(Dialog_ChooseColor), "ChooseMechAccentColor".Translate().ToString(), Find.FactionManager.OfPlayer.MechColor, CachedColors, (Color Color) =>
                {
                    Find.FactionManager.OfPlayer.MechColor = Color;

                    foreach (Pawn dirty in MechanitorUtility.MechsInPlayerFaction())
                    {
                        PortraitsCache.SetDirty(dirty);
                    }

                    ChooseColors.Style.TextColor = Find.FactionManager.OfPlayer.MechColor;
                });
            };
            Root.AddChild(ChooseColors);

            ((Button)DataView.GetHeaderByID("Pawns")).Text = "Header_Mechs".Translate();
        }

        public override void OnWindowClosed(object Sender, EventArgs EventArgs)
        {
            if (Sender is Dialog_ChooseColor)
            {
                // Mechs's DataView has to be reset to reflect any changes to Colors.
                WindowManager.ToggleWindow(this);
                Mechs Reset = new Mechs();
                WindowManager.ToggleWindow(Reset);
                Reset.Bounds = Bounds;
            }
        }

        public override void InitiateHeaders()
        {
            base.InitiateHeaders();

            // Reduce the Name' Header's width as it's too long for Mechs.
            DataView.GetHeaderByID("Pawns").Width = 200f;

            string[] Names = {"ControlGroup", "AutoRepair", "Draft", "Overseer"};

            foreach (string Name in Names)
            {
                Button Header = new Button(("Header_" + Name).Translate());
                Header.SetStyle("Mechs.Header");
                Header.Style.DrawBackground = true;
                Header.ID = Name;
                Header.Size = new Vector2((25f + (12f * Prefs.UIScale)), DataView.HeaderHeight);
                Header.Label.Style.FontType = GameFont.Small;
                Header_Register(Header);

                string IconPath = Globals.TryGetTexturePathFromAlias(Name);

                if (IconPath != Name)
                {
                    Header.AddImage(IconPath);
                    Header.RenderStyle = ButtonStyle.Image;
                    Header.Image.InheritParentSize = false;
                    Header.Image.ScaleMode = ScaleMode.ScaleToFit;
                    Header.Image.Anchor = Anchor.Center;
                    Header.Image.Size = new Vector2(25f, 25f);
                    // Move Text to ToolTip.
                    Header.ToolTipText = Header.Text;
                    Header.Text = string.Empty;
                }

                DataView.AddColumn(Header);
            }

            foreach (MechWorkModeDef Def in (from F in DefDatabase<MechWorkModeDef>.AllDefsListForReading orderby F.uiOrder select F))
            {
                Button Header = new Button(ButtonStyle.Image, Def.iconPath);
                Header.SetStyle("Mechs.Header");
                Header.Image.ScaleMode = ScaleMode.ScaleToFit;
                Header.Image.Anchor = Anchor.Center;
                Header.Style.DrawBackground = true;
                Header.ID = Def.defName;
                Header.Size = new Vector2(DataView.CellHeight, DataView.HeaderHeight);
                Header.ToolTipText = Def.description;
                Header_Register(Header);

                DataView.AddColumn(Header);
            }

            // Overseer header has to be larger as it has Pawn items.
            DataView.GetHeaderByID("Overseer").Width = 160f;
            DataView.UpdatePositions();

            InitiateHeaders_Areas();
        }

        public override void Populate()
        {
            base.Populate();

            List<MechWorkModeDef> Defs = (from F in DefDatabase<MechWorkModeDef>.AllDefsListForReading orderby F.uiOrder select F).ToList();

            int Index_ControlGroup = DataView.GetHeaderIndexByID("ControlGroup");
            int Index_AutoRepair = DataView.GetHeaderIndexByID("AutoRepair");
            int Index_Draft = DataView.GetHeaderIndexByID("Draft");
            int Index_Overseer = DataView.GetHeaderIndexByID("Overseer");

            Dictionary<string, int> Indexes = new Dictionary<string, int>();

            // Doing it like this to ensure correct order without resorting to any hard defined indexes.
            foreach (MechWorkModeDef Def in Defs)
            {
                int i = DataView.GetHeaderIndexByID(Def.defName);

                if (i != -1)
                {
                    Indexes.Add(Def.defName, i);
                }
            }

            foreach (DataViewRow Row in DataView.Rows)
            {
                ListViewItem_Pawn Item = (ListViewItem_Pawn)Row.Cells[0].GetChildWithID("Pawn");

                Row.ReplaceCell(Index_ControlGroup, new DataViewRowCell_ControlGroup(Item.Pawn));
                Row.ReplaceCell(Index_AutoRepair, new DataViewRowCell_AutoRepair(Item.Pawn));
                Row.ReplaceCell(Index_Draft, new DataViewRowCell_Draft(Item.Pawn));
                Row.ReplaceCell(Index_Overseer, new DataViewRowCell_Overseer(Item.Pawn));

                foreach (MechWorkModeDef Def in Defs)
                {
                    Row.ReplaceCell(Indexes[Def.defName], new DataViewRowCell_WorkMode(Item.Pawn, Def));
                }
            }

            Populate_Areas();
        }
    }
}
