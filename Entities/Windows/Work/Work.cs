using RimWorld;
using System.Text;
using Verse;

namespace BigMod.Entities.Windows.Work
{
    public class Work : DataPawnWindow
    {
        public int Floor_Value = 0;
        public int Ceiling_Value = 4;

        // TODO: DataView needs tooltip for telling the user about the modifiers
        // TODO: needs icons
        public Work()
        {
            Identifier = "Window_Work";

            AddButtonCloseX();
            AddButtonResize();
        }

        public override void InitiateHeaders()
        {
            base.InitiateHeaders();

            foreach (WorkTypeDef WorkDef in DefDatabase<WorkTypeDef>.AllDefs)
            {
                Button Header = new Button(WorkDef.labelShort.CapitalizeFirst());
                Header.SetStyle("Work.Header");
                Header.Style.DrawBackground = true;
                Header.ID = WorkDef.defName;
                Header.Size = new Vector2((Verse.Text.CalcSize(Header.Text).x + (12f * Prefs.UIScale)), DataView.HeaderHeight);
                Header.Label.Style.FontType = GameFont.Small;
                Header_Register(Header);

                Header.ToolTipText = (WorkDef.gerundLabel.CapitalizeFirst() + Environment.NewLine + Environment.NewLine + WorkDef.description + Environment.NewLine + Environment.NewLine + SpecificWorkListString(WorkDef));

                DataView.AddColumn(Header);
            }
        }

        public override void Populate()
        {
            base.Populate();

            foreach (DataViewRow Row in DataView.Rows)
            {
                ListViewItem_Pawn Item = (ListViewItem_Pawn)Row.Cells[0].GetChildWithID("Pawn");

                foreach (WorkTypeDef WorkDef in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    Row.ReplaceCell(DataView.GetHeaderIndexByID(WorkDef.defName), new DataViewRowCell_Work(this, Item.Pawn, WorkDef));
                }
            }
        }

        /// <summary>
        /// Copied from PawnColumnWorker_WorkPriority.SpecificWorkListString because it's private.
        /// </summary>
        public static string SpecificWorkListString(WorkTypeDef def)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < def.workGiversByPriority.Count; i++)
            {
                stringBuilder.Append(def.workGiversByPriority[i].LabelCap);
                if (def.workGiversByPriority[i].emergency)
                {
                    stringBuilder.Append(" (" + "EmergencyWorkMarker".Translate() + ")");
                }
                if (i < def.workGiversByPriority.Count - 1)
                {
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }

        public override void Header_Sort(Button Button)
        {
            Button.ToggleState = !Button.ToggleState;

            int X = DataView.GetHeaderIndexByID(Button.ID);

            if (!WindowManager.IsAltDown())
            {
                if (Button.ToggleState)
                {
                    DataView.Rows = DataView.Rows.OrderBy((F) => (DataViewRowCell_Data)F.Cells[X]).ToList();
                }
                else
                {
                    DataView.Rows = DataView.Rows.OrderByDescending((F) => (DataViewRowCell_Data)F.Cells[X]).ToList();
                }
            }
            else
            {
                // Alternative Sorting behavior that Sorts based on Pawn's average Skill for the Work instead of by Priority set.
                if (Button.ToggleState)
                {
                    DataView.Rows = DataView.Rows.OrderBy((F) => ((ListViewItem_Pawn)F.Cells[0].GetChildWithID("Pawn")).Pawn.skills.AverageOfRelevantSkillsFor(((DataViewRowCell_Work)F.Cells[X]).WorkDef)).ToList();
                }
                else
                {
                    DataView.Rows = DataView.Rows.OrderByDescending((F) => ((ListViewItem_Pawn)F.Cells[0].GetChildWithID("Pawn")).Pawn.skills.AverageOfRelevantSkillsFor(((DataViewRowCell_Work)F.Cells[X]).WorkDef)).ToList();
                }
            }

            DataView.UpdatePositions();
        }
    }
}
