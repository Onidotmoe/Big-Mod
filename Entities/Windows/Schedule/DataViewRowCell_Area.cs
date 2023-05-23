using Verse;

namespace BigMod.Entities.Windows.Schedule
{
    public class DataViewRowCell_Area : DataViewRowCell_Data
    {
        public Area Area;
        private bool _Restricted;
        /// <summary>
        /// Restricted Area means that the Pawn is only allowed inside that Area and not in any other. If no restricted area is set, then the Pawn is unrestricted and can go anywhere.
        /// </summary>
        public bool Restricted
        {
            get
            {
                return _Restricted;
            }
            set
            {
                if (_Restricted != value)
                {
                    _Restricted = value;
                    Style.DrawBackground = _Restricted;

                    if (_Restricted)
                    {
                        Button.Label.Style.TextColor = Globals.GetColor("DataViewRowCell_Area.Header.Selected.TextColor");
                    }
                    else
                    {
                        Button.Label.Style.TextColor = Globals.GetColor("DataViewRowCell_Area.Header.Unselected.TextColor");
                    }

                    DoOnDataChanged();
                }
            }
        }

        public Button Button = new Button();
        public Pawn Pawn;

        public DataViewRowCell_Area(Pawn Pawn, Area Area)
        {
            this.Pawn = Pawn;
            this.Area = Area;

            ID = Area.Label;
            Style.BackgroundColor = Area.Color;
            // MouseOver is drawn by the Cell instead.
            Button.Style.DrawMouseOver = false;
            Button.Label.Style.TextColor = Globals.GetColor("DataViewRowCell_Area.Header.Unselected.TextColor");
            Button.Label.Style.WordWrap = true;
            Button.Label.Style.FontType = GameFont.Tiny;
            Button.InheritParentSize = true;
            Button.Text = Area.Label;
            AddChild(Button);
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return ((-IsLocked.CompareTo(Other.IsLocked) * 2) + Restricted.CompareTo(((DataViewRowCell_Area)Other).Restricted));
        }

        #region "DataViewRowCell_Data"

        /// <summary>
        /// When all Cells are Unrestricted, the BackgroundColor of all Cells is the same.
        /// </summary>
        public void ToggleRestrictions(bool Toggle = true)
        {
            Panel Unrestricted = DataViewRow.GetChildWithID("Unrestricted");

            if (Unrestricted != null)
            {
                Unrestricted.IsVisible = !Toggle;
            }
        }

        public override void Pull()
        {
            Restricted = (Pawn.playerSettings.AreaRestriction == Area);
        }

        public override void Push()
        {
            // Sometimes Pawns can't have Areas assigned.
            if (IsLocked)
            {
                return;
            }

            IEnumerable<DataViewRowCell_Area> Cells = DataViewRow.Cells.OfType<DataViewRowCell_Area>();

            if (Restricted)
            {
                Pawn.playerSettings.AreaRestriction = Area;

                // Update all other Cells
                foreach (DataViewRowCell_Area Cell in Cells)
                {
                    if (Cell != this)
                    {
                        // Update the other Cells.
                        Cell.Pull();
                    }
                }
            }
            else if (Pawn.playerSettings.AreaRestriction == Area)
            {
                // Clear the restricted Area if it's this Area we just toggled.
                Pawn.playerSettings.AreaRestriction = null;
            }

            bool Toggle = Cells.All((F) => !F.Restricted);

            foreach (DataViewRowCell_Area Cell in Cells)
            {
                // If all Cells are unrestricted, then set the same Background Color to indicate that this Pawn isn't restricted to any areas.
                Cell.ToggleRestrictions(!Toggle);
            }
        }

        public override void Copy(DataViewRowCell_Data Cell)
        {
            if (!IsLocked)
            {
                Restricted = ((DataViewRowCell_Area)Cell).Restricted;
            }
        }

        public override void Floor()
        {
            if (!IsLocked)
            {
                Restricted = false;
            }
        }

        public override void Ceiling()
        {
            if (!IsLocked)
            {
                Restricted = true;
            }
        }

        public override void AddDelta(int Delta, bool Cycling = true)
        {
            if (IsLocked)
            {
                return;
            }

            // Only allow Cycling with MouseScrolling.
            if (Cycling && (WindowManager.MouseWheelDelta() != 0))
            {
                List<DataViewRowCell_Area> Cells = DataViewRow.Cells.OfType<DataViewRowCell_Area>().ToList();

                // Move the Restricted status with Delta.
                int X = (Cells.FirstIndexOf((F) => F.Restricted) + Delta);

                if (X > (Cells.Count - 1))
                {
                    // -1 is Unrestricted
                    X = -1;
                }
                else if (X <= -2)
                {
                    // -2 or less is when we try to go backwards when nothing is Restricted, loop to the Last Cell.
                    X = (Cells.Count - 1);
                }

                if ((X >= 0) && (X <= (Cells.Count - 1)))
                {
                    Cells[X].Ceiling();
                }
                else
                {
                    foreach (DataViewRowCell_Area Cell in Cells)
                    {
                        Cell.Floor();
                    }
                }
            }
            else
            {
                Restricted = (Delta < 0);
            }
        }

        public override void DoOnDataChanged()
        {
            base.DoOnDataChanged();

            Push();
        }

        /// <summary>
        /// The game currently doesn't support multiple restricted Areas at a time.
        /// </summary>
        public override bool HandleModifiers(int Delta)
        {
            if (!WindowManager.IsCtrlDown())
            {
                return false;
            }
            else
            {
                // Ctrl is used for copying a value across Cells
                return true;
            }
        }

        public override void DoOnMouseEnter(object Sender, MouseEventArgs EventArgs)
        {
            if (IsLocked)
            {
                return;
            }

            // Set Restricted if Mouse is already Down so we don't need to click again.
            if (WindowManager.IsMouseDownCurrently() && !WindowManager.IsCtrlDown() && !WindowManager.IsShiftDown())
            {
                if (!WindowManager.IsMouseDownRightCurrently())
                {
                    Ceiling();
                }
                else
                {
                    // Dragging and holding down Right-Mouse clears the field by Flooring it.
                    Floor();
                }
            }
            else
            {
                base.DoOnMouseEnter(Sender, EventArgs);
            }
        }

        #endregion "DataViewRowCell_Data"
    }
}
