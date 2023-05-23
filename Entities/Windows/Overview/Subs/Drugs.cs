using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class Drugs : DropDown, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public DrugPolicy Drug;

        public Drugs(float Width, float Height) : base(string.Empty, Width, Height)
        {
            Style.DrawBackground = true;

            ToolTipText = "Current_Drugs".Translate();

            Populate();
        }

        public void Populate()
        {
            ListView.Clear();

            foreach (DrugPolicy Entry in Current.Game.drugPolicyDatabase.AllPolicies)
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
            Drug = Pawn.drugs.CurrentPolicy;

            Text = Drug.label;
        }

        public void Push()
        {
            Pawn.drugs.CurrentPolicy = Drug;
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }

        public override void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            base.DoOnClickedItem(Sender, EventArgs);

            Drug = (DrugPolicy)((ListViewItem)Sender).Data;
            Push();
            Pull();
        }
    }
}
