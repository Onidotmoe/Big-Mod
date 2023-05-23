using System.Reflection;
using System.Xml.Serialization;
using Verse;

namespace BigMod.Entities.Windows.Resources
{
    public class GroupDefinitions
    {
        [XmlArray]
        [XmlArrayItem("Group")]
        public List<GroupDefinition> Items;
    }

    public class ItemDefinition
    {
        /// <summary>
        /// Name of a <see cref="ThingDef"/> to add manually.
        /// </summary>
        [XmlAttribute]
        public string DefName;
        /// <summary>
        /// Used by <see cref="Windows.Architect.Group"/> for <see cref="Designator"/> without <see cref="BuildableDef"/>.
        /// </summary>
        [XmlAttribute]
        public string Special;
    }

    public class GroupDefinition : ItemDefinition
    {
        /// <summary>
        /// String that will be displayed on the <see cref="ListViewItemGroup_Resource"/> item header.
        /// </summary>
        [XmlAttribute]
        public string Name;
        /// <summary>
        /// String array of special handling properties for different categories.
        /// </summary>
        [XmlAttribute]
        public string Attributes;
        /// <summary>
        /// String array of <see cref="ThingCategoryDef"/> to filter items with.
        /// </summary>
        [XmlAttribute]
        public string Categories;
        /// <summary>
        /// String array of <see cref="PropertyInfo"/> names to filter items with, properties must be boolean.
        /// </summary>
        [XmlAttribute]
        public string Properties;
        /// <summary>
        /// String array of <see cref="FieldInfo"/> names to filter items with, fields must be boolean.
        /// </summary>
        [XmlAttribute]
        public string Fields;
        /// <summary>
        /// String array of <see cref="ThingCategoryDef"/> to filter items with, items in these categories will not be allowed in this group.
        /// </summary>
        [XmlAttribute]
        public string Exclude;
        /// <summary>
        /// List of sub-items.
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Group", typeof(GroupDefinition))]
        [XmlArrayItem("Item", typeof(ItemDefinition))]
        public List<ItemDefinition> Items;
        /// <summary>
        /// Name of the texture alias to display on the <see cref="ListViewItemGroup_Resource"/> item.
        /// </summary>
        [XmlAttribute]
        public string Icon;
    }

    public class Group : Item
    {
        public List<string> Attributes;
        public List<ThingCategoryDef> Categories;
        public List<ThingCategoryDef> Exclude;
        public List<PropertyInfo> Properties;
        public List<FieldInfo> Fields;
        public string Icon;
        public string Name;

        public List<Item> Items;

        public Group()
        {
        }

        public Group(GroupDefinition GroupDefinition)
        {
            Icon = GroupDefinition.Icon;
            Name = GroupDefinition.Name;

            Categories = StringToCategoryDef(GroupDefinition.Categories);
            Exclude = StringToCategoryDef(GroupDefinition.Exclude);

            if (!string.IsNullOrWhiteSpace(GroupDefinition.Fields))
            {
                Fields = GroupDefinition.Fields.Split(',').Select((F) => typeof(ThingDef).GetField(F, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(GroupDefinition.Properties))
            {
                Properties = GroupDefinition.Properties.Split(',').Select((F) => typeof(ThingDef).GetProperty(F, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(GroupDefinition.Attributes))
            {
                Attributes = GroupDefinition.Attributes.Split(',').ToList();
            }

            Items = new List<Item>();

            foreach (ItemDefinition ItemDefinition in GroupDefinition.Items)
            {
                if (ItemDefinition.GetType() == typeof(GroupDefinition))
                {
                    Items.Add(new Group((GroupDefinition)ItemDefinition));
                }
                else
                {
                    Items.Add(new Item(ItemDefinition));
                }
            }
        }

        /// <summary>
        /// Converts a comma delimited string into a <see cref="ThingCategoryDef"/> list.
        /// </summary>
        /// <param name="CategoryArray">Comma delimited string.</param>
        /// <returns>List of ThingCategories in the supplied string.</returns>
        public List<ThingCategoryDef> StringToCategoryDef(string CategoryArray)
        {
            List<ThingCategoryDef> Categories = new List<ThingCategoryDef>();

            if (!string.IsNullOrWhiteSpace(CategoryArray))
            {
                foreach (string CategoryName in CategoryArray.Split(','))
                {
                    Categories.Add(ThingCategoryDef.Named(CategoryName));
                }
            }

            return Categories;
        }

        public override bool Filter(ThingDef ThingDef)
        {
            if (ThingDef.thingCategories.Intersect(Categories).Any() && !ThingDef.thingCategories.Intersect(Exclude).Any())
            {
                return true;
            }
            else if ((Fields != null) && Fields.Any((F) => (F.GetValue(ThingDef) is bool Value) && Value))
            {
                return true;
            }
            else if ((Properties != null) && Properties.Any((F) => (F.GetValue(ThingDef) is bool Value) && Value))
            {
                return true;
            }
            else
            {
                foreach (Item Item in Items)
                {
                    // Do not cascade filtering to subgroups
                    if ((Item.GetType() == typeof(Item)) && Item.Filter(ThingDef))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public class Item
    {
        public string DefName;

        /// <summary>
        /// Checks if the specified item matches the filter of this definition.
        /// </summary>
        /// <param name="ThingDef">ThingDef to filer with.</param>
        /// <returns>True if the specified item matches passes the filter.</returns>
        public virtual bool Filter(ThingDef ThingDef)
        {
            return (DefName == ThingDef.defName);
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
