using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListViewItem_Needs : ListViewItem, IPull
    {
        public Need Need;
        public ProgressBar ProgressBar = new ProgressBar();
        public Label Value = new Label();
        public float Marker;
        public float Modifier = 1f;

        public ListViewItem_Needs(float Width, Need Need)
        {
            this.Need = Need;
            this.Width = Width;

            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Label.Offset = new Vector2(5f, 0f);

            ProgressBar.InheritParentSize = true;
            ProgressBar.DoRequest = false;
            ProgressBar.Delta = Need.GUIChangeArrow;

            Value.Size = new Vector2(50f, Height);
            Value.Style.TextAnchor = TextAnchor.MiddleRight;
            Value.Anchor = Anchor.TopRight;
            Value.OffsetX = -5f;

            AddRange(ProgressBar, Value);

            ProgressBar.MoveToFront();
            ProgressBar.DoArrows = true;

            if (Need.def.scaleBar && (Need.MaxLevel < 1f))
            {
                Modifier = Need.MaxLevel;
            }

            Text = Need.LabelCap;
        }

        public void Pull()
        {
            if (Need.CurInstantLevelPercentage >= 0f)
            {
                float NewMarker = (Need.CurInstantLevelPercentage * Modifier);

                if (Marker != NewMarker)
                {
                    // Remove the Old Marker first.
                    ProgressBar.GetChildWithID("Marker")?.RemoveFromParent();
                    ProgressBar.AddChild(ProgressBar.Marker(NewMarker, Width, Height));
                }

                Marker = (Need.CurInstantLevelPercentage * Modifier);
            }

            if (ProgressBar.Percentage != Need.CurLevelPercentage)
            {
                ProgressBar.Percentage = Need.CurLevelPercentage;
                // Update only if value has changed.
                Value.Text = Need.CurLevelPercentage.ToStringPercent();
                ToolTipText = Need.GetTipString();

                List<float> Ticks = Globals.Get_Need_threshPercents(Need);
                // Remove Existing Ticks and add new ones.
                ProgressBar.GetChildrenWithID("Marker").ForEach((F) => F.RemoveFromParent());

                if (Ticks != null)
                {
                    foreach (float Tick in Ticks)
                    {
                        ProgressBar.AddChild(ProgressBar.Tick((Tick * Modifier), ProgressBar.Percentage, Width, 2f, (Height / 5f)));
                    }
                }
                if (Need.def.scaleBar)
                {
                    int Number = 1;

                    while ((float)Number < Need.MaxLevel)
                    {
                        ProgressBar.AddChild(ProgressBar.Tick((((float)Number / Need.MaxLevel) * Modifier), ProgressBar.Percentage, Width, 2f, (Height / 2f)));

                        Number++;
                    }
                }
            }
        }
    }
}
