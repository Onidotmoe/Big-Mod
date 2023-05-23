using BigMod.Entities.Windows.Resources;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Architect
{
    public class Group : Item
    {
        public List<Designator> Specials;
        public List<DesignationCategoryDef> Categories;
        public List<DesignationCategoryDef> Exclude;
        public List<Item> Items;
        public string Icon;
        public string Name;

        public Group(GroupDefinition GroupDefinition)
        {
            Icon = GroupDefinition.Icon;
            Name = GroupDefinition.Name;

            Categories = StringToDesignationCategoryDef(GroupDefinition.Categories);
            Exclude = StringToDesignationCategoryDef(GroupDefinition.Exclude);

            Specials = new List<Designator>();
            Items = new List<Item>();

            foreach (ItemDefinition ItemDefinition in GroupDefinition.Items)
            {
                if (ItemDefinition.GetType() == typeof(GroupDefinition))
                {
                    Items.Add(new Group((GroupDefinition)ItemDefinition));
                }
                else if (!string.IsNullOrWhiteSpace(ItemDefinition.Special))
                {
                    string[] AssemblyClassPath = ItemDefinition.Special.Split(',');
                    Designator Designator = (Activator.CreateInstance(AssemblyClassPath[0], AssemblyClassPath[1]).Unwrap() as Designator);

                    if (Designator != null)
                    {
                        Specials.Add(Designator);
                    }
                }
                else
                {
                    Items.Add(new Item(ItemDefinition));
                }
            }
        }

        public bool Filter(Designator Designator)
        {
            if (Designator is Designator_Build Build)
            {
                if (Categories.Contains(Build.PlacingDef.designationCategory) && !Exclude.Contains(Build.PlacingDef.designationCategory))
                {
                    return true;
                }
                else
                {
                    foreach (Item Item in Items)
                    {
                        // Do not cascade filtering to subgroups
                        if ((Item.GetType() == typeof(Item)) && Item.Filter(Build.PlacingDef))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                return Categories.Any((F) => F.specialDesignatorClasses.Contains(Designator.GetType()));
            }

            return false;
        }

        /// <summary>
        /// Converts a comma delimited string into a <see cref="DesignationCategoryDef"/> list.
        /// </summary>
        /// <param name="CategoryArray">Comma delimited string.</param>
        /// <returns>List of DesignationCategoryDef in the supplied string.</returns>
        public List<DesignationCategoryDef> StringToDesignationCategoryDef(string CategoryArray)
        {
            List<DesignationCategoryDef> Categories = new List<DesignationCategoryDef>();

            if (!string.IsNullOrWhiteSpace(CategoryArray))
            {
                foreach (string CategoryName in CategoryArray.Split(','))
                {
                    Categories.Add(DefDatabase<DesignationCategoryDef>.GetNamed(CategoryName));
                }
            }

            return Categories;
        }
    }

    public class Item
    {
        public string DefName;
        public string Special;

        /// <summary>
        /// Checks if the specified item matches the filter of this definition.
        /// </summary>
        /// <param name="BuildableDef">BuildableDef to filer with.</param>
        /// <remarks>Needs to be <see cref="Verse.BuildableDef"/> as <see cref="Verse.ThingDef"/> doesn't cover stuff like Bridge which is of class <see cref="Verse.TerrainDef"/>.</remarks>
        /// <returns>True if the specified item matches passes the filter.</returns>
        public virtual bool Filter(BuildableDef BuildableDef)
        {
            return (DefName == BuildableDef.defName);
        }

        public Item()
        {
        }

        public Item(ItemDefinition ItemDefinition)
        {
            DefName = ItemDefinition.DefName;
        }
    }
}
