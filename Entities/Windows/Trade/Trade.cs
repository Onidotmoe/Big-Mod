using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BigMod.Entities.Windows.Trade
{
    public class Trade : WindowPanel
    {
        /// <summary>
        /// Player's inventory, rendered on the Left side of the screen.
        /// </summary>
        public TradeOverview Player;
        /// <summary>
        /// Trader's inventory, rendered on the Right side of the screen.
        /// </summary>
        public TradeOverview Trader;

        public Label Transaction_Value = new Label(true, true);
        public Label Transaction_Count = new Label(true, true);
        public Label Transaction_Mass = new Label(true, true);
        public Label Transaction_Efficiency = new Label(true, true);

        public ITrader Merchant;
        public Pawn Negotiator;

        public Dialog_Trade Vanilla;

        public Button GiftMode = new Button("GiftMode".Translate());
        public Button Accept = new Button("Accept".Translate());

        public TraderCard Card_Negotiator;
        public TraderCard Card_Trader;

        public Image Icon_GiftMode = new Image(Globals.TryGetTexturePathFromAlias("GiftMode"));
        public bool IsGiftMode
        {
            get
            {
                return GiftMode.ToggleState;
            }
        }

        /* TODO:
		 *
		GiftMode not fully implemented -- look at -> if (Widgets.ButtonText(rect4, TradeSession.giftMode ? ("OfferGifts".Translate() + " (" + FactionGiftUtility.GetGoodwillChange(TradeSession.deal.AllTradeables, TradeSession.trader.Faction).ToStringWithSign() + ")") : "AcceptButton".Translate(), true, true, true))
		Change color of accept button when in giftmode

		Decorative entities, transparent grayish lines, decorative imagery

		Accept finalization not fully implemented.
		*/

        public Trade(Dialog_Trade Dialog_Trade) : base(new Rect(0, 0, UI.screenWidth, UI.screenHeight))
        {
            Vanilla = Dialog_Trade;
            Vanilla.closeOnAccept = false;
            Vanilla.closeOnCancel = false;
            Vanilla.closeOnClickedOutside = false;

            // Can only close using the Close button, we need enter for transfering items.
            SetCloseOn();

            // Has to be Super or SubSuper otherwise TextInputs can't be used.
            layer = WindowLayer.SubSuper;

            absorbInputAroundWindow = true;
            preventCameraMotion = true;
            forcePause = true;

            CanReset = false;
            IsDraggable = false;
            IsLockable = false;

            Player = new TradeOverview(new Vector2((Width * 0.05f), (Height * 0.1f)), new Vector2((Width * 0.45f), (Height * 0.82f)));
            Trader = new TradeOverview(new Vector2((Width * 0.50f), (Height * 0.1f)), new Vector2((Width * 0.45f), (Height * 0.82f)));

            Player.Trade = this;
            Trader.Trade = this;

            Root.AddRange(Player, Trader, Accept, Icon_GiftMode, GiftMode, Transaction_Value, Transaction_Count, Transaction_Mass);

            AddButtonClose();
            ButtonClose.Anchor = Anchor.BottomRight;
            ButtonClose.Offset = new Vector2(-(Width * 0.05f), -(Height * 0.025f));
            ButtonClose.Size = new Vector2(80f, 25f);

            GiftMode.Anchor = Anchor.BottomLeft;
            GiftMode.Style.DrawBackground = true;
            GiftMode.Size = new Vector2(100f, 25f);
            GiftMode.Offset = new Vector2(Player.X, -(Height * 0.025f));
            GiftMode.CanToggle = true;
            GiftMode.OnClick += GiftMode_OnClick;
            // Save the Background Color as it will be overridden when toggled.
            GiftMode.Data = GiftMode.Style.BackgroundColor;

            Accept.Anchor = Anchor.BottomLeft;
            Accept.Style.DrawBackground = true;
            Accept.Style.BorderColor = Globals.GetColor("Trade.GiftMode");
            Accept.Style.BorderThickness = 2;
            Accept.Size = new Vector2(80f, 25f);
            Accept.Offset = new Vector2((Trader.Right - ButtonClose.Width - Accept.Width - 10f), -(Height * 0.025f));
            Accept.OnClick += Accept_OnClick;

            Icon_GiftMode.Anchor = Anchor.BottomLeft;
            Icon_GiftMode.Size = new Vector2((Accept.Height * 1.5f), (Accept.Height * 1.5f));
            Icon_GiftMode.Offset = new Vector2((Accept.Offset.x - Icon_GiftMode.Width - 2f), (Accept.Offset.y + 6f));
            Icon_GiftMode.Style.Color = Globals.GetColor("Trade.GiftMode");
            Icon_GiftMode.IsVisible = false;

            Populate();
            UpdateLabels();

            // Sort by Name
            Player.DoOnSortClick(Player.Sort_Name, EventArgs.Empty);
            Trader.DoOnSortClick(Trader.Sort_Name, EventArgs.Empty);

            Transaction_Value.Offset = new Vector2((ButtonClose.OffsetRight - 105f), (ButtonClose.Offset.y - 25f));
            Transaction_Count.Offset = new Vector2((ButtonClose.OffsetRight - 205f), Transaction_Value.Offset.y);
            Transaction_Mass.Offset = new Vector2((ButtonClose.OffsetRight - 305f), Transaction_Value.Offset.y);

            Transaction_Value.Anchor = Anchor.BottomRight;
            Transaction_Count.Anchor = Anchor.BottomRight;
            Transaction_Mass.Anchor = Anchor.BottomRight;

            Transaction_Value.Style.TextAnchor = TextAnchor.MiddleRight;
            Transaction_Count.Style.TextAnchor = TextAnchor.MiddleRight;
            Transaction_Mass.Style.TextAnchor = TextAnchor.MiddleRight;

            // Explain what these are to reduce confusion.
            Transaction_Value.ToolTipText = "Transaction_Value".Translate();
            Transaction_Count.ToolTipText = "Transaction_Count".Translate();
            Transaction_Mass.ToolTipText = "Transaction_Mass".Translate();
            Transaction_Value.IgnoreMouse = false;
            Transaction_Count.IgnoreMouse = false;
            Transaction_Mass.IgnoreMouse = false;
        }

        public void UpdateLabels()
        {
            bool CanFinalize = true;

            Player.UpdateLabels();
            Trader.UpdateLabels();

            float Traded_FromPlayer = Trader.Incoming.Sum((F) => F.Value_Total);
            float Traded_FromTrader = Player.Incoming.Sum((F) => F.Value_Total);

            float Balance = (Traded_FromPlayer - Traded_FromTrader);

            int Count_FromPlayer = Trader.Incoming.Sum((F) => F.Count);
            int Count_FromTrader = Player.Incoming.Sum((F) => F.Count);

            float Mass_FromPlayer = Trader.Incoming.Sum((F) => F.Mass_Total);
            float Mass_FromTrader = Player.Incoming.Sum((F) => F.Mass_Total);

            Transaction_Value.Text += $"{Balance,5:0.00}{"DollarSign".Translate()}";
            Transaction_Count.Text += $"{(Count_FromTrader - Count_FromPlayer),5:0.00}x";
            Transaction_Mass.Text += $"{(Mass_FromTrader - Mass_FromPlayer),5:0.00}{"Kg".Translate()}";

            // If the trade itself isn't enough, check if we have enough Silver to compensate.
            if (Balance < 0)
            {
                int Silver_Player = TradeSession.deal.CurrencyTradeable.CountHeldBy(Transactor.Colony);
                int Silver_Trader = TradeSession.deal.CurrencyTradeable.CountHeldBy(Transactor.Trader);

                ListViewItem_Trade Silver_Intransit_Player = Trader.Incoming.FirstOrDefault((F) => (F.ThingDef == ThingDefOf.Silver));
                ListViewItem_Trade Silver_Intransit_Trader = Player.Incoming.FirstOrDefault((F) => (F.ThingDef == ThingDefOf.Silver));

                // Remove the Silver that's being traded from our calculation
                if (Silver_Intransit_Player != null)
                {
                    Silver_Player -= Silver_Intransit_Player.Count;
                }
                if (Silver_Intransit_Trader != null)
                {
                    Silver_Trader -= Silver_Intransit_Trader.Count;
                }

                float Silver_Value = ((Silver_Player - Silver_Trader) * TradeSession.deal.CurrencyTradeable.BaseMarketValue);

                Balance += Silver_Value;

                CanFinalize = false;
            }

            // Change Text color based on if Can afford
            if (Balance >= 0)
            {
                Transaction_Value.Style.TextColor = Globals.GetColor("Trade.CanAfford");
            }
            else
            {
                Transaction_Value.Style.TextColor = Globals.GetColor("Trade.CantAfford");
            }

            float MassCapacity_Player = 0f;
            float MassCapacity_Trader = 0f;

            // Caravan capacity for Player only matters if they're on the move on the World Map.
            if ((Player.Caravan != null) && Player.Caravan.pather.Moving)
            {
                MassCapacity_Player = Player.GetCaravanRemainingMassCapacity();
            }
            if (Trader.Caravan != null)
            {
                MassCapacity_Trader = Trader.GetCaravanRemainingMassCapacity();
            }

            // Change Text color based on if there's a Caravan with enough carrying capacity
            if ((MassCapacity_Player < 0f) || (MassCapacity_Trader < 0f))
            {
                CanFinalize = false;
                Transaction_Mass.Style.TextColor = Globals.GetColor("Trade.CantAfford");
            }
            else
            {
                Transaction_Mass.Style.TextColor = Globals.GetColor("Trade.CanAfford");
            }

            Accept.IsEnabled = CanFinalize;
        }

        /// <summary>
        /// Populates Data based on <see cref="TradeSession"/>.
        /// </summary>
        public void Populate()
        {
            Merchant = TradeSession.trader;
            Negotiator = TradeSession.playerNegotiator;

            Player.Trader = Negotiator;
            Trader.Trader = (Pawn)Merchant;

            Player.Caravan = Negotiator.GetCaravan();
            // TODO: Merchant Caravan doesn't return a Caravan!!!
            Trader.Caravan = Trader.Trader.GetCaravan();

            Card_Trader = new TraderCard(Trader.Trader);
            Card_Negotiator = new TraderCard(Negotiator);
            Root.AddRange(Card_Trader, Card_Negotiator);

            Card_Trader.Offset = new Vector2(-Player.Offset.x, 5f);
            Card_Negotiator.Offset = new Vector2(Player.Offset.x, 5f);

            Card_Trader.Anchor = Anchor.TopRight;
            Card_Trader.RightAlign();

            List<(Tradeable Tradable, Thing Thing)> Buying = new List<(Tradeable Tradable, Thing Thing)>();
            List<(Tradeable Tradable, Thing Thing)> Selling = new List<(Tradeable Tradable, Thing Thing)>();

            foreach (Tradeable Tradable in TradeSession.deal.AllTradeables)
            {
                if (Tradable.thingsColony.Any())
                {
                    Buying.Add((Tradable, Tradable.FirstThingColony));
                }
                if (Tradable.thingsTrader.Any())
                {
                    Selling.Add((Tradable, Tradable.FirstThingTrader));
                }
            }

            foreach ((Tradeable Tradable, Thing Thing) ValuePair in Buying)
            {
                AddToPlayer(ValuePair.Tradable, ValuePair.Thing);
            }
            foreach ((Tradeable Tradable, Thing Thing) ValuePair in Selling)
            {
                AddToTrader(ValuePair.Tradable, ValuePair.Thing);
            }
        }

        public override void PreClose()
        {
            base.PreClose();

            Vanilla.Close();
        }

        public void AddToPlayer(Tradeable Tradable, Thing Thing)
        {
            ListViewItem_Trade Item = new ListViewItem_Trade(Tradable, Thing, true);
            Item.Origin = Player.Inventory;
            Player.AddItem(Item);
        }

        public void AddToTrader(Tradeable Tradable, Thing Thing)
        {
            ListViewItem_Trade Item = new ListViewItem_Trade(Tradable, Thing);
            Item.Origin = Trader.Inventory;
            Trader.AddItem(Item);
        }

        public void To(ListViewItem_Trade Item, int Amount)
        {
            TradeOverview Destination = ((Item.Owner == Player) ? Trader : Player);
            ListViewItem_Trade Existing = Destination.Inventory.Items.Cast<ListViewItem_Trade>().FirstOrDefault((F) => ((F.Tradable == Item.Tradable) && (F.Origin == Item.Origin)));

            // If there's already a exixting Item from the Transfered Item's Inventory, then increment that Item instead of adding a new entry.
            if (Existing != null)
            {
                Existing.Count += Amount;
                Existing.UpdateLabels();
                Existing.Owner.UpdateLabels();
            }
            else
            {
                ListViewItem_Trade TradeItem = new ListViewItem_Trade(Item.Tradable, Item.Thing, (Destination == Player));
                TradeItem.Origin = Item.Origin;
                TradeItem.Count = Amount;
                TradeItem.UpdateLabels();

                if (Destination.Inventory != Item.Origin)
                {
                    TradeItem.Header.Style.BorderThickness = 1;
                    TradeItem.Header.Style.DrawBorder = true;

                    if (Destination == Player)
                    {
                        // The Item Originated from the Player's Inventory, remove it from the Preview Transfer List.
                        // We're sending it back to the Player's Inventory.
                        if (Player.Inventory == Item.Origin)
                        {
                            Trader.Incoming.Remove(Item);
                        }
                        else
                        {
                            Player.Incoming.Add(TradeItem);
                        }

                        TradeItem.Header.Style.BorderColor = Globals.GetColor("Trade.FromTrader.ListViewItem_Trade.BorderColor");
                    }
                    else
                    {
                        if (Trader.Inventory == Item.Origin)
                        {
                            Player.Incoming.Remove(Item);
                        }
                        else
                        {
                            Trader.Incoming.Add(TradeItem);
                        }

                        TradeItem.Header.Style.BorderColor = Globals.GetColor("Trade.FromPlayer.ListViewItem_Trade.BorderColor");
                    }
                }

                Destination.AddItem(TradeItem);
            }

            UpdateLabels();
        }

        public void GiftMode_OnClick(object Sender, EventArgs EventArgs)
        {
            GiftMode.ToggleState = !GiftMode.ToggleState;

            if (GiftMode.ToggleState)
            {
                GiftMode.Style.BackgroundColor = Globals.GetColor("Trade.GiftMode");
                Icon_GiftMode.IsVisible = true;
                Accept.Style.DrawBorder = true;
            }
            else
            {
                // Reset the Background Color.
                GiftMode.Style.BackgroundColor = (Color)GiftMode.Data;
                Icon_GiftMode.IsVisible = false;
                Accept.Style.DrawBorder = false;
            }
        }

        public void Accept_OnClick(object Sender, EventArgs EventArgs)
        {
            // TODO: Perform Trade finalization

            // Remember to take silver into account
        }
    }
}
