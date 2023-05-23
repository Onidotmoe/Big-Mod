using Verse;

namespace BigMod.Entities.Windows.Resources
{
    public class ListViewItem_Resource : ListViewItem
    {
        /// <summary>
        /// Just a sugarcoat to access <see cref="ListViewItem.Header.Label"/>.
        /// </summary>
        public Label Name
        {
            get
            {
                return Header.Label;
            }
            set
            {
                Header.Label = value;
            }
        }
        public Label Amount = new Label();
        public Label Weight = new Label();
        public Label Value = new Label();
        public Label Delta = new Label();
        public int Old_Count;
        public int Count;
        /// <summary>
        /// Definition of this Item's thing.
        /// </summary>
        public ThingDef ThingDef;
        /// <summary>
        /// Called by <see cref="DoOnRequest"/>.
        /// </summary>
        public event EventHandler OnRequest;

        public ListViewItem_Resource(Vector2 Size, ThingDef ThingDef, int Count) : base()
        {
            this.Size = ((Size != default) ? Size : MinSize);
            this.Count = Count;
            this.ThingDef = ThingDef;

            // Icon, Name, Amount, Weight, Total base Value, up/down delta arrow
            Image = new Image(Widgets.GetIconFor(ThingDef), new Rect(0, 0, Height, Height));
            Image.SetStyle("ListViewItem_Resource.Image");
            Image.Style.DrawMouseOver = true;
            Image.Style.Color = ThingDef.uiIconColor;
            // Indicate that the user can interact with this Image.
            Image.OnClick += (obj, e) =>
            {
                if (!Globals.HandleModifierSelection(this.ThingDef))
                {
                    WindowManager.OpenWindowVanilla(new Dialog_InfoCard(this.ThingDef, null));
                }
            };
            Image.OnMouseEnter += Architect.ListViewItemGroup_Architect.Image_OnMouseEnter;
            AddChild(Image);

            Name.IgnoreMouse = false;
            Name.Text = ThingDef.LabelCap;
            Name.ToolTipText = ThingDef.description.CapitalizeFirst();
            Name.Style.TextAnchor = TextAnchor.MiddleLeft;
            Name.Size = new Vector2((Width / 2), Height);
            Name.Offset = new Vector2((Image.Width + 2f), 0);
            Name.InheritParentSize = true;
            Name.LimitToParent = true;

            Amount.IgnoreMouse = false;
            Amount.SetStyle("ListViewItem_Resource.ListView.Item");
            Amount.Style.TextAnchor = TextAnchor.MiddleRight;
            Amount.Anchor = Anchor.TopRight;
            Amount.Size = new Vector2(40, Height);
            Amount.Offset = new Vector2(-17f - Amount.Width * 2, 0f);
            AddChild(Amount);

            Weight.IgnoreMouse = false;
            Weight.SetStyle("ListViewItem_Resource.ListView.Item");
            Weight.Style.TextAnchor = TextAnchor.MiddleRight;
            Weight.Anchor = Anchor.TopRight;
            Weight.Size = Amount.Size;
            Weight.Offset = new Vector2(-17f - Amount.Width, 0f);
            AddChild(Weight);

            Value.IgnoreMouse = false;
            Value.SetStyle("ListViewItem_Resource.ListView.Item");
            Value.Style.TextAnchor = TextAnchor.MiddleRight;
            Value.Anchor = Anchor.TopRight;
            Value.Size = Amount.Size;
            Value.Offset = new Vector2(-17f, 0f);
            AddChild(Value);

            Delta.IgnoreMouse = false;
            Delta.SetStyle("ListViewItem_Resource.ListView.Item");
            Delta.Anchor = Anchor.TopRight;
            Delta.Size = new Vector2(15f, Height);
            Delta.Offset = new Vector2(-2f, 0f);
            Delta.Style.FontType = GameFont.Medium;
            AddChild(Delta);

            UpdateListing();
        }

        public void UpdateListing()
        {
            Amount.ToolTipText = "TotalStacks".Translate() + Math.Round((double)(Count / ThingDef.stackLimit), 2).ToString();
            Amount.Text = Count.ToString();
            // Remove the decimal points
            Weight.Text = ((int)(ThingDef.BaseMass * Count)).ToString();
            Weight.ToolTipText =
                "BaseMassValue_All".Translate()
                + Environment.NewLine + "PerItem".Translate() + ThingDef.BaseMass.ToString() + "Kg".Translate();

            Value.Text = ((int)(ThingDef.BaseMarketValue * Count)).ToString();
            Value.ToolTipText =
                "BaseMarketValue_All".Translate()
                + Environment.NewLine + "PerItem".Translate() + ThingDef.BaseMarketValue.ToString() + "DollarSign".Translate();

            Value.ToolTipText +=
                Environment.NewLine + "CostEffectiveness".Translate() + Math.Round(((ThingDef.BaseMarketValue / ThingDef.BaseMass) * Count), 2).ToString()
                + Environment.NewLine + "PerItem".Translate() + Math.Round((ThingDef.BaseMarketValue / ThingDef.BaseMass), 2).ToString()
                + Environment.NewLine + "CostEffectivenessInfo".Translate();

            // Delta will only be updated if it has changed
            if (Old_Count < Count)
            {
                Delta.Text = "TriangleUp".Translate();
                Delta.Style.TextColor = Globals.GetColor("ListViewItem_Resource.UpArrow");
                Delta.ToolTipText = "IncreasedBy".Translate() + (Count - Old_Count).ToString();
            }
            else if (Old_Count > Count)
            {
                Delta.Text = "TriangleDown".Translate();
                Delta.Style.TextColor = Globals.GetColor("ListViewItem_Resource.DownArrow");
                Delta.ToolTipText = "DecreasedBy".Translate() + (Old_Count - Count).ToString();
            }
        }

        public virtual void DoOnRequest(int Count)
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            Old_Count = this.Count;
            this.Count = Count;

            if ((Count <= 0) && !ThingDef.resourceReadoutAlwaysShow)
            {
                RemoveFromItemParent();
            }
            else
            {
                UpdateListing();
            }
        }
    }
}
