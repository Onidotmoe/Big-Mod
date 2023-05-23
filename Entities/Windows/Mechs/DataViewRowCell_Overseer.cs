using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Mechs
{
    public class DataViewRowCell_Overseer : Animals.DataViewRowCell_Master
    {
        public override Pawn Master
        {
            get
            {
                return Pawn.GetOverseer();
            }
            set
            {
                if ((value == null) && ((Master != null) || (value != Master)))
                {
                    MechanitorUtility.ForceDisconnectMechFromOverseer(Pawn);
                    Pull();
                    UpdateSiblings();
                }

                if ((value != null) && (Master == null))
                {
                    value.relations.AddDirectRelation(PawnRelationDefOf.Overseer, Pawn);

                    // Update the DropDown.ListView
                    Pull();
                    UpdateSiblings();
                }
            }
        }

        public DataViewRowCell_Overseer(Pawn Pawn) : base(Pawn)
        {
        }

        public override bool CanAssign()
        {
            return true;
        }

        public override void Pull()
        {
            ToolTipText = "RightClickToOpenContextMenu".Translate();

            if (Master == null)
            {
                RemoveItem();
            }
            else
            {
                AddItem();
            }
        }

        public override void DropDown_OnClickRight(object Sender, MouseEventArgs MouseEventArgs)
        {
            // DropDown should only be updated when it's being expanded.

            DropDown.ListView.Clear();

            // Modified from TrainableUtility.MasterSelectButton_GenerateMenu
            foreach (Pawn Colonist in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                ListViewItem_Pawn Colonist_Item = new ListViewItem_Pawn(Colonist);
                Colonist_Item.Display = true;

                AcceptanceReport Report =  MechanitorUtility.CanControlMech(Colonist, Pawn);

                Colonist_Item.ToolTipText = Report.Reason;

                if (!Report || (Colonist.mechanitor?.CanOverseeSubject(Pawn) == false))
                {
                    Colonist_Item.IsLocked = true;
                    Colonist_Item.Selectable = false;
                }
                else
                {
                    // Modified from MechanitorBandwidthGizmo.GizmoOnGUI
                    Colonist_Item.ToolTipText += "Bandwidth".Translate().Colorize(ColoredText.TipSectionTitleColor) + " : " + (Colonist.mechanitor.UsedBandwidth.ToString("F0") + " / " + Colonist.mechanitor.TotalBandwidth.ToString("F0"));
                }

                DropDown.AddItem(Colonist_Item);
            }

            DropDown.ListView.Items.Sort((ListViewItem A, ListViewItem B) =>
            {
                int Result = (-B.IsLocked.CompareTo(A.IsLocked) * 2);

                Pawn Pawn_A = ((ListViewItem_Pawn)A).Pawn;
                Pawn Pawn_B = ((ListViewItem_Pawn)B).Pawn;

                if (MechanitorUtility.IsMechanitor(Pawn_A) && MechanitorUtility.IsMechanitor(Pawn_B))
                {
                    Result += Pawn_A.mechanitor.TotalBandwidth.CompareTo(Pawn_B.mechanitor.TotalBandwidth);
                }

                return Result;
            });

            DropDown.ListView.UpdatePositions();

            // Place it at the Top
            DropDown.InsertItem(0, new ListViewItem("NoneLower".TranslateSimple()));
        }

        public void UpdateSiblings()
        {
            // These have to be updated whenever the Overseer changes.
            DataViewRow.Cells.OfType<DataViewRowCell_ControlGroup>().ToList().ForEach((F) => F.Pull());
            DataViewRow.Cells.OfType<DataViewRowCell_WorkMode>().ToList().ForEach((F) => F.Pull());
            // Needs to update its Bandwidth usage.
            DataViewRow.Cells.OfType<DataViewRowCell_Overseer>().ToList().ForEach((F) => F.Pull());
        }

        public override void AddItem_ToolTip()
        {
            Item.ToolTipText = "Bandwidth".Translate().Colorize(ColoredText.TipSectionTitleColor) + " : " + (Master.mechanitor.UsedBandwidth.ToString("F0") + " / " + Master.mechanitor.TotalBandwidth.ToString("F0"));
        }
    }
}
