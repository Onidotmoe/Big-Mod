using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    /// <summary>
    /// Replaces the MainTabWindow_Mechs class when its button is being pressed in the ingame HUD.
    /// </summary>
    public class Animals : DataPawnWindow_Area
    {
        public override Rect DefaultBounds { get; set; } = new Rect(10, (UI.screenHeight - 700), 1165, 600);
        public Button AutoSlaughter;
        // Copied from MainTabWindow_Animals.Pawns
        public override IEnumerable<Pawn> Pawns
        {
            get
            {
                return from Pawn in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer) where Pawn.RaceProps.Animal select Pawn;
            }
        }

        public Animals()
        {
            Identifier = "Animals";

            AutoSlaughter = new Button("AutoSlaughter".Translate());
            AutoSlaughter.SetStyle("Animals.Button.AutoSlaughter");
            AutoSlaughter.Style.DrawBackground = true;
            AutoSlaughter.Size = new Vector2(120f, (DataView.CellHeight - 2f));
            AutoSlaughter.Anchor = Anchor.BottomLeft;
            AutoSlaughter.OffsetX = AreaManager.OffsetRight;
            AutoSlaughter.OnClick += (obj, e) =>
            {
                WindowManager.TryToggleWindowVanilla(typeof(Dialog_AutoSlaughter), Find.CurrentMap);
            };
            Root.AddChild(AutoSlaughter);

            ((Button)DataView.GetHeaderByID("Pawns")).Text = "Header_Animals".Translate();
        }

        public Button AddHeader<T>(string ID)
        {
            Button Header = new Button(("Header_" + ID).Translate());
            Header.SetStyle("Animals.Header");
            Header.Style.DrawBackground = true;
            Header.ID = ID;
            Header.Size = new Vector2((25f + (12f * Prefs.UIScale)), DataView.HeaderHeight);
            Header.Label.Style.FontType = GameFont.Small;
            Header.Data = typeof(T);
            Header_Register(Header);

            string IconPath = Globals.TryGetTexturePathFromAlias(ID);

            if (IconPath != ID)
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

            return Header;
        }

        public override void InitiateHeaders()
        {
            base.InitiateHeaders();

            // Reduce the Name' Header's width as it's too long for Animals.
            DataView.GetHeaderByID("Pawns").Width = 200f;

            AddHeader<Wildlife.DataViewRowCell_Gender>("Gender");
            AddHeader<Wildlife.DataViewRowCell_LifeStage>("LifeStage");
            AddHeader<DataViewRowCell_Age>("Age");
            AddHeader<DataViewRowCell_Pregnant>("Pregnant");
            AddHeader<DataViewRowCell_Bond>("Bond");
            AddHeader<DataViewRowCell_Master>("Master");
            AddHeader<DataViewRowCell_FollowDrafted>("FollowDrafted");
            AddHeader<DataViewRowCell_FollowFieldwork>("FollowFieldwork");
            AddHeader<DataViewRowCell_ReleaseToWild>("ReleaseToWild");
            AddHeader<DataViewRowCell_Slaughter>("Slaughter");
            AddHeader<DataViewRowCell_Sterilize>("Sterilize");

            foreach (TrainableDef Def in from Def in DefDatabase<TrainableDef>.AllDefsListForReading orderby Def.listPriority descending select Def)
            {
                Button Header = AddHeader<DataViewRowCell_Trainable>(Def.defName);
                Header.ToolTipText = Def.description;
            }

            AddHeader<DataViewRowCell_MedicalCare>("MedicalCare");

            // Master header has to be larger as it has Pawn items.
            DataView.GetHeaderByID("Master").Width = 160f;
            DataView.UpdatePositions();

            InitiateHeaders_Areas();
        }

        public override void Populate()
        {
            base.Populate();

            // Dynamically add the Cells.
            for (int Y = 0; Y < DataView.Rows.Count; Y++)
            {
                ListViewItem_Pawn Item = (ListViewItem_Pawn)DataView[Y][0].GetChildWithID("Pawn");

                for (int X = 0; X < DataView.Header.Count; X++)
                {
                    if (DataView.Header[X].Data is not Schedule.DataViewRowCell_Area)
                    {
                        Type Data = DataView.Header[X].Data as Type;

                        if (Data != null)
                        {
                            DataViewRowCell Cell = DataView[Y][X];

                            if (Cell.GetType() != Data)
                            {
                                if (Data != typeof(DataViewRowCell_Trainable))
                                {
                                    DataView[Y].ReplaceCell(X, (DataViewRowCell_Data)Activator.CreateInstance(Data, Item.Pawn));
                                }
                                else
                                {
                                    DataView[Y].ReplaceCell(X, (DataViewRowCell_Data)Activator.CreateInstance(Data, Item.Pawn, DefDatabase<TrainableDef>.GetNamed(DataView.Header[X].ID, false)));
                                }
                            }
                        }
                    }
                }
            }

            Populate_Areas();
        }
    }
}
