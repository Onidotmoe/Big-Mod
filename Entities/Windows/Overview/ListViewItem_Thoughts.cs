using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListViewItem_Thoughts : ListViewItem, IPull
    {
        public Pawn Pawn { get; set; }
        public Label Value = new Label();
        public Label Count = new Label();
        public Thought Thought;

        public ListViewItem_Thoughts(Pawn Pawn, Thought Thought)
        {
            this.Pawn = Pawn;
            this.Thought = Thought;

            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Label.Style.WordWrap = true;
            Header.Label.Offset = new Vector2(5f, 0f);
            Header.Label.InheritParentSize_Modifier = new Vector2(-30f, 0f);

            Value.Size = new Vector2(25f, Height);
            Value.Style.TextAnchor = TextAnchor.MiddleRight;
            Value.Anchor = Anchor.TopRight;
            Value.OffsetX = -5f;

            Count.Size = new Vector2(50f, Height);
            Count.Style.TextAnchor = TextAnchor.MiddleRight;
            Count.Anchor = Anchor.TopRight;
            Count.OffsetX = -30f;

            Text = Thought.LabelCap;

            AddRange(Value, Count);
        }

        public override void DoOnListViewAdded()
        {
            base.DoOnListViewAdded();

            // TODO: Some Text is still calculated too small!?
            // Will first get its true size when added to a ListView.
            // Allows for a more consistent look.
            Height = ((int)Math.Ceiling(Header.Label.GetTextHeight() / 5f) * 5f);
        }

        public void Pull()
        {
            ToolTipText = Thought.Description;

            List<Thought> Group = new List<Thought>();
            Pawn.needs.mood.thoughts.GetMoodThoughts(Thought, Group);

            Thought Leading = PawnNeedsUIUtility.GetLeadingThoughtInGroup(Group);

            int DurationTicks = Thought.DurationTicks;

            if (DurationTicks > 5)
            {
                if (Pawn.DevelopmentalStage.Baby())
                {
                    ToolTipText += (Leading.BabyTalk + Environment.NewLine + "Translation".Translate() + ": " + Leading.Description).Colorize(ColoredText.SubtleGrayColor);
                }
                else
                {
                    ToolTipText += Leading.Description;
                }

                if (Leading is Thought_Memory Memory)
                {
                    if (Group.Count == 1)
                    {
                        ToolTipText += Environment.NewLine + Environment.NewLine + "ThoughtExpiresIn".Translate((DurationTicks - Memory.age).ToStringTicksToPeriod(true, false, true, true));
                    }
                    else
                    {
                        IEnumerable<Thought_Memory> Memories = Group.Cast<Thought_Memory>();

                        int Min = Memories.Min((F) => F.age);
                        int Max = Memories.Max((F) => F.age);

                        ToolTipText += Environment.NewLine
                            + Environment.NewLine + "ThoughtStartsExpiringIn".Translate((DurationTicks - Max).ToStringTicksToPeriod(true, false, true, true))
                            + Environment.NewLine + "ThoughtFinishesExpiringIn".Translate((DurationTicks - Min).ToStringTicksToPeriod(true, false, true, true));
                    }
                }
            }

            if (Group.Count > 1)
            {
                Count.Text = Group.Count.ToString() + "x";
                Count.IsVisible = true;
            }
            else
            {
                Count.IsVisible = false;
            }

            float ThoughtMoodOffset = Pawn.needs.mood.thoughts.MoodOffsetOfGroup(Thought);
            Value.Text = ThoughtMoodOffset.ToString();

            if (ThoughtMoodOffset > 0)
            {
                Value.Style.TextColor = Globals.GetColor("ListView_Thoughts.Item.Value.Positive");
            }
            else if (ThoughtMoodOffset < 0)
            {
                Value.Style.TextColor = Globals.GetColor("ListView_Thoughts.Item.Value.Negative");
            }
        }
    }
}
