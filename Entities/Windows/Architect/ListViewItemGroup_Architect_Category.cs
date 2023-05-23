using Verse;

namespace BigMod.Entities.Windows.Architect
{
    /// <summary>
    /// Category and Group holder.
    /// </summary>
    public class ListViewItemGroup_Architect_Category : ListViewItemGroup
    {
        /// <summary>
        /// Defines the filtering for this group.
        /// </summary>
        public Group GroupDefinition;

        public ListViewItemGroup_Architect_Category(string Text, string TexturePath, Group Group) : base(Text.Translate())
        {
            GroupDefinition = Group;

            Header.CanToggle = true;

            if (!string.IsNullOrWhiteSpace(TexturePath))
            {
                SetTexture(TexturePath);
                Header.Label.Offset = new Vector2(Height + 5f, 0);
            }

            if (Group.Specials.Any())
            {
                foreach (Designator Designator in Group.Specials)
                {
                    AddItem(new ListViewItem_Architect_Special(Designator));
                }
            }
        }
    }
}
