using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class Overview : WindowPanel
    {
        public override Rect DefaultBounds { get; set; } = new Rect(((UI.screenWidth / 2f) - (UI.screenWidth * 0.80f / 2f)), ((UI.screenHeight / 2f) - (UI.screenHeight * 0.7f / 2f)), (UI.screenWidth * 0.80f), (UI.screenHeight * 0.75f));

        public ListView Pawns;
        public Search Search;

        public OverviewPawn PawnPanel;

        public Overview()
        {
            CameraMotion = false;
            CameraMouseOverZooming = false;

            Pawns = new ListView(Mathf.Min((Width * 0.3f), 300f), (Height - 25f));
            Pawns.SetStyle("Overview.Pawns.ListView");
            Pawns.InheritParentSize = true;
            Pawns.InheritParentSize_Modifier = new Vector2(0f, -25f);
            Pawns.MaxSize = new Vector2(Pawns.Width, 0f);
            Pawns.AllowSelection = true;
            Pawns.StickySelection = true;
            Pawns.OnSelectionChanged += DoOnSelectionChanged;

            Search = new Search(Mathf.Min((Width * 0.3f), 300f), InheritParentSizeWidth: false);
            Search.OffsetY = -3f;
            Search.Anchor = Anchor.BottomLeft;

            PawnPanel = new OverviewPawn((Width * 0.7f), Height);
            PawnPanel.InheritParentSize = true;
            PawnPanel.InheritParentSize_Modifier = new Vector2(-Pawns.Width, 0f);
            PawnPanel.OffsetX = Pawns.OffsetRight;

            Root.AddRange(Pawns, Search, PawnPanel);

            Populate();

            AddButtonCloseX();
            AddButtonResize();
        }

        public void Populate()
        {
            Pawns.Clear();

            List<Pawn> Alive = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists;

            if (Alive.Any())
            {
                foreach (Pawn Pawn in Alive)
                {
                    // Do not allow Pawns to be selected or PopOut in this ListView.
                    Pawns.AddItem(new ListViewItem_Pawn(Pawn) { Display = true });
                }

                // Populate the Overview with the First Item.
                Pawns.Items.First().Select();
            }
        }

        public void DoOnSelectionChanged(object Sender, EventArgs EventArgs)
        {
            // Possible for Sender to be null if using ListView.DoOnSelectionChanged
            if (Sender != null)
            {
                SetPawn(((ListViewItem_Pawn)Sender).Pawn);
            }
        }

        public void SetPawn(Pawn Pawn)
        {
            PawnPanel.SetPawn(Pawn);
        }

        public override void PostClose()
        {
            base.PostClose();

            // If the last Overview Window has been closed, remove the ToolTipItem.
            if (!WindowManager.TryGetWindow(out Overview Overview) && ListViewItem_Inventory.GetToolTipItem(out ListViewItem_Inventory Item))
            {
                // Ensure all ToolTips are being closed.
                WindowManager.GetWindowsWithIdentifier("PopOut_Preview_ListViewItem_Inventory").Cast<ToolTip>().ToList().ForEach((F) => F.Dispose());

                Item.RemoveFromItemParent();
                Item.RemoveFromParent();
            }
        }
    }
}
