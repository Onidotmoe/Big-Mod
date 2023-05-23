using Verse;

namespace BigMod.Entities.Windows.Architect
{
    /// <summary>
    /// Recipe ingredient display item.
    /// </summary>
    public class ListViewItem_Architect_Recipe : ListViewItem
    {
        public Label Cost;
        public Label Divider;
        public Label Balance;
        public ThingDef ThingDef;

        public ListViewItem_Architect_Recipe(Vector2 Size, ThingDef ThingDef, int Price)
        {
            this.Size = Size;
            this.ThingDef = ThingDef;

            Selectable = false;
            Header.CanToggle = false;

            int Count = Find.CurrentMap.resourceCounter.GetCount(ThingDef);

            Image = new Image(ThingDef.uiIcon);
            Image.Size = new Vector2((Header.Height - 2), (Header.Height - 2));
            Image.Style.Color = ThingDef.uiIconColor;
            Image.OnClick += (obj, e) =>
            {
                Globals.HandleModifierSelection(this.ThingDef);
            };
            Image.Offset = new Vector2(1f, 1f);
            Image.ToolTipText = ThingDef.LabelCap;
            AddChild(Image);

            Cost = new Label(Price.ToString());
            Cost.SetStyle("ListViewItem_Architect_Recipe.Cost");
            Cost.Style.TextAnchor = TextAnchor.LowerRight;
            Cost.Anchor = Anchor.TopRight;
            Cost.Size = new Vector2(((Header.Width - Image.Width) / 2), Header.Height);
            Cost.Offset = new Vector2(-5f, 0f);
            AddChild(Cost);

            Divider = new Label("|");
            Divider.SetStyle("ListViewItem_Architect_Recipe.Divider");
            Divider.Style.TextAnchor = TextAnchor.LowerCenter;
            Divider.Anchor = Anchor.TopRight;
            Divider.Size = new Vector2(3, Header.Height);
            Divider.Offset = new Vector2(-(Cost.Width + 5f), 0f);
            AddChild(Divider);

            Balance = new Label(Count.ToString());
            Balance.SetStyle("ListViewItem_Architect_Recipe.Balance");
            Balance.Style.TextAnchor = TextAnchor.LowerRight;
            Balance.Anchor = Anchor.TopRight;
            Balance.Size = new Vector2(((Header.Width - Image.Width) / 2), Header.Height);
            Balance.Offset = new Vector2(-(Cost.Width + Divider.Width + 10f), 0f);

            Balance.Style.TextColor = ((Count > Price) ? Style.TextColor : Globals.GetColor("ListViewItem_Architect_Recipe.Header.CantAfford"));
            AddChild(Balance);
        }
    }
}
