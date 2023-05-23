using BigMod.Entities.Interface;
using BigMod.Entities.Windows.Overview;
using RimWorld.Planet;
using System.Reflection;
using Verse;

namespace BigMod.Entities.Windows.Inspect
{
    /// <summary>
    /// Represents a Inspect Item for the 3D World screen.
    /// </summary>
    public class ListViewItem_Inspect_World : ListViewItem_Inspect
    {
        // TODO: Is not finished
        public static PropertyInfo SelTile = typeof(WITab).GetProperty("SelTile", BindingFlags.Instance | BindingFlags.NonPublic);
        public override Vector2 MinSize { get; set; } = new Vector2(0f, 295f);
        public Tile Tile;
        public ListViewItem_Inspect_World()
        {
            Header.IsVisible = false;
            Style.DrawMouseOver = false;

        }

        public override void Update()
        {
            base.Update();

            if (Tile != SelTile.GetValue(Target))
            {
                Tile = (Tile)SelTile.GetValue(Target);
            }
        }

        public override bool Filter(string Search)
        {
            return false;
        }
        public override void AddTabs(Thing Target)
        {

        }
    }
}
