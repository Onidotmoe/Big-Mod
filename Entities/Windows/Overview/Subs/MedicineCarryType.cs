using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    /// <summary>
    /// Created its own Class to reduce complexity of <see cref="OverviewPawn"/>.
    /// </summary>
    public class MedicineCarryType : DropDown, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public ThingDef ThingDef;

        public MedicineCarryType(float Width, float Height) : base(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("Medicine"))
        {
            Size = new Vector2(Width, Height);

            Style.DrawBackground = true;

            ToolTipText = "CarryMedicineQuality".Translate();

            Populate();
        }

        public void Populate()
        {
            ListView.Clear();

            foreach (ThingDef ThingDef in InventoryStockGroupDefOf.Medicine.thingDefs)
            {
                ListViewItem Item = new ListViewItem(ThingDef.LabelCap);
                Item.Data = ThingDef;
                Item.ToolTipText = ThingDef.DescriptionDetailed;

                Item.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
                Item.Header.Label.Style.FontType = GameFont.Tiny;

                Item.Image = new Image();
                Item.Image.Size = new Vector2(Item.Height, Item.Height);
                Item.Image.Texture = ThingDef.uiIcon;
                Item.Image.Style.Color = ThingDef.uiIconColor;
                Item.AddChild(Item.Image);

                Item.Header.Label.OffsetX = (Item.Image.Width + 5f);

                AddItem(Item);
            }

            if (ListView.Items.Any())
            {
                // Ensure Items are large enough to show all text without clipping.
                SizeExpanded.x = Mathf.Max(SizeExpanded.x, (ListView.Items.Max((F) => F.Header.Label.GetTextSize().x) + ListView.Items.First().Height + 5f));
            }
        }

        public void Pull()
        {
            ThingDef = Pawn.inventoryStock.GetDesiredThingForGroup(InventoryStockGroupDefOf.Medicine);

            Image.Texture = ThingDef.uiIcon;
            Image.Style.Color = ThingDef.uiIconColor;
        }

        public void Push()
        {
            Pawn.inventoryStock.SetThingForGroup(InventoryStockGroupDefOf.Medicine, ThingDef);
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }

        public override void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            base.DoOnClickedItem(Sender, EventArgs);

            ThingDef = (ThingDef)((ListViewItem)Sender).Data;
            Push();
            Pull();
        }
    }
}
