using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class Outfits : DropDown, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public Outfit Outfit;

        public Outfits(float Width, float Height) : base(string.Empty, Width, Height)
        {
            Style.DrawBackground = true;

            ToolTipText = "Current_Outfits".Translate();

            Populate();
        }

        public void Populate()
        {
            ListView.Clear();

            foreach (Outfit Entry in Current.Game.outfitDatabase.AllOutfits)
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
            Outfit = Pawn.outfits.CurrentOutfit;

            Text = Outfit.label;
        }

        public void Push()
        {
            Pawn.outfits.CurrentOutfit = Outfit;
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }

        public override void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            base.DoOnClickedItem(Sender, EventArgs);

            Outfit = (Outfit)((ListViewItem)Sender).Data;
            Push();
            Pull();
        }
    }
}
