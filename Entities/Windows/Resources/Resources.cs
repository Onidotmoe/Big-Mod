using BigMod.Entities.Interface;
using System.IO;
using Verse;

namespace BigMod.Entities.Windows.Resources
{
    public class Resources : ListWindow, IOnRequest
    {
        public override Rect DefaultBounds { get; set; } = new Rect((UI.screenWidth - 265), 60, 260, 400);
        public int RequestCurrent { get; set; }
        public virtual int RequestRate { get; set; } = 1800;

        public event EventHandler OnRequest;
        public static List<Group> SortingGroups;

        public Resources()
        {
            Identifier = "Window_Resources";

            ListView.AllowSelection = false;
            ListView.IsMultiSelect = false;

            CanCloseRightClick = true;

            // Initiate the sorting groups the first time we are opening this window
            if (SortingGroups == null)
            {
                List<GroupDefinition> GroupDefinitions = Globals.Read<GroupDefinitions>(Path.Combine(BigMod.Directory, "Resources/Resources.xml")).Items;
                SortingGroups = new List<Group>();

                foreach (GroupDefinition Definition in GroupDefinitions)
                {
                    SortingGroups.Add(new Group(Definition));
                }
            }

            Root.OnWhileMouseOver += (obj, e) =>
            {
                DrawBackground = true;
            };
            Root.OnMouseLeave += (obj, e) =>
            {
                DrawBackground = false;
            };
        }

        public override void Update()
        {
            base.Update();

            if (IsVisible)
            {
                if (RequestCurrent >= RequestRate)
                {
                    RequestCurrent = 0;
                    DoOnRequest();
                }
                else
                {
                    RequestCurrent++;
                }
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            if (!ListView.Items.Any())
            {
                Populate();
            }
        }

        /// <summary>
        /// Gets all items that should be displayed.
        /// </summary>
        /// <returns>Dictionary of all items that should be displayed.</returns>
        public static Dictionary<ThingDef, (List<Thing> Things, int Count)> GetAllListableCommodities()
        {
            Dictionary<ThingDef, (List<Thing> Things, int Count)> Commodities = Globals.GetAllCommodities();
            Dictionary<ThingDef, (List<Thing> Things, int Count)> ListableCommodities = new Dictionary<ThingDef, (List<Thing> Things, int Count)>();

            foreach (var Commodity in Commodities)
            {
                if ((Commodity.Value.Count > 0) || Commodity.Key.resourceReadoutAlwaysShow)
                {
                    ListableCommodities.Add(Commodity.Key, Commodity.Value);
                }
            }

            return ListableCommodities;
        }

        /// <summary>
        /// Used once on initiation to populate the ListView with the grouping tree.
        /// </summary>
        public void Populate()
        {
            List<ListViewItem> Items = new List<ListViewItem>();

            foreach (Group Group in SortingGroups)
            {
                Items.AddRange(CreateGroupingTree(Group));
            }

            ListView.AddRange(Items);

            DoOnRequest();
        }

        public static List<ListViewItemGroup_Resource> CreateGroupingTree(Group Group)
        {
            List<ListViewItemGroup_Resource> Items = new List<ListViewItemGroup_Resource>();

            ListViewItemGroup_Resource ResourceGroup = new ListViewItemGroup_Resource(Group.Name, Group.Icon, Group);

            Items.Add(ResourceGroup);

            foreach (Item Item in Group.Items)
            {
                if (Item.GetType() == typeof(Group))
                {
                    ResourceGroup.AddRange(CreateGroupingTree((Group)Item));
                }
            }

            return Items;
        }

        public void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            Dictionary<ThingDef, (List<Thing> Things, int Count)> ListableCommodities = GetAllListableCommodities();
            List<ListViewItem> Items = ListView.GetItemsReclusively();
            List<ListViewItem_Resource> Resources = Items.OfType<ListViewItem_Resource>().ToList();
            List<ListViewItemGroup_Resource> Groups = Items.OfType<ListViewItemGroup_Resource>().ToList();

            foreach (var Commodity in ListableCommodities)
            {
                bool Found = false;

                // Update existing item
                foreach (ListViewItem_Resource Resource in Resources)
                {
                    if (Resource.ThingDef == Commodity.Key)
                    {
                        Resource.DoOnRequest(Commodity.Value.Count);
                        Found = true;
                        break;
                    }
                }

                // Item doesn't exist, create one
                if (!Found)
                {
                    foreach (ListViewItemGroup_Resource Group in Groups)
                    {
                        if (Group.GroupDefinition.Filter(Commodity.Key))
                        {
                            Group.AddItem(new ListViewItem_Resource(new Vector2(Width, ListView.MinSize.y), Commodity.Key, Commodity.Value.Count));
                            break;
                        }
                    }
                }
            }

            foreach (ListViewItemGroup_Resource Group in Groups)
            {
                Group.DoOnRequest();
            }
        }
    }
}
