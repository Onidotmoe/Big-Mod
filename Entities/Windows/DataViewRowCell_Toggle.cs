namespace BigMod.Entities.Windows
{
    public class DataViewRowCell_Toggle : DataViewRowCell_Data
    {
        public Button Button = new Button();

        public DataViewRowCell_Toggle()
        {
            Button.SetStyle("DataViewRowCell_Toggle.Button");
            // MouseOver is drawn by the Cell instead.
            Button.Style.DrawMouseOver = false;
            Button.Style.DrawBackground = true;
            Button.InheritParentSize = true;
            Button.OnToggleStateChanged += (obj, e) => DoOnDataChanged();
            AddChild(Button);
        }

        public virtual bool CanToggle()
        {
            return true;
        }

        public override void Copy(DataViewRowCell_Data Cell)
        {
            // Check if ToggleState is allowed to change.
            if (CanToggle())
            {
                Button.ToggleState = ((DataViewRowCell_Toggle)Cell).Button.ToggleState;
            }
        }

        public override void Floor()
        {
            if (CanToggle())
            {
                Button.ToggleState = false;
            }
        }

        public override void Ceiling()
        {
            if (CanToggle())
            {
                Button.ToggleState = true;
            }
        }

        public override void AddDelta(int Delta, bool Cycling = true)
        {
            // Don't allow Mousescrolling to affect the value as it's annoying when just trying to scroll the DataView.
            if (!Cycling && CanToggle())
            {
                Button.ToggleState = (Delta > 0);
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            // Locked Cells should sort to the bottom.
            return (-(Button.IsLocked.CompareTo(((DataViewRowCell_Toggle)Other).Button.IsLocked) * 2) + Button.ToggleState.CompareTo(((DataViewRowCell_Toggle)Other).Button.ToggleState));
        }

        public override void DoOnDataChanged()
        {
            base.DoOnDataChanged();

            Push();
        }
    }
}
