using RimWorld;
using Verse;

namespace BigMod.Entities.Windows
{
    /// <summary>
    /// Base class for Data Windows with Area Management.
    /// </summary>
    public abstract class DataPawnWindow_Area : DataPawnWindow
    {
        public Button AreaManager;

        protected DataPawnWindow_Area()
        {
            Identifier = "Area";

            // Prevent the Scrollbars from overlapping the Area Buttons.
            DataView.InheritParentSize_Modifier += new Vector2(0f, -DataView.CellHeight);
            DataView.Height -= DataView.CellHeight;

            AreaManager = new Button("AreaManager".Translate());
            AreaManager.SetStyle("Area.Button.AreaManager");
            AreaManager.Style.DrawBackground = true;
            AreaManager.Size = new Vector2(120f, (DataView.CellHeight - 2f));
            AreaManager.Anchor = Anchor.BottomLeft;
            AreaManager.Offset = new Vector2(5f, 0f);
            AreaManager.OnClick += (obj, e) =>
            {
                WindowManager.TryToggleWindowVanilla(typeof(Dialog_ManageAreas), Find.CurrentMap);
            };
            Root.AddChild(AreaManager);

            AddButtonCloseX();
            AddButtonResize();

            BigMod.WindowStack.OnWindowClosed += OnWindowClosed;
        }

        public virtual void OnWindowClosed(object Sender, EventArgs EventArgs)
        {
            if (Sender is Dialog_ManageAreas)
            {
                // Areas's DataView has to be reset to reflect any changes to Areas.
                WindowManager.ToggleWindow(this);
                // Do it like this to allow derived classes to be created dynamically.
                WindowPanel Reset = ((WindowPanel)Activator.CreateInstance(GetType(), null));
                WindowManager.ToggleWindow(Reset);
                Reset.Bounds = Bounds;
            }
        }

        public override void PreClose()
        {
            base.PreClose();

            BigMod.WindowStack.OnWindowClosed -= OnWindowClosed;
        }

        public void InitiateHeaders_Areas()
        {
            List<Area> Areas = Find.CurrentMap.areaManager.AllAreas;

            foreach (Area Area in Areas)
            {
                if (Area.AssignableAsAllowed())
                {
                    Button Header = new Button(Area.Label);
                    Header.SetStyle("Area.Header");
                    Header.Style.BackgroundColor = Area.Color;
                    Header.Style.DrawBackground = true;
                    Header.Label.Style.WordWrap = true;
                    Header.ID = Area.Label;
                    Header.Size = new Vector2(56f, DataView.HeaderHeight);
                    Header.Data = Area;
                    Header_Register(Header);
                    Header.CanToggle = true;
                    Header.OnWhileMouseOver += (obj, e) =>
                    {
                        Area.MarkForDraw();
                    };

                    DataView.AddColumn(Header);
                }
            }
        }

        public void Populate_Areas()
        {
            IEnumerable<Area> Areas = DataView.Header.Where((F) => (F.Data is Area)).Select((F) => (Area)F.Data);

            if (Areas.Any())
            {
                foreach (DataViewRow Row in DataView.Rows)
                {
                    ListViewItem_Pawn Item = (ListViewItem_Pawn)Row.Cells[0].GetChildWithID("Pawn");

                    Label Label = new Label("Unrestricted".Translate());
                    Label.SetStyle("Area.DataViewRow.Unrestricted");
                    Label.Style.DrawBackground = true;
                    Label.ID = "Unrestricted";
                    Label.UseAnchoring = false;
                    // Set default to allow Mouse detection for ToolTips.
                    Label.IgnoreMouse = false;

                    float First = DataView.GetHeaderByID(Areas.First().Label).X;
                    float Last = DataView.GetHeaderByID(Areas.Last().Label).Right;

                    Label.X = First;
                    Label.Size = new Vector2((Last - First), DataView.CellHeight);

                    bool LockAreas = false;

                    if (Item.Pawn.playerSettings.SupportsAllowedAreas)
                    {
                        // Disable Mouse detection as ToolTips aren't needed here.
                        Label.IgnoreMouse = true;
                    }
                    else
                    {
                        LockAreas = true;
                    }

                    // Always add Cells as they are needed for Sorting.
                    foreach (Area Area in Areas)
                    {
                        Schedule.DataViewRowCell_Area Cell = new Schedule.DataViewRowCell_Area(Item.Pawn, Area);
                        Cell.IsLocked = LockAreas;

                        Row.ReplaceCell(DataView.GetHeaderIndexByID(Area.Label), Cell);
                    }

                    if (Item.Pawn.RaceProps.IsMechanoid && (Item.Pawn.GetOverseer() == null))
                    {
                        Label.Text = "NoOverseer".Translate();
                    }
                    else if (AnimalPenUtility.NeedsToBeManagedByRope(Item.Pawn))
                    {
                        CompAnimalPenMarker Pen = AnimalPenUtility.GetCurrentPenOf(Item.Pawn, false);

                        if (Pen != null)
                        {
                            Label.Text = "InPen".Translate() + ": " + Pen.label;
                        }
                        else
                        {
                            Label.Text = "Unpenned".Translate();
                            Label.ToolTipText = "UnpennedTooltip".Translate();
                        }
                    }
                    else if (Item.Pawn.RaceProps.Dryad)
                    {
                        Label.Text = "CannotAssignArea".Translate();
                    }

                    Row.AddChild(Label);
                }

                // Cells need to be Updated after they've been added to reflect on current restrictions.
                foreach (Schedule.DataViewRowCell_Area Cell in DataView.Cells.OfType<Schedule.DataViewRowCell_Area>())
                {
                    Cell.Push();
                }
            }
        }
    }
}
