using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Architect
{
    /// <summary>
    /// Material entry for a recipe variation.
    /// </summary>
    public class ListViewItem_Architect_Material : ListViewItem
    {
        /// <summary>
        /// Definition of this Item's thing based on its parent. Like a Wall.
        /// </summary>
        public BuildableDef BuildableDef;

        /// <summary>
        /// Definition of the stuff material this thing is made from. Like Logs.
        /// </summary>
        public ThingDef StuffDef;

        /// <summary>
        /// Designator for this Stuff Item Recipe.
        /// </summary>
        public Designator_Build Designator;

        public Label Balance;
        public Label Cost;
        public int Amount_Balance;
        public int Amount_Cost;
        public Color CantAfford = Globals.GetColor("ListViewItem_Architect_Material.Header.CantAfford");

        /// <summary>
        /// Called by <see cref="DoOnRequest"/>.
        /// </summary>
        public event EventHandler OnRequest;

        /// <summary>
        /// For the build menu, handles clicks, designations, and cost.
        /// </summary>
        /// <param name="BuildableDef">The Thing we want to build.</param>
        /// <param name="StuffDef">The Stuff material this thing is made of.</param>
        /// <param name="SanitizedName">Removes this string from the display name.</param>
        public ListViewItem_Architect_Material(Vector2 Size, BuildableDef BuildableDef, ThingDef StuffDef, string SanitizedName)
        {
            this.Size = Size;
            this.BuildableDef = BuildableDef;
            this.StuffDef = StuffDef;

            // Building designator to build this item
            Designator = new Designator_Build(BuildableDef);

            if (BuildableDef.MadeFromStuff)
            {
                // Set Material
                Designator.SetStuffDef(StuffDef);
            }

            Image = new Image(StuffDef.uiIcon);
            Image.SetStyle("ListViewItem_Architect_Material.Image");
            Image.Style.DrawMouseOver = true;
            Image.Style.Color = StuffDef.uiIconColor;
            Image.Anchor = Anchor.Center;
            Image.Size = new Vector2((Header.Height - 2), (Header.Height - 2));
            AddChild(Image);

            Balance = new Label();
            Balance.SetStyle("ListViewItem_Architect_Material.Balance");
            Balance.Style.TextAnchor = TextAnchor.LowerCenter;
            Balance.Style.FontType = GameFont.Tiny;
            Balance.InheritParentSize = true;
            Balance.OffsetY += 2f;
            AddChild(Balance);

            Cost = new Label();
            Cost.SetStyle("ListViewItem_Architect_Material.Cost");
            Cost.Style.TextAnchor = TextAnchor.UpperCenter;
            Cost.Style.FontType = GameFont.Tiny;
            Cost.InheritParentSize = true;
            Cost.OffsetY += -2f;
            AddChild(Cost);

            //// We use LabelCap instead of LabelAsStuff because we want "Limestone Blocks" and not just "Limestone"
            Image.ToolTipText = StuffDef.LabelCap.CapitalizeFirst().ToString().ReplaceFirst(SanitizedName, string.Empty);
        }

        public void UpdateListing()
        {
            Amount_Balance = Find.CurrentMap.resourceCounter.GetCount(StuffDef);

            // Throws a warning if CostListAdjusted when MadeFromStuff is false.
            if (BuildableDef.MadeFromStuff)
            {
                // Returns the correct amount with any special stuff count taking into consideration
                Amount_Cost = BuildableDef.CostListAdjusted(StuffDef, false).First((F) => F.thingDef == StuffDef).count;
            }
            else
            {
                Amount_Cost = BuildableDef.CostList.First().count;
            }

            Balance.Text = Amount_Balance.ToString();
            Balance.Style.TextColor = ((Amount_Balance > Amount_Cost) ? Style.TextColor : CantAfford);
            Cost.Text = Amount_Cost.ToString();
        }

        public override void DoOnWhileMouseOver(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnWhileMouseOver(Sender, EventArgs);

            // Highlight all text on this entity
            Balance.IsMouseOver = true;
            Cost.IsMouseOver = true;
        }

        public override void DoOnMouseLeave(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnMouseLeave(Sender, EventArgs);

            if (IsVisible)
            {
                Balance.IsMouseOver = false;
                Cost.IsMouseOver = false;
                Balance.Style.TextColor = ((Amount_Balance > Amount_Cost) ? Style.TextColor : CantAfford);
            }
        }

        /// <summary>
        /// Handles data retrieving.
        /// </summary>
        public virtual void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            // Visible checks if this specific subitem can be built
            if (!IsHidden)
            {
                UpdateListing();
            }
        }
    }
}
