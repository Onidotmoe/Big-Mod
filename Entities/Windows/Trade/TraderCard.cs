using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Trade
{
    public class TraderCard : Panel
    {
        public Pawn Pawn;

        public Image Icon_Faction;
        public Image Icon_Religion;
        /// <summary>
        /// Hold's the Portrait, all outer interactions should be made through the Portrait and not directly using this ListView.
        /// </summary>
        /// <remarks>You can get Portrait's ListView using <see cref="ListViewItem.ListView"/>.</remarks>
        private ListView ListView;
        public ListViewItem_Pawn Portrait;

        public Label Label_Faction = new Label(true, true);
        public Label Label_Kind = new Label(true, true);
        public Label Label_Name = new Label(true, true);
        public Label Label_Religion = new Label(true, true);

        public TraderCard(Pawn Pawn)
        {
            this.Pawn = Pawn;

            InheritChildrenSize = true;

            // Make it a little larger to prevent it from adding scrollbars
            ListView = new ListView(53f, 103f, false);
            ListView.IgnoreMouse = true;
            ListView.Style.DrawBackground = false;

            Portrait = new ListViewItem_Pawn(Pawn);
            Portrait.RenderStyle = UnitStyle.Square;
            Portrait.IgnoreMouse = true;
            ListView.AddItem(Portrait);
            // Update it so it cleans itself up a bit.
            Portrait.DoOnRequest();
            // Hide these
            Portrait.Weapon.IsVisible = false;
            Portrait.Name.IsVisible = false;
            Portrait.Nickname.IsVisible = false;
            Portrait.Title.IsVisible = false;
            // Make sure the Selection Overlay is not drawn.
            Portrait.IsSelected = false;

            Icon_Faction = new Image(Pawn.Faction.def.factionIconPath);
            Icon_Faction.Style.Color = Pawn.Faction.Color;
            Icon_Faction.Size = new Vector2(25f, 25f);

            Label_Faction.Text = Pawn.Faction.Name;
            Label_Faction.Style.TextColor = Pawn.Faction.Color;

            Label_Kind.Text = ((Pawn.trader != null) ? Pawn.trader.traderKind.LabelCap : Portrait.Nickname.Text + " " + Portrait.Title.Text);
            Label_Name.Text = Pawn.NameFullColored.Resolve();
            Label_Name.IgnoreMouse = false;

            Icon_Religion = new Image();

            if (Pawn.Ideo != null)
            {
                Icon_Religion.SetTexture(Pawn.Ideo.iconDef.iconPath);
                Icon_Religion.Style.Color = Pawn.Ideo.Color;
                Icon_Religion.Size = new Vector2(25f, 25f);

                Label_Religion.Text = Pawn.Ideo.name;
                Label_Religion.Style.TextColor = Pawn.Faction.Color;
            }
            else
            {
                Icon_Religion.IsVisible = false;
                Label_Religion.IsVisible = false;
            }

            AddRange(ListView, Icon_Faction, Label_Faction, Label_Kind, Label_Name, Icon_Religion, Label_Religion);

            float Talking = Pawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking);
            float Hearing = Pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);

            Label_Name.ToolTipText =
                (PawnCapacityDefOf.Talking.GetLabelFor(Pawn).CapitalizeFirst() + " : " + Talking.ToString("0.00")
                + Environment.NewLine + PawnCapacityDefOf.Hearing.GetLabelFor(Pawn).CapitalizeFirst() + " : " + Talking.ToString("0.00"));

            // Modified from Dialog_Trade.PostOpen
            if ((Talking < 0.95f) || (Hearing < 0.95f))
            {
                TaggedString TaggedString;

                if (Talking < 0.95f)
                {
                    TaggedString = "NegotiatorTalkingImpaired".Translate(Pawn.LabelShort, Pawn);
                }
                else
                {
                    TaggedString = "NegotiatorHearingImpaired".Translate(Pawn.LabelShort, Pawn);
                }

                TaggedString += "\n\n" + "NegotiatorCapacityImpaired".Translate();

                Label_Name.ToolTipText += Environment.NewLine + TaggedString.Resolve();
            }

            LeftAlign();
        }

        public void LeftAlign()
        {
            ListView.Anchor = Anchor.TopLeft;
            ListView.Offset = new Vector2(15f, 10f);

            Icon_Faction.Anchor = Anchor.TopLeft;
            Icon_Faction.Offset = new Vector2((ListView.OffsetRight - 18f), 10f);

            Label_Faction.Anchor = Anchor.TopLeft;
            Label_Faction.Style.TextAnchor = TextAnchor.MiddleLeft;
            Label_Faction.Offset = new Vector2((Icon_Faction.OffsetRight + 5f), 12f);

            Label_Kind.Anchor = Anchor.TopLeft;
            Label_Kind.Style.TextAnchor = TextAnchor.MiddleLeft;
            Label_Kind.Offset = new Vector2(Label_Faction.Offset.x, (Label_Faction.Offset.y + 19f));

            Label_Name.Anchor = Anchor.TopLeft;
            Label_Name.Style.TextAnchor = TextAnchor.MiddleLeft;
            Label_Name.Offset = new Vector2(Label_Faction.Offset.x, (Label_Kind.Offset.y + 19f));

            Icon_Religion.Anchor = Anchor.TopLeft;
            Icon_Religion.Offset = new Vector2(Icon_Faction.Offset.x, 68f);

            Label_Religion.Anchor = Anchor.TopLeft;
            Label_Religion.Style.TextAnchor = TextAnchor.MiddleLeft;
            Label_Religion.Offset = new Vector2(Label_Faction.Offset.x, (Label_Name.Offset.y + 19f));

            // Update size as Child positions have been rearranged.
            DoInheritChildrenSize();
        }

        /// <summary>
        /// Moves the position of Portrait and other Entities to be Right-Aligned on the Card, instead of default Left-Aligned.
        /// </summary>
        public void RightAlign()
        {
            ListView.Anchor = Anchor.TopRight;
            ListView.Offset = new Vector2(-15f, 10f);

            Icon_Faction.Anchor = Anchor.TopRight;
            Icon_Faction.Offset = new Vector2((ListView.Offset.x - ListView.Width + 18f), 10f);

            Label_Faction.Anchor = Anchor.TopRight;
            Label_Faction.Style.TextAnchor = TextAnchor.MiddleRight;
            Label_Faction.Offset = new Vector2((Icon_Faction.Offset.x - Icon_Faction.Width - 5f), 12f);

            Label_Kind.Anchor = Anchor.TopRight;
            Label_Kind.Style.TextAnchor = TextAnchor.MiddleRight;
            Label_Kind.Offset = new Vector2(Label_Faction.Offset.x, (Label_Faction.Offset.y + 19f));

            Label_Name.Anchor = Anchor.TopRight;
            Label_Name.Style.TextAnchor = TextAnchor.MiddleRight;
            Label_Name.Offset = new Vector2(Label_Faction.Offset.x, (Label_Kind.Offset.y + 19f));

            Icon_Religion.Anchor = Anchor.TopRight;
            Icon_Religion.Offset = new Vector2(Icon_Faction.Offset.x, 68f);

            Label_Religion.Anchor = Anchor.TopRight;
            Label_Religion.Style.TextAnchor = TextAnchor.MiddleRight;
            Label_Religion.Offset = new Vector2(Label_Faction.Offset.x, (Label_Name.Offset.y + 19f));

            DoInheritChildrenSize();
        }
    }
}
