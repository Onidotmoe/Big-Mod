using Verse;

namespace BigMod.Entities.Windows.Architect
{
    internal class ListViewItem_Architect_Special : ListViewItem
    {
        public Designator Designator;

        public ListViewItem_Architect_Special(Designator Designator) : base(Designator.LabelCap)
        {
            this.Designator = Designator;

            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Label.Offset = new Vector2((Height + 3f), 0);
            ToolTipText = Designator.Desc.CapitalizeFirst();

            Image = new Image((Texture2D)Designator.icon);
            Image.Size = new Vector2(Height, Height);
            Image.Style.Color = Color.white;
            Image.OnMouseEnter += ListViewItemGroup_Architect.Image_OnMouseEnter;
            AddChild(Image);
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClick(Sender, EventArgs);

            Designator Selected = Find.DesignatorManager.SelectedDesignator;

            // Deselect if already selected
            if ((Selected != null) && (Selected == Designator))
            {
                Find.DesignatorManager.Deselect();
            }
            else
            {
                Find.DesignatorManager.Select(Designator);
            }
        }

        // TODO: Needs to have some indicator that it can be right-clicked
        public override void DoOnClickRight(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClickRight(Sender, EventArgs);

            // Must have a valid click action
            if ((Designator.RightClickFloatMenuOptions.FirstOrDefault() != null) && (Designator.RightClickFloatMenuOptions.Any((F) => (F.action != null))))
            {
                ContextMenu ContextMenu = new ContextMenu(Designator.RightClickFloatMenuOptions);
                ContextMenu.CloseOnOptionClick = true;
                ContextMenu.Open();
            }
        }
    }
}
