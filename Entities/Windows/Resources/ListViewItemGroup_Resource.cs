using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Resources
{
    public class ListViewItemGroup_Resource : ListViewItemGroup
    {
        /// <summary>
        /// Line that appears at the bottom of the item as a separator.
        /// </summary>
        public Image Line;
        /// <summary>
        /// Called by <see cref="DoOnRequest"/>.
        /// </summary>
        public event EventHandler OnRequest;
        /// <summary>
        /// Defines the filtering for this group.
        /// </summary>
        public Group GroupDefinition;
        /// <summary>
        /// Used to display additional information.
        /// </summary>
        public Label Special;

        public ListViewItemGroup_Resource(string Text, string TexturePath, Group Group) : base(Text)
        {
            GroupDefinition = Group;

            Style.DrawMouseOver = false;

            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Style.DrawBackground = false;

            Line = new Image();
            Line.RenderStyle = ImageStyle.Color;
            Line.Style.Color = Globals.GetColor("ListViewItemGroup_Resource.Line");
            Line.InheritParentSize = true;
            Line.LimitToParent = true;
            Line.MaxSize = new Vector2(0, 1);
            Line.Anchor = Anchor.BottomLeft;
            Header.AddChild(Line);

            if (!string.IsNullOrWhiteSpace(TexturePath))
            {
                AddImage(TexturePath);
                Header.Label.Offset = new Vector2(Image.Width + 2f, 0);
            }
        }

        public void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            IEnumerable<ListViewItem_Resource> AllItems = Flatten().OfType<ListViewItem_Resource>();

            // If there aren't any non-group subitems, hide this group
            IsHidden = !AllItems.Any();

            if (!IsHidden)
            {
                if ((GroupDefinition.Attributes != null) && GroupDefinition.Attributes.Contains("DisplayFoodNutrients"))
                {
                    if (Special == null)
                    {
                        Special = new Label();
                        Special.Anchor = Anchor.TopRight;
                        Special.SizeToText();
                        Special.Height = Header.Height;
                        Special.IgnoreMouse = false;
                        Special.ToolTipText = "HumanEdibleNutrients".Translate();
                        Special.Offset = new Vector2(-20, 0);
                        Header.AddChild(Special);
                    }

                    float Nutrients = 0;

                    foreach (ListViewItem_Resource Item in AllItems)
                    {
                        if (Item.ThingDef.IsNutritionGivingIngestible && Item.ThingDef.ingestible.HumanEdible)
                        {
                            Nutrients += (Item.ThingDef.GetStatValueAbstract(StatDefOf.Nutrition, null) * (float)Item.Count);
                        }
                    }

                    Special.Text = ((int)Nutrients).ToString();
                }
            }
        }

        public void AddImage(string TexturePath)
        {
            Image = new Image(Globals.TryGetTexturePathFromAlias(TexturePath), MinSize.x, MinSize.y);
            Image.Style.Color = Color.white;
            AddChild(Image);
        }
    }
}
