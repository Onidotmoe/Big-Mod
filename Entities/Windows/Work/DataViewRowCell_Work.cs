using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Work
{
    public class DataViewRowCell_Work : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public WorkTypeDef WorkDef;

        public Work Work;

        public Button Button = new Button();
        public bool CanWork;

        public ProgressBar Skill;
        public Image Passion;
        private int _Priority;
        public int Priority
        {
            get
            {
                return _Priority;
            }
            set
            {
                if ((_Priority != value) && CanWork)
                {
                    _Priority = value;
                    Button.Text = Priority.ToString();

                    if (_Priority == Work.Floor_Value)
                    {
                        Button.Label.IsVisible = false;
                    }
                    else
                    {
                        Button.Label.Style.TextColor = Globals.TryGetColor(("DataViewRowCell_Work.PriorityColor." + _Priority.ToString()), "DataViewRowCell_Work.PriorityColor.Fallback");
                        Button.Label.IsVisible = true;
                    }

                    DoOnDataChanged();
                }
            }
        }

        public DataViewRowCell_Work(Work Work, Pawn Pawn, WorkTypeDef WorkDef)
        {
            this.Work = Work;
            this.Pawn = Pawn;
            this.WorkDef = WorkDef;

            ID = WorkDef.defName;

            Skill = new ProgressBar();
            Skill.SetStyle("DataViewRowCell_Work.ProgressBar");
            Skill.InheritParentSize = true;
            Skill.DoRequest = false;
            Skill.UseThresholds = true;
            Skill.Thresholds = new Dictionary<float, Color>();
            Skill.AddThresholds("DataViewRowCell_Work.ProgressBar.Thresholds.");
            Skill.Percentage = (Pawn.skills.AverageOfRelevantSkillsFor(WorkDef) / SkillRecord.MaxLevel);
            AddChild(Skill);

            // MouseOver is drawn by the Cell instead.
            Button.Style.DrawMouseOver = false;
            Button.InheritParentSize = true;
            AddChild(Button);

            Passion PassionLevel = Pawn.skills.MaxPassionOfRelevantSkillsFor(WorkDef);

            if (PassionLevel != RimWorld.Passion.None)
            {
                Passion = new Image();
                Passion.Size = new Vector2(10f, 10f);
                Passion.Anchor = Anchor.BottomRight;
                Passion.Style.Color = Globals.GetColor("DataViewRowCell_Work.Passion.Color");
                Passion.IgnoreMouse = false;

                // Keep here for reference :
                // Passion.ToolTipText = "PassionNone".Translate(0.35f.ToStringPercent("F0"));

                if (PassionLevel == RimWorld.Passion.Minor)
                {
                    Passion.ToolTipText = "PassionMinor".Translate(1f.ToStringPercent("F0"));
                    Passion.SetTexture(Globals.TryGetTexturePathFromAlias("PassionMinor"));
                }
                else if (PassionLevel == RimWorld.Passion.Major)
                {
                    Passion.ToolTipText = "PassionMajor".Translate(1.5f.ToStringPercent("F0"));
                    Passion.SetTexture(Globals.TryGetTexturePathFromAlias("PassionMajor"));
                }

                AddChild(Passion);
            }

            UpdateCanWork();

            if (!CanWork)
            {
                IsLocked = true;
                AddToolTipIcon();
            }

            Pull();
            UpdateToolTipText();
        }

        public void UpdateToolTipText()
        {
            ToolTipText = WidgetsWork.TipForPawnWorker(Pawn, WorkDef, CanWork);
        }

        public override void DrawToolTip()
        {
            if (IsVisible && IsMouseOver && !IgnoreMouse && !string.IsNullOrWhiteSpace(ToolTipText))
            {
                // Simple ID to update the existing ToolTip so it doesn't blink when Priority is updated.
                TooltipHandler.TipRegion(Bounds, new TipSignal(ToolTipText, 100));
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return Priority.CompareTo(((DataViewRowCell_Work)Other).Priority);
        }

        #region "DataViewRowCell_Data"

        public void UpdateCanWork()
        {
            if ((Pawn.workSettings == null) || !Pawn.workSettings.EverWork || Pawn.WorkTypeIsDisabled(WorkDef))
            {
                CanWork = false;
            }
            else
            {
                CanWork = true;
            }
        }

        public override void Pull()
        {
            Priority = Pawn.workSettings.GetPriority(WorkDef);
        }

        public override void Push()
        {
            Pawn.workSettings.SetPriority(WorkDef, Priority);
        }

        public override void Copy(DataViewRowCell_Data Host)
        {
            Priority = ((DataViewRowCell_Work)Host).Priority;
        }

        public override void Floor()
        {
            Priority = Work.Floor_Value;
        }

        public override void Ceiling()
        {
            Priority = Work.Ceiling_Value;
        }

        public override void AddDelta(int Delta, bool Cycling = true)
        {
            int PriorityDelta = (Priority + Delta);

            if (Cycling)
            {
                if (PriorityDelta < Work.Floor_Value)
                {
                    PriorityDelta = Work.Ceiling_Value;
                }
                else if (PriorityDelta > Work.Ceiling_Value)
                {
                    PriorityDelta = Work.Floor_Value;
                }
            }
            else
            {
                PriorityDelta = Mathf.Clamp(PriorityDelta, Work.Floor_Value, Work.Ceiling_Value);
            }

            Priority = PriorityDelta;
        }

        public override void DoOnDataChanged()
        {
            base.DoOnDataChanged();

            Push();
            UpdateToolTipText();
        }

        public override bool DoOnMouseEnter_Filter(DataViewRowCell_Data Cell)
        {
            return ((DataViewRowCell_Work)Cell).CanWork;
        }

        #endregion "DataViewRowCell_Data"
    }
}
