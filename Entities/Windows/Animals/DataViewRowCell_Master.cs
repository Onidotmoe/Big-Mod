using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Animals
{
    public class DataViewRowCell_Master : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public DropDown DropDown = new DropDown();
        /// <summary>
        /// Master Pawn Item.
        /// </summary>
        public DataViewRowCell_Pawn Item;
        public virtual Pawn Master
        {
            get
            {
                return Pawn.playerSettings?.Master;
            }
            set
            {
                if (Pawn.playerSettings != null)
                {
                    Pawn.playerSettings.Master = value;
                }
            }
        }

        public DataViewRowCell_Master(Pawn Pawn)
        {
            this.Pawn = Pawn;

            DropDown.SetStyle(GetType().Name + ".DropDown");
            // MouseOver is drawn by the Cell instead.
            DropDown.Style.DrawMouseOver = false;
            DropDown.Style.DrawBackground = true;
            DropDown.InheritParentSize = true;
            DropDown.IsVisible = CanAssign();
            DropDown.OnClickedItem += DoOnClickedItem;
            DropDown.CanToggle = false;
            // Use Right-Click to open the menu instead.
            DropDown.OnClickRight += (obj, e) =>
            {
                DropDown_OnClickRight(obj, e);
                DropDown.Toggle();
            };
            AddChild(DropDown);

            if (!DropDown.IsVisible)
            {
                AddToolTipIcon();
            }
        }

        public virtual bool CanAssign()
        {
            // Copied from PawnColumnWorker_Master.CanAssignMaster
            return (Pawn.RaceProps.Animal && (Pawn.Faction == Faction.OfPlayer) && Pawn.training.HasLearned(TrainableDefOf.Obedience));
        }

        public override void Pull()
        {
            if (!CanAssign())
            {
                return;
            }

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

        public virtual void DropDown_OnClickRight(object Sender, MouseEventArgs MouseEventArgs)
        {
            // DropDown should only be updated when it's being expanded.

            // Update ToolTip Position again before opening it.
            DropDown.ToolTip_Offset = (DataViewRow.Position - DataView.ScrollPosition);

            // Clear Items before generating new ones.
            DropDown.ListView.Clear();

            // Modified from TrainableUtility.MasterSelectButton_GenerateMenu
            foreach (Pawn Colonist in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                ListViewItem_Pawn Colonist_Item = new ListViewItem_Pawn(Colonist);
                Colonist_Item.Display = true;

                Colonist_Item.ToolTipText = RelationsUtility.LabelWithBondInfo(Colonist, Pawn);

                int Level = Colonist.skills.GetSkill(SkillDefOf.Animals).Level;

                if (!TrainableUtility.CanBeMaster(Colonist, Pawn, true))
                {
                    Colonist_Item.IsLocked = true;
                    Colonist_Item.Selectable = false;

                    int Minimum = TrainableUtility.MinimumHandlingSkill(Pawn);

                    if (Level < Minimum)
                    {
                        Colonist_Item.ToolTipText += $" ({"SkillTooLow".Translate(SkillDefOf.Animals.LabelCap, Level, Minimum)})";
                    }
                }
                else
                {
                    Colonist_Item.ToolTipText += $" ({SkillDefOf.Animals.LabelCap} {"Level".Translate()} : {Level})";
                }

                DropDown.AddItem(Colonist_Item);
            }

            DropDown.ListView.Items.Sort((A, B) => (-B.IsLocked.CompareTo(A.IsLocked) * 2) + ((ListViewItem_Pawn)B).Pawn.skills.GetSkill(SkillDefOf.Animals).Level.CompareTo(((ListViewItem_Pawn)A).Pawn.skills.GetSkill(SkillDefOf.Animals).Level));
            DropDown.ListView.UpdatePositions();

            // Place it at the Top
            DropDown.InsertItem(0, new ListViewItem("NoneLower".TranslateSimple()));
        }

        public void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            if (Sender is ListViewItem_Pawn Item)
            {
                Master = Item.Pawn;
                AddItem();
            }
            else
            {
                Master = null;
                RemoveItem();
            }

            DoOnDataChanged();
        }

        public void AddItem()
        {
            if ((Item != null) && (Item.Pawn != Master))
            {
                // Remove mismatching item.
                RemoveItem();
            }

            if (Item == null)
            {
                Item = new DataViewRowCell_Pawn(Master);
                Item.InheritParentSize = true;
                Item.InheritParentPosition = true;
                Item.Item.Display = true;
                // Behaves differently when it's not replacing a Cell.
                Item.Item.Bounds = Bounds;
                Item.Item.Offset = Offset;
                Item.UseAnchoring = true;
                AddChild(Item);

                AddItem_ToolTip();
            }

            DropDown.Text = string.Empty;
        }

        public virtual void AddItem_ToolTip()
        {
            // Show Master Level while MouseOver.
            Item.ToolTipText += $"{SkillDefOf.Animals.LabelCap} {"Level".Translate()} : {Master.skills.GetSkill(SkillDefOf.Animals).Level}";
        }

        public void RemoveItem()
        {
            if (Item != null)
            {
                RemoveChild(Item);
                Item = null;
            }

            DropDown.Text = "NoneLower".TranslateSimple();
        }
        public override void DoOnDataViewAdded()
        {
            base.DoOnDataViewAdded();

            DataView.OnScrollPositionChanged += DoOnScrollPositionChanged;
        }

        public void DoOnScrollPositionChanged(object Sender, EventArgs EventArgs)
        {
            // Close when Scroll Position has changed
            DropDown.ToggleState = false;
            // Because DataViewRowCell is rendering inside a Group, some position breaks, so we have to manually adapt the behavior.
            DropDown.ToolTip_Offset = (DataViewRow.Position - DataView.ScrollPosition);
        }
        public override int CompareTo(DataViewRowCell_Data Other)
        {
            DataViewRowCell_Master OtherCell = (DataViewRowCell_Master)Other;

            if ((Item != null) && (OtherCell.Item != null))
            {
                return Item.CompareTo(OtherCell.Item);
            }
            if ((Item == null) && (OtherCell.Item != null))
            {
                return -1;
            }
            if ((Item != null) && (OtherCell.Item == null))
            {
                return 1;
            }

            return (CanAssign().CompareTo(OtherCell.CanAssign()));
        }
    }
}
