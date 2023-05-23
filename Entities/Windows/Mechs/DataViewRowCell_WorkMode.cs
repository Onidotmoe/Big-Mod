using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Mechs
{
    public class DataViewRowCell_WorkMode : DataViewRowCell_Toggle
    {
        public Pawn Pawn;
        public MechWorkModeDef Def;

        public DataViewRowCell_WorkMode(Pawn Pawn, MechWorkModeDef Def)
        {
            this.Pawn = Pawn;
            this.Def = Def;

            Button.AddImage(Def.iconPath);

            Button.CanToggle = CanToggle();
        }

        public override void Pull()
        {
            MechanitorControlGroup ControlGroup = Pawn.GetMechControlGroup();

            if (ControlGroup != null)
            {
                Button.ToggleState = (Def == ControlGroup.WorkMode);
                Button.IsLocked = false;
            }
            else
            {
                Button.ToggleState = false;
                Button.IsLocked = true;
            }
        }

        public override void Push()
        {
            MechanitorControlGroup ControlGroup = Pawn.GetMechControlGroup();

            if (ControlGroup != null)
            {
                if (Button.ToggleState)
                {
                    ControlGroup.SetWorkMode(Def);
                    UpdateAll();
                }
                else if (ControlGroup.WorkMode == Def)
                {
                    // Prevent deselection.
                    Button.ToggleState = true;
                }
            }
        }

        /// <summary>
        /// <para>The game doesn't allow a single ControlGroup to have different WorkDefs assigned to its members.</para>
        /// <para>All WorkMode cells have to be updated in the GUI.</para>
        /// </summary>
        public void UpdateAll()
        {
            Pawn Overseer = Pawn.GetOverseer();
            MechanitorControlGroup ControlGroup = Pawn.GetMechControlGroup();

            foreach (DataViewRowCell_WorkMode Cell in DataView.Cells.OfType<DataViewRowCell_WorkMode>())
            {
                if ((Cell.Pawn.GetOverseer() == Overseer) && (Cell.Pawn.GetMechControlGroup()?.Index == ControlGroup.Index))
                {
                    Cell.Pull();
                }
            }
        }
    }
}
