using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Trade
{
    public class ListViewItem_Trade : ListViewItem
    {
        public TradeOverview Owner;
        public ThingDef ThingDef;
        private int _Count;
        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                if (_Count != value)
                {
                    _Count = value;

                    // Amount of Items have changed, therefor Total Value & Weight would also have changed.
                    Value_Total = (Value * _Count);
                    Mass_Total = (Mass * _Count);

                    UpdateLabels();
                }
            }
        }
        public float Value;
        public float Value_Total;
        public float Mass;
        public float Mass_Total;
        public float Efficiency;
        public ListView Origin;
        public Thing Thing;
        public Tradeable Tradable;
        public Label Label_Value = new Label(true);
        public Label Label_Value_Total = new Label(true);
        public Label Label_Count = new Label(true);
        public Label Label_Mass = new Label(true);
        public Label Label_Mass_Total = new Label(true);
        public Label Label_Efficiency = new Label(true);
        public Button Push = new Button("OneX".Translate());
        public TextInput TextInput = new TextInput(TextInputStyle.Numeric);

        public ListViewItem_Trade(Tradeable Tradable, Thing Thing, bool Player = false)
        {
            this.Thing = Thing;
            this.Tradable = Tradable;

            ThingDef = Thing.def;

            TradeAction TradeAction = (Player ? TradeAction.PlayerSells : TradeAction.PlayerBuys);
            Transactor Transactor = (Player ? Transactor.Colony : Transactor.Trader);

            Value = Tradable.GetPriceFor(TradeAction);
            Mass = ThingDef.BaseMass;
            Count = Mathf.Max(1, Tradable.CountHeldBy(Transactor));

            Efficiency = ((Value / Mass) * Count);

            UpdateLabels();

            Label_Value.Height = Height;
            Label_Value_Total.Height = Height;
            Label_Count.Height = Height;
            Label_Mass.Height = Height;
            Label_Mass_Total.Height = Height;
            Label_Efficiency.Height = Height;

            Label_Value.Anchor = Anchor.TopRight;
            Label_Value_Total.Anchor = Anchor.TopRight;
            Label_Count.Anchor = Anchor.TopRight;
            Label_Mass.Anchor = Anchor.TopRight;
            Label_Mass_Total.Anchor = Anchor.TopRight;
            Label_Efficiency.Anchor = Anchor.TopRight;

            Label_Efficiency.Offset = new Vector2(-455f, 0f);
            Label_Mass.Offset = new Vector2(-380f, 0f);
            Label_Mass_Total.Offset = new Vector2(-305f, 0f);
            Label_Count.Offset = new Vector2(-250f, 0f);
            Label_Value.Offset = new Vector2(-175f, 0f);
            Label_Value_Total.Offset = new Vector2(-95f, 0f);

            AddRange(Push, TextInput, Label_Value, Label_Value_Total, Label_Count, Label_Mass, Label_Mass_Total, Label_Efficiency);

            ToolTipText = ThingDef.DescriptionDetailed;

            if (ThingDef.category == ThingCategory.Pawn)
            {
                // Animals are Pawns too!
                Pawn Pawn = (Pawn)Thing;

                if (ThingDef.race.hasGenders)
                {
                    Text += Globals.GetGenderUnicodeSymbol(Pawn.gender) + " ";
                }

                // Pad the number to the right a bit
                Text += $"{Pawn.ageTracker.AgeNumberString,2:###} ";

                // Species
                Text += Pawn.kindDef.LabelCap + " ";

                if (Pawn.RaceProps.Animal)
                {
                    ToolTipText += TrainableUtility.GetIconTooltipText(Pawn);

                    if (TrainableUtility.GetAllColonistBondsFor(Pawn).Any())
                    {
                        Text += "- " + "Bonded".Translate() + " ";
                    }
                }

                if (Pawn.health.hediffSet.HasHediff(HediffDefOf.Pregnant, true))
                {
                    Text += PawnColumnWorker_Pregnant.GetTooltipText(Pawn) + " ";
                }

                Text += "- " + Pawn.Name.ToStringFull + " ";

                if (Pawn.equipment != null)
                {
                    // Add anything they're wearing to their total value
                    foreach (ThingWithComps Equipment in Pawn.equipment.AllEquipmentListForReading)
                    {
                        Value += Equipment.MarketValue;
                    }
                }
            }
            else
            {
                Text = ThingDef.LabelCap;
            }

            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Label.Offset = new Vector2((Height + 5f), 0);

            if (Thing != null)
            {
                ThingStyleDef StyleDef = Thing.StyleDef;

                if ((StyleDef != null) && !StyleDef.overrideLabel.NullOrEmpty())
                {
                    Text = StyleDef.overrideLabel;
                }

                CompQuality Quality = Thing.TryGetComp<CompQuality>();

                if (Quality != null)
                {
                    Header.Label.Style.TextColor = Globals.TryGetColor(Quality.Quality.GetLabel().CapitalizeFirst(), "White");
                }

                if (Thing.HitPoints > 0)
                {
                    Text += " (" + ((float)Thing.HitPoints / (float)Thing.MaxHitPoints).ToStringPercent() + ")";
                }

                if (ThingDef.IsApparel && ((Apparel)Thing).WornByCorpse)
                {
                    Text += " (" + "Tainted".Translate() + ")";
                }
            }

            Image = new Image(ThingDef.uiIcon);
            Image.Size = new Vector2(Height, Height);
            Image.SetStyle("ListViewItem_Trade.Image");
            Image.Style.DrawMouseOver = true;
            Image.Style.Color = ThingDef.uiIconColor;
            Image.ScaleMode = ScaleMode.ScaleToFit;
            Image.OnMouseEnter += Architect.ListViewItemGroup_Architect.Image_OnMouseEnter;
            Image.OnClick += OpenInfo;
            AddChild(Image);

            TextInput.SetStyle("ListViewItem_Trade.TextInput");
            TextInput.Min = 0f;
            TextInput.Max = Count;
            TextInput.Size = new Vector2(45f, (Height - 2f));
            TextInput.Offset = new Vector2(-70f, 0f);
            TextInput.Anchor = Anchor.CenterRight;
            TextInput.Style.TextOffset = new Vector2(-2f, 0f);
            TextInput.OnTextChanged += (obj, e) =>
            {
                if (TextInput.Numeric >= TextInput.Max)
                {
                    // Indicate that the Max amount of items are set for transfer.
                    TextInput.Style.TextColor = Globals.GetColor("ListViewItem_Trade.TextInput.MaxedOut.TextColor");
                    TextInput.Style.MouseOverTextColor = Globals.GetColor("ListViewItem_Trade.TextInput.MaxedOut.MouseOverTextColor");
                }
                else
                {
                    // Reset Colors.
                    TextInput.Style.TextColor = Globals.GetColor("ListViewItem_Trade.TextInput.TextColor");
                    TextInput.Style.MouseOverTextColor = Globals.GetColor("ListViewItem_Trade.TextInput.MouseOverTextColor");
                }
            };

            Push.Size = new Vector2(35f, (Height - 2f));
            Push.Offset = new Vector2(-20f, 0f);
            Push.Anchor = Anchor.CenterRight;
            Push.Style.DrawBackground = true;
            Push.ToolTipText = "TradeOne".Translate();

            Push.OnClick += Transfer;

            Push.OnMouseEnter += (obj, e) =>
            {
                // TODO: fires rapidly, likely multiple per frame
                // Allow Drag-Input to work when holding down mouse and going onto this Button from a previous Item's Button.
                // Make sure the mouse is actually moved onto this button, instead of the button jumping up while items are being transfered and the list shrinks.
                if (WindowManager.IsMouseDownCurrently() && ((Math.Abs(WindowManager.MousePositionDelta().x) >= (Push.Height * 0.25f)) || (Math.Abs(WindowManager.MousePositionDelta().y) >= (Push.Height * 0.25f))))
                {
                    Transfer(obj, e);
                }
            };

            Push.OnWhileMouseOver += (obj, e) =>
            {
                if (WindowManager.IsCtrlDown() && WindowManager.IsShiftDown())
                {
                    Push.Text = "TenX".Translate();
                    Push.ToolTipText = "TradeTen".Translate();
                }
                else if (WindowManager.IsShiftDown())
                {
                    Push.Text = "FiveX".Translate();
                    Push.ToolTipText = "TradeFive".Translate();
                }
                else if (WindowManager.IsAltDown())
                {
                    Push.Text = "All".Translate();
                    Push.ToolTipText = "TradeAll".Translate();
                }
                else
                {
                    Push.Text = "OneX".Translate();
                    Push.ToolTipText = "TradeOne".Translate();
                }
            };

            Push.OnMouseLeave += (obj, e) =>
            {
                Push.Text = "OneX".Translate();
                Push.ToolTipText = "TradeOne".Translate();
            };

            IsEnabled = Tradable.TraderWillTrade;

            // Hide these when the Trader isn't willing to trade this Item
            TextInput.IsVisible = IsEnabled;
            Push.IsVisible = IsEnabled;
        }

        public void UpdateLabels()
        {
            Label_Value.Text = Value.ToString("0.00") + "DollarSign".Translate();
            Label_Value_Total.Text = "(" + (Value * Count).ToString("0.00") + ")";
            Label_Count.Text = Count.ToString() + "x";
            Label_Mass.Text = Mass.ToString("0.00") + "Kg".Translate();
            Label_Mass_Total.Text = "(" + (Mass * Count).ToString("0.00") + ")";
            Label_Efficiency.Text = Efficiency.ToString("0.00");
        }

        public void Transfer(object Sender, EventArgs EventArgs)
        {
            int Amount = 1;

            if (WindowManager.IsCtrlDown() && WindowManager.IsShiftDown())
            {
                Amount = 10;
            }
            else if (WindowManager.IsShiftDown())
            {
                Amount = 5;
            }
            else if (WindowManager.IsAltDown())
            {
                Amount = Count;
            }

            Amount = Math.Min(Amount, Count);

            Owner.Trade.To(this, Amount);
            Count -= Amount;

            if (Count <= 0)
            {
                // Delete this Item if we're transfering all of it
                RemoveFromItemParent();
            }
            else
            {
                UpdateLabels();
            }

            Owner.UpdateLabels();
        }

        public void OpenInfo(object Sender, MouseEventArgs EventArgs)
        {
            // Modified from Widgets.InfoCardButton
            Dialog_InfoCard Dialog_InfoCard = new Dialog_InfoCard(ThingDef, Thing.Stuff, Thing.GetStyleSourcePrecept());
            // Needs to be above this Item's Parent Window.
            Dialog_InfoCard.layer = WindowLayer.Super;
            WindowManager.OpenWindowVanilla(Dialog_InfoCard);
        }
    }
}
