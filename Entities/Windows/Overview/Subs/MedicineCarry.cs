using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    /// <summary>
    /// Created its own Class to reduce complexity of <see cref="OverviewPawn"/>.
    /// </summary>
    public class MedicineCarry : TextInput, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public ThingDef ThingDef;

        public MedicineCarry(float Width, float Height) : base(TextInputStyle.Numeric, InventoryStockGroupDefOf.Medicine.min, InventoryStockGroupDefOf.Medicine.max)
        {
            Size = new Vector2(Width, Height);

            Style.DrawBackground = true;
            Style.TextOffset = new Vector2(-5f, 0f);

            ToolTipText = "CarryMedicine".Translate();
        }

        public void Pull()
        {
            Numeric = Pawn.inventoryStock.GetDesiredCountForGroup(InventoryStockGroupDefOf.Medicine);
        }

        public void Push()
        {
            Pawn.inventoryStock.SetCountForGroup(InventoryStockGroupDefOf.Medicine, Numeric);
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }

        public override void DoOnTextChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnTextChanged(Sender, EventArgs);

            // Text changes when this Entity spawns too.
            if (Pawn != null)
            {
                Push();
                Pull();
            }
        }
    }
}
