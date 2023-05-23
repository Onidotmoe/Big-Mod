using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Wildlife
{
    /// <summary>
    /// Replaces the MainTabWindow_Wildlife class when its button is being pressed in the ingame HUD.
    /// </summary>
    public class Wildlife : DataPawnWindow
    {
        public override Rect DefaultBounds { get; set; } = new Rect(10, (UI.screenHeight - 700), 435, 600);
        // Copied from MainTabWindow_Wildlife.Pawns
        public override IEnumerable<Pawn> Pawns
        {
            get
            {
                return from Pawn in Find.CurrentMap.mapPawns.AllPawns where (Pawn.Spawned && ((Pawn.Faction == null) || Pawn.Faction == Faction.OfInsects) && Pawn.AnimalOrWildMan() && !Pawn.Position.Fogged(Pawn.Map) && !Pawn.IsPrisonerInPrisonCell()) select Pawn;
            }
        }

        public Wildlife()
        {
            Identifier = "Wildlife";

            AddButtonCloseX();
            AddButtonResize();

            ((Button)DataView.GetHeaderByID("Pawns")).Text = "Header_Wildlife".Translate();
        }

        public override void InitiateHeaders()
        {
            base.InitiateHeaders();

            // Reduce the Name' Header's width as it's too long for animals.
            DataView.GetHeaderByID("Pawns").Width = 200f;

            // Ignore list as we still want other mods or future additions to be automatically handled.
            List<Type> Ignore = new List<Type>() { typeof(PawnColumnWorker_Label), typeof(PawnColumnWorker_Gap), typeof(PawnColumnWorker_Info), typeof(PawnColumnWorker_RemainingSpace), typeof(PawnColumnWorker_MentalState), typeof(PawnColumnWorker_ManhunterOnDamageChance), typeof(PawnColumnWorker_ManhunterOnTameFailChance) };

            List<PawnColumnDef> Columns = PawnTableDefOf.Wildlife.columns;

            // Move the Predator column to after the LifeStage Column.
            int i = Columns.FirstIndexOf((F) => F.workerClass == typeof(PawnColumnWorker_Predator));
            PawnColumnDef ColumnDef = Columns[i];
            Columns.RemoveAt(i);
            i = Columns.FirstIndexOf((F) => F.workerClass == typeof(PawnColumnWorker_LifeStage));
            // Set the Texture Path here so it'll be generated below.
            Columns[i].headerIcon = Globals.TryGetTexturePathFromAlias("LifeStage");
            Columns.Insert((i + 1), ColumnDef);

            foreach (PawnColumnDef Column in Columns)
            {
                if (Ignore.Contains(Column.workerClass))
                {
                    continue;
                }

                Button Header = new Button(Column.LabelCap);
                Header.SetStyle("Wildlife.Header");
                Header.Style.DrawBackground = true;
                Header.ID = Column.defName;
                Header.Size = new Vector2((25f + (12f * Prefs.UIScale)), DataView.HeaderHeight);
                Header.Label.Style.FontType = GameFont.Small;
                Header_Register(Header);

                if (!string.IsNullOrWhiteSpace(Column.headerIcon))
                {
                    Header.AddImage(Column.headerIcon);
                    Header.RenderStyle = ButtonStyle.Image;
                    Header.Image.InheritParentSize = false;
                    Header.Image.ScaleMode = ScaleMode.ScaleToFit;
                    Header.Image.Anchor = Anchor.Center;
                    Header.Image.Size = new Vector2(25f, 25f);
                }

                Header.ToolTipText = Column.headerTip + Environment.NewLine + Environment.NewLine + Column.description;

                if (string.IsNullOrWhiteSpace(Header.ToolTipText) && (Header.Image != null))
                {
                    // If it's a Icon, remove text and add missing ToolTip
                    Header.Text = string.Empty;
                    Header.ToolTipText = Column.LabelCap;
                }

                DataView.AddColumn(Header);
            }
        }

        public override void Populate()
        {
            base.Populate();

            foreach (DataViewRow Row in DataView.Rows)
            {
                ListViewItem_Pawn Item = (ListViewItem_Pawn)Row.Cells[0].GetChildWithID("Pawn");

                for (int i = 0; i < DataView.Header.Count; i++)
                {
                    string defName = DataView.Header[i].ID;
                    DataViewRowCell_Data Cell = null;

                    if (defName == "Gender")
                    {
                        Cell = new DataViewRowCell_Gender(Item.Pawn);
                    }
                    else if (defName == "LifeStage")
                    {
                        Cell = new DataViewRowCell_LifeStage(Item.Pawn);
                    }
                    else if (defName == "Predator")
                    {
                        Cell = new DataViewRowCell_Predator(Item.Pawn);
                    }
                    else if (defName == DesignationDefOf.Hunt.defName)
                    {
                        Cell = new DataViewRowCell_Hunt(Item.Pawn);
                    }
                    else if (defName == DesignationDefOf.Tame.defName)
                    {
                        Cell = new DataViewRowCell_Tame(Item.Pawn);
                    }

                    if (Cell != null)
                    {
                        Row.ReplaceCell(i, Cell);
                    }
                }
            }
        }
    }
}
