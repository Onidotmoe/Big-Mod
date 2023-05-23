using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Mechs
{
    public class DataViewRowCell_ControlGroup : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public Button Button = new Button();
        public MechanitorControlGroup ControlGroup
        {
            get
            {
                return Pawn.GetMechControlGroup();
            }
        }

        public DataViewRowCell_ControlGroup(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Button.InheritParentSize = true;
            AddChild(Button);
        }

        public override void Pull()
        {
            if (Pawn.IsGestating())
            {
                Button.Text = "Gestating".Translate().Colorize(ColoredText.SubtleGrayColor);
                Button.IsLocked = true;
                Button.IsVisible = true;
            }
            else if (Pawn.GetOverseer() != null)
            {
                Button.IsLocked = false;
                Button.IsVisible = true;

                Button.Text = ControlGroup.Index.ToString();
            }
            else
            {
                Button.IsVisible = false;
            }

            // Update all WorkMode Cells to reflect the Group they're now in.
            DataViewRow.Cells.OfType<DataViewRowCell_WorkMode>().ToList().ForEach((F) => F.Pull());
        }

        /// <summary>
        /// <para>Tries to Assign the specified ControlGroup to the Pawn. Will not Assign if the new Group is the same as the existing ControlGroup.</para>
        /// <para>Checks for <see cref="Pawn.IsGestating"/> and <see cref="Pawn.GetOverseer"/>.</para>
        /// </summary>
        /// <param name="Group">New Group to Assign.</param>
        /// <returns>True if Group is not the same as the current ControlGroup.</returns>
        public bool TryAssign(MechanitorControlGroup Group)
        {
            if ((Group != null) && (Group != ControlGroup) && !Pawn.IsGestating() && (Pawn.GetOverseer() != null))
            {
                Group.Assign(Pawn);
                DoOnDataChanged();
                return true;
            }

            return false;
        }

        public override void Copy(DataViewRowCell_Data Cell)
        {
            TryAssign(((DataViewRowCell_ControlGroup)Cell).ControlGroup);
        }

        public override void Floor()
        {
            TryAssign(Pawn.GetOverseer()?.mechanitor.controlGroups.FirstOrDefault());
        }

        public override void Ceiling()
        {
            TryAssign(Pawn.GetOverseer()?.mechanitor.controlGroups.LastOrDefault());
        }

        public override void AddDelta(int Delta, bool Cycling = true)
        {
            if (!Pawn.IsGestating() && (Pawn.GetOverseer() != null))
            {
                List<MechanitorControlGroup> ControlGroups = Pawn.GetOverseer().mechanitor.controlGroups;
                Delta = (ControlGroups.IndexOf(ControlGroup) + Delta);

                if (Cycling)
                {
                    if (Delta < 0)
                    {
                        // Indexes are zero based
                        Delta = (ControlGroups.Count - 1);
                    }
                    else if (Delta > (ControlGroups.Count - 1))
                    {
                        Delta = 0;
                    }
                }
                else
                {
                    Delta = Mathf.Clamp(Delta, 0, (ControlGroups.Count - 1));
                }

                TryAssign(ControlGroups[Delta]);
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return ControlGroup.Index.CompareTo(((DataViewRowCell_ControlGroup)Other).ControlGroup.Index);
        }

        public override void DoOnDataChanged()
        {
            base.DoOnDataChanged();

            Pull();
        }
    }
}
