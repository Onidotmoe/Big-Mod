using RimWorld;
using System.Text;
using Verse;

namespace BigMod.Entities.Windows
{
    public enum UnitStyle
    {
        /// <summary>
        /// Will draw as a square portrait of the pawn, with mood in the background, health in the bottom, and weapons underneath, additional information in tooltip or expanded button
        /// </summary>
        Square,
        /// <summary>
        /// Will draw as a line with the Pawn on the left, weapon afterwards, then miscs, then name
        /// </summary>
        Line
    }

    /// <summary>
    /// Represents a panel that shows a overview for a single pawn.
    /// </summary>
    public class ListViewItem_Pawn : ListViewItem
    {
        /// <summary>
        /// Parent Pawns Window.
        /// </summary>
        public Pawns Pawns;
        /// <summary>
        /// Pawn to retrieve data from.
        /// </summary>
        public Pawn Pawn;
        private UnitStyle _RenderStyle = UnitStyle.Line;
        /// <summary>
        /// If the Pawn is a animal and not human.
        /// </summary>
        /// <remarks>Animals don't have equipment and weapons.</remarks>
        public bool IsAnimal
        {
            get
            {
                return Pawn.RaceProps.Animal;
            }
        }
        /// <summary>
        /// If the Pawn is a Mech and not human.
        /// </summary>
        public bool IsMech
        {
            get
            {
                return Pawn.RaceProps.IsMechanoid;
            }
        }
        /// <summary>
        /// If the Pawn is human.
        /// </summary>
        public bool IsHuman
        {
            get
            {
                return Pawn.RaceProps.Humanlike;
            }
        }
        /// <summary>
        /// Is this Pawn's Faction the Player's Faction?
        /// </summary>
        public bool IsPlayerFaction
        {
            get
            {
                return (Pawn.Faction == Faction.OfPlayer);
            }
        }
        public UnitStyle RenderStyle
        {
            get
            {
                return _RenderStyle;
            }
            set
            {
                if (_RenderStyle != value)
                {
                    _RenderStyle = value;

                    switch (value)
                    {
                        case UnitStyle.Line:
                            DoStyleLine();
                            break;

                        case UnitStyle.Square:
                            DoStyleSquare();
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Called by <see cref="DoOnRequest"/>.
        /// </summary>
        public event EventHandler OnRequest;
        /// <summary>
        /// Displays Mech's current Energy level.
        /// </summary>
        public ProgressBar Energy;
        /// <summary>
        /// Displays Pawn's current Heatlh.
        /// </summary>
        public ProgressBar Health = new ProgressBar();
        /// <summary>
        /// Displays Pawn's current Mood.
        /// </summary>
        public ProgressBar Mood = new ProgressBar();
        /// <summary>
        /// Displays Pawn's current weapon.
        /// </summary>
        public Image Weapon = new Image();
        /// <summary>
        /// Displays Pawn's current status icon.
        /// </summary>
        public Image Status = new Image();
        /// <summary>
        /// Displays Critical information, like bleedingout.
        /// </summary>
        public Label Critical = new Label();
        /// <summary>
        /// Context Menu when Right-clicked.
        /// </summary>
        public ContextMenu ContextMenu;
        /// <summary>
        /// Background used behin <see cref="Image"/> in <see cref="UnitStyle.Line"/> mode and behin <see cref="Mood"/> in <see cref="UnitStyle.Square"/>.
        /// </summary>
        public Image Background = new Image();
        /// <summary>
        /// Pawn's Nickname.
        /// </summary>
        public Label Nickname = new Label();
        /// <summary>
        /// Full name without Nickname.
        /// </summary>
        public Label Name = new Label();
        /// <summary>
        /// Pawn's Title, doesn't have to be royalty.
        /// </summary>
        public Label Title = new Label();
        /// <summary>
        /// Size when in <see cref="UnitStyle.Square"/> mode.
        /// </summary>
        public Vector2 SquareSize = new Vector2(50f, 120f);
        /// <summary>
        /// Toggle if the ContextMenu should be available.
        /// </summary>
        public bool AllowContextMenu = true;
        /// <summary>
        /// <para>Toggle if this Item can be pulled out of the ListView.</para>
        /// <oara>Affect the ContextMenu too.</oara>
        /// </summary>
        public bool CanPopOut = true;
        /// <summary>
        /// Whether clicking on this Item will manipulate Pawn Selection.
        /// </summary>
        public bool PawnSelectable = true;
        private bool _Display;
        /// <summary>
        /// <para>Display Items don't show Status or Extended names.</para>
        /// <para>Display Items are mainly used in Context Menu selection, as they have less space in them.</para>
        /// </summary>
        public bool Display
        {
            get
            {
                return _Display;
            }
            set
            {
                if (_Display != value)
                {
                    _Display = value;

                    if (value)
                    {
                        DoDisplay();
                    }
                    else
                    {
                        if (RenderStyle == UnitStyle.Line)
                        {
                            DoStyleLine();
                        }
                        else if (RenderStyle == UnitStyle.Square)
                        {
                            DoStyleSquare();
                        }
                    }
                }
            }
        }

        public ListViewItem_Pawn(Pawn Pawn, Vector2 MinSize = default)
        {
            if (MinSize != default)
            {
                this.MinSize = MinSize;
            }

            Header.CanToggle = true;

            Size = ((Size != default) ? Size : MinSize);
            Header.Style.DrawBackground = false;

            this.Pawn = Pawn;

            Image = new Image();

            if (Pawn.needs.mood == null)
            {
                // Animals don't have the mood component.
                Mood.IsVisible = false;
            }

            AddRange(Mood, Health, Background, Image, Weapon, Status, Critical);
            Header.AddRange(Nickname, Title, Name);

            if (IsMech)
            {
                Energy = new ProgressBar();
                // Insert it after Health so it gets rendered in the correct order.
                InsertChild(Children.IndexOf(Health), Energy);
            }

            DoStyleLine();

            // Retrieve data immediately
            DoOnRequest();
        }

        // TODO:
        // Add Royal symbol prefix to title : ⚜️👑
        // Religion Label & Icon missing
        // Missing Biotech dlc infos
        private void DoStyleLine()
        {
            Header.InheritParentSize = true;
            Size = MinSize;

            Mood.Anchor = Anchor.TopLeft;
            Mood.MaxSize = Vector2.zero;
            Mood.InheritParentSize = true;
            Mood.LimitToParent = true;
            Mood.InheritParentSize_Modifier = new Vector2(0f, -(Height * 0.1f));
            Mood.IsVertical = false;

            Health.Anchor = Anchor.BottomLeft;
            Health.InheritParentSize = true;
            Health.LimitToParent = true;
            Health.MaxSize = new Vector2(0f, (Height * 0.1f));
            // Reset Offset when going back from Square RenderStyle.
            Health.Offset = Vector2.zero;

            Mood.ColorMin = Globals.GetColor("ListViewItem_Pawn.Mood.Style.Line.ColorMin");
            Mood.ColorMax = Globals.GetColor("ListViewItem_Pawn.Mood.Style.Line.ColorMax");
            Health.ColorMin = Globals.GetColor("ListViewItem_Pawn.Health.Style.Line.ColorMin");
            Health.ColorMax = Globals.GetColor("ListViewItem_Pawn.Health.Style.Line.ColorMax");

            if (Energy != null)
            {
                Energy.Anchor = Anchor.BottomLeft;
                Energy.InheritParentSize = true;
                Energy.LimitToParent = true;
                Energy.MaxSize = new Vector2(0f, (Height * 0.1f));
                Energy.Offset = new Vector2(0f, -Health.MaxSize.y);

                Energy.ColorMin = Globals.GetColor("ListViewItem_Pawn.Energy.Style.Line.ColorMin");
                Energy.ColorMax = Globals.GetColor("ListViewItem_Pawn.Energy.Style.Line.ColorMax");
            }

            // Move Background behin header, in the render queue.
            BringToBack(Background);
            // Move the Header to the back of the render queue, so it appears infront of the Health and Mood bars
            BringToBack(Header);

            Background.SetStyle("ListViewItem_Pawn.Image.Background");
            Background.RenderStyle = ImageStyle.Color;
            Background.IgnoreMouse = true;
            Background.Size = new Vector2(Height, Height);

            Image.SetStyle("ListViewItem_Pawn.Image");
            Image.RenderStyle = ImageStyle.Fitted;
            Image.ScaleMode = ScaleMode.ScaleToFit;
            Image.Size = new Vector2(Height, Height);
            Image.Offset = Vector2.zero;

            Weapon.SetStyle("ListViewItem_Pawn.Image");
            Weapon.ScaleMode = ScaleMode.ScaleToFit;
            Weapon.Anchor = Anchor.TopLeft;
            // Will be toggled visible in DoOnRequest if Pawn has a primary weapon
            Weapon.IsVisible = false;
            Weapon.Offset = new Vector2(Image.OffsetRight, 0f);
            Weapon.Size = new Vector2(Height, Height);

            Nickname.SetStyle("ListViewItem_Pawn.Label");
            Nickname.RenderStyle = LabelStyle.None;
            Nickname.Style.TextAnchor = TextAnchor.MiddleLeft;
            Nickname.Anchor = Anchor.TopRight;
            Nickname.ToolTipText = null;
            Nickname.InheritParentSize = true;
            Nickname.LimitToParent = true;
            Nickname.IgnoreMouse = false;
            // Animals don't have weapons
            Nickname.InheritParentSize_Modifier = new Vector2(-((IsAnimal ? Height : Weapon.OffsetRight) + Height + 2f), 0f);
            Nickname.Offset = new Vector2(-Height, 0f);

            Title.SetStyle("ListViewItem_Pawn.Label");
            Title.RenderStyle = LabelStyle.None;
            Title.Style.TextAnchor = TextAnchor.MiddleCenter;
            Title.Anchor = Anchor.TopRight;
            Title.InheritParentSize = true;
            Title.LimitToParent = true;
            Title.InheritParentSize_Modifier = Nickname.InheritParentSize_Modifier;
            Title.Offset = Nickname.Offset;

            Name.SetStyle("ListViewItem_Pawn.Label");
            Name.RenderStyle = LabelStyle.None;
            Name.Style.TextAnchor = TextAnchor.MiddleRight;
            Name.Anchor = Anchor.TopRight;
            Name.ToolTipText = null;
            Name.InheritParentSize = true;
            Name.LimitToParent = true;
            Name.InheritParentSize_Modifier = Nickname.InheritParentSize_Modifier;
            Name.Offset = Nickname.Offset;

            Status.SetStyle("ListViewItem_Pawn.Image");
            Status.RenderStyle = ImageStyle.Fitted;
            Status.Anchor = Anchor.TopRight;
            Status.IsVisible = false;
            Status.Size = new Vector2(Height, Height);
            Status.Offset = Vector2.zero;

            Critical.SetStyle("ListViewItem_Pawn.Label.Critical");
            Critical.RenderStyle = LabelStyle.None;
            Critical.Anchor = Anchor.TopRight;
            Critical.IgnoreMouse = false;
            Critical.IsVisible = false;
            Critical.Offset = new Vector2(-Height, 1f);
            Critical.Size = (Verse.Text.CalcSize("00:00") + new Vector2(3f, -2f));

            DoOnNamesVisibilityChanged();
            DoDisplay();
        }

        private void DoStyleSquare()
        {
            Header.InheritParentSize = false;
            Header.Size = new Vector2(SquareSize.x, SquareSize.x);
            Size = SquareSize;

            Mood.InheritParentSize = false;
            Mood.LimitToParent = false;
            Mood.MaxSize = Header.Size;
            Mood.Size = Header.Size;
            Mood.IsVertical = true;

            Health.InheritParentSize = false;
            Health.LimitToParent = false;
            Health.Anchor = Anchor.TopLeft;
            Health.MaxSize = Vector2.zero;
            Health.Size = new Vector2(Header.Width, (Header.Height * 0.1f));
            Health.Offset = new Vector2(0f, Header.Height);

            Mood.ColorMin = Globals.GetColor("ListViewItem_Pawn.Mood.Style.Square.ColorMin");
            Mood.ColorMax = Globals.GetColor("ListViewItem_Pawn.Mood.Style.Square.ColorMax");
            Health.ColorMin = Globals.GetColor("ListViewItem_Pawn.Health.Style.Square.ColorMin");
            Health.ColorMax = Globals.GetColor("ListViewItem_Pawn.Health.Style.Square.ColorMax");

            if (Energy != null)
            {
                Energy.InheritParentSize = false;
                Energy.LimitToParent = false;
                Energy.Anchor = Anchor.TopLeft;
                Energy.MaxSize = Vector2.zero;
                Energy.Size = new Vector2(Header.Width, (Header.Height * 0.05f));
                Energy.Offset = new Vector2(0f, (Header.Height - Energy.Height));

                Energy.ColorMin = Globals.GetColor("ListViewItem_Pawn.Energy.Style.Square.ColorMin");
                Energy.ColorMax = Globals.GetColor("ListViewItem_Pawn.Energy.Style.Square.ColorMax");
            }

            Background.MoveToFront();
            Background.Size = Header.Size;

            Image.Size = new Vector2((Width * 1.2f), (Width * 1.2f));
            Image.Offset = new Vector2(-(Width * 0.1f), -(Width * 0.2f));

            Weapon.Size = new Vector2(Width, (Height / 2.6f));
            Weapon.Anchor = Anchor.BottomLeft;
            Weapon.Offset = Vector2.zero;

            Nickname.RenderStyle = LabelStyle.Fit;
            Nickname.Style.TextAnchor = TextAnchor.MiddleCenter;
            Nickname.InheritParentSize = false;
            Nickname.Size = new Vector2(Width, (Header.Height / 3f));
            Nickname.Offset = new Vector2(0f, (Header.Height * 1.15f));

            // TODO: Needs a fontsize inbetween Small & Tiny
            Title.RenderStyle = LabelStyle.Fit;
            Title.Style.TextAnchor = TextAnchor.MiddleCenter;
            Title.InheritParentSize = false;
            Title.Size = new Vector2(Width, (Header.Height / 3f));
            Title.Offset = Vector2.zero;
            // Title should render above everything else
            BringToBack(Title);

            Name.IsVisible = false;

            Status.Size = (Header.Size / 2.5f);
            Status.Anchor = Anchor.TopLeft;
            Status.Offset = new Vector2(0f, (Header.Height - Status.Height));

            Critical.Anchor = Anchor.BottomCenter;
            Critical.Offset = new Vector2(0f, -25f);
            Critical.Height -= 2f;

            DoOnNamesVisibilityChanged();
            DoDisplay();
        }

        private void DoDisplay()
        {
            if (Display)
            {
                if (RenderStyle == UnitStyle.Line)
                {
                    Selectable = true;

                    Nickname.IsVisible = true;
                    Title.IsVisible = false;
                    Name.IsVisible = false;
                    Weapon.IsVisible = false;
                    Status.IsVisible = false;
                    Critical.IsVisible = false;

                    Nickname.InheritParentSize_Modifier = new Vector2(-(Height + 2f), 0f);
                    Nickname.Offset = Vector2.zero;

                    Header.Style.DrawMouseOver = false;
                    AllowContextMenu = false;
                    CanPopOut = false;
                    IsSelected = false;
                    PawnSelectable = false;
                }
                else if (RenderStyle == UnitStyle.Square)
                {
                    Selectable = false;

                    Nickname.IsVisible = false;
                    Name.IsVisible = false;
                    Title.IsVisible = false;
                    Weapon.IsVisible = false;

                    Style.DrawMouseOver = false;

                    AllowContextMenu = false;
                    CanPopOut = false;

                    Critical.Anchor = Anchor.TopLeft;
                    Critical.Style.FontType = GameFont.Medium;
                    Critical.Style.TextAnchor = TextAnchor.MiddleCenter;
                    Critical.Offset = new Vector2(0f, 5f);
                    Critical.SizeToText_Horizontal = false;
                    Critical.SizeToText_Vertical = true;
                    Critical.Width = SquareSize.x;
                    Critical.RenderStyle = LabelStyle.GUI;
                }
            }
        }

        /// <summary>
        /// Handles data retrieving.
        /// </summary>
        public virtual void DoOnRequest()
        {
            OnRequest?.Invoke(this, EventArgs.Empty);

            Update_Weapon();
            Update_Names();

            if (Find.Selector.IsSelected(Pawn))
            {
                Select();
            }
            else
            {
                Deselect();
            }

            Health.Percentage = Pawn.health.summaryHealth.SummaryHealthPercent;

            // TODO: this has to be moved over to OnMouseEnter instead
            // Don't update ToolTipText if Mouse isn't over.
            if (IsMouseOver)
            {
                Image.ToolTipText = Pawn.MainDesc(true);
                Health.ToolTipText = HealthUtility.GetGeneralConditionLabel(Pawn);

                if (!Pawn.Dead)
                {
                    IEnumerable<IGrouping<BodyPartRecord, Hediff>> BodyParts = Globals.HealthCardUtility_VisibleHediffGroupsInOrder(Pawn, true);

                    StringBuilder StringBuilder = new StringBuilder();

                    foreach (IGrouping<BodyPartRecord, Hediff> BodyPart in BodyParts)
                    {
                        BodyPartRecord Record = BodyPart.Key;
                        // TODO: values should be right-aligned, might just do string padding instead
                        if (BodyPart.First().Part != null)
                        {
                            Pair<string, Color> Pair = HealthUtility.GetPartConditionLabel(Pawn, Record);

                            StringBuilder.AppendInNewLine(Record.LabelCap + " : ");

                            foreach (IGrouping<int, Hediff> Group in from Part in BodyPart group Part by Part.UIGroupKey)
                            {
                                StringBuilder.Append(Group.First().LabelCap.Colorize(Pair.Second));

                                if (Group.Count() > 1)
                                {
                                    StringBuilder.Append(" " + Group.Count() + "x");
                                }

                                break;
                            }
                        }
                        else
                        {
                            StringBuilder.AppendInNewLine("WholeBody".Translate().Colorize(Globals.GetColor("ListViewItem_Pawn.Health.ToolTipText.ColorBad")));
                        }
                    }

                    Health.ToolTipText += "\n\n" + ((StringBuilder.Length > 0) ? StringBuilder.ToString() : "NoHealthConditions".Translate());
                }
            }

            if (Energy != null)
            {
                Energy.Percentage = Pawn.needs.energy.CurLevelPercentage;

                Energy.ToolTipText = ("CurrentMechEnergyFallPerDay".Translate() + " : " + (Pawn.needs.energy.FallPerDay / 100f).ToStringPercent());
            }

            if (Mood.IsVisible)
            {
                // Mood is null if dead
                Mood.Percentage = ((Health.Percentage > 0f) ? Pawn.needs.mood.CurLevelPercentage : 0f);

                if (Pawn.needs.mood != null)
                {
                    Mood.ToolTipText = $"{"Mood".Translate()} : {Pawn.needs.mood.MoodString.CapitalizeFirst()}";
                }
            }

            if (!IsAnimal && !IsMech)
            {
                // Incapacitated Pawns should have regular zoom levels.
                if (((Pawns == null) || (!Pawns.Settings.Portrait_Headshot)) || Pawn.Downed)
                {
                    Image.Texture = PortraitsCache.Get(Pawn, Image.Size, Rot4.South, ColonistBarColonistDrawer.PawnTextureCameraOffset, ColonistBarColonistDrawer.PawnTextureCameraZoom, true, true, true, true, null, null, false).ToTexture2D();
                }
                else
                {
                    Image.Texture = PortraitsCache.Get(Pawn, Image.Size, Rot4.South, new Vector3(0f, 0f, 0.45f), 2f, true, true, true, true, null, null, false).ToTexture2D();
                }
            }
            else
            {
                Image.Texture = PortraitsCache.Get(Pawn, Image.Size, Rot4.East, default(Vector3), Pawn.kindDef.controlGroupPortraitZoom, true, true, true, true, null, null, false).ToTexture2D();
            }

            Image.Texture.filterMode = FilterMode.Point;

            Update_Status();

            float BleedRateTotal = Pawn.health.hediffSet.BleedRateTotal;

            if (BleedRateTotal > 0.01f)
            {
                int Ticks = HealthUtility.TicksUntilDeathDueToBloodLoss(Pawn);

                if ((Pawn.genes != null) && Pawn.genes.HasGene(GeneDefOf.Deathless))
                {
                    Critical.Text = "Deathless".Translate();
                    Critical.ToolTipText = string.Empty;
                    Critical.IsVisible = true;
                }
                else
                {
                    int Hours = (int)Math.Floor(Ticks / 2500f);
                    int Minutes = (int)Math.Floor((Ticks % 2500f) / 60f);

                    Critical.Text = $"{Hours:00}:{Minutes:00}";

                    if (IsMouseOver)
                    {
                        Critical.ToolTipText = "BleedingRate".Translate() + " : " + "TimeToDeath".Translate(Ticks.ToStringTicksToPeriod(true, false, true, true)).CapitalizeFirst();
                    }

                    Critical.IsVisible = true;
                }

                Name.OffsetX = -Status.Width - Critical.Width;
            }
            else
            {
                Critical.ToolTipText = null;
                Critical.Text = null;
                Critical.IsVisible = false;

                Name.OffsetX = Nickname.OffsetX;
            }

            if (Pawn.Dead)
            {
                Critical.Text = "Dead".Translate();
            }
        }

        public void Update_Names()
        {
            if (Nickname.IsVisible)
            {
                if (Pawn.Name != null)
                {
                    if (Pawn.Name is NameTriple NameTriple)
                    {
                        Nickname.Text = NameTriple.Nick;
                    }
                    else
                    {
                        Nickname.Text = ((NameSingle)Pawn.Name).Name;
                    }
                }
                else if (IsAnimal || IsMech)
                {
                    // Species
                    Nickname.Text = Pawn.kindDef.LabelCap;
                }
            }

            if (Title.IsVisible)
            {
                if (!IsAnimal && !IsMech)
                {
                    if (Pawn.royalty?.MostSeniorTitle != null)
                    {
                        Title.ToolTipText = Pawn.royalty.MainTitle().GetLabelFor(Pawn).CapitalizeFirst();
                    }
                    else if (!string.IsNullOrEmpty(Pawn.story.TitleCap))
                    {
                        Title.Text = Pawn.story.TitleCap;
                    }
                }
                else
                {
                    Title.IsVisible = false;
                }
            }

            if (Name.IsVisible)
            {
                if (Pawn.Name is NameTriple NameTriple)
                {
                    Name.Text = NameTriple.First + " " + NameTriple.Last;
                }
                else
                {
                    Name.IsVisible = false;
                }
            }
        }

        public void Update_Weapon()
        {
            if (IsAnimal || Pawn.Downed || Display)
            {
                Weapon.IsVisible = false;
                return;
            }

            ThingWithComps Primary = Pawn.equipment.Primary;

            if (Primary != null)
            {
                Weapon.Texture = Primary.def.uiIcon;
                Weapon.Style.Color = ((Primary.Stuff != null) && (Primary.Stuff.stuffProps != null) ? Primary.Stuff.stuffProps.color : Primary.DrawColor);

                Weapon.ToolTipText = Primary.LabelNoParenthesisCap.AsTipTitle() + Environment.NewLine + Environment.NewLine + Primary.def.DescriptionDetailed + Environment.NewLine;

                CompQuality Quality = Primary.TryGetComp<CompQuality>();

                if (Quality != null)
                {
                    Weapon.ToolTipText += Environment.NewLine + Quality.CompInspectStringExtra();
                }

                if (Primary.def.useHitPoints)
                {
                    Weapon.ToolTipText += Environment.NewLine + "Condition".Translate() + $"{Primary.HitPoints} / {Primary.MaxHitPoints} ({Math.Round(((float)Primary.HitPoints / (float)Primary.MaxHitPoints), 2)}%)";
                }

                Weapon.ToolTipText +=
                    Environment.NewLine + "Value".Translate() + Primary.def.BaseMarketValue.ToString() + "DollarSign".Translate()
                    + Environment.NewLine + "Mass".Translate() + Primary.def.BaseMass.ToString() + "Kg".Translate()
                    + Environment.NewLine + "CostEffectiveness".Translate() + Math.Round((Primary.def.BaseMarketValue / Primary.def.BaseMass), 2).ToString()
                    + Environment.NewLine + "CostEffectivenessInfo".Translate();

                Weapon.IsVisible = true;
            }
            else if (Pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                // TODO: While in a inspect display item it has to be in the upper left corner
                if ((Pawns == null) || Pawns.Settings.Visible_Weapon_Pacifist)
                {
                    Weapon.Texture = Globals.GetTextureFromAlias("X_Small");
                    Weapon.Style.Color = Color.white;
                    Weapon.ToolTipText = "IsIncapableOfViolenceLower".Translate(Pawn.LabelShort, Pawn);
                    Weapon.IsVisible = true;
                }
                else
                {
                    Weapon.IsVisible = false;
                }
            }
            else if (RenderStyle == UnitStyle.Line)
            {
                // Don't show the attack icon underneath the preview if square
                Weapon.Texture = Globals.GetTextureFromAlias("Unarmed");
                Weapon.Style.Color = Color.white;
                Weapon.ToolTipText = string.Empty;
                Weapon.IsVisible = true;
            }
            else
            {
                Weapon.Style.Color = Color.clear;
                Weapon.ToolTipText = string.Empty;
                Weapon.IsVisible = false;
            }
        }

        public void Update_Status()
        {
            (string IconPath, string StatusDescription) = Globals.GetStatus(Pawn);

            if (!string.IsNullOrWhiteSpace(IconPath))
            {
                Status.SetTexture(IconPath);
                Status.IsVisible = true;
            }
            else
            {
                Status.IsVisible = false;
                Status.ClearTexture();
            }

            Status.ToolTipText = StatusDescription;
        }

        public void DoOnNamesVisibilityChanged()
        {
            if (Pawns != null)
            {
                Nickname.IsVisible = Pawns.Settings.Visible_Nickname;
                Title.IsVisible = Pawns.Settings.Visible_Title;
                Name.IsVisible = ((RenderStyle == UnitStyle.Line) && Pawns.Settings.Visible_Name);
            }

            if (RenderStyle == UnitStyle.Line)
            {
                // TODO: should be moved into a expanded tooltip panel
                //Nickname.Text = (Globals.GetGenderUnicodeSymbol(Pawn.gender) + " " + Nickname.Text);

                if (Title.IsVisible)
                {
                    Title.Style.TextAnchor = TextAnchor.MiddleCenter;

                    if (!Nickname.IsVisible)
                    {
                        Title.Style.TextAnchor = TextAnchor.MiddleLeft;
                    }
                    else if (!Name.IsVisible)
                    {
                        Title.Style.TextAnchor = TextAnchor.MiddleRight;
                    }
                }
                if (Name.IsVisible)
                {
                    Name.Style.TextAnchor = TextAnchor.MiddleRight;

                    if (!Nickname.IsVisible && !Title.IsVisible)
                    {
                        Name.Style.TextAnchor = TextAnchor.MiddleLeft;
                    }
                    else
                    {
                        Name.Style.TextAnchor = TextAnchor.MiddleRight;
                    }
                }
            }
            else
            {
                // TODO: Font is too big!
                //Name.Style.Font.fontNames
            }
        }

        #region "ContextMenu"

        public void ToggleContextMenu()
        {
            if (ContextMenu == null)
            {
                ContextMenuOpen();
            }
            else
            {
                ContextMenuClose();
            }
        }

        public void ContextMenuOpen()
        {
            ContextMenu = new ContextMenu();

            if (Pawns != null)
            {
                ContextMenu.ListView.AddItem(GetToggleOptions());
            }

            if (CanPopOut)
            {
                ContextMenu.AddOption("PopOut".Translate(), () => PopOut(this, MouseEventArgs.Empty));
            }

            ContextMenu.AddOption("Rename".Translate(), Globals.GetTextureFromAlias("Rename"), OnClick: () => Find.WindowStack.Add(PawnNamingUtility.NamePawnDialog(Pawn)));

            if (Pawn.IsFreeColonist)
            {
                ContextMenu.AddOption("Banish".Translate(), Globals.GetTextureFromAlias("Banish"), PawnBanishUtility.GetBanishButtonTip(Pawn), () => PawnBanishUtility.ShowBanishPawnConfirmationDialog(Pawn));
            }

            // Changes the Style of all Items in the ListView.
            ContextMenu.AddOption("CycleRenderStyle".Translate(), () =>
            {
                ListViewStyle Cycled_ListView = ((RenderStyle == UnitStyle.Line) ? ListViewStyle.Grid : ListViewStyle.List);
                RenderStyle = ((RenderStyle == UnitStyle.Line) ? UnitStyle.Square : UnitStyle.Line);
                bool ExtendItemsHorizontally = (RenderStyle == UnitStyle.Line);

                // Apply the change to all members of the parent ListView when Shift-Clicked or if it isn't in a Group.
                if (WindowManager.IsShiftDown() || (Group == null))
                {
                    ListView.RenderStyle = Cycled_ListView;
                    ListView.ExtendItemsHorizontally = ExtendItemsHorizontally;

                    List<ListViewItem> Items = ListView.GetItemsReclusively();

                    Items.OfType<ListViewItem_Pawn>().ToList().ForEach((F) => F.RenderStyle = RenderStyle);
                    Items.OfType<ListViewItemGroup>().ToList().ForEach((F) =>
                    {
                        F.RenderStyle = Cycled_ListView;
                        F.ExtendItemsHorizontally = ExtendItemsHorizontally;
                        F.DoOnSizeChanged(F, EventArgs.Empty);
                    });
                }

                if (Group != null)
                {
                    Group.RenderStyle = Cycled_ListView;
                    Group.ExtendItemsHorizontally = ExtendItemsHorizontally;

                    Group.Items.OfType<ListViewItem_Pawn>().ToList().ForEach((F) => F.RenderStyle = RenderStyle);
                    Group.DoOnSizeChanged(Group, EventArgs.Empty);
                }

                ListView.UpdateViewPortBounds();
                ListView.UpdatePositions();
            }).ToolTipText = "CycleRenderStyle_ToolTipText".Translate();

            ContextMenu.AddOption("DropAll".Translate(), Globals.GetTextureFromAlias("Drop"), "DropAll_ToolTipText".Translate(), () => Overview.ListViewItem_Inventory.InterfaceDropAll(Pawn));

            if (!IsAnimal && !IsMech)
            {
                if ((Pawn.guilt != null) && Pawn.guilt.IsGuilty && Pawn.IsFreeColonist && !Pawn.IsQuestLodger())
                {
                    ContextMenu.AddOptionToggle("Execute".Translate(), Globals.GetTextureFromAlias("Execute"), "Execute_ToolTipText".Translate(), Pawn.guilt.awaitingExecution, () =>
                    {
                        Pawn.guilt.awaitingExecution = !Pawn.guilt.awaitingExecution;

                        Messages.Message((Pawn.guilt.awaitingExecution ? "Execute_Message_Mark".Translate(Pawn) : "Execute_Message_Unmark".Translate(Pawn)), Pawn, MessageTypeDefOf.SilentInput, false);

                        Update_Status();
                    });
                }
            }
            else if (IsPlayerFaction)
            {
                ContextMenu.AddOptionToggle("Slaughter".Translate(), Globals.GetTextureFromAlias("Slaughter"), "Slaughter_ToolTipText_Toggle".Translate(), Pawn.ShouldBeSlaughtered(), () =>
                {
                    Messages.Message((Pawn.ShouldBeSlaughtered() ? "Slaughter_Message_Mark".Translate(Pawn) : "Slaughter_Message_Unmark".Translate(Pawn)), Pawn, MessageTypeDefOf.SilentInput, false);

                    SlaughterDesignatorUtility.CheckWarnAboutBondedAnimal(Pawn);
                    SlaughterDesignatorUtility.CheckWarnAboutVeneratedAnimal(Pawn);

                    Update_Status();
                });
            }

            if (Pawn.IsFreeColonist && !Pawn.IsQuestLodger() && (Pawn.royalty != null) && (Pawn.royalty.AllTitlesForReading.Count > 0))
            {
                // TODO:
                //System.Text.StringBuilder StringBuilder = new System.Text.StringBuilder();

                //FloatMenuUtility.MakeMenu<RoyalTitle>(Pawn.royalty.AllTitlesForReading, (RoyalTitle title) => "RenounceTitle".Translate() + ": " + "TitleOfFaction".Translate(title.def.GetLabelCapFor(Pawn), title.faction.GetCallLabel()), delegate(RoyalTitle title)

                //"RenounceTitleDescription".Translate(Pawn.Named("PAWN"), "TitleOfFaction".Translate(title.def.GetLabelCapFor(Pawn), title.faction.GetCallLabel()).Named("TITLE"), StringBuilder.ToString().TrimEndNewlines().Named("EFFECTS"));




                //ContextMenu.AddOption("Renounce".Translate(), "Renounce_ToolTipText".Translate(), Texture2D: Globals.GetTextureFromAlias("Renounce"), OnClick: () => Pawn.inventory.DropAllNearPawn(Pawn.Position, false, true));
            }

            ContextMenu.Root.OnMouseLeave += (obj, e) =>
            {
                if (!ListView.IsMouseOver)
                {
                    ContextMenuClose();
                }
            };

            ContextMenu.Open();
        }

        public ListViewItem GetToggleOptions()
        {
            ListViewItem Item = new ListViewItem();

            Item.Style.DrawMouseOver = false;
            Item.Style.DrawBackground = false;
            Item.Header.Style.DrawMouseOver = false;
            Item.Header.Style.DrawBackground = false;

            Func<string, string, bool, Button> CreateToggleOption = (string Icon, string ToolTipText, bool InitialState) =>
            {
                Button Button = new Button(ButtonStyle.Image, new Vector2(Item.Height, Item.Height), Globals.TryGetTexturePathFromAlias(Icon), true);
                Button.Style.DrawBackground = true;
                Button.ToggleState = InitialState;
                Button.SetStyle("ListViewItem_Pawn.ContextMenu.Toggle");
                Button.ToolTipText = ToolTipText.Translate();

                return Button;
            };

            Button Toggle_Nickname = CreateToggleOption("Star", "Toggle_Nickname", Pawns.Settings.Visible_Nickname);
            Button Toggle_Title = CreateToggleOption("Crown", "Toggle_Title", Pawns.Settings.Visible_Title);
            Button Toggle_Name = CreateToggleOption("Asterisk", "Toggle_Name", Pawns.Settings.Visible_Name);
            Button Toggle_Pacifist = CreateToggleOption("X", "Toggle_Pacifist", Pawns.Settings.Visible_Weapon_Pacifist);
            Button Toggle_Portrait_Headshot = CreateToggleOption("Camera", "Toggle_Portrait_Headshot", Pawns.Settings.Portrait_Headshot);

            Toggle_Title.OffsetX = Toggle_Nickname.Width;
            Toggle_Name.OffsetX = Toggle_Title.OffsetRight;
            Toggle_Pacifist.OffsetX = Toggle_Name.OffsetRight;
            Toggle_Portrait_Headshot.OffsetX = Toggle_Pacifist.OffsetRight;

            // Can't pass byref to anonymous function so we have to explicitly do it like this.
            Toggle_Nickname.OnToggleStateChanged += (obj, e) => { Pawns.Settings.Visible_Nickname = Toggle_Nickname.ToggleState; DoOnNamesVisibilityChanged(); };
            Toggle_Title.OnToggleStateChanged += (obj, e) => { Pawns.Settings.Visible_Title = Toggle_Title.ToggleState; DoOnNamesVisibilityChanged(); };
            Toggle_Name.OnToggleStateChanged += (obj, e) => { Pawns.Settings.Visible_Name = Toggle_Name.ToggleState; DoOnNamesVisibilityChanged(); };
            Toggle_Pacifist.OnToggleStateChanged += (obj, e) => { Pawns.Settings.Visible_Weapon_Pacifist = Toggle_Pacifist.ToggleState; Update_Weapon(); };
            Toggle_Portrait_Headshot.OnToggleStateChanged += (obj, e) => { Pawns.Settings.Portrait_Headshot = Toggle_Portrait_Headshot.ToggleState; DoOnRequest(); };

            Item.AddRange(Toggle_Nickname, Toggle_Title, Toggle_Name, Toggle_Pacifist, Toggle_Portrait_Headshot);

            return Item;
        }

        public void ContextMenuClose(object Sender = null, EventArgs EventArgs = null)
        {
            if (Sender != null)
            {
                // From a event, if mouseover, don't close the context menu
                if ((ContextMenu != null) && !ContextMenu.IsMouseOver)
                {
                    ContextMenu.Close();
                    ContextMenu = null;
                }
            }
            else if (ContextMenu != null)
            {
                ContextMenu.Close();
                ContextMenu = null;
            }
        }

        public override void DoOnBoundsChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnBoundsChanged(Sender, EventArgs);
            ContextMenuClose();
        }

        public override void DoOnListViewAdded()
        {
            base.DoOnListViewAdded();

            ListView.OnMouseLeave += ContextMenuClose;
            ListView.OnMouseDown += ContextMenuClose;
        }

        public override void DoOnListViewRemoved()
        {
            base.DoOnListViewRemoved();

            ListView.OnMouseLeave -= ContextMenuClose;
            ListView.OnMouseDown -= ContextMenuClose;
        }

        public void PopOut(object Sender, MouseEventArgs MouseEventArgs)
        {
            RemoveFromItemParent();

            Pawns PopOut = new Pawns(this);
            PopOut.Bounds = new Rect(WindowManager.GetMousePosition(), new Vector2((Width + 20f), (Height + 20f)));
            PopOut.Open();
            ContextMenuClose();
        }

        #endregion "ContextMenu"

        #region "Input"

        public override void DoOnClickDouble(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClickDouble(Sender, EventArgs);

            RimWorld.Planet.GlobalTargetInfo Target = (RimWorld.Planet.GlobalTargetInfo)Pawn;

            if (Target.IsValid)
            {
                WindowManager.AttachCameraToTarget(Target, PawnSelectable);
            }
        }

        public override void DoOnWhileMouseOver(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnWhileMouseOver(Sender, EventArgs);

            TargetHighlighter.Highlight(Pawn, true, false, false);
        }

        public override void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            if (WindowManager.IsAltDown())
            {
                WindowManager.OpenWindowVanilla(new Dialog_InfoCard(Pawn));
            }
            else
            {
                if (!WindowManager.IsCtrlDown())
                {
                    if (!WindowManager.IsShiftDown())
                    {
                        // Toggle is handled here.
                        base.DoOnClick(Sender, EventArgs);

                        if (!Display && (ListView != null))
                        {
                            // Deselect Others.
                            foreach (ListViewItem_Pawn Item in ListView.GetItemsReclusively().OfType<ListViewItem_Pawn>())
                            {
                                if (Item != this)
                                {
                                    ListView.DeselectItem(Item);
                                }
                            }
                        }
                        else if (!Selectable)
                        {
                            CameraJumper.TryJump(Pawn);
                        }
                    }
                }
                else if (CanPopOut)
                {
                    AttachToMouse();
                }
            }
        }

        public override void DoOnWhileMouseDown(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnWhileMouseDown(Sender, EventArgs);

            if (WindowManager.IsShiftDown())
            {
                Select();
            }
        }

        public override void Select(bool All = false)
        {
            // This Entity is special in that it's also used without a ListView, therefor its Selection behavior doesn't work exactly the same as a regular ListViewItem.
            if (ListView != null)
            {
                base.Select(All);
            }
            else
            {
                IsSelected = true;
            }
        }

        public override void Deselect(bool All = false)
        {
            if (ListView != null)
            {
                base.Deselect(All);
            }
            else
            {
                IsSelected = false;
            }
        }

        public override void Toggle()
        {
            if (ListView != null)
            {
                base.Toggle();
            }
            else
            {
                IsSelected = !IsSelected;
            }
        }

        public override void DoOnSelectionChanged()
        {
            base.DoOnSelectionChanged();

            // Header renders Selection colors as it's smaller in Square mode.
            Header.IsSelected = IsSelected;

            if (PawnSelectable)
            {
                if (IsSelected)
                {
                    if (!Find.Selector.IsSelected(Pawn))
                    {
                        Find.Selector.Select(Pawn);
                    }
                }
                else if (Find.Selector.IsSelected(Pawn))
                {
                    Find.Selector.Deselect(Pawn);
                }
            }
        }

        public void AttachToMouse()
        {
            ToolTip ToolTip = new ToolTip();
            ToolTip.ManualDisposal = true;
            ToolTip.Identifier = "PopOut_Preview_ListViewItem_Pawn";
            ToolTip.Size = new Vector2((Width + 5f), (Height + 5f));
            ToolTip.Offset = new Vector2(-8f, -8f);
            ToolTip.DrawBackground = false;
            ToolTip.DrawBorder = false;
            ToolTip.IgnoreMouseInput = false;

            ListView ListView = new ListView(Width, Height, false);
            ListView.Style.DrawBackground = false;
            ListView.MinSize = ((RenderStyle == UnitStyle.Square) ? SquareSize : ListView.MinSize);
            ListView.RenderStyle = ((RenderStyle == UnitStyle.Square) ? ListViewStyle.Grid : ListViewStyle.List);
            ListView.ExtendItemsHorizontally = (ListView.RenderStyle == ListViewStyle.List);
            ToolTip.Root.AddChild(ListView);

            RemoveFromItemParent();
            ListView.AddItem(this);

            // Ignore Mouse while being dragged.
            IgnoreMouse = true;

            ToolTip.Root.OnMouseUp += (obj, e) =>
            {
                IgnoreMouse = false;

                List<Panel> Entities = WindowManager.GetEntitiesUnderMouse();

                // Because of the way ListView Items are implemented the code above retrieves more than the actually amount of items we want, we can filter them out like this.
                ListViewItem_Pawn Item = Entities.OfType<ListView>().SelectMany((F) => F.GetItemsReclusively()).OfType<ListViewItem_Pawn>().Except(this).FirstOrDefault((F) => F.IsMouseOver);
                Pawns MouseOverWindow = Entities.Select((F) => F.ParentWindow).OfType<Pawns>().FirstOrDefault((F) => F.IsMouseOver);

                if (Item != null)
                {
                    // Match the RenderStyle of the destination.
                    RenderStyle = Item.RenderStyle;

                    RemoveFromItemParent();
                    Pawns = Item.Pawns;

                    // Alt force-creates a Group.
                    if (WindowManager.IsAltDown())
                    {
                        ListViewItemGroup Group = Pawns.CreateGroup(Pawns.GetGroupID(Item.Pawn));

                        int Index = Item.ListView.Items.IndexOf(Item);
                        Item.ListView.InsertItem(Index, Group);

                        Item.RemoveFromItemParent();

                        Group.AddItem(Item);
                        Group.AddItem(this);
                    }
                    else
                    {
                        // Add this Item to the same IItemList as the Item we are targeting.
                        if (Item.Group != null)
                        {
                            Item.Group.InsertItem(Item.Group.Items.IndexOf(Item), this);
                        }
                        else
                        {
                            Item.ListView.InsertItem(Item.ListView.Items.IndexOf(Item), this);
                        }

                        Item.ListView.UpdateViewPortBounds();
                        Item.ListView.UpdatePositions();
                    }
                }
                else if (MouseOverWindow != null)
                {
                    RemoveFromItemParent();
                    Pawns = MouseOverWindow;

                    if (WindowManager.IsAltDown())
                    {
                        MouseOverWindow.AddItem(this);
                    }
                    else
                    {
                        MouseOverWindow.ListView.AddItem(this);
                    }
                }
                else if (CanPopOut)
                {
                    PopOut(obj, e);
                }

                ToolTip.Dispose();
            };

            BigMod.WindowManager.AddToolTip(ToolTip);
        }

        public override void DoOnClickRight(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnClickRight(Sender, EventArgs);

            if (AllowContextMenu)
            {
                ToggleContextMenu();
            }
        }

        #endregion "Input"
    }
}
