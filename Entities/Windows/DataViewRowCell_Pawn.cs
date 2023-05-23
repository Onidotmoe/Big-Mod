using Verse;

namespace BigMod.Entities.Windows
{
    public class DataViewRowCell_Pawn : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public ListViewItem_Pawn Item;
        public override Comparer<DataViewRowCell_Data> Comparator { get; set; } = Comparer<DataViewRowCell_Data>.Create((A, B) => ((DataViewRowCell_Pawn)A).Item.Nickname.Text.CompareTo(((DataViewRowCell_Pawn)B).Item.Nickname.Text));

        public DataViewRowCell_Pawn(Pawn Pawn)
        {
            this.Pawn = Pawn;

            // DataView.CellHeight
            Item = new ListViewItem_Pawn(Pawn, new Vector2(25f, 25f));
            Item.ID = "Pawn";
            Item.InheritParentSize = true;
            Item.InheritParentPosition = true;
            // Don't allow the context menu to be opened when Right-Clicking.
            Item.AllowContextMenu = false;
            Item.CanPopOut = false;
            // MouseOver is drawn by the cell instead.
            Item.Header.Style.DrawMouseOver = false;
            Item.Style.DrawMouseOver = false;
            // Make sure the Selection Overlay is not drawn.
            Item.IsSelected = false;

            AddChild(Item);
        }

        public override void Pull()
        {
            base.Pull();

            Item.DoOnRequest();
        }
    }
}
