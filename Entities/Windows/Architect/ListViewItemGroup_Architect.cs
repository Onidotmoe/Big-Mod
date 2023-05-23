using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Architect
{
    /// <summary>
    /// The main recipe button that holds all the different material versions of the building.
    /// </summary>
    public class ListViewItemGroup_Architect : ListViewItemGroup
    {
        /// <summary>
        /// Called by <see cref="DoOnRequest"/>.
        /// </summary>
        public event EventHandler OnRequest;
        /// <summary>
        /// Build designator used to activate the recipe.
        /// </summary>
        public Designator_Build Designator;
        /// <summary>
        /// Definition of this Item's thing based on its parent. Like a Wall.
        /// </summary>
        public BuildableDef BuildableDef;
        /// <summary>
        /// Displays the main or selected Recipe available for this designator in the Group ListViewItemGroup.
        /// </summary>
        public ListViewItem Recipe;
        /// <summary>
        /// Child of <see cref="Recipe"/>, holds all the recipe ingredients.
        /// </summary>
        public ListView Recipe_ListView;
        /// <summary>
        /// Item that holds <see cref="Materials_ListView"/>.
        /// </summary>
        public ListViewItem Materials;
        /// <summary>
        /// Child of <see cref="Materials"/>, holds all the different Material versions of the main recipe.
        /// </summary>
        public ListView Materials_ListView;
        /// <summary>
        /// Additional ToolTip information icon.
        /// </summary>
        public Image ToolTipIcon;

        public ListViewItemGroup_Architect(Designator_Build Designator)
        {
            this.Designator = Designator;
            BuildableDef = Designator.PlacingDef;

            Style.DrawMouseOver = false;

            Header.CanToggle = true;
            Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Header.Label.Offset = new Vector2(_Bounds.height + 5f, 0);
            Text = Designator.LabelCap;

            if (BuildableDef.constructionSkillPrerequisite > 0)
            {
                Header.ToolTipText += $"{"SkillNeededForConstructing".Translate(SkillDefOf.Construction.LabelCap)} : {BuildableDef.constructionSkillPrerequisite}" + Environment.NewLine;
            }
            if (BuildableDef.artisticSkillPrerequisite > 0)
            {
                Header.ToolTipText += $"{"SkillNeededForConstructing".Translate(SkillDefOf.Artistic.LabelCap)} : {BuildableDef.artisticSkillPrerequisite}" + Environment.NewLine;
            }
            if (!string.IsNullOrWhiteSpace(Designator.Desc))
            {
                Header.ToolTipText += Designator.Desc + Environment.NewLine;
            }

            Image = new Image((Texture2D)Designator.ResolvedIcon());
            Image.SetStyle("ListViewItemGroup_Architect.Image");
            Image.Style.Color = Designator.IconDrawColor;
            Image.ScaleMode = ScaleMode.ScaleToFit;
            Image.Bounds = new Rect(Header.Position, new Vector2(_Bounds.height, _Bounds.height));
            Image.OnClick += OpenInfo;
            Image.OnMouseEnter += Image_OnMouseEnter;
            AddChild(Image);

            Materials = new ListViewItem();
            Materials.SetStyle("ListViewItemGroup_Architect.Materials");
            Materials.Header.SetStyle("ListViewItemGroup_Architect.Materials.Header");
            Materials.Offset = new Vector2(5f, 0);
            Materials.InheritChildrenSizeHeight = true;
            Materials.CanFilter = false;
            Materials.Selectable = false;
            Materials.Header.CanToggle = false;
            AddItem(Materials);

            Materials_ListView = new ListView(Header.Width, (Header.Height * 1.6f), false, false);
            Materials_ListView.RenderStyle = ListViewStyle.Grid;
            Materials_ListView.Style.DrawBackground = false;
            Materials_ListView.InheritParentSizeWidth = true;
            Materials_ListView.IgnoreGUIScroll = true;
            Materials_ListView.ExtendItemsHorizontally = false;
            Materials_ListView.ExpandVertically = true;
            Materials_ListView.AllowSelection = true;
            Materials_ListView.ScrollbarSize = 0f;
            Materials_ListView.ShowScrollbarHorizontal = false;
            Materials_ListView.ShowScrollbarVertical = false;
            Materials_ListView.OnSelectionChanged += MaterialsListView_OnSelectionChanged;
            Materials.AddChild(Materials_ListView);

            Recipe = new ListViewItem();
            Recipe.SetStyle("ListViewItemGroup_Architect.Recipe");
            Recipe.Header.SetStyle("ListViewItemGroup_Architect.Recipe.Header");
            Recipe.Offset = new Vector2(5f, 0);
            Recipe.InheritChildrenSizeHeight = true;
            // Do not raise event for child Items, they shouldn't be filtered.
            Recipe.CanFilter = false;
            Recipe.Selectable = false;
            Recipe.Header.CanToggle = false;
            AddItem(Recipe);

            Recipe_ListView = new ListView(Recipe.Width, Recipe.Height, false, false);
            Recipe_ListView.Style.DrawBackground = false;
            Recipe_ListView.ExpandVertically = true;
            Recipe_ListView.InheritParentSizeWidth = true;
            Recipe_ListView.IgnoreGUIScroll = true;
            // Has to be here otherwise the ListView's children will have a offset.
            Recipe_ListView.ScrollbarSize = 0f;
            Recipe_ListView.ShowScrollbarHorizontal = false;
            Recipe_ListView.ShowScrollbarVertical = false;
            Recipe.AddChild(Recipe_ListView);

            // Set initial lock state.
            IsUnLocked();

            if (!CanDisplay())
            {
                // It can't expand so remove the dots.
                Text.ReplaceFirst("...", string.Empty);
            }
            else if (!Text.EndsWith("..."))
            {
                // Add dots to indicate that this Item can be expanded.
                Text += "...";
            }
        }

        public ListViewItem_Architect_Material GetSelectedItemMaterial()
        {
            return (Materials_ListView.SelectedItems.Any() ? (ListViewItem_Architect_Material)Materials_ListView.SelectedItems.FirstOrDefault() : null);
        }

        /// <summary>
        /// Generates and updates items when Expanding.
        /// </summary>
        public override void DoOnExpanding()
        {
            base.DoOnExpanding();

            // Force update
            DoOnRequest();
        }

        public bool CanDisplay()
        {
            // Only Items that require resources to build can be expanded.
            // Some buildings aren't MadeFromStuff and need to have their CostList checked instead.
            // If the research needed for this Group isn't completed, then the Group isn't available.
            return (Designator.PlacingDef.BuildableByPlayer && ((Designator.PlacingDef.MadeFromStuff || (Designator.PlacingDef.CostList != null) && Designator.PlacingDef.CostList.Any())) && IsUnLocked());
        }

        public override bool CanExpand()
        {
            // Clicking on the Image shouldn't affect the Group expansion state.
            return (CanDisplay() && !Image.IsMouseOver);
        }

        public override bool CanCollapse()
        {
            return !Image.IsMouseOver;
        }

        public void AddToolTipIcon()
        {
            if (ToolTipIcon == null)
            {
                ToolTipIcon = new Image(Globals.TryGetTexturePathFromAlias("Locked"), 10f, 10f);
                ToolTipIcon.Style.Color = Globals.GetColor("ListViewItemGroup_Architect.ToolTipIcon.Color");
                ToolTipIcon.Anchor = Anchor.TopRight;
                ToolTipIcon.Offset = new Vector2(-2f, 2f);
                AddChild(ToolTipIcon);
            }
        }

        public bool SkillRequirementMet(SkillDef SkillDef, int Skill, out string Text)
        {
            Text = string.Empty;

            if (!AnyColonistWithSkill(Skill, SkillDef, false))
            {
                Text += "NoColonistWithSkillTip".Translate(Faction.OfPlayer.def.pawnsPlural).Colorize(Globals.GetColor("ListViewItemGroup_Architect.ToolTipIcon.NoColonistWithSkillTip")) + Environment.NewLine;
            }
            else if (!AnyColonistWithSkill(Skill, SkillDef, true))
            {
                Text += "AllColonistsWithSkillHaveDisabledConstructingTip".Translate(Faction.OfPlayer.def.pawnsPlural, WorkTypeDefOf.Construction.gerundLabel).Colorize(Globals.GetColor("ListViewItemGroup_Architect.ToolTipIcon.AllColonistsWithSkillHaveDisabledConstructingTip")) + Environment.NewLine;
            }
            if (Designator.AnyMechWithSkillsRequired(BuildableDef, out Pawn Pawn))
            {
                Text += "MechCanBuildThis".Translate(Pawn.kindDef.label) + Environment.NewLine;
            }

            if (Text == string.Empty)
            {
                Text += "NoColonistWithAllSkillsForConstructing".Translate(Faction.OfPlayer.def.pawnsPlural) + Environment.NewLine;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks and applies behavior if this Item is currently available to the player.
        /// </summary>
        /// <returns>True if all the prerequisites are fulfilled.</returns>
        public bool IsUnLocked()
        {
            if (!Header.IsLocked)
            {
                string Text = string.Empty;

                if (BuildableDef.constructionSkillPrerequisite > 0)
                {
                    SkillRequirementMet(SkillDefOf.Construction, BuildableDef.constructionSkillPrerequisite, out Text);
                }
                if (BuildableDef.artisticSkillPrerequisite > 0)
                {
                    SkillRequirementMet(SkillDefOf.Artistic, BuildableDef.artisticSkillPrerequisite, out Text);
                }
                if (!Designator.PlacingDef.IsResearchFinished)
                {
                    // TODO: Formatting isn't preserved with the vanilla tooltip behavior.
                    Text += "RequiresResearch".Translate().Colorize(Globals.GetColor("ListViewItemGroup_Architect.ToolTipIcon.RequiresResearch")) + Environment.NewLine;

                    foreach (ResearchProjectDef Research in Designator.PlacingDef?.researchPrerequisites)
                    {
                        Text += Research.LabelCap + Environment.NewLine;
                    }
                }

                Header.IsLocked = !string.IsNullOrWhiteSpace(Text);

                if (Header.IsLocked)
                {
                    AddToolTipIcon();
                    ToolTipIcon.ToolTipText = Text;

                    IsExpanded = false;
                    // Remove items from memory.
                    Recipe_ListView.Clear();
                    Materials_ListView.Clear();
                }
            }

            return !Header.IsLocked;
        }

        public void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            if (IsVisible && IsExpanded && Designator.PlacingDef.BuildableByPlayer && IsUnLocked())
            {
                List<ThingDef> Resources = Globals.GetMapResources_CanMake(Designator.PlacingDef);

                if ((Resources == null) || !Resources.Any() || !Designator.PlacingDef.MadeFromStuff)
                {
                    Resources = new List<ThingDef>();
                    ThingDef DefaultStuff = GenStuff.DefaultStuffFor(Designator.PlacingDef);

                    if (DefaultStuff != null)
                    {
                        Resources.Add(DefaultStuff);
                    }
                    else
                    {
                        // Designator.PlacingDef.CostStuffCount doesn't correctly give us if the Item needs resources to be built.
                        if ((Designator.PlacingDef.CostList != null) && Designator.PlacingDef.CostList.Any())
                        {
                            // Takes the first as this is only to allow selection and expanding the group to display the recipe.
                            // Otherwise we'd get multiple material items that just do the same thing.
                            Resources.Add(Designator.PlacingDef.CostList.First().thingDef);
                        }
                    }
                }

                if (Resources.Any())
                {
                    Materials.IsVisible = true;

                    string SanitizedName = " " + Designator.PlacingDef.label.ReplaceFirst("...", string.Empty);

                    // Has to be a square
                    Vector2 ItemSize = new Vector2((Header.Height * 1.6f), (Header.Height * 1.6f));
                    // Existing Items
                    List<ListViewItem_Architect_Material> DesignatorItems = Materials_ListView.Items.OfType<ListViewItem_Architect_Material>().ToList();

                    foreach (ThingDef StuffDef in Resources)
                    {
                        ListViewItem_Architect_Material Instance = DesignatorItems.FirstOrDefault((F) => F.StuffDef == StuffDef);

                        if (Instance == null)
                        {
                            Instance = new ListViewItem_Architect_Material(ItemSize, BuildableDef, StuffDef, SanitizedName);
                            Materials_ListView.AddItem(Instance);
                        }

                        Instance.DoOnRequest();
                    }
                }

                // Update Item Height.
                Materials.Height = Materials_ListView.Height;

                UpdateListing();
            }
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClick(Sender, EventArgs);

            if (Header.IsEnabled && !Designator.PlacingDef.MadeFromStuff && ((Designator.PlacingDef.CostList == null) || !Designator.PlacingDef.CostList.Any()))
            {
                if (!Header.IsSelected)
                {
                    Find.DesignatorManager.Select(Designator);
                }
                else
                {
                    Find.DesignatorManager.Deselect();
                }
            }
        }

        public void UpdateListing()
        {
            Recipe_ListView.Clear();

            ThingDef SelectedStuff = GetSelectedItemMaterial()?.StuffDef;

            if (SelectedStuff == null)
            {
                SelectedStuff = GenStuff.DefaultStuffFor(Designator.PlacingDef);
            }

            List<ThingDefCountClass> Resources;

            if ((SelectedStuff != null) && Designator.PlacingDef.MadeFromStuff)
            {
                Resources = Designator.PlacingDef.CostListAdjusted(SelectedStuff);
            }
            else
            {
                // If this list is empty, then the recipe is free
                Resources = Designator.PlacingDef.CostList;
            }

            if ((Resources != null) && Resources.Any())
            {
                Recipe.IsVisible = true;

                foreach (ThingDefCountClass Resource in Resources)
                {
                    Recipe_ListView.AddItem(new ListViewItem_Architect_Recipe(new Vector2(Header.Width, Header.Height), Resource.thingDef, Resource.count));
                }

                // Update Item Height.
                Recipe.Height = Recipe_ListView.Height;

                DoOnChildrenChanged(this, EventArgs.Empty);
            }
            else
            {
                Recipe.IsVisible = false;
                Materials.IsVisible = false;

                AddToolTipIcon();
                ToolTipIcon.ToolTipText = "NoMaterialsRequired".Translate().Colorize(Globals.GetColor("ListViewItemGroup_Architect.ToolTipIcon.NoMaterialsRequired"));
            }

            UpdateItemParent();
        }

        public void MaterialsListView_OnSelectionChanged(object Sender, EventArgs EventArgs)
        {
            ListViewItem_Architect_Material Item = (ListViewItem_Architect_Material)Sender;

            if (!WindowManager.IsShiftDown())
            {
                if (Item.IsSelected && Item.Selectable)
                {
                    Find.DesignatorManager.Select(Item.Designator);

                    UpdateListing();
                    UpdateImage();
                }
                else
                {
                    Find.DesignatorManager.Deselect();
                }
            }
            else
            {
                if (Item.BuildableDef is ThingDef ThingDef)
                {
                    // Modified from Widgets.InfoCardButton
                    WindowManager.OpenWindowVanilla(new Dialog_InfoCard(ThingDef, Item.StuffDef));
                }
            }
        }

        public static void Image_OnMouseEnter(object Sender, MouseEventArgs EventArgs)
        {
            ListViewItem Item = (ListViewItem)((Panel)Sender).Parent;

            // Don't make a new ToolTip if there's a existing one targeting the same item
            if (WindowManager.Instance.GetToolTipFromHost(Item.Image) == null)
            {
                TooltipHandler.ClearTooltipsFrom(new Rect(Item.GetAbsolutePosition(), Item.Header.Size));
                WindowManager.Instance.RemoveToolTips("Item Image Preview");

                Image ToolTipImage = new Image(Item.Image.Texture);
                ToolTipImage.ScaleMode = ScaleMode.ScaleToFit;
                ToolTipImage.Size = (Item.Image.Size * 3f);
                ToolTipImage.Style = Item.Image.Style;

                ToolTip ToolTip = new ToolTip();
                ToolTip.Identifier = "Item Image Preview";
                ToolTip.ToolTipHostRect = new Rect(Item.GetAbsolutePosition() + Item.Image.Offset, Item.Image.Size);
                ToolTip.Bounds = ToolTipImage.Bounds;
                ToolTip.Root.AddChild(ToolTipImage);

                WindowManager.Instance.AddToolTip(ToolTip);
            }
        }

        public void OpenInfo(object Sender, MouseEventArgs EventArgs)
        {
            if (BuildableDef is ThingDef ThingDef)
            {
                WindowManager.OpenWindowVanilla(new Dialog_InfoCard(ThingDef, GetSelectedItemMaterial()?.StuffDef));

                UpdateImage();
            }
        }

        public void UpdateImage()
        {
            if (BuildableDef.MadeFromStuff)
            {
                ListViewItem_Architect_Material SelectedMaterial = GetSelectedItemMaterial();

                if (SelectedMaterial != null)
                {
                    Image.Style.Color = BuildableDef.GetColorForStuff(SelectedMaterial.StuffDef);
                }
            }
        }

        /// <summary>
        /// Copy of the private method in Designator_Build.AnyColonistWithSkill.
        /// </summary>
        public static bool AnyColonistWithSkill(int skill, SkillDef skillDef, bool careIfDisabled)
        {
            foreach (Pawn freeColonist in Find.CurrentMap.mapPawns.FreeColonists)
            {
                if (freeColonist.skills.GetSkill(skillDef).Level >= skill && (!careIfDisabled || freeColonist.workSettings.WorkIsActive(WorkTypeDefOf.Construction)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
