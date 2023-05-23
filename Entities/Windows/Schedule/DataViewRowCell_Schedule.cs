using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Schedule
{
    public class DataViewRowCell_Schedule : DataViewRowCell_Data
    {
        public Pawn Pawn;
        private TimeAssignmentDef _AssignmentDef;
        public TimeAssignmentDef AssignmentDef
        {
            get
            {
                return _AssignmentDef;
            }
            set
            {
                if (_AssignmentDef != value)
                {
                    _AssignmentDef = value;

                    Style.BackgroundColor = Globals.GetColor("Schedule.Button.TimeAssignmentDef." + _AssignmentDef.defName + ".BackgroundColor");

                    DoOnDataChanged();
                }
            }
        }

        public int Hour;
        public bool IsNightOwl;
        public static int NightOwl_Start = 23;
        public static int NightOwl_Stop = 5;

        public Schedule Schedule;

        public static TimeAssignmentDef Value_Floor = DefDatabase<TimeAssignmentDef>.AllDefs.First();
        public static TimeAssignmentDef Value_Ceiling = DefDatabase<TimeAssignmentDef>.AllDefs.Last();

        // TODO: needs icons for assignment types
        public DataViewRowCell_Schedule(Schedule Schedule, int Hour, Pawn Pawn)
        {
            this.Schedule = Schedule;
            this.Hour = Hour;
            this.Pawn = Pawn;

            ID = Hour.ToString();
            Style.DrawBackground = true;

            HandleNightOwl();
        }

        public void HandleNightOwl()
        {
            if (HasTraitNightOwl(Pawn))
            {
                if ((Hour >= NightOwl_Start) || (Hour <= NightOwl_Stop))
                {
                    IsNightOwl = true;

                    Image Image = new Image(Globals.TryGetTexturePathFromAlias("NightOwl"));
                    Image.SetStyle("DataViewRowCell_Schedule.NightOwl");
                    Image.Size = new Vector2(10f, 10f);
                    Image.Anchor = Anchor.BottomRight;
                    Image.IgnoreMouse = false;
                    Image.ToolTipText = TraitDef.Named("NightOwl").degreeDatas.First().description.Formatted(Pawn.Named("PAWN"));
                    AddChild(Image);
                }
            }
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            // Modifiers should be pass on like normal.
            if ((Schedule.ActiveAssignmentDef != null) && !WindowManager.IsCtrlDown() && !WindowManager.IsShiftDown())
            {
                // Apply the Brush if it's active.
                if (AssignmentDef != Schedule.ActiveAssignmentDef)
                {
                    AssignmentDef = Schedule.ActiveAssignmentDef;
                }
                else
                {
                    // If we're already the Brush value, then clear the field by Flooring it.
                    Floor();
                }
            }
            else
            {
                base.DoOnClick(Sender, EventArgs);
            }
        }

        public override void DoOnMouseEnter(object Sender, MouseEventArgs EventArgs)
        {
            // Allow the Brush to paint without needing to click again.
            if ((Schedule.ActiveAssignmentDef != null) && WindowManager.IsMouseDownCurrently() && !WindowManager.IsCtrlDown() && !WindowManager.IsShiftDown())
            {
                if (!WindowManager.IsMouseDownRightCurrently())
                {
                    // Apply the Brush if it's active.
                    if (AssignmentDef != Schedule.ActiveAssignmentDef)
                    {
                        AssignmentDef = Schedule.ActiveAssignmentDef;
                    }
                }
                else
                {
                    // Dragging with the Brush and holding down Right-Mouse clears the field by Flooring it.
                    Floor();
                }
            }
            else
            {
                base.DoOnMouseEnter(Sender, EventArgs);
            }
        }

        public override void DoOnClickRight(object Sender, MouseEventArgs EventArgs)
        {
            // If there's any active Brush, Right-Clicking will always clear the field by Flooring it.
            if (Schedule.ActiveAssignmentDef != null)
            {
                Floor();
            }
            else
            {
                base.DoOnClickRight(Sender, EventArgs);
            }
        }

        public static bool HasTraitNightOwl(Pawn Pawn)
        {
            return Pawn.story.traits.HasTrait(TraitDef.Named("NightOwl"));
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return AssignmentDef.label.CompareTo(((DataViewRowCell_Schedule)Other).AssignmentDef.label);
        }

        #region "DataViewRowCell_Data"

        public override void Pull()
        {
            AssignmentDef = Pawn.timetable.GetAssignment(Hour);
        }

        public override void Push()
        {
            Pawn.timetable.SetAssignment(Hour, AssignmentDef);
        }

        public override void Copy(DataViewRowCell_Data Host)
        {
            AssignmentDef = ((DataViewRowCell_Schedule)Host).AssignmentDef;
        }

        public override void Floor()
        {
            AssignmentDef = Value_Floor;
        }

        public override void Ceiling()
        {
            AssignmentDef = Value_Ceiling;
        }

        public override void AddDelta(int Delta, bool Cycling = true)
        {
            List<TimeAssignmentDef> TimeAssignmentDefs = DefDatabase<TimeAssignmentDef>.AllDefs.ToList();

            int AssignmentDelta = (TimeAssignmentDefs.IndexOf(AssignmentDef) + Delta);
            int Assignment_Min = TimeAssignmentDefs.IndexOf(Value_Floor);
            int Assignment_Max = TimeAssignmentDefs.IndexOf(Value_Ceiling);

            if (Cycling)
            {
                if (AssignmentDelta < Assignment_Min)
                {
                    AssignmentDelta = Assignment_Max;
                }
                else if (AssignmentDelta > Assignment_Max)
                {
                    AssignmentDelta = Assignment_Min;
                }
            }
            else
            {
                AssignmentDelta = Mathf.Clamp(AssignmentDelta, Assignment_Min, Assignment_Max);
            }

            AssignmentDef = TimeAssignmentDefs[AssignmentDelta];
        }

        public override void DoOnDataChanged()
        {
            base.DoOnDataChanged();

            Push();
        }

        #endregion "DataViewRowCell_Data"
    }
}
