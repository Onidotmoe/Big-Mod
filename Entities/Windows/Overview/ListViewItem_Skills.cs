using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListViewItem_Skills : ListViewItem, IPull
    {
        public Pawn Pawn { get; set; }
        public SkillRecord Skill;
        public SkillDef Def;
        public ProgressBar ProgressBar;
        public Label Value = new Label();

        public ListViewItem_Skills(Pawn Pawn, SkillDef Def)
        {
            this.Pawn = Pawn;
            this.Def = Def;

            Text = Def.skillLabel.CapitalizeFirst();
            Skill = Pawn.skills.GetSkill(Def);

            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Label.Offset = new Vector2(5f, 0f);

            Value.Size = new Vector2(25f, Height);
            Value.Style.TextAnchor = TextAnchor.MiddleRight;
            Value.Anchor = Anchor.TopRight;
            Value.OffsetX = -5f;
            AddChild(Value);

            if (Skill.TotallyDisabled)
            {
                Value.Style.TextColor = Globals.GetColor("ListViewItem_Skills.Value.Disabled.TextColor");
                Value.Text = "-";

                Header.Label.Style.TextColor = Value.Style.TextColor;
            }
            else
            {
                Image = new Image();
                Image.Style.Color = Color.white;
                Image.Size = new Vector2(Height, Height);
                Image.Anchor = Anchor.TopRight;
                Image.Offset = new Vector2(-25f, 0f);

                if (Skill.passion != Passion.None)
                {
                    if (Skill.passion == Passion.Minor)
                    {
                        Image.SetTexture(Globals.TryGetTexturePathFromAlias("ListViewItem_Skills.PassionMinor"));
                    }
                    else if (Skill.passion == Passion.Major)
                    {
                        Image.SetTexture(Globals.TryGetTexturePathFromAlias("ListViewItem_Skills.PassionMajor"));
                    }
                }

                ProgressBar = new ProgressBar();
                ProgressBar.InheritParentSize = true;
                ProgressBar.DoRequest = false;
                ProgressBar.ColorMin = Globals.GetColor("ListViewItem_Skills.ProgressBar.ColorMin");
                ProgressBar.ColorMax = Globals.GetColor("ListViewItem_Skills.ProgressBar.ColorMax");

                AddRange(Image, ProgressBar);
                ProgressBar.MoveToFront();
            }
        }

        public void Pull()
        {
            ToolTipText = Globals.SkillUI_GetSkillDescription(Skill);

            if (ProgressBar != null)
            {
                Value.Text = Skill.Level.ToStringCached();

                ProgressBar.Percentage = ((float)Skill.Level / 20f);
            }
        }
    }
}
