using BigMod.Entities.Interface;
using System.IO;
using System.Xml.Serialization;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class Equipment : Panel, IPawn, IPull
    {
        public static List<SlotDefinition> SlotDefinitions = Globals.Read<SlotDefinitionArray>(Path.Combine(BigMod.Directory, "Resources/Equipment.xml")).Slots;
        public static List<SlotDefinition> SlotDefinitions_Line = Globals.Read<SlotDefinitionArray>(Path.Combine(BigMod.Directory, "Resources/Equipment_Line.xml")).Slots;
        public List<Equipment_Slot> Slots = new List<Equipment_Slot>();
        public Pawn Pawn { get; set; }
        public Vector2 SizeSlot = new Vector2(50f, 50f);
        private DisplayStyle _RenderStyle = DisplayStyle.Window;
        public DisplayStyle RenderStyle
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

                    // Style has changed, reset the Slots.
                    Populate();
                }
            }
        }

        public Equipment(float Width, float Height)
        {
            Size = new Vector2(Width, Height);

            // Register events for when a Pawn's equipped items are changed.
            BigMod.Pawn_ApparelTracker.ApparelChanged += TrackerApparel;
            BigMod.Pawn_EquipmentTracker.EquipmentAdded += TrackerEquipment;
            BigMod.Pawn_EquipmentTracker.EquipmentRemoved += TrackerEquipment;
        }

        public Equipment(Vector2 Size, Vector2 Offset = default) : this(Size.x, Size.y)
        {
            this.Offset = Offset;

            Populate();
        }

        public void TrackerApparel(object Sender, BigMod.Pawn_ApparelTracker.ApparelEventArgs EventArgs)
        {
            // Update all slots when something changes but only if we're visible and if the event Pawn is the same.
            // We can be invisible in a ListViewItem_Inspect_Pawn class instance.
            if (IsVisible && (((RimWorld.Pawn_ApparelTracker)Sender).pawn == Pawn))
            {
                Pull();
            }
        }
        public void TrackerEquipment(object Sender, BigMod.Pawn_EquipmentTracker.EquipmentEventArgs EventArgs)
        {
            // Update all slots when something changes but only if we're visible and if the event Pawn is the same.
            // We can be invisible in a ListViewItem_Inspect_Pawn class instance.
            if (IsVisible && (((Pawn_EquipmentTracker)Sender).pawn == Pawn))
            {
                Pull();
            }
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Slots.ForEach((F) => F.SetPawn(Pawn));
        }

        public void Populate()
        {
            Slots.ForEach((F) => F.RemoveFromParent());
            Slots.Clear();

            foreach (SlotDefinition SlotDefinition in ((RenderStyle == DisplayStyle.Window) ? SlotDefinitions : SlotDefinitions_Line))
            {
                Equipment_Slot Slot = new Equipment_Slot(SizeSlot, new Vector2(SlotDefinition.X, SlotDefinition.Y));

                if (Enum.TryParse(SlotDefinition.ID, out EquipmentType EquipmentType))
                {
                    Slot.EquipmentType = EquipmentType;
                }
                else
                {
                    Slot.ApparelLayerDef = DefDatabase<ApparelLayerDef>.GetNamed(SlotDefinition.ID, false);
                }

                Slots.Add(Slot);
                AddChild(Slot);
            }
        }
        public void Pull()
        {
            Slots.ForEach((F) => F.Pull());
        }

        public void DisplayCanAccept(Thing Thing, bool ToolTips = false)
        {
            Slots.ForEach((F) => F.DisplayCanAccept(Thing, ToolTips));
        }
        public bool Exists(Thing Thing)
        {
            return Slots.Exists((F) => (F.Item?.Thing == Thing));
        }
        /// <summary>
        /// Adds the given Item to the first Slot that can Accept it.
        /// </summary>
        /// <param name="Item">Item to add to a Slot.</param>
        /// <returns>True if Item was added to a Slot.</returns>
        public bool AddItem(ListViewItem_Inventory Item)
        {
            foreach (Equipment_Slot Slot in Slots)
            {
                if (Slot.AddItem(Item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a Instance of <see cref="Equipment"/> class from a currently Open Window.
        /// </summary>
        /// <param name="Equipment">The first Instance of <see cref="Equipment"/> class.</param>
        /// <returns>True if a <see cref="Equipment"/> Instance was found.</returns>
        public static bool GetInstance(out Equipment Equipment)
        {
            Equipment = WindowManager.Instance.Windows.SelectMany((F) => F.Root.GetChildrenFlatten().OfType<Equipment>()).OrderByDescending((F) => F.ParentWindow.IsMouseOver).FirstOrDefault();

            return (Equipment != null);
        }

        public class SlotDefinitionArray
        {
            [XmlArray]
            [XmlArrayItem("Slot")]
            public List<SlotDefinition> Slots;
        }

        public class SlotDefinition
        {
            [XmlAttribute]
            public string ID;
            [XmlAttribute]
            public float X;
            [XmlAttribute]
            public float Y;
        }
    }
}
