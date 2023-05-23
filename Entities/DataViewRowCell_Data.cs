using Verse;

namespace BigMod.Entities
{
    public class DataViewRowCell_Data : DataViewRowCell, IComparable<DataViewRowCell_Data>
    {
        /// <summary>
        /// Used to determine which Cell was Last Dragged over when Copying Assignment across Cells.
        /// </summary>
        public float LastMouseOverTime { get; set; }

        /// <summary>
        /// Time before the <see cref="LastMouseOverTime"/> becomes invalid.
        /// </summary>
        public float LastMouseOverTime_Threshold { get; set; } = 3f;

        /// <summary>
        /// Additional ToolTip information icon.
        /// </summary>
        public Image ToolTipIcon;
        /// <summary>
        /// Filter used in <see cref="DoOnMouseEnter_Filter"/>
        /// </summary>
        public Func<DataViewRowCell_Data, bool> DelegateDoOnMouseEnter_Filter;
        public virtual Comparer<DataViewRowCell_Data> Comparator { get; set; }

        public DataViewRowCell_Data()
        {
            DelegateDoOnMouseEnter_Filter = DoOnMouseEnter_Filter;
        }

        public event EventHandler OnDataChanged;
        public int IncrementBy = 1;
        public int DecrementBy = -1;

        public virtual void Pull()
        {
        }

        public virtual void Push()
        {
        }

        public virtual void Copy(DataViewRowCell_Data Cell)
        {
        }

        public virtual void Floor()
        {
        }

        public virtual void Ceiling()
        {
        }

        public virtual void AddDelta(int Delta, bool Cycling = true)
        {
        }

        public virtual void DoOnDataChanged()
        {
            OnDataChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual int CompareTo(DataViewRowCell_Data Other)
        {
            return ((Comparator != null) ? Comparator.Compare(this, Other) : 0);
        }

        public void AddToolTipIcon()
        {
            if (ToolTipIcon == null)
            {
                ToolTipIcon = new Image(Globals.TryGetTexturePathFromAlias("Locked"), (9f * Prefs.UIScale), (9f * Prefs.UIScale));
                ToolTipIcon.Style.Color = Globals.GetColor("ListViewItemGroup_Architect.ToolTipIcon.Color");
                ToolTipIcon.Anchor = Anchor.BottomRight;
                ToolTipIcon.Offset = new Vector2(-1f, 0f);
                AddChild(ToolTipIcon);
            }
        }

        public void RemoveToolTipIcon()
        {
            if (ToolTipIcon != null)
            {
                RemoveChild(ToolTipIcon);
                ToolTipIcon = null;
            }
        }

        #region "Callbacks & Modifiers"

        public override void DoOnDataViewRowAdded()
        {
            base.DoOnDataViewRowAdded();

            Pull();
        }

        /// <summary>
        /// Handles all modifiers for applying Delta.
        /// </summary>
        /// <param name="Delta">Delta to add to all Cells in the same Row as this Cell.</param>
        /// <returns>True if Handled.</returns>
        public virtual bool HandleModifiers(int Delta)
        {
            if (!WindowManager.IsCtrlDown())
            {
                if (WindowManager.IsShiftDown())
                {
                    if (!WindowManager.IsAltDown())
                    {
                        // If Shift is Down, Apply Delta to all Cells in this Row
                        foreach (DataViewRowCell Cell in DataViewRow.Cells)
                        {
                            // This Cell has already been handled.
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
                        // If Alt is down, set Min or Max of all Cells in this Row
                        foreach (DataViewRowCell Cell in DataViewRow.Cells)
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
                // Ctrl is used for copying a value across Cells
                return true;
            }

            return false;
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClick(Sender, EventArgs);

            if (!HandleModifiers(DecrementBy))
            {
                AddDelta(DecrementBy);
            }
        }

        public override void DoOnClickRight(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClickRight(Sender, EventArgs);

            if (!HandleModifiers(IncrementBy))
            {
                AddDelta(IncrementBy);
            }
        }

        public override void DoOnMouseWheel(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnMouseWheel(Sender, EventArgs);

            if (!HandleModifiers((int)EventArgs.Delta))
            {
                AddDelta((int)EventArgs.Delta);
            }
        }

        public override void DoOnWhileMouseOver(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnWhileMouseOver(Sender, EventArgs);

            if (WindowManager.IsCtrlDown() && WindowManager.IsMouseDownCurrently())
            {
                // DoOnMouseLeave does not fire in a way we can use for this behavior.
                LastMouseOverTime = Time.time;
            }
        }

        public override void DoOnMouseEnter(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnMouseEnter(Sender, EventArgs);

            // Ctrl + MouseDown Copies the Previous Cell's value to this Cell.
            if (WindowManager.IsCtrlDown() && WindowManager.IsMouseDownCurrently())
            {
                // Find the Last Cell we were MouseOver, does not matter if it was in the same Row.
                // Last Cell has to be pass the filter.
                // MaxBy will give us the most recent MouseOver Cell.
                IEnumerable<DataViewRowCell_Data> Cells = DataView.Cells.OfType<DataViewRowCell_Data>().Where((F) => DoOnMouseEnter_Filter(F));

                // MaxBy doesn't like Empty Lists.
                if (Cells.Any())
                {
                    DataViewRowCell_Data LastCell = Cells.MaxBy((F) => F.LastMouseOverTime);

                    if ((LastCell != null) && ((Time.time - LastCell.LastMouseOverTime) < LastCell.LastMouseOverTime_Threshold))
                    {
                        if (!WindowManager.IsShiftDown())
                        {
                            Copy(LastCell);
                        }
                        else
                        {
                            IEnumerable<DataViewRowCell_Data> LastCells = LastCell.DataViewRow.Cells.OfType<DataViewRowCell_Data>();
                            IEnumerable<DataViewRowCell_Data> RowCells = DataViewRow.Cells.OfType<DataViewRowCell_Data>();

                            // If Shift is down copy LastCell's Row settings to this Cell's Row.
                            foreach (DataViewRowCell_Data Last in LastCells)
                            {
                                foreach (DataViewRowCell_Data Cell in RowCells)
                                {
                                    if (Last.Index == Cell.Index)
                                    {
                                        Cell.Copy(Last);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual bool DoOnMouseEnter_Filter(DataViewRowCell_Data Cell)
        {
            return true;
        }
    }

    #endregion "Callbacks & Modifiers"
}
