using BigMod.Entities.Windows.Overview;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Inspect
{
    public class ListViewItem_Inspect_Thing : ListViewItem_Inspect
    {
        public Thing Thing;
        public List<Thing> Things = new List<Thing>();
        public ListViewItem_Inventory Portrait;
        public Label Header_Name = new Label(){ IgnoreMouse = false };
        public Label Header_Inspect = new Label() { IgnoreMouse = false };
        public Label Header_Hitpoints = new Label();
        public ListView Stats;
        public override Vector2 MinSize { get; set; } = new Vector2(0f, 315f);
        // TODO: missing rename button, missing next in cell button
        // TODO: rename should be in the context menu when rightclicking the image
        /*
         					if ((p = (singleSelectedThing as Pawn)) != null)
					{
						if (p.playerSettings != null && p.playerSettings.UsesConfigurableHostilityResponse)
						{
							num -= 24f;
							HostilityResponseModeUtility.DrawResponseButton(new Rect(num, 0f, 24f, 24f), p, false);
							lineEndWidth += 24f;
						}
						if ((p.Faction == Faction.OfPlayer && p.RaceProps.Animal && p.RaceProps.hideTrainingTab) || (ModsConfig.BiotechActive && p.IsColonyMech))
						{
							num -= 30f;
							TrainingCardUtility.DrawRenameButton(new Rect(num, 0f, 30f, 30f), p);
							lineEndWidth += 30f;
						}
						if (p.guilt != null && p.guilt.IsGuilty)
						{
							num -= 26f;
							Rect rect2 = new Rect(num, 0f, 26f, 26f);
							GUI.DrawTexture(rect2, TexUI.GuiltyTex);
							TooltipHandler.TipRegion(rect2, () => p.guilt.Tip, 6321223);
							lineEndWidth += 26f;
						}
					}

         */

        public ListViewItem_Inspect_Thing(Thing Thing)
        {
            SetThing(Thing);

            Header.IsVisible = false;
            Style.DrawMouseOver = false;

            Header_Name.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header_Name.Style.FontType = GameFont.Medium;
            Header_Name.Style.TextColor = Globals.GetColor("ListViewItem_Inspect_Thing.Header_Name.TextColor");
            Header_Name.RenderStyle = LabelStyle.Fit;
            Header_Name.Offset = new Vector2((Portrait.Right + 5f), 5f);
            Header_Name.Size = new Vector2(375f, 28f);
            Header_Inspect.Style.TextColor = Globals.GetColor("ListViewItem_Inspect_Thing.Header_Inspect.TextColor");
            Header_Inspect.Style.TextAnchor = TextAnchor.UpperLeft;
            Header_Inspect.Style.FontType = GameFont.Tiny;
            Header_Inspect.Offset = new Vector2(Header_Name.X, 29f);
            Header_Inspect.Size = new Vector2(375f, (Portrait.Bottom - Header_Name.Bottom + 10f));

            Header_Hitpoints.Style.TextColor = Globals.GetColor("ListViewItem_Inspect_Thing.Header_Hitpoints.TextColor");
            Header_Hitpoints.Style.TextAnchor = TextAnchor.MiddleRight;
            Header_Hitpoints.Style.FontType = GameFont.Tiny;
            Header_Hitpoints.Style.TextOffset = new Vector2(-3f, 0f);
            Header_Hitpoints.RenderStyle = LabelStyle.GUI;
            Header_Hitpoints.Offset = new Vector2(Portrait.X, Portrait.Bottom - 4f);
            Header_Hitpoints.Size = new Vector2(Portrait.Width, 15f);

            Stats = new ListView(Width, Height);
            Stats.Offset = new Vector2(Portrait.OffsetX, (Portrait.Bottom + 15f));
            Stats.RenderStyle = ListViewStyle.Grid;
            Stats.ExtendItemsHorizontally = false;
            Stats.InheritParentSize = true;
            Stats.InheritParentSize_Modifier = new Vector2((Portrait.OffsetX * -2f), -(Portrait.Bottom + 25f));
            AddRange(Header_Name, Header_Inspect, Header_Hitpoints, Stats);

            if (Tabs?.Any() == true)
            {
                MinSize += new Vector2(0f, 20f);

                Portrait.OffsetY += 20f;
                Header_Name.OffsetY += 20f;
                Header_Inspect.OffsetY += 20f;
                Header_Hitpoints.OffsetY += 20f;
                Stats.OffsetY += 20f;
                Stats.InheritParentSize_Modifier += new Vector2(0f, -20f);
            }

            Pull();
        }

        public void SetThing(Thing Thing)
        {
            this.Thing = Thing;
            Target = Thing;

            if (!Things.Contains(Thing))
            {
                Things.Add(Thing);
            }

            if (Portrait != null)
            {
                RemoveChild(Portrait);
                Portrait = null;
            }

            Portrait = new ListViewItem_Inventory(Thing, false);
            Portrait.SetStyle(GetType().Name + ".Item");
            Portrait.Header.SetStyle(GetType().Name + ".Item.Header");
            Portrait.UseAnchoring = true;
            Portrait.SquareSize = new Vector2(80f, 80f);
            Portrait.Offset = new Vector2(5f, 5f);
            AddChild(Portrait);
            // Header_Hitpoints should be rendered ontop of Portrait.
            Portrait.MoveToFront();
            Portrait.RenderStyle = ItemStyle.Square;

            // Is disabled because ItemStyle.Square was initially made for Equipment_Slot
            Portrait.Header.Style.DrawBackground = true;
            Portrait.Header.OnClick += Portrait_Header_OnClick;

            Portrait.Pull();

            // Hitpoints has to be updated as not all items will have the same amount.
            Pull_Hitpoints();

            Inspect.TempTarget = Target;
            AddTabs(Target);
        }

        public void RemoveThing(Thing Thing)
        {
            Things.Remove(Thing);

            if (Things.Any())
            {
                if (this.Thing == Thing)
                {
                    SetThing(Things.First());
                }
            }
            else
            {
                RemoveFromItemParent();
            }
        }

        public override bool Validate()
        {
            // Go backwards and remove all non-selected items.
            for (int i = (Things.Count - 1); i >= 0; i--)
            {
                if (!Find.Selector.SelectedObjects.Contains(Things[i]))
                {
                    RemoveThing(Things[i]);
                }
            }

            if (!Things.Any())
            {
                RemoveFromItemParent();
                return false;
            }

            return true;
        }

        private void Pull_Hitpoints()
        {
            Header_Hitpoints.IsVisible = (Thing.MaxHitPoints > 0);

            if (Header_Hitpoints.IsVisible)
            {
                // Negative Hitpoints values are not valid.
                Header_Hitpoints.Text = $"{((Thing.HitPoints >= 0) ? Thing.HitPoints : Thing.MaxHitPoints)} / {Thing.MaxHitPoints}";
            }
        }

        public override void Pull()
        {
            Stats.Clear();

            Portrait.Pull();

            Header_Inspect.Text = Thing.GetInspectString();

            Pull_Hitpoints();

            int Count = Things.Sum((F) => F.stackCount);

            string Hitpoints =  (Header_Hitpoints.IsVisible ? ((Count > 1) ? $" ({"Average".Translate()} {((float)Things.Sum((F) => ((F.HitPoints >= 0) ? F.HitPoints : F.MaxHitPoints)) / (float)Things.Sum((F) => F.MaxHitPoints)) * 100f:F0}%)" : $" ({((float)Thing.HitPoints / (float)Thing.MaxHitPoints) * 100f:F0}%)") : string.Empty);

            Header_Name.Text = Thing.LabelNoParenthesisCap + Hitpoints + ((Count > 1) ? $" {Count}x" : string.Empty);

            StatRequest Request = ((Thing.def is BuildableDef BuildableDef) ? StatRequest.For(BuildableDef, Thing.Stuff, (Thing.TryGetQuality(out QualityCategory Quality) ? Quality : QualityCategory.Normal)) : StatRequest.ForEmpty());

            IEnumerable<StatDef> Defs = DefDatabase<StatDef>.AllDefs.Where((F) => F.Worker.ShouldShowFor(Request));
            List<StatDrawEntry> Entries = new List<StatDrawEntry>();

            if (Request.BuildableDef != null)
            {
                foreach (StatDef Def in Defs)
                {
                    Entries.Add(new StatDrawEntry(Def.category, Def, Request.BuildableDef.GetStatValueAbstract(Def, Request.StuffDef), StatRequest.For(Request.BuildableDef, Request.StuffDef, QualityCategory.Normal), ToStringNumberSense.Undefined, null, false));
                }
            }

            Entries.AddRange(Thing.SpecialDisplayStats());
            Entries.AddRange(Thing.def.SpecialDisplayStats(Request));

            float ItemWidth = ((Stats.Width / 2f) - (Stats.ItemMargin.x * 3f));
            // TODO: has to show average values for all selected things
            // Only allow a single instance of a stat to be displayed. Using LabelCap as some items don't have a StatDef
            foreach (StatDrawEntry Entry in Entries.GroupBy((F) => F.LabelCap).Select((F) => F.First()))
            {
                if (Entry.ShouldDisplay)
                {
                    ListViewItem Item = new ListViewItem(Entry.LabelCap);
                    Item.Header.Label.Style.TextColor = Globals.GetColor("ListViewItem_Inspect_Thing.Item.Header.Label.TextColor");
                    Item.Width = ItemWidth;
                    Item.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
                    Item.Header.Label.Style.TextOffset = new Vector2(5f, 0f);
                    Item.Header.Label.RenderStyle = LabelStyle.GUI;
                    Item.Header.Label.Style.WordWrap = true;
                    Item.ToolTipText = Entry.GetExplanationText(Request);

                    Label Value = new Label(Entry.ValueString, new Vector2(50f, 20f));
                    Value.ID = "Value";
                    Value.Style.TextColor = Globals.GetColor("ListViewItem_Inspect_Thing.Item.Value.TextColor");
                    Value.Style.TextAnchor = TextAnchor.MiddleRight;
                    Value.Style.TextOffset = new Vector2(-4f, 0f);
                    Value.RenderStyle = LabelStyle.GUI;
                    Value.Anchor = Anchor.TopRight;
                    Value.SizeToText();
                    Item.AddChild(Value);

                    Item.Height = Mathf.Max(Item.Header.Label.GetTextHeight(Item.Width - Value.Width), 20f);

                    IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks = Entry.GetHyperlinks(Request);

                    if ((Hyperlinks != null) && Hyperlinks.Any())
                    {
                        Item.OnClick += (obj, e) => Hyperlinks.First().ActivateHyperlink();
                    }

                    Stats.AddItem(Item);
                }
            }
        }

        public override bool TryMerge(Thing Other)
        {
            if (Thing.def == Other.def)
            {
                if ((Thing != Other) && !Things.Contains(Other))
                {
                    Things.Add(Other);

                    // IComparable not implemented for Vector2 and Vector3;
                    Things = Things.OrderByDescending((F) => Comparer<float>.Default.Compare(F.Position.x, F.Position.z)).ToList();
                }

                return true;
            }

            return false;
        }

        public override bool Filter(string Search)
        {
            return (ListView.Items.Any((F) => (F.Text.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0) || ((Label)F.GetChildWithID("Value")).Text.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0));
        }

        public void Portrait_Header_OnClick(object Sender, MouseEventArgs EventArgs)
        {
            RimWorld.Planet.GlobalTargetInfo Target = Thing;

            if (Things.Count > 1)
            {
                // Cycle though all things.
                int i = (Things.IndexOf(Thing) + 1);
                i = ((i > (Things.Count - 1)) ? 0 : i);

                // Move to next item.
                Target = Things[i];

                SetThing(Things[i]);
            }

            if (Target.IsValid)
            {
                CameraJumper.TryJump(Target);
            }
        }
    }
}
