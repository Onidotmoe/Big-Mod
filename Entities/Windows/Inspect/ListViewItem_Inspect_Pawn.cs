using BigMod.Entities.Interface;
using BigMod.Entities.Windows.Overview;
using Verse;

namespace BigMod.Entities.Windows.Inspect
{
    public class ListViewItem_Inspect_Pawn : ListViewItem_Inspect, IPawn
    {
        // TODO: overseer & master missing, current activity and queued activity missing
        public Pawn Pawn { get; set; }
        public OverviewPawn Overview;
        public override Vector2 MinSize { get; set; } = new Vector2(0f, 315f);
        public ListViewItem_Inspect_Pawn(Pawn Pawn)
        {
            Overview = new OverviewPawn(Width, Height, DisplayStyle.Inspect);
            Overview.InheritParentSize = true;

            Header.IsVisible = false;
            Style.DrawMouseOver = false;

            AddChild(Overview);

            SetPawn(Pawn);
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
            Target = Pawn;

            Overview.SetPawn(Pawn);
            Overview.Portrait.PawnSelectable = false;
            // Remove Selected Color as it's not useful in this Pawn.
            Overview.Portrait.Selectable = false;
            Overview.Portrait.Style.SelectedColor = default;
            Overview.Portrait.Header.Style.SelectedColor = default;

            Inspect.TempTarget = Target;
            AddTabs(Target);

            if (Tabs?.Any() == true)
            {
                MinSize += new Vector2(0f, 20f);

                Overview.OffsetY = 20f;
                Overview.InheritParentSize_Modifier = new Vector2(0f, -20f);
            }
        }
        public override bool Filter(string Search)
        {
            return (Overview.Children.OfType<Label>().Where((F) => F.IsVisible && string.IsNullOrEmpty(F.Text)).Any((F) => F.Text.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0));
        }
        public override void DoOnListViewAdded()
        {
            base.DoOnListViewAdded();

            ListView.OnScrollPositionChanged += DoOnScrollPositionChanged;
        }
        public void DoOnScrollPositionChanged(object Sender, EventArgs EventArgs)
        {
            // Close when Scroll Position has changed
            Overview.Current_Drugs.ToggleState = false;
            Overview.Current_Foods.ToggleState = false;
            Overview.Current_Outfits.ToggleState = false;

            // Sub ToolTip positions have to be updated manually.
            Overview.Current_Drugs.ToolTip_Offset = (-ListView?.ScrollPosition ?? Vector2.zero);
            Overview.Current_Foods.ToolTip_Offset = Overview.Current_Drugs.ToolTip_Offset;
            Overview.Current_Outfits.ToolTip_Offset = Overview.Current_Drugs.ToolTip_Offset;
        }
    }
}
