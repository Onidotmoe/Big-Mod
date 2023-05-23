using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Schedule
{
    public class Schedule : DataPawnWindow_Area
    {
        public TimeAssignmentDef ActiveAssignmentDef;
        public static int HoursInADay = 24;
        public List<Button> Brushes = new List<Button>();
        public ToolTip ToolTip;
        public Label ToolTip_Label;

        public Schedule()
        {
            Identifier = "Schedule";

            List<TimeAssignmentDef> AssignmentDefs = DefDatabase<TimeAssignmentDef>.AllDefs.ToList();

            for (int i = 0; i < AssignmentDefs.Count; i++)
            {
                TimeAssignmentDef TimeDef = AssignmentDefs[i];

                Button Button = new Button(TimeDef.LabelCap);
                Button.SetStyle("Schedule.Button.TimeAssignmentDef." + TimeDef.defName);
                Button.ColorToggleOff = Globals.TryGetColor("Schedule.Button.TimeAssignmentDef." + TimeDef.defName + ".BackgroundColor", "Schedule.Button.TimeAssignmentDef.Fallback.BackgroundColor");
                Button.Style.BackgroundColor = Button.ColorToggleOff;
                Button.Anchor = Anchor.BottomCenter;
                Button.Style.DrawBackground = true;
                Button.Size = new Vector2(80f, (DataView.CellHeight - 2f));
                Button.Offset = new Vector2(((Button.Width * i) - (Button.Width * (AssignmentDefs.Count - 1) / 2) + 5f), 0f);
                Button.ID = TimeDef.LabelCap;
                Button.Data = TimeDef;
                Button.CanToggle = true;

                Button.OnToggleStateChanged += (obj, e) =>
                {
                    if (Button.ToggleState)
                    {
                        foreach (Button Brush in Brushes)
                        {
                            if (Brush != Button)
                            {
                                Brush.ToggleState = false;
                            }
                        }

                        ActiveAssignmentDef = (TimeAssignmentDef)Button.Data;
                        ToggleBrush(true);
                    }
                    else
                    {
                        // Toggles Painting off. Cell's default behavior is cycle through the different AssignmentDefs.
                        ActiveAssignmentDef = null;
                        ToggleBrush(false);
                    }

                    Button.IsSelected = Button.ToggleState;
                };

                Root.AddChild(Button);
                Brushes.Add(Button);
            }

            ToolTip = new ToolTip();
            ToolTip.ToolTipHost = Root;
            ToolTip.Size = new Vector2(80f, (DataView.CellHeight - 2f));
            ToolTip.Offset = new Vector2(7f, 12f);
            ToolTip_Label = new Label();
            ToolTip_Label.Size = (ToolTip.Size - Vector2.one);
            ToolTip.Root.AddChild(ToolTip_Label);

            Root.OnMouseEnter += DoOnMouseEnter;
            Root.OnMouseLeave += DoOnMouseLeave;
        }

        public void DoOnMouseEnter(object Sender, EventArgs EventArgs)
        {
            ToggleBrush(ActiveAssignmentDef != null);
        }

        public void DoOnMouseLeave(object Sender, EventArgs EventArgs)
        {
            ToggleBrush(false);
        }

        public void ToggleBrush(bool Toggle = true)
        {
            if (Toggle)
            {
                ToolTip_Label.Text = ActiveAssignmentDef.LabelCap;
                ToolTip.Root.Style.BackgroundColor = ActiveAssignmentDef.color.ToTransparent(0.25f);

                if (!WindowManager.Instance.ToolTipExists(ToolTip))
                {
                    WindowManager.Instance.AddToolTip(ToolTip);
                }
            }
            else
            {
                if (WindowManager.Instance.ToolTipExists(ToolTip))
                {
                    WindowManager.Instance.RemoveToolTip(ToolTip);
                }
            }
        }

        public override void PreClose()
        {
            ToggleBrush(false);
        }

        public override void InitiateHeaders()
        {
            base.InitiateHeaders();

            for (int i = 0; i < HoursInADay; i++)
            {
                Button Header = new Button(i.ToString());
                Header.SetStyle("Schedule.Header");
                Header.Style.DrawBackground = true;
                Header.ID = i.ToString();
                Header.Size = new Vector2(25f, DataView.HeaderHeight);
                Header.Label.Style.FontType = GameFont.Small;
                Header_Register(Header);
                Header.CanToggle = true;

                // Add a Icon that indicates it's inside the NightOwl prefered time and also that it's night.
                if ((i >= DataViewRowCell_Schedule.NightOwl_Start) || (i <= DataViewRowCell_Schedule.NightOwl_Stop))
                {
                    Image Image = new Image(Globals.TryGetTexturePathFromAlias("NightTime"));
                    Image.SetStyle("Schedule.Header.NightTime");
                    Image.Size = new Vector2(10f, 10f);
                    Image.Anchor = Anchor.BottomRight;
                    Image.IgnoreMouse = false;
                    Image.ToolTipText = "NightTime_Description".Translate();

                    Header.AddChild(Image);
                }

                DataView.AddColumn(Header);
            }

            InitiateHeaders_Areas();
        }

        public override void Populate()
        {
            base.Populate();

            IEnumerable<Area> Areas = DataView.Header.Where((F) => (F.Data is Area)).Select((F) => (Area)F.Data);

            foreach (DataViewRow Row in DataView.Rows)
            {
                ListViewItem_Pawn Item = (ListViewItem_Pawn)Row.Cells[0].GetChildWithID("Pawn");

                for (int i = 0; i < HoursInADay; i++)
                {
                    Row.ReplaceCell(DataView.GetHeaderIndexByID(i.ToString()), new DataViewRowCell_Schedule(this, i, Item.Pawn));
                }
            }

            Populate_Areas();
        }
    }
}
