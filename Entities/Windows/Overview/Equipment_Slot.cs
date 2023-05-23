using BigMod.Entities.Interface;
using RimWorld;
using Verse;
using Verse.AI;

namespace BigMod.Entities.Windows.Overview
{
    public class Equipment_Slot : Panel, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public EquipmentType EquipmentType;
        public ApparelLayerDef ApparelLayerDef;
        public ListViewItem_Inventory Item;
        public Color BackgroundColorDefault;
        public Color ColorCanAccept;
        public Color ColorCantAccept;
        public bool CanDragRemoveItem = true;
        public Equipment_Slot(Vector2 Size, Vector2 Offset) : base(Size, Offset)
        {
            Style.DrawBackground = true;
            Style.DrawMouseOver = true;
        }

        public void SetPawn(Pawn Pawn)
        {
            // Clear incase a Pawn already exists.
            Clear();
            this.Pawn = Pawn;
        }

        public void Pull()
        {
            if (Item != null)
            {
                // Items in Transit are on their way to be picked up.
                if ((Item.Thing.holdingOwner == null) && !Item.InTransit)
                {
                    // Item isn't being held or intransit, remove it.
                    Item = null;
                    Item.RemoveFromParent();
                }
                else
                {
                    // A Item already exists.
                    Item.Pull();
                    return;
                }
            }

            List<ThingWithComps> Things;

            if (EquipmentType != EquipmentType.None)
            {
                Things = Pawn.equipment.AllEquipmentListForReading;
            }
            else
            {
                Things = Pawn.apparel.WornApparel.Cast<ThingWithComps>().ToList();
            }

            ThingWithComps Thing = Things.FirstOrDefault((F) => CanAccept(F));

            if (Thing == null)
            {
                // Get Items Reserved by this Pawn.
                Verse.AI.ReservationManager ReservationManager = Pawn.MapHeld.reservationManager;
                IEnumerable<Thing> ReservedThings = ReservationManager.AllReservedThings().Where((F) => ReservationManager.ReservedBy(F, Pawn));

                foreach (Thing Transit in ReservedThings)
                {
                    if (ListViewItem_Inventory.InterfaceInTransit(Pawn, Transit) && CanAccept(Transit))
                    {
                        Thing = (ThingWithComps)Transit;
                        break;
                    }
                }
            }

            if (Thing != null)
            {
                Item = new ListViewItem_Inventory(Thing);
                AddChild(Item);
                Item.Fallback = this;
                Item.RenderStyle = ItemStyle.Square;
            }

            ToolTipText = ((EquipmentType != EquipmentType.None) ? EquipmentType.ToString() : (ApparelLayerDef.LabelCap.ToString() + "\n\n" + ApparelLayerDef.description));
        }

        /// <summary>
        /// Adds the given Item to this Slot if this Slot can Accept it.
        /// </summary>
        /// <param name="Item">Item to add to this Slot.</param>
        /// <returns>True if Item was added to this Slot.</returns>
        public bool AddItem(ListViewItem_Inventory Item)
        {
            if (CanAccept(Item.Thing))
            {
                // Pawns can only ever equip a single item into a equipment slot, take 1 from the item stack and leave the rest as is.
                if (Item.Count > 1)
                {
                    Item = Item.Split(1);
                }

                Item.RemoveFromItemParent();
                this.Item = Item;

                AddChild(Item);
                Item.Fallback = this;
                Item.RenderStyle = ItemStyle.Square;

                if (!IsAlreadyEquipped(Item.Thing))
                {
                    ListViewItem_Inventory.InterfaceSwap(Pawn, Item.Thing, 1);
                }

                return true;
            }

            return false;
        }

        public bool IsAlreadyEquipped(Thing Thing)
        {
            if (Thing.def.IsWeapon)
            {
                return Pawn.equipment.Contains(Thing);
            }
            else if (Thing.def.IsApparel)
            {
                return Pawn.apparel.Contains(Thing);
            }

            return false;
        }

        public bool CanAccept(Thing Thing)
        {
            // EquipmentUtility also handles Biocoded check.
            return (((Thing.def.IsWeapon && (EquipmentType == Thing.def.equipmentType)) || (Thing.def.IsApparel && Thing.def.apparel.layers.Contains(ApparelLayerDef))) && EquipmentUtility.CanEquip(Thing, Pawn));
        }

        public void DisplayCanAccept(Thing Thing, bool ToolTips = false)
        {
            if (Thing != null)
            {
                if (CanAccept(Thing))
                {
                    if (ToolTips)
                    {
                        ToolTipText = "CanAccept".Translate();
                    }

                    Style.BackgroundColor = ColorCanAccept;
                }
                else
                {
                    if (ToolTips)
                    {
                        ToolTipText = "CantAccept".Translate();

                        if (EquipmentType != EquipmentType.None)
                        {
                            EquipmentUtility.CanEquip(Thing, Pawn, out string Reason);

                            ToolTipText += "\n\n" + Reason;
                        }
                    }

                    Style.BackgroundColor = ColorCantAccept;
                }
            }
            else
            {
                Style.BackgroundColor = BackgroundColorDefault;
            }
        }

        public void Clear()
        {
            Pawn = null;

            if (Item != null)
            {
                RemoveChild(Item);
                Item = null;
            }

            DisplayCanAccept(null);
        }

        public override void SetStyle(string Palette)
        {
            base.SetStyle(Palette);

            // Save this Color as the base Style will be overwrite it when DisplayCanAccept is running.
            BackgroundColorDefault = Style.BackgroundColor;

            ColorCanAccept = Globals.GetColor("OverviewPawn.Slot.CanAccept.BackgroundColor");
            ColorCantAccept = Globals.GetColor("OverviewPawn.Slot.CantAccept.BackgroundColor");
        }
    }
}
