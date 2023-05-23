using RimWorld;
using Verse;

namespace BigMod.Entities.Windows
{
    /// <summary>
    /// Generic Window with a DataView with Pawns in the first Column, with sorting preset to Pawn Names.
    /// </summary>
    public class DataPawnWindow : WindowPanel
    {
        public DataView DataView;

        /// <summary>
        /// Time before the <see cref="LastMouseOverTime"/> becomes invalid.
        /// </summary>
        public float LastMouseOverTime_Threshold = 3f;

        public int IncrementBy = 1;
        public int DecrementBy = -1;
        public override Rect DefaultBounds { get; set; } = new Rect(0, (UI.screenHeight - 300f), 1200f, 200f);

        public virtual IEnumerable<Pawn> Pawns
        {
            get
            {
                return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists;
            }
        }

        public DataPawnWindow()
        {
            Identifier = "DataPawnWindow";

            CameraMouseOverZooming = false;
            CanCloseRightClick = true;

            DataView = new DataView(Width, Height);
            DataView.InheritParentSize = true;
            DataView.InheritParentSize_Modifier = new Vector2(-4f, -4f);
            Root.AddChild(DataView);

            InitiateHeaders();
            Populate();
        }

        public virtual void InitiateHeaders()
        {
            Button Header = new Button("Header_Pawns".Translate());
            Header.SetStyle("DataPawnWindow.Header");
            Header.Style.DrawBackground = true;
            Header.ID = "Pawns";
            Header.Size = new Vector2(300f, DataView.HeaderHeight);
            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Label.Offset = new Vector2(8f, 0f);
            Header.Data = typeof(DataViewRowCell_Pawn);
            Header_Register(Header);

            DataView.AddColumn(Header);
        }

        public virtual void Populate()
        {
            foreach (Pawn Pawn in Pawns)
            {
                DataViewRow Row = new DataViewRow(DataView);
                Row.SetStyle("DataPawnWindow.DataViewRow");
                Row.Style.DrawBackground = false;
                Row.ReplaceCell(0, new DataViewRowCell_Pawn(Pawn));
            }
        }

        #region "Cells & Headers"

        public virtual void Header_Register(Button Button)
        {
            Button.OnClick += Header_OnClick;
            Button.OnClickRight += Header_OnClickRight;
            Button.OnMouseWheel += Header_OnMouseWheel;
            Button.OnWhileMouseOver += Header_OnWhileMouseOver;
            Button.OnMouseEnter += Header_OnMouseEnter;
        }

        public virtual void Header_Unregister(Button Button)
        {
            Button.OnClick -= Header_OnClick;
            Button.OnClickRight -= Header_OnClickRight;
            Button.OnMouseWheel -= Header_OnMouseWheel;
            Button.OnWhileMouseOver -= Header_OnWhileMouseOver;
            Button.OnMouseEnter -= Header_OnMouseEnter;
        }

        public virtual void Header_Sort(Button Button)
        {
            Button.ToggleState = !Button.ToggleState;

            int X = DataView.GetHeaderIndexByID(Button.ID);

            if (Button.ToggleState)
            {
                // Ascending order
                DataView.Rows.Sort((A, B) => ((DataViewRowCell_Data)A[X]).CompareTo((DataViewRowCell_Data)B[X]));
            }
            else
            {
                // Descending order
                DataView.Rows.Sort((A, B) => ((DataViewRowCell_Data)B[X]).CompareTo((DataViewRowCell_Data)A[X]));
            }

            DataView.UpdatePositions();
        }

        /// <summary>
        /// Applies Delta to the Column the given Header Button is associated with.
        /// </summary>
        /// <param name="Button">Header Button to get Column from.</param>
        /// <param name="Delta">Delta Integer to apply.</param>
        /// <returns>True if Handled.</returns>
        public bool Header_DeltaPriority(Button Button, int Delta)
        {
            if (!WindowManager.IsCtrlDown())
            {
                if (WindowManager.IsShiftDown())
                {
                    int Index = DataView.GetHeaderIndexByID(Button.ID);

                    if (!WindowManager.IsAltDown())
                    {
                        // If Shift is Down, Apply Delta to all Cells in this Column
                        foreach (DataViewRowCell Cell in DataView.GetColumn(Index))
                        {
                            if (Cell is DataViewRowCell_Data DataCell)
                            {
                                // We don't want Cycling when Modifying Rows.
                                DataCell.AddDelta(Delta, false);
                            }
                        }

                        return true;
                    }
                    else
                    {
                        // If Alt is down, set Min or Max of all Cells in this Column
                        foreach (DataViewRowCell Cell in DataView.GetColumn(Index))
                        {
                            if (Cell is DataViewRowCell_Data DataCell)
                            {
                                // If Delta is negative, set Min otherwise set Max.
                                if (Delta < 0)
                                {
                                    DataCell.Floor();
                                }
                                else
                                {
                                    DataCell.Ceiling();
                                }
                            }
                        }

                        return true;
                    }
                }
            }
            else
            {
                // Ctrl is used for copying values across Columns
                return true;
            }

            return false;
        }

        public void Header_OnClick(object Sender, EventArgs EventArgs)
        {
            if (!Header_DeltaPriority((Button)Sender, DecrementBy))
            {
                Header_Sort((Button)Sender);
            }
        }

        public void Header_OnClickRight(object Sender, EventArgs EventArgs)
        {
            if (!Header_DeltaPriority((Button)Sender, IncrementBy))
            {
                Header_Sort((Button)Sender);
            }
        }

        public void Header_OnMouseEnter(object Sender, EventArgs EventArgs)
        {
            // Modified from DataViewRowCell_Work.DoOnMouseEnter
            // Ctrl + MouseDown Copies the Previous Columns Priorities to this Column.
            if (WindowManager.IsCtrlDown() && WindowManager.IsMouseDownCurrently())
            {
                // Find the last Header Cell we were MouseOver.
                IEnumerable<Panel> Header = DataView.Header.Where((F) => (F.Data != null));

                // MaxBy doesn't like Empty Lists.
                if (Header.Any())
                {
                    // MaxBy will give us the most recent MouseOver header.
                    Panel LastCell = Header.MaxBy((F) => (float)F.Data);

                    if ((LastCell != null) && ((Time.time - (float)LastCell.Data) < LastMouseOverTime_Threshold))
                    {
                        int X = DataView.GetHeaderIndexByID(LastCell.ID);

                        List<DataViewRowCell_Data> Cells = DataView.GetColumn(DataView.GetHeaderIndexByID(((Panel)Sender).ID)).Cast<DataViewRowCell_Data>().ToList();

                        for (int Y = 0; Y < Cells.Count; Y++)
                        {
                            Cells[Y].Copy((DataViewRowCell_Data)DataView.GetCell(X, Y));
                        }
                    }
                }
            }
        }

        public void Header_OnMouseWheel(object Sender, MouseEventArgs EventArgs)
        {
            Header_DeltaPriority((Button)Sender, (int)EventArgs.Delta);
        }

        public void Header_OnWhileMouseOver(object Sender, EventArgs EventArgs)
        {
            if (WindowManager.IsCtrlDown() && WindowManager.IsMouseDownCurrently())
            {
                // Modified from DataViewRowCell_Work.DoOnWhileMouseOver
                // DoOnMouseLeave does not fire in a way we can use for this behavior.
                ((Panel)Sender).Data = Time.time;
            }
        }

        #endregion "Cells & Headers"
    }
}
