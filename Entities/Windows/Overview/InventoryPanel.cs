using Verse;

namespace BigMod.Entities.Windows.Overview
{
    // TODO: toggle to hide the panel and shrink parent size
    public class InventoryPanel : Panel
    {
        public static int Modifier_Shift = 1;
        public static int Modifier_Ctrl = 10;
        public static int Modifier_Alt = 100;
        public static float Modifier_Split = 0.5f;
        public event EventHandler OnEmbed;
        public ListView_Stats ListView;
        public DropDown CategoryFilter = new DropDown("NoFilter".Translate(), 25f, 25f);
        public Search Search;
        public ThingCategoryDef SelectedCategory;
        /// <summary>
        /// Displays additional information like Weight, Value, Item Count.
        /// </summary>
        public Label Label = new Label();
        /// <summary>
        /// Pawns can only carry 1 stack of a specific thing. Use this to limit the GUI's ability to add more than 1 stack to its list.
        /// </summary>
        public bool LimitStackAmount;

        public InventoryPanel(Vector2 Size, Vector2 Offset = default, bool ShowScrollbarVertical = false) : base(Size, Offset)
        {
            ListView = new ListView_Stats(Size, new Vector2(0f, (CategoryFilter.Height + 5f)), ShowScrollbarVertical);
            ListView.SetStyle("InventoryPanel.ListView");
            ListView.InheritParentSize = true;
            ListView.Filter = Filter;

            Search = new Search(Width);
            Search.Anchor = Anchor.BottomLeft;
            Search.OffsetY = -5f;
            Search.TextInput.OnTextChanged += OnTypingFinished;
            Search.InheritParentSizeWidth = true;
            Search.InheritChildrenSize_Modifier = new Vector2(-10f, 0f);

            CategoryFilter.SetStyle("InventoryPanel.CategoryFilter");
            CategoryFilter.Style.DrawBackground = true;
            CategoryFilter.InheritParentSizeWidth = true;
            CategoryFilter.OnClickedItem += DoOnClickedItem;

            ListView.InheritParentSize_Modifier = new Vector2(0f, (-CategoryFilter.Height - Search.Height - 15f));

            AddRange(ListView, CategoryFilter, Search);

            Populate();
        }

        /// <summary>
        /// Tries to Embed the <paramref name="Item"/> into this ListView.
        /// </summary>
        /// <param name="Item">The Item to add to this ListView.</param>
        /// <returns>True only if complete embedding was succesful. False if partial or failed embedding.</returns>
        public bool TryEmbed(ListViewItem_Inventory Item)
        {
            int Amount = GetModifierCount(Item.Count);
            Amount = ((Amount > 0) ? Amount : Item.Count);

            if (Amount >= 0)
            {
                if (LimitStackAmount)
                {
                    Amount = ApplyStackLimit(Item, Amount);

                    if (Amount <= 0)
                    {
                        return false;
                    }
                }

                ListViewItem_Inventory Split = Item.Split(Amount);
                ListView.AddItem(Split);
                DoOnEmbed(Split, EventArgs.Empty);

                if (SelectedCategory != null)
                {
                    // When adding Items and the Category Sorting is active, they have to be re-sorted to position them correctly.
                    Sort();
                }

                if (Item.Count <= 0)
                {
                    // Update the Preview colors on the Equipment slots.
                    if (Item.GetHostOverviewPawn(out OverviewPawn OverviewPawn))
                    {
                        OverviewPawn.Equipment.Pull();
                    }

                    Item.RemoveFromItemParent();
                    Item.RemoveFromParent();

                    Item.RenderStyle = ItemStyle.Line;

                    return true;
                }
            }

            return false;
        }

        public void Populate()
        {
            ListViewItem NoFilter = new ListViewItem("NoFilter".Translate());
            NoFilter.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            NoFilter.Header.Label.OffsetX = 5f;
            CategoryFilter.AddItem(NoFilter);

            IEnumerable<ThingCategoryDef> Categories = DefDatabase<ThingCategoryDef>.AllDefs.OrderBy((F) => F.label);

            foreach (ThingCategoryDef Category in Categories)
            {
                ListViewItem Item = new ListViewItem(Category.LabelCap);
                Item.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
                Item.Header.Label.OffsetX = 5f;
                Item.ToolTipText = Category.description;
                Item.Data = Category;

                CategoryFilter.AddItem(Item);
            }
        }

        public void DoOnEmbed(object Sender, EventArgs EventArgs)
        {
            OnEmbed?.Invoke(Sender, EventArgs);
        }

        public void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            ListViewItem Item = (ListViewItem)Sender;

            if (Item.Data is ThingCategoryDef ThingCategoryDef)
            {
                SelectedCategory = ThingCategoryDef;
                ListView.IsFilterActive = true;
            }
            else
            {
                SelectedCategory = null;
                ListView.IsFilterActive = !string.IsNullOrEmpty(Search.Text);
            }

            CategoryFilter.Text = Item.Text;
            CategoryFilter.ToolTipText = Item.ToolTipText;

            ListView.DoOnFilter(ListView, EventArgs.Empty);
        }

        public void OnTypingFinished(object Sender, EventArgs EventArgs)
        {
            // Whitespace should be a valid Search query.
            ListView.IsFilterActive = !(string.IsNullOrEmpty(Search.Text) && (SelectedCategory == null));
            ListView.DoOnFilter(ListView, EventArgs.Empty);
            Sort();
        }
        public void Sort()
        {
            List<ListViewItem> Items = ListView.Items;

            var Ordered = from ListViewItem in ListView.Items
                          let InventoryItem = ListViewItem as ListViewItem_Inventory
                          where InventoryItem != null
                          select InventoryItem into Item orderby Item.Text
                          group Item by Item.SortingType().Sort into Groups orderby Groups.First().SortingType().Order descending
                          select Groups into Groups from Member in Groups select Member;

            Items = Items.Except(Ordered).ToList();
            Items.SortBy((F) => F.Text);

            // Add the Excepted Items to the bottom of the list.
            List<ListViewItem> OrderedItems = Ordered.Cast<ListViewItem>().ToList();
            OrderedItems.AddRange(Items);
            ListView.Items = OrderedItems;

            ListView.UpdatePositions();
            ListView.UpdateVirtualization();
        }
        public bool Filter(ListViewItem Item)
        {
            // string.Contains(string, StringComparison.InvariantCultureIgnoreCase) doesn't seem to be available to us.
            return ((Item is ListViewItem_Inventory InventoryItem) && ((SelectedCategory == null) || InventoryItem.Thing.def.IsWithinCategory(SelectedCategory)) && (string.IsNullOrEmpty(Search.Text) || (InventoryItem.Text?.IndexOf(Search.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)));
        }

        /// <summary>
        /// Pawns can only carry 1 whole stack of each Thing. This applies the limit based on how much of that Thing the Pawn already is carrying.
        /// </summary>
        /// <param name="Item">Interface Item that hold the Thing and ThingDef.</param>
        /// <param name="Amount">Amount to be validated if it can go into the Pawn's inventory.</param>
        /// <returns>Available space for the given ThingDef in the Pawn's inventory.</returns>
        public int ApplyStackLimit(ListViewItem_Inventory Item, int Amount)
        {
            // Pawns can only carry 1 stack of something.
            Amount = Math.Min(Item.Thing.def.stackLimit, Amount);

            if (Item.GetHostOverviewPawn(out OverviewPawn OverviewPawn))
            {
                Amount -= OverviewPawn.Pawn.inventory.Count(Item.Thing.def);

                // Do not allow more than 1 stack of something.
                Amount -= ListView.Items.OfType<ListViewItem_Inventory>().Where((F) => F.Thing.def == Item.Thing.def).Sum((F) => F.Count);
            }

            return Amount;
        }

        public static int GetModifierCount(int Count)
        {
            int Amount = Count;

            if (WindowManager.IsShiftDown())
            {
                Amount = Modifier_Shift;

                if (WindowManager.IsCtrlDown())
                {
                    // Also holding Ctrl Splits it by 10
                    Amount = Modifier_Ctrl;

                    if (WindowManager.IsAltDown())
                    {
                        // Also holding Alt Splits it by 100
                        Amount = Modifier_Alt;
                    }
                }
                else if (WindowManager.IsAltDown())
                {
                    // If Alt is down but Ctrl isn't, then Split it in half.
                    Amount = (int)(Count * Modifier_Split);
                }
            }

            // Prevents Negative values.
            return Mathf.Clamp(Amount, 0, Count);
        }
    }
}
