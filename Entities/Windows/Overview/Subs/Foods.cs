using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class Foods : DropDown, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public FoodRestriction Food;

        public Foods(float Width, float Height) : base(string.Empty, Width, Height)
        {
            Style.DrawBackground = true;

            ToolTipText = "Current_Foods".Translate();

            Populate();
        }

        public void Populate()
        {
            ListView.Clear();

            foreach (FoodRestriction Entry in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
            {
                ListViewItem Item = new ListViewItem(Entry.label);
                Item.Data = Entry;

                Item.Header.Label.Offset = new Vector2(5f, 0f);
                Item.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
                Item.Header.Label.Style.FontType = GameFont.Tiny;

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
            Food = Pawn.foodRestriction.CurrentFoodRestriction;

            Text = Food.label;
        }

        public void Push()
        {
            Pawn.foodRestriction.CurrentFoodRestriction = Food;
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }

        public override void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            base.DoOnClickedItem(Sender, EventArgs);

            Food = (FoodRestriction)((ListViewItem)Sender).Data;
            Push();
            Pull();
        }
    }
}
