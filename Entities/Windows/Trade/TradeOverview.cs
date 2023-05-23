using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using Verse;

namespace BigMod.Entities.Windows.Trade
{
    // TODO: needs updating and testing
    public class TradeOverview : Panel
    {
        public Trade Trade;
        public ListView Inventory;

        public Button Sort_Name = new Button("Trade_Name".Translate());
        public Button Sort_Value = new Button("Trade_Value".Translate());
        public Button Sort_Value_Total = new Button("Trade_ValueTotal".Translate());
        public Button Sort_Count = new Button("Trade_Count".Translate());
        public Button Sort_Mass = new Button("Trade_Mass".Translate());
        public Button Sort_Mass_Total = new Button("Trade_MassTotal".Translate());
        public Button Sort_Efficiency = new Button("Trade_Efficiency".Translate());

        public float Valuta;
        public Label Cash = new Label(true, true);
        public Label Value = new Label(true, true);
        public Label Count = new Label(true, true);
        public Label Mass = new Label(true, true);
        public Label MassCapacity = new Label(true, true);
        public Search Search;

        public Pawn Trader;
        public Caravan Caravan;

        /// <summary>
        /// List of Items that don't originate from this Inventory and are in transit from another's inventory.
        /// </summary>
        /// <remarks>These Items will be commited to this Inventory when the Player accepts the trade.</remarks>
        public List<ListViewItem_Trade> Incoming = new List<ListViewItem_Trade>();
        public virtual event EventHandler OnSortClicked;

        public TradeOverview(Vector2 Offset, Vector2 Size)
        {
            this.Offset = Offset;
            this.Size = Size;

            Style.DrawBackground = false;
            Style.DrawBorder = false;

            Vector2 ButtonSize = new Vector2(60f, 40f);

            Inventory = new ListView(Width, (Height * 0.87f));
            Inventory.SetStyle("TradeOverview.Inventory");
            Inventory.AlternateColors = true;
            Inventory.Offset = new Vector2(0, ButtonSize.y + 2f);

            Search = new Search((Width * 0.35f));
            Search.LimitToParent = true;
            Search.Anchor = Anchor.TopRight;
            Search.Offset = new Vector2(-5f, (Inventory.OffsetBottom + 28f));
            Search.TextInput.OnTextChanged += SimpleTextFilter;

            AddRange(Inventory, Search, Sort_Name, Sort_Value, Sort_Value_Total, Sort_Count, Sort_Mass, Sort_Mass_Total, Sort_Efficiency, Cash, Value, Count, Mass, MassCapacity);

            Sort_Name.Anchor = Anchor.TopRight;
            Sort_Efficiency.Anchor = Anchor.TopRight;
            Sort_Mass.Anchor = Anchor.TopRight;
            Sort_Mass_Total.Anchor = Anchor.TopRight;
            Sort_Count.Anchor = Anchor.TopRight;
            Sort_Value.Anchor = Anchor.TopRight;
            Sort_Value_Total.Anchor = Anchor.TopRight;

            Sort_Name.Offset = new Vector2(-(Inventory.Width - 80f), 0f);
            Sort_Efficiency.Offset = new Vector2(-470f, 0f);
            Sort_Mass.Offset = new Vector2(-395f, 0f);
            Sort_Mass_Total.Offset = new Vector2(-320f, 0f);
            Sort_Count.Offset = new Vector2(-260f, 0f);
            Sort_Value.Offset = new Vector2(-185f, 0f);
            Sort_Value_Total.Offset = new Vector2(-110f, 0f);

            Sort_Name.Size = ButtonSize;
            Sort_Value.Size = ButtonSize;
            Sort_Value_Total.Size = ButtonSize;
            Sort_Count.Size = ButtonSize;
            Sort_Mass.Size = ButtonSize;
            Sort_Mass_Total.Size = ButtonSize;
            Sort_Efficiency.Size = ButtonSize;

            Sort_Name.Style.WordWrap = true;
            Sort_Value.Style.WordWrap = true;
            Sort_Value_Total.Style.WordWrap = true;
            Sort_Count.Style.WordWrap = true;
            Sort_Mass.Style.WordWrap = true;
            Sort_Mass_Total.Style.WordWrap = true;
            Sort_Efficiency.Style.WordWrap = true;

            Sort_Name.CanToggle = true;
            Sort_Value.CanToggle = true;
            Sort_Value_Total.CanToggle = true;
            Sort_Count.CanToggle = true;
            Sort_Mass.CanToggle = true;
            Sort_Mass_Total.CanToggle = true;
            Sort_Efficiency.CanToggle = true;

            Sort_Name.ToggleState = true;
            Sort_Value.ToggleState = true;
            Sort_Value_Total.ToggleState = true;
            Sort_Count.ToggleState = true;
            Sort_Mass.ToggleState = true;
            Sort_Mass_Total.ToggleState = true;
            Sort_Efficiency.ToggleState = true;

            // Using Data as a extra Toggle option
            Sort_Name.Data = true;
            Sort_Value.Data = true;
            Sort_Value_Total.Data = true;
            Sort_Count.Data = true;
            Sort_Mass.Data = true;
            Sort_Mass_Total.Data = true;
            Sort_Efficiency.Data = true;

            Sort_Name.Label.Style.TextAnchor = TextAnchor.LowerCenter;
            Sort_Value.Label.Style.TextAnchor = TextAnchor.LowerCenter;
            Sort_Value_Total.Label.Style.TextAnchor = TextAnchor.LowerCenter;
            Sort_Count.Label.Style.TextAnchor = TextAnchor.LowerCenter;
            Sort_Mass.Label.Style.TextAnchor = TextAnchor.LowerCenter;
            Sort_Mass_Total.Label.Style.TextAnchor = TextAnchor.LowerCenter;
            Sort_Efficiency.Label.Style.TextAnchor = TextAnchor.LowerCenter;

            Sort_Name.OnClick += DoOnSortClick;
            Sort_Efficiency.OnClick += DoOnSortClick;
            Sort_Mass.OnClick += DoOnSortClick;
            Sort_Mass_Total.OnClick += DoOnSortClick;
            Sort_Count.OnClick += DoOnSortClick;
            Sort_Value.OnClick += DoOnSortClick;
            Sort_Value_Total.OnClick += DoOnSortClick;

            Sort_Name.OnClickRight += DoOnSortClickRight;
            Sort_Efficiency.OnClickRight += DoOnSortClickRight;
            Sort_Mass.OnClickRight += DoOnSortClickRight;
            Sort_Mass_Total.OnClickRight += DoOnSortClickRight;
            Sort_Count.OnClickRight += DoOnSortClickRight;
            Sort_Value.OnClickRight += DoOnSortClickRight;
            Sort_Value_Total.OnClickRight += DoOnSortClickRight;

            Cash.Style.TextAnchor = TextAnchor.MiddleLeft;
            MassCapacity.Style.TextAnchor = TextAnchor.MiddleLeft;

            // Explain what these are to reduce confusion.
            Value.ToolTipText = "Trade_ValueTotal".Translate();
            Count.ToolTipText = "Trade_CountTotal".Translate();
            Mass.ToolTipText = "Trade_MassTotal".Translate();
            Value.IgnoreMouse = false;
            Count.IgnoreMouse = false;
            Mass.IgnoreMouse = false;

            Value.Style.TextAnchor = TextAnchor.MiddleRight;
            Count.Style.TextAnchor = TextAnchor.MiddleRight;
            Mass.Style.TextAnchor = TextAnchor.MiddleRight;

            Cash.Offset = new Vector2(5f, (Inventory.OffsetBottom + 3f));
            Cash.ToolTipText = (ThingDefOf.Silver.LabelCap + Environment.NewLine + ThingDefOf.Silver.DescriptionDetailed);
            Cash.IgnoreMouse = false;

            MassCapacity.Offset = new Vector2((5f + 50f), Cash.Offset.y);
            MassCapacity.ToolTipText = "MassCapacity".Translate();
            MassCapacity.IgnoreMouse = false;

            Mass.Anchor = Anchor.TopRight;
            Count.Anchor = Anchor.TopRight;
            Value.Anchor = Anchor.TopRight;

            Mass.Offset = new Vector2(-455f, Cash.Offset.y);
            Count.Offset = new Vector2(-303f, Cash.Offset.y);
            Value.Offset = new Vector2(-170f, Cash.Offset.y);

            Sort_Efficiency.ToolTipText = "CostEffectivenessInfo".Translate();
        }

        public void UpdateLabels()
        {
            List<ListViewItem_Trade> Inventory_Items = Inventory.Items.Cast<ListViewItem_Trade>().ToList();
            Inventory_Items.AddRange(Incoming);

            Transactor Transactor = Transactor.Trader;

            if (Trader.IsColonist)
            {
                Transactor = Transactor.Colony;
            }

            int Silver = TradeSession.deal.CurrencyTradeable.CountHeldBy(Transactor);

            // Update the Silver amount
            Valuta = (Silver * TradeSession.deal.CurrencyTradeable.BaseMarketValue);

            Cash.Text = Valuta.ToString() + "DollarSign".Translate();

            // We don't care about stuff the Trader isn't willing to trade.
            Value.Text = Inventory_Items.Where((F) => F.IsEnabled).Sum((F) => F.Value_Total).ToString("0.00") + "DollarSign".Translate();
            Count.Text = Inventory_Items.Where((F) => F.IsEnabled).Sum((F) => F.Count).ToString() + "x";
            Mass.Text = Inventory_Items.Where((F) => F.IsEnabled).Sum((F) => F.Mass_Total).ToString("0.00") + "Kg".Translate();

            if (Caravan != null)
            {
                float Capacity = GetCaravanRemainingMassCapacity();

                MassCapacity.Text = Capacity.ToString("0.00") + "Kg".Translate();

                if (Capacity >= 0)
                {
                    MassCapacity.Style.TextColor = Globals.GetColor("Trade.CanAfford");
                }
                else
                {
                    MassCapacity.Style.TextColor = Globals.GetColor("Trade.CantAfford");
                }
            }
            else
            {
                // If not part of a Caravan, should not have any mass limitations.
                MassCapacity.IsVisible = false;
            }
        }

        public float GetCaravanRemainingMassCapacity()
        {
            float MassConsumed = Inventory.Items.Cast<ListViewItem_Trade>().Sum((F) => F.Mass_Total);

            return (Caravan.MassCapacity - Caravan.MassUsage - MassConsumed);
        }

        private FieldInfo GetFieldInfo(Button Button, bool GetLabel = false)
        {
            string IsLabel = string.Empty;

            if (GetLabel)
            {
                IsLabel = "Label_";
            }

            FieldInfo FieldInfo = null;

            if (Button == Sort_Name)
            {
                // ListViewItem.Text is a Property not a Field.
                return null;
            }
            else if (Button == Sort_Value)
            {
                FieldInfo = typeof(ListViewItem_Trade).GetField(IsLabel + "Value");
            }
            else if (Button == Sort_Value_Total)
            {
                FieldInfo = typeof(ListViewItem_Trade).GetField(IsLabel + "Value_Total");
            }
            else if (Button == Sort_Count)
            {
                FieldInfo = typeof(ListViewItem_Trade).GetField(IsLabel + "Count");
            }
            else if (Button == Sort_Mass)
            {
                FieldInfo = typeof(ListViewItem_Trade).GetField(IsLabel + "Mass");
            }
            else if (Button == Sort_Mass_Total)
            {
                FieldInfo = typeof(ListViewItem_Trade).GetField(IsLabel + "Mass_Total");
            }
            else if (Button == Sort_Efficiency)
            {
                FieldInfo = typeof(ListViewItem_Trade).GetField(IsLabel + "Efficiency");
            }

            return FieldInfo;
        }

        public void DoOnSortClick(object Sender, EventArgs EventArgs)
        {
            OnSortClicked?.Invoke(Sender, EventArgs);

            Button Button = (Button)Sender;
            Button.ToggleState = !Button.ToggleState;

            FieldInfo FieldInfo = GetFieldInfo(Button);

            List<ListViewItem_Trade> Inventory_Items = Inventory.Items.Cast<ListViewItem_Trade>().ToList();

            List<ListViewItem_Trade> Disabled = Inventory_Items.Where((F) => !F.IsEnabled).ToList();

            Inventory_Items = Inventory_Items.Except(Disabled).Except(Incoming).ToList();

            if (FieldInfo != null)
            {
                if (Button.ToggleState)
                {
                    Inventory.Items = Inventory_Items.OrderBy((F) => FieldInfo.GetValue(F)).Cast<ListViewItem>().ToList();

                    Disabled = Disabled.OrderBy((F) => FieldInfo.GetValue(F)).ToList();
                    Incoming = Incoming.OrderBy((F) => FieldInfo.GetValue(F)).ToList();
                }
                else
                {
                    Inventory.Items = Inventory_Items.OrderByDescending((F) => FieldInfo.GetValue(F)).Cast<ListViewItem>().ToList();

                    Disabled = Disabled.OrderByDescending((F) => FieldInfo.GetValue(F)).ToList();
                    Incoming = Incoming.OrderByDescending((F) => FieldInfo.GetValue(F)).ToList();
                }
            }
            else
            {
                // FieldInfo was null, sort by Name instead.
                if (Button.ToggleState)
                {
                    Inventory.Items = Inventory_Items.OrderBy((F) => F.Text).Cast<ListViewItem>().ToList();

                    Disabled = Disabled.OrderBy((F) => F.Text).ToList();
                    Incoming = Incoming.OrderBy((F) => F.Text).ToList();
                }
                else
                {
                    Inventory.Items = Inventory_Items.OrderByDescending((F) => F.Text).Cast<ListViewItem>().ToList();

                    Disabled = Disabled.OrderByDescending((F) => F.Text).ToList();
                    Incoming = Incoming.OrderByDescending((F) => F.Text).ToList();
                }
            }

            Inventory.Items.AddRange(Disabled);
            Inventory.Items.AddRange(Incoming);

            Inventory.UpdatePositions();
        }

        public void DoOnSortClickRight(object Sender, EventArgs EventArgs)
        {
            Button Button = (Button)Sender;
            // Using Data as a extra Toggle option
            Button.Data = !(bool)Button.Data;
            bool Toggle = (bool)Button.Data;

            FieldInfo FieldInfo = GetFieldInfo(Button, true);

            List<ListViewItem_Trade> Inventory_Items = Inventory.Items.Cast<ListViewItem_Trade>().ToList();

            if (FieldInfo != null)
            {
                // Right-clicking hides the corresponding column
                foreach (ListViewItem_Trade Item in Inventory_Items)
                {
                    ((Label)FieldInfo.GetValue(Item)).IsVisible = Toggle;
                }
            }
            else
            {
                // Toggle Name
                foreach (ListViewItem_Trade Item in Inventory_Items)
                {
                    Item.Header.Label.IsVisible = Toggle;
                }
            }
        }

        public void AddItem(ListViewItem_Trade Item)
        {
            Item.Owner = this;
            Item.RemoveFromItemParent();
            Inventory.AddItem(Item);
            UpdateLabels();
        }

        public void RemoveItem(ListViewItem_Trade Item)
        {
            Item.Owner = null;
            Item.RemoveFromItemParent();
            Inventory.RemoveItem(Item);
            UpdateLabels();
        }

        public void SimpleTextFilter(object Sender, EventArgs EventArgs)
        {
            // TODO:
            //ListWindow.SimpleTextFilter(Inventory, Search.Text);
        }
    }
}
