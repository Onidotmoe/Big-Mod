using RimWorld;
using System.Text;
using Verse;
using Verse.AI;

namespace BigMod.Entities.Windows.Overview
{
    public enum ItemStyle
    {
        Line,
        Square
    }

    public class ListViewItem_Inventory : ListViewItem
    {
        private ItemStyle _RenderStyle = ItemStyle.Line;
        public ItemStyle RenderStyle
        {
            get
            {
                return _RenderStyle;
            }
            set
            {
                if (_RenderStyle != value)
                {
                    _RenderStyle = value;

                    switch (value)
                    {
                        case ItemStyle.Line:
                            DoStyleLine();
                            break;

                        case ItemStyle.Square:
                            DoStyleSquare();
                            break;
                    }
                }
            }
        }
        public Thing Thing;
        public List<Thing> Things;
        /// <summary>
        /// Just a sugarcoat to access <see cref="ListViewItem.Header.Label"/>.
        /// </summary>
        public Label Name
        {
            get
            {
                return Header.Label;
            }
            set
            {
                Header.Label = value;
            }
        }
        /// <summary>
        /// Size when in <see cref="ItemStyle.Square"/> mode.
        /// </summary>
        public Vector2 SquareSize = new Vector2(50f, 50f);
        /// <summary>
        /// <para>Total Amount of Items, note that these are stacked visually in the GUI and can exceed the StackLimit for items.</para>
        /// <para>Defaults to 1.</para>
        /// </summary>
        public int Count = 1;
        public Label Amount = new Label(true);
        /// <summary>
        /// Is this Item currently Reserved by Anyone in the Player's Faction?
        /// </summary>
        public bool InTransit
        {
            get
            {
                return Thing.MapHeld.reservationManager.IsReservedByAnyoneOf(Thing, Faction.OfPlayer);
            }
        }
        public bool CanDrop;
        /// <summary>
        /// Can this Item ever be removed?
        /// </summary>
        /// <remarks>Basic clothing cannot be removed manually.</remarks>
        public bool CanRemove = true;
        public Button Drop = new Button(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("Drop"));
        /// <summary>
        /// Can this Item be consumed?
        /// </summary>
        /// <remarks>Requires that it's on a Pawn that is willing to consume it.</remarks>
        public bool CanEat
        {
            get
            {
                Pawn Pawn = (Thing?.ParentHolder?.ParentHolder as Pawn);

                if (Pawn != null)
                {
                    return FoodUtility.WillIngestFromInventoryNow(Pawn, Thing);
                }

                return false;
            }
        }
        public Button Eat = new Button(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("Eat"));

        /// <summary>
        /// Panel that the Item will return to if Dropped in a invalid place.
        /// </summary>
        public Panel Fallback;
        private bool _IsForced;
        /// <summary>
        /// If equipped by a Pawn, was the Pawn forced to equip it?
        /// </summary>
        public bool IsForced
        {
            get
            {
                return _IsForced;
            }
            set
            {
                if (_IsForced != value)
                {
                    _IsForced = value;
                    UpdateToolTipIcon();
                }
            }
        }
        /// <summary>
        /// Additional ToolTip information icon.
        /// </summary>
        public Image ToolTipIcon;
        public ProgressBar Condition = new ProgressBar();

        /// <summary>
        /// ListViewItem for <see cref="Equipment"/> and <see cref="InventoryPanel"/> inside the <see cref="OverviewPawn"/> window.
        /// </summary>
        /// <param name="Thing">Thing to Create ListViewItem for.</param>
        /// <param name="CanDrop">If this Item can be dropped or not. Used for when the Item is in storage.</param>
        public ListViewItem_Inventory(Thing Thing, bool CanDrop = true)
        {
            this.Thing = Thing;

            if (Thing.def.IsApparel && (Thing.ParentHolder is Pawn_ApparelTracker Tracker))
            {
                CanRemove = Tracker.IsLocked((Apparel)Thing);
            }
            if (Thing.ParentHolder?.ParentHolder is Pawn Owner)
            {
                // Don't show the Drop Button if the Owner isn't in the Player faction.
                CanDrop = (CanDrop && ((Owner.Faction == Faction.OfPlayer) || Owner.Dead));
            }

            // Embedded Weapons like the Mechanoids weapons cannot be removed.
            CanRemove = CanRemove && !Thing.def.destroyOnDrop;

            this.CanDrop = (CanDrop && CanRemove && !InTransit);

            UpdateToolTipIcon();
            UpdateToolTipText();

            Drop.OnClick += (obj, e) => DoDrop();
            Eat.OnClick += (obj, e) => DoEat();

            Image = new Image();
            Image.OnClickRight += OpenInfo;

            Condition.IsVisible = Thing.def.useHitPoints;

            AddRange(Condition, Image, Amount, Drop, Eat);

            DoStyleLine();

            Pull();
        }
        private void DoStyleLine()
        {
            UseAnchoring = false;
            Size = MinSize;

            Header.Style.DrawBackground = true;

            Condition.Anchor = Anchor.BottomLeft;
            Condition.InheritParentSize = true;
            Condition.LimitToParent = true;
            Condition.MaxSize = new Vector2(0f, (Height * 0.1f));
            // Reset Offset when going back from Square RenderStyle.
            Condition.Offset = Vector2.zero;

            Condition.ColorMin = Globals.GetColor("ListViewItem_Inventory.Condition.Style.Line.ColorMin");
            Condition.ColorMax = Globals.GetColor("ListViewItem_Inventory.Condition.Style.Line.ColorMax");

            Image.SetStyle("ListViewItem_Inventory.Image");
            Image.Style.DrawMouseOver = true;
            Image.RenderStyle = ImageStyle.Fitted;
            Image.ScaleMode = ScaleMode.ScaleToFit;
            Image.Size = new Vector2(Height, Height);
            Image.OnMouseEnter += Architect.ListViewItemGroup_Architect.Image_OnMouseEnter;

            if (Widgets.GetIconFor(Thing, Image.Size, Thing.Rotation, (Thing.stackCount == 1), out float Scale, out float Angle, out Vector2 Proportions, out Color Color) is Texture2D Texture)
            {
                Image.Texture = Texture;
            }
            else
            {
                Image.Texture = ((Thing.Stuff != null) ? Thing.Stuff.uiIcon : Thing.def.uiIcon);
            }

            Image.Style.Color = Color;

            Drop.Anchor = Anchor.TopRight;
            Drop.Size = new Vector2(Height, Height);
            Drop.Image.Style.Color = Color.white;
            Drop.ToolTipText = "DropThing".Translate();
            Drop.IsVisible = CanDrop;

            Eat.Anchor = Anchor.TopRight;
            Eat.Size = new Vector2(Height, Height);
            Eat.Image.Style.Color = Color.white;
            Eat.ToolTipText = "ConsumeThing".Translate(Thing.LabelShort, Thing);
            Eat.IsVisible = CanEat;
            Eat.OffsetX = -Drop.Width;

            Name.IsVisible = true;
            Name.Text = Thing.LabelCapNoCount;
            Name.Style.TextAnchor = TextAnchor.MiddleLeft;
            Name.Size = new Vector2((Width / 2), Height);
            Name.Offset = new Vector2((Image.Width + 2f), 0);
            Name.InheritParentSize = true;
            Name.LimitToParent = true;

            Amount.IsVisible = true;
            Amount.Anchor = Anchor.TopRight;
            Amount.Style.TextAnchor = TextAnchor.MiddleRight;
            Amount.Text = Count.ToString() + "x";
            Amount.Height = Height;
            Amount.OffsetX = (-5f + (CanDrop ? -Drop.Width : 0f));
        }

        private void DoStyleSquare()
        {
            UseAnchoring = true;
            Header.Size = SquareSize;
            Size = SquareSize;

            Condition.InheritParentSize = false;
            Condition.LimitToParent = false;
            Condition.Anchor = Anchor.TopLeft;
            Condition.MaxSize = Vector2.zero;
            Condition.Size = new Vector2(Header.Width, (Header.Height * 0.1f));
            Condition.Offset = new Vector2(0f, Header.Height);

            Condition.ColorMin = Globals.GetColor("ListViewItem_Inventory.Condition.Style.Square.ColorMin");
            Condition.ColorMax = Globals.GetColor("ListViewItem_Inventory.Condition.Style.Square.ColorMax");

            // Disable for when it goes into a Equipment_Slot to allow the background CanAccept color to be shown.
            Header.Style.DrawBackground = false;

            Image.Size = SquareSize;
            Image.OnMouseEnter -= Architect.ListViewItemGroup_Architect.Image_OnMouseEnter;

            // Only visible on MouseOver
            Drop.IsVisible = false;
            Drop.Anchor = Anchor.BottomRight;
            Drop.MoveToBack();

            Eat.IsVisible = false;
            Eat.Anchor = Anchor.BottomRight;
            Eat.MoveToBack();

            Name.IsVisible = false;

            // Don't show Amount Label if there's only 1.
            Amount.IsVisible = (Count > 1);
            Amount.Anchor = Anchor.BottomRight;
            Amount.OffsetX = 0f;
        }

        public void Pull()
        {
            Name.Text = Thing.LabelCap;

            if (Condition.IsVisible)
            {
                // Negative Hitpoints values are not valid.
                Condition.Percentage = ((float)((Thing.HitPoints >= 0) ? Thing.HitPoints : Thing.MaxHitPoints) / (float)Thing.MaxHitPoints);
            }

            UpdateToolTipText();

            UpdateEat();
        }

        public void UpdateToolTipText()
        {
            // TODO: Regardless of MouseOver and updating the text, the ToolTip will blink periodically while visible. Maybe something todo with the item being readded?

            // Thing.LabelNoParenthesisCap returns a key already exist exception, so we're doing this instead.
            int Start = Thing.LabelCap.LastIndexOf('(');

            if (Start != -1)
            {
                ToolTipText = Thing.LabelCap.Substring(0, (Start - 1));
            }
            else
            {
                ToolTipText = Thing.LabelCap;
            }

            ToolTipText += GenLabel.LabelExtras(Thing, Count, true, true)
                + "\n\n" + Thing.DescriptionDetailed
                + "\n" + Thing.GetInspectString()
                + $"\n\n{"DollarSign".Translate()}{((int)(Thing.def.BaseMarketValue * Count)),5:0.00}"
                + "\n" + (Thing.def.BaseMass * Count).ToStringMass()
                + (Thing.def.useHitPoints ? $"\n{Thing.HitPoints} / {Thing.MaxHitPoints}" : string.Empty);
        }
        public void AddToolTipIcon()
        {
            if (ToolTipIcon == null)
            {
                ToolTipIcon = new Image(Globals.TryGetTexturePathFromAlias("Locked"), (9f * Prefs.UIScale), (9f * Prefs.UIScale));
                ToolTipIcon.Style.Color = Globals.GetColor("ListViewItem_Inventory.ToolTipIcon.Color");
                ToolTipIcon.Anchor = Anchor.TopRight;
                ToolTipIcon.Offset = new Vector2(1f, 1f);
                AddChild(ToolTipIcon);
            }
        }

        public void RemoveToolTipIcon()
        {
            if (ToolTipIcon != null)
            {
                RemoveChild(ToolTipIcon);
                ToolTipIcon = null;
            }
        }

        public void UpdateToolTipIcon()
        {
            // HoldingOwner is false when the Item is on a Pawn.
            if (!Thing.holdingOwner.OfType<Pawn>().Any() || InTransit)
            {
                StringBuilder Builder = new StringBuilder();

                if (InTransit)
                {
                    Builder.AppendLine("InTransit".Translate());
                }
                if (!CanDrop)
                {
                    Builder.AppendLine("CantDrop".Translate());
                }
                if (IsForced)
                {
                    Builder.AppendLine("ForcedApparel".Translate());
                }
                if (!CanRemove)
                {
                    Builder.AppendLine("MessageCantUnequipLockedApparel".Translate());
                }

                if (!string.IsNullOrWhiteSpace(Builder.ToString()))
                {
                    AddToolTipIcon();
                    ToolTipIcon.ToolTipText = Builder.ToString();
                }
                else
                {
                    RemoveToolTipIcon();
                }
            }
            else
            {
                RemoveToolTipIcon();
            }
        }
        public void UpdateEat()
        {
            Eat.IsVisible = !InTransit && CanEat;
        }
        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            if (GetToolTipItem(out ListViewItem_Inventory Item))
            {
                // ToolTip items block OnClick events.
                return;
            }

            base.DoOnClick(Sender, EventArgs);

            if (!Globals.HandleModifierSelection(Thing.def, Thing.Stuff) && !Drop.IsMouseOver && !Eat.IsMouseOver && CanRemove)
            {
                // Is this Item is in a Slot that doesn't allow it to be drag removed?
                if ((Parent is not Equipment_Slot Slot) || Slot.CanDragRemoveItem)
                {
                    AttachToMouse();
                }
            }
        }

        public override void DoOnClickRight(object Sender, MouseEventArgs EventArgs)
        {
            if (GetToolTipItem(out ListViewItem_Inventory Item))
            {
                // ToolTip items block OnClick events.
                return;
            }

            base.DoOnClickRight(Sender, EventArgs);

            // Shift-Right-Clicking send it out to a alternative location based on where it is right now.
            if (WindowManager.IsShiftDown())
            {
                if (ListView?.Parent is InventoryPanel Inventory)
                {
                    if (Equipment.GetInstance(out Equipment Instance) && (Instance.ParentWindow == ParentWindow))
                    {
                        // Try to move however much is capable of going into the Equipment Slot and leave the rest where it is now.
                        Instance.AddItem(this);
                    }
                }
                else if (Parent is Equipment_Slot Slot)
                {
                    // Ensure that it's allowed to be moved
                    // Make sure to still check if it's actually in a Slot.
                    if (CanRemove && OverviewPawn.GetInstance(out OverviewPawn Instance))
                    {
                        if (!Instance.Inventory.TryEmbed(this))
                        {
                            // If it can't go into the Pawn's Inventory push it to Storage instead.
                            Instance.Storage.TryEmbed(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Splits this Item into another Item with the specified size.
        /// </summary>
        /// <param name="Output">Count of the Output Item.</param>
        /// <returns>New Item with the specified size.</returns>
        public ListViewItem_Inventory Split(int Output)
        {
            // Make sure we don't try to remove more than we have.
            Output = Math.Min(Output, Count);

            ListViewItem_Inventory Item = new ListViewItem_Inventory(Thing, CanDrop);
            Item.Fallback = Fallback;
            Item.Count = Output;
            Item.Amount.Text = Output.ToString() + "x";

            Count -= Output;
            Amount.Text = Count.ToString() + "x";

            return Item;
        }

        /// <summary>
        /// <para>Returns this Item to its Fallback location, if it has one.</para>
        /// <para>Fallback is only valid for <see cref="Equipment_Slot"/> if the Slot is empty.</para>
        /// </summary>
        public void DoFallback()
        {
            if (Fallback is InventoryPanel Inventory)
            {
                Inventory.TryEmbed(this);
            }
            else if ((Fallback is Equipment_Slot Slot) && (Slot.Item == null))
            {
                Slot.AddItem(this);
            }
            else
            {
                if (GetHostOverviewPawn(out OverviewPawn OverviewPawn))
                {
                    Pawn Pawn = (Thing?.ParentHolder?.ParentHolder as Pawn);

                    if ((Pawn != null) && (Pawn == OverviewPawn.Pawn))
                    {
                        // Move the Swapped Item to the Pawn's Inventory if its ToolTip is dropped in a invalid place.
                        OverviewPawn.Inventory?.TryEmbed(this);
                    }
                    else
                    {
                        // Move it to Storage if it's not currently on this Pawn.
                        OverviewPawn.Storage?.TryEmbed(this);
                    }
                }
            }
        }

        public void DoDrop()
        {
            if (InterfaceDrop(Thing))
            {
                RemoveFromItemParent();
            }
        }

        public void DoEat()
        {
            if (InterfaceEat(Thing))
            {
                RemoveFromItemParent();
            }
        }
        public void OpenInfo(object Sender, MouseEventArgs EventArgs)
        {
            // Right-click is using the Shift Modifier.
            if (!WindowManager.IsShiftDown())
            {
                // Modified from Widgets.InfoCardButton
                if (Thing is IConstructible Constructible)
                {
                    if (Thing.def.entityDefToBuild is ThingDef ThingDef)
                    {
                        WindowManager.OpenWindowVanilla(new Dialog_InfoCard(ThingDef, Constructible.EntityToBuildStuff(), null));
                    }
                    else
                    {
                        WindowManager.OpenWindowVanilla(new Dialog_InfoCard(Thing.def.entityDefToBuild, null));
                    }
                }
                else
                {
                    WindowManager.OpenWindowVanilla(new Dialog_InfoCard(Thing, null));
                }
            }
        }

        public override void DoOnMouseEnter(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnMouseEnter(Sender, EventArgs);

            if ((RenderStyle == ItemStyle.Square) && CanDrop)
            {
                Drop.IsVisible = true;
            }
        }

        public override void DoOnMouseLeave(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnMouseLeave(Sender, EventArgs);

            if ((RenderStyle == ItemStyle.Square) && CanDrop)
            {
                Drop.IsVisible = false;
            }
        }

        public override Vector2 GetAbsolutePosition()
        {
            // Modified from ListViewItem.GetAbsolutePosition to check for ListView null.
            return (Position + ((ListView != null) ? ListView.Position : Vector2.zero) + ParentWindow.Position - ((ListView != null) ? ListView.ScrollPosition : Vector2.zero));
        }

        public bool GetHostOverviewPawn(out OverviewPawn OverviewPawn)
        {
            OverviewPawn = (Fallback ?? ListView)?.GetAncestor<OverviewPawn>();

            return (OverviewPawn != null);
        }

        public void AttachToMouse()
        {
            // TODO: Redoing it :
            //  Only Equipment and InventoryPanel can have this ListViewItem.

            // Holding Shift down, keeps the item held by the cursor.









            // TODO: scrap this and redo it
            if ((Parent is Equipment_Slot Slot) && !Slot.CanDragRemoveItem)
            {
                // This Item is in a Slot that doesn't allow it to be drag removed.
                return;
            }

            // Modified from ListViewItem_Pawn.AttachToMouse
            ToolTip ToolTip = new ToolTip();
            ToolTip.ManualDisposal = true;
            ToolTip.Identifier = "PopOut_Preview_ListViewItem_Inventory";
            ToolTip.Size = new Vector2((Width + 5f), (Height + 5f));
            ToolTip.Offset = new Vector2(-8f, -8f);
            ToolTip.DrawBackground = false;
            ToolTip.DrawBorder = false;
            ToolTip.IgnoreMouseInput = false;

            ListView ListView = new ListView(Width, Height, false);
            ListView.Style.DrawBackground = false;
            ListView.MinSize = ((RenderStyle == ItemStyle.Square) ? SquareSize : ListView.MinSize);
            ListView.RenderStyle = ((RenderStyle == ItemStyle.Square) ? ListViewStyle.Grid : ListViewStyle.List);
            ListView.ExtendItemsHorizontally = (ListView.RenderStyle == ListViewStyle.List);
            ToolTip.Root.AddChild(ListView);

            // RemoveFromItemParent doesn't remove it from the Slot.
            RemoveFromParent();
            RemoveFromItemParent();
            ListView.AddItem(this);

            // Ignore Mouse while being dragged.
            IgnoreMouse = true;

            if (Equipment.GetInstance(out Equipment Instance))
            {
                Instance.DisplayCanAccept(Thing);
            }

            // TODO: can't drop a single or multiplier amounts anymore, instead of hold and drag it has to be click and drag so you can drop specific amounts
            ToolTip.Root.OnMouseUp += (obj, e) =>
            {
                IgnoreMouse = false;

                List<Panel> Entities = WindowManager.GetEntitiesUnderMouse().Except(this).Where((F) => F.IsVisible).ToList();

                // A Inventory Item can only go into a InventoryPanel, Equipment_Slot, or a GroupItem.
                ListViewItem_Inventory Item = Entities.OfType<ListViewItem_Inventory>().FirstOrDefault();
                InventoryPanel Inventory = Entities.OfType<InventoryPanel>().FirstOrDefault();
                Equipment_Slot Slot = Entities.OfType<Equipment_Slot>().FirstOrDefault();

                bool Handled = false;

                if ((Inventory != null) && (Inventory.TryEmbed(this) || (Count <= 0)))
                {
                    Handled = true;

                    if (Item != null)
                    {
                        // Has to be removed first otherwise it would be added twice below.
                        RemoveFromItemParent();

                        if (Item.Group != null)
                        {
                            Item.Group.InsertItem(Item.Group.Items.IndexOf(Item), this);
                        }
                        else if (Item.ListView != null)
                        {
                            // Items in Slots don't have a ListView.
                            Item.ListView.InsertItem(Item.ListView.Items.IndexOf(Item), this);
                        }
                    }
                }
                else if (Slot != null)
                {
                    if (Slot.Item == null)
                    {
                        Handled = Slot.AddItem(this);
                    }
                    else
                    {
                        // Swap the existing item with the currently held item.
                        ListViewItem_Inventory Equipped = Slot.Item;
                        Equipped.RemoveFromItemParent();
                        Handled = Slot.AddItem(this);

                        Equipped.AttachToMouse();
                    }
                }

                if (!Handled)
                {
                    DoFallback();
                }

                ToolTip.Dispose();

                // Equipment might have been removed or changed meanwhile, try to get it again and reset the Display.
                if (Equipment.GetInstance(out Equipment Instance))
                {
                    Instance.DisplayCanAccept(null);
                }

                UpdateToolTipIcon();
            };

            BigMod.WindowManager.AddToolTip(ToolTip);
        }
        /// <summary>
        /// Used for Sorting items inside a <see cref="InventoryPanel"/>.
        /// </summary>
        /// <returns>Nameof a property which it qualifies as otherwise returns a empty string. And Order weight.</returns>
        public (string Sort, int Order) SortingType()
        {
            string Sorting = string.Empty;
            int Order = -10;

            switch (true)
            {
                // Order of operations here matters as some items like Wood IsStuff and IsWeapon but we want it to only be sorted by IsStuff.
                 case true when Thing.def.IsNonResourceNaturalRock:
                    Sorting = nameof(Thing.def.IsNonResourceNaturalRock);
                    Order = -5;
                    break;
                case true when Thing.def.IsStuff:
                    Sorting = nameof(Thing.def.IsStuff);
                    Order = -5;
                    break;
                case true when Thing.def.IsCorpse:
                    Sorting = nameof(Thing.def.IsCorpse);
                    Order = -4;
                    break;
                case true when Thing.def.IsRawFood():
                    Sorting = "IsRawFood";
                    Order = -3;
                    break;
                case true when Thing.def.IsProcessedFood:
                    Sorting = nameof(Thing.def.IsProcessedFood);
                    Order = -3;
                    break;
                case true when Thing.def.IsIngestible:
                    Sorting = nameof(Thing.def.IsIngestible);
                    Order = -3;
                    break;
                case true when Thing.def.IsDrug:
                    Sorting = nameof(Thing.def.IsDrug);
                    Order = -2;
                    break;
                case true when Thing.def.IsMedicine:
                    Sorting = nameof(Thing.def.IsMedicine);
                    Order = -1;
                    break;
                case true when Thing.def.IsApparel:
                    Sorting = nameof(Thing.def.IsApparel);
                    Order = 0;
                    break;
                case true when Thing.def.IsWeapon:
                    Sorting = nameof(Thing.def.IsWeapon);
                    Order = 1;
                    break;

                default:
                    break;
            }

            return (Sorting, Order);
        }
        /// <summary>
        /// <para>Removes <see cref="Thing"/> and <see cref="Things"/> from the ReservationManager.</para>
        /// <para>Cancels all Jobs targeting the <see cref="Thing"/> and <see cref="Things"/>.</para>
        /// </summary>
        public void ReleaseReservation()
        {
            ReservationManager Manager = Thing.MapHeld.reservationManager;

            if (Things != null)
            {
                foreach (Thing Thing in Things)
                {
                    Manager.ReleaseAllForTarget(Thing);
                    CancelJobsTargetingThing(Thing);
                }
            }
            else
            {
                Manager.ReleaseAllForTarget(Thing);
                CancelJobsTargetingThing(Thing);
            }
        }

        #region "Statics"

        public static void CancelJobsTargetingThing(Thing Thing)
        {
            Globals.WriteLineReset();
            Globals.WriteLine("---> CANCEL TARGET : ", Thing.ToString());

            foreach (Pawn Pawn in Thing.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (Pawn.IsColonistPlayerControlled || Pawn.IsColonyMechPlayerControlled)
                {
                    Globals.WriteLine("---> PAWN : " , Pawn.ToString());
                    // ToList creates a copy of the list to allow us to loop through all the jobs and modify them.
                    foreach (Job Job in Pawn.jobs.AllJobs().ToList())
                    {
                        if (Job.AnyTargetIs(Thing))
                        {
                            Globals.WriteLine("---> END JOB : ", Job.ToString());
                            Pawn.jobs.EndCurrentOrQueuedJob(Job, JobCondition.InterruptForced);
                        }
                    }
                }
                else
                {
                    Globals.WriteLine("---> PAWN NOT PLAYER CONTROLLED : ", Pawn.ToString());
                }
            }
        }

        /// <summary>
        /// Gets the Item currently in a ToolTip Preview Window.
        /// </summary>
        /// <param name="Item">Item in ToolTip Window.</param>
        /// <returns>True if Item isn't null.</returns>
        public static bool GetToolTipItem(out ListViewItem_Inventory Item)
        {
            ToolTip ToolTip = WindowManager.Instance.GetToolTip("PopOut_Preview_ListViewItem_Inventory");
            
            Item = ToolTip?.Root.GetChildrenFlatten().OfType<ListViewItem_Inventory>().FirstOrDefault();

            return (Item != null);
        }

        /// <summary>
        /// Is the given Thing currently being Targeted for Equipping or Wearing by the given Pawn.
        /// </summary>
        /// <param name="Pawn">Pawn to check if is going to equip or wear the Thing.</param>
        /// <param name="Thing">Thing to check if is a target for a job to equip or wear by the Pawn.</param>
        /// <returns>True if Pawn has a job that tells them to equip or wear the given Thing.</returns>
        public static bool InterfaceInTransit(Pawn Pawn, Thing Thing)
        {
            foreach (Job Job in Pawn.jobs.AllJobs())
            {
                if ((Job.def == JobDefOf.Equip) || (Job.def == JobDefOf.Wear))
                {
                    if (Job.AnyTargetIs(Thing))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tells a Pawn to go and Pickup the specified Thing and add it to their Inventory.
        /// </summary>
        /// <param name="Pawn">Pawn that has to do the work.</param>
        /// <param name="Thing">Thing to Pickup.</param>
        /// <param name="Amount">Optional Amount to Pickup.</param>
        /// <returns>True if job was successfully created.</returns>
        public static bool InterfacePickup(Pawn Pawn, Thing Thing, int Amount = 0)
        {
            if ((Thing.ParentHolder.ParentHolder != null) && (Thing.ParentHolder.ParentHolder is Pawn Owner && (Owner == Pawn)))
            {
                // Can't pickup if we're already carrying it.
                return false;
            }

            Amount = ((Amount == 0) ? Thing.stackCount : Amount);

            if (Amount <= 0)
            {
                return false;
            }

            // Pawns can only carry 1 stack of something.
            Amount = Math.Min(Thing.def.stackLimit, Amount);

            List<Thing> Items = Globals.FindAllOf(Thing.def, true);

            bool JobCreated = false;

            // Cycles through all available stacks of the specified ThingDef and creates jobs to fulfill the requested Amount.
            foreach (Thing Item in Items)
            {
                if (Amount > 0)
                {
                    Job Job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, Item);
                    Job.count = Math.Min(Amount, Item.stackCount);
                    Amount -= Job.count;

                    Pawn.jobs.TryTakeOrderedJob(Job, new JobTag?(JobTag.TakeForInventoryStock), true);

                    JobCreated = true;
                }
                else
                {
                    break;
                }
            }

            return JobCreated;
        }

        /// <summary>
        /// Tells a Pawn to go and Store the specified Thing.
        /// </summary>
        /// <param name="Pawn">Pawn that has to do the work.</param>
        /// <param name="Thing">Thing to Store.</param>
        /// <param name="Amount">Optional Amount to Store.</param>
        /// <returns>True if job was successfully created.</returns>
        public static bool InterfaceHaulToStorage(Pawn Pawn, Thing Thing, int Amount = 0)
        {
            if (Amount <= 0)
            {
                return false;
            }

            Job Job = HaulAIUtility.HaulToStorageJob(Pawn, Thing);

            if (Job == null)
            {
                return false;
            }

            Job.count = ((Amount == 0) ? Thing.stackCount : Amount);

            return Pawn.jobs.TryTakeOrderedJob(Job, new JobTag?(JobTag.UnloadingOwnInventory), true);
        }

        /// <summary>
        /// Tells a Pawn to go pickup a Thing and equip it.
        /// </summary>
        /// <param name="Pawn">Pawn that has to do the work.</param>
        /// <param name="Thing">Thing to Equip.</param>
        /// <returns>True if job was successfully created.</returns>
        public static bool InterfaceEquip(Pawn Pawn, Thing Thing)
        {
            Job Job = JobMaker.MakeJob(JobDefOf.Equip, Thing);

            if (Job == null)
            {
                return false;
            }

            Job.count = 1;

            return Pawn.jobs.TryTakeOrderedJob(Job, new JobTag?(JobTag.Misc), true);
        }

        /// <summary>
        /// Tells a Pawn to Unequip a Thing and move it to their Inventory.
        /// </summary>
        /// <param name="Pawn">Pawn that has to do the work.</param>
        /// <param name="Thing">Thing to Unequip.</param>
        /// <returns>True if job was successfully created.</returns>
        public static bool InterfaceUnequip(Pawn Pawn, Thing Thing)
        {
            Job Job = null;

            if (Thing.def.IsApparel)
            {
                Job = JobMaker.MakeJob(JobDefOf.RemoveApparel, Thing);
            }
            else if (Thing.def.IsWeapon)
            {
                // TODO: Find a way to move from Equipment slot to Inventory without dropping it first.
                Job = JobMaker.MakeJob(JobDefOf.DropEquipment, Thing);
            }

            if (Job == null)
            {
                return false;
            }

            Job.count = Thing.stackCount;

            return Pawn.jobs.TryTakeOrderedJob(Job, new JobTag?(JobTag.Misc), true);
        }

        /// <summary>
        /// Tells a Pawn to Unequip any Item that would prevent them from Equipping the specified Thing, then they'll equip that Thing.
        /// </summary>
        /// <param name="Pawn">Pawn that has to do the work.</param>
        /// <param name="Thing">Thing to Equip.</param>
        /// <returns>True if job was successfully created.</returns>
        public static bool InterfaceSwap(Pawn Pawn, Thing Thing, int Amount)
        {
            Pawn.equipment.MakeRoomFor((ThingWithComps)Thing);

            if (InterfacePickup(Pawn, Thing))
            {
                Job Job = null;

                if (Thing.def.IsApparel)
                {
                    Job = JobMaker.MakeJob(JobDefOf.Wear, Thing);
                }
                else if (Thing.def.IsWeapon)
                {
                    Job = JobMaker.MakeJob(JobDefOf.Equip, Thing);
                }

                if (Job == null)
                {
                    return false;
                }

                Job.count = Amount;

                return Pawn.jobs.TryTakeOrderedJob(Job, new JobTag?(JobTag.Misc), true);
            }

            return false;
        }

        /// <summary>
        /// Tells a Pawn to Drop the specified Thing from their Inventory.
        /// </summary>
        /// <param name="Thing">Thing to Drop.</param>
        /// <returns>True if job was successfully created.</returns>
        public static bool InterfaceDrop(Thing Thing)
        {
            // Modified from ITab_Pawn_Gear.InterfaceDrop
            Apparel Apparel = Thing as Apparel;

            Pawn Pawn = (Thing?.ParentHolder?.ParentHolder as Pawn);

            if ((Pawn == null) && (Thing?.ParentHolder?.ParentHolder is Corpse Corpse))
            {
                Pawn = Corpse.InnerPawn;
            }

            if (Pawn != null)
            {
                if ((Apparel != null) && (Pawn.apparel != null) && Pawn.apparel.WornApparel.Contains(Apparel))
                {
                    Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.RemoveApparel, Apparel), new JobTag?(JobTag.Misc), false);
                    return true;
                }
                if ((Pawn.equipment != null) && Pawn.equipment.AllEquipmentListForReading.Contains(Thing))
                {
                    Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, Thing), new JobTag?(JobTag.Misc), false);
                    return true;
                }
                if (!Thing.def.destroyOnDrop)
                {
                    Pawn.inventory.innerContainer.TryDrop(Thing, Pawn.Position, Pawn.Map, ThingPlaceMode.Near, out Thing ResultingThing, null, null);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Tells a Pawn to Consume the specified Thing in their Inventory.
        /// </summary>
        /// <param name="Thing"></param>
        /// <returns>True if job was successfully created.</returns>
        public static bool InterfaceEat(Thing Thing)
        {
            Pawn Pawn = (Thing?.ParentHolder?.ParentHolder as Pawn);

            if (Pawn != null)
            {
                FoodUtility.IngestFromInventoryNow(Pawn, Thing);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tells a Pawn to Drop Everything in their Inventory and all Equipment.
        /// </summary>
        /// <param name="Pawn">Pawn with Inventory.</param>
        /// <returns>True only if successfully dropped Inner inventory container.</returns>
        public static bool InterfaceDropAll(Pawn Pawn)
        {
            Pawn.equipment.DropAllEquipment(Pawn.Position, false, false);
            Pawn.apparel.DropAll(Pawn.Position, false, Pawn.Destroyed, null);

            return Pawn.inventory.innerContainer.TryDropAll(Pawn.Position, Pawn.Map, ThingPlaceMode.Near, null, null, true);
        }

        #endregion "Statics"
    }
}
