using BigMod.Entities.Interface;
using BigMod.Entities.Windows.Resources;
using RimWorld;
using System.IO;
using Verse;

namespace BigMod.Entities.Windows.Architect
{
    /// <summary>
    /// Replaces the MainTabWindow_Architect class when its button is being pressed in the ingame HUD.
    /// </summary>
    public class Architect : ListWindow, IOnRequest
    {
        public int RequestCurrent { get; set; }
        public int RequestRate { get; set; } = 1200;

        public event EventHandler OnRequest;
        public static List<Group> SortingGroups;
        public override Rect DefaultBounds { get; set; } = new Rect(10, (UI.screenHeight - 410), 260, 400);

        public Architect() : base(Should_MouseOverToggleVisibility: false)
        {
            Identifier = "Window_Architect";

            ListView.AllowSelection = false;
            ListView.IsMultiSelect = false;

            DrawBackground = true;
            DrawBorder = true;

            // Initiate the sorting groups the first time we are opening this window
            if (SortingGroups == null)
            {
                List<GroupDefinition> GroupDefinitions = Globals.Read<GroupDefinitions>(Path.Combine(BigMod.Directory, "Resources/Architect.xml")).Items;
                SortingGroups = new List<Group>();

                foreach (GroupDefinition Definition in GroupDefinitions)
                {
                    SortingGroups.Add(new Group(Definition));
                }
            }
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

        public static List<ListViewItemGroup_Architect_Category> CreateGroupingTree(Group Group)
        {
            List<ListViewItemGroup_Architect_Category> Items = new List<ListViewItemGroup_Architect_Category>();

            ListViewItemGroup_Architect_Category ResourceGroup = new ListViewItemGroup_Architect_Category(Group.Name, Group.Icon, Group);

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

            // Flatten all possible buildables
            List<Designator> Designators = DefDatabase<DesignationCategoryDef>.AllDefsListForReading.SelectMany((F) => F.ResolvedAllowedDesignators).GroupBy((F) => F.Label).Select((F) => F.First()).ToList();
            List<ListViewItem> Items = ListView.GetItemsReclusively();
            List<ListViewItem_Architect_Special> Special = Items.OfType<ListViewItem_Architect_Special>().ToList();
            List<ListViewItemGroup_Architect> Recipes = Items.OfType<ListViewItemGroup_Architect>().ToList();
            List<ListViewItemGroup_Architect_Category> Groups = Items.OfType<ListViewItemGroup_Architect_Category>().ToList();
            // Designators in this list will be added outside of all groups to the bottom of the listview
            List<Designator> OverflowDesignators = new List<Designator>();

            // TODO: Only update visible ones

            foreach (Designator Designator in Designators)
            {
                bool Found = false;

                // Update existing item
                foreach (ListViewItemGroup_Architect Recipe in Recipes)
                {
                    if (Recipe.Designator == Designator)
                    {
                        Recipe.DoOnRequest();

                        Found = true;
                        break;
                    }
                }

                if (!Found)
                {
                    Found = Special.Any((F) => (F.Designator == Designator));
                }

                // Item doesn't exist, create one
                if (!Found)
                {
                    foreach (ListViewItemGroup_Architect_Category Group in Groups)
                    {
                        if (Group.GroupDefinition.Filter(Designator))
                        {
                            if (Designator is Designator_Build Build)
                            {
                                Group.AddItem(new ListViewItemGroup_Architect(Build));
                            }
                            else
                            {
                                Group.AddItem(new ListViewItem_Architect_Special(Designator));
                            }

                            Found = true;
                            break;
                        }
                    }

                    if (!Found)
                    {
                        // Do not pass on regular Designators as they are harder to filter
                        if (Designator is Designator_Build Build)
                        {
                            OverflowDesignators.Add(Build);
                        }
                    }
                }
            }

            // Add these to the bottom, outside of any group
            foreach (Designator Designator in OverflowDesignators)
            {
                if (Designator is Designator_Build Build)
                {
                    ListView.AddItem(new ListViewItemGroup_Architect(Build));
                }
            }
        }
    }
}
