using RimWorld;
using System.Reflection;
using Verse;

namespace BigMod.Entities.Windows
{
    /// <summary>
    /// Used to hold miscellaneous actions in the Lower-Right corner of the gameplay UI.
    /// </summary>
    public class Tray : ListWindow
    {
        public override Rect DefaultBounds { get; set; } = new Rect((UI.screenWidth - 100f), (UI.screenHeight - 50f), 100f, 25f);
        public event EventHandler OnExpanding;
        public event EventHandler OnCollapsing;
        public bool IsExpanded
        {
            get
            {
                return Toggle.ToggleState;
            }
            set
            {
                Toggle.ToggleState = value;
            }
        }
        public Button Toggle;
        public Button Menu;
        public Vector2 Size_Expanded = new Vector2(250f, 300f);
        public Vector2 Size_Collapsed = new Vector2(100f, 25f);

        public Tray() : base(false, false, true, false)
        {
            Identifier = "Window_Tray";

            // Allow MouseScroll zooming as it will get absorbed by the ListView anyways.
            CameraMouseOverZooming = true;

            // Default MinSize is 50fx50f.
            Root.MinSize = Size_Collapsed;

            Toggle = new Button(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("Toggle"), true);
            Toggle.ToolTipText = "Toggle_Tray".Translate();
            Toggle.Anchor = Anchor.BottomRight;
            Toggle.Size = new Vector2(25f, 25f);
            Toggle.Offset = new Vector2(-75f, 0f);
            Toggle.OnToggleStateChanged += OnToggleStateChanged;

            Menu = new Button(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("Menu"));
            Menu.ToolTipText = "Toggle_Menu".Translate();
            Menu.Anchor = Anchor.BottomRight;
            Menu.Size = new Vector2(25f, 25f);
            Menu.Offset = new Vector2(-50f, 0f);
            Menu.OnClick += (obj, e) =>
            {
                // Game doesn't like using : WindowManager.TryToggleWindowVanilla(MainButtonDefOf.Menu.tabWindowClass);
                Find.MainTabsRoot.ToggleTab(MainButtonDefOf.Menu, true);
            };

            Root.AddRange(Toggle, Menu);

            Search.IsVisible = false;
            Search.Offset += new Vector2(0f, -Toggle.Height);
            Search.InheritParentSize_Modifier = new Vector2(-25f, 0f);

            Populate();

            // Will grow to parent size with respect to this modifier
            ListView.InheritParentSize_Modifier = new Vector2(-5f, -Toggle.Height - Search.Height - 8f);
            ListView.Size = new Vector2(Width, (Height + ListView.InheritParentSize_Modifier.y));

            AddButtonResize();
            ButtonResize.IsVisible = false;

            // We aren't using VisibleOnMouseOverOnly_ButtonResize because we only want it to be visible when MouseOver and if it's expanded.
            Root.OnWhileMouseOver += (obj, e) =>
            {
                if (IsExpanded)
                {
                    Search.IsVisible = true;
                    ButtonResize.IsVisible = true;
                    DrawBackground = true;
                }
            };
            Root.OnMouseLeave += (obj, e) =>
            {
                Search.IsVisible = false;
                ButtonResize.IsVisible = false;
                DrawBackground = false;
            };

            IsLocked = true;
        }

        public virtual void Populate()
        {
            ListView.AddRange(GetOptionsVanilla());

            ListView.AddItem(CreateOptionWindowToggle<Resources.Resources>("Toggle_Resources".Translate(), null, "Resources"));
            ListView.AddItem(CreateOptionWindowToggle<Pawns>("Toggle_Pawns".Translate(), null, "Pawns"));
            ListView.AddItem(CreateOptionWindowToggle<Orders.Orders>("Toggle_Orders".Translate(), null, "Orders"));
            ListView.AddItem(CreateOptionWindowToggle<DisplayDate>("Toggle_DisplayDate".Translate(), null, "DisplayDate"));
            ListView.AddItem(CreateOptionWindowToggle<DisplayWeather>("Toggle_DisplayWeather".Translate(), null, "DisplayWeather"));
        }

        public virtual void OnToggleStateChanged(object Sender, EventArgs EventArgs)
        {
            if (Toggle.ToggleState)
            {
                DoOnExpanding();
            }
            else
            {
                DoOnCollapsing();
            }
        }

        public virtual void DoOnExpanding()
        {
            // Don't allow resetting when expanded.
            CanReset = false;

            ListView.IsVisible = true;
            Search.IsVisible = true;
            ButtonResize.IsVisible = true;
            // Use last size if it exists.
            Vector2 SizeLast = ((Root.Data != null) ? (Vector2)Root.Data : default);

            if (SizeLast != default)
            {
                Bounds = new Rect((X + Width - SizeLast.x), (Y + Toggle.Height - SizeLast.y), SizeLast.x, SizeLast.y);
            }
            else
            {
                // We need to reposition the window as it grows from the TopLeft but we want it to grow from the Bottom up.
                Bounds = new Rect((X + Width - Size_Expanded.x), (Y + Toggle.Height - Size_Expanded.y), Size_Expanded.x, Size_Expanded.y);
            }

            OnExpanding?.Invoke(this, EventArgs.Empty);
        }

        public virtual void DoOnCollapsing()
        {
            CanReset = true;

            // Background is not visible when collapsed.
            DrawBackground = false;

            // Store last size.
            Root.Data = Size;

            Bounds = new Rect((X + Width - Size_Collapsed.x), (Y - Toggle.Height + Height), Size_Collapsed.x, Size_Collapsed.y);

            ListView.IsVisible = false;
            Search.IsVisible = false;
            ButtonResize.IsVisible = false;

            OnCollapsing?.Invoke(this, EventArgs.Empty);
        }

        public ListViewItem CreateOptionWindowToggle<T>(string Text, string ToolTip, string TexturePath) where T : WindowPanel
        {
            ListViewItem Item = new ListViewItem(Text);
            Item.SetStyle("Tray.ListViewItem");
            Item.Header.SetStyle("Tray.ListViewItem.Header");
            Item.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Item.ToolTipText = ToolTip;
            Item.IsSelected = WindowManager.TryGetWindow(out T Window);
            Item.OnClick += (obj, e) =>
            {
                Item.IsSelected = !WindowManager.TryToggleWindow<T>();
            };

            if (!string.IsNullOrWhiteSpace(TexturePath))
            {
                Item.Header.Label.Offset = new Vector2(25f, 0f);

                Item.Image = new Image(Globals.TryGetTexturePathFromAlias(TexturePath));
                Item.Image.Size = new Vector2(20f, 20f);
                Item.Image.Style.Color = Color.white;
                Item.AddChild(Item.Image);
            }

            BigMod.WindowStack.OnWindowOpened += (obj, e) =>
            {
                if (obj is T)
                {
                    Item.IsSelected = WindowManager.TryGetWindow(out T Window);
                }
            };
            BigMod.WindowStack.OnWindowClosed += (obj, e) =>
            {
                if (obj is T)
                {
                    Item.IsSelected = WindowManager.TryGetWindow(out T Window);
                }
            };

            return Item;
        }

        /// <summary>
        /// Vanilla PlaySettings are hardcoded and have to each manually be converted to a ListViewItem.
        /// </summary>
        /// <returns>List of the Converted options.</returns>
        public List<ListViewItem> GetOptionsVanilla()
        {
            List<ListViewItem> Items = new List<ListViewItem>();

            PlaySettings PlaySettings = Find.PlaySettings;
            Type PlaySettings_Type = typeof(PlaySettings);

            Items.Add(CreateOptionVanilla("Label_ZoneVisibility".Translate(), "ZoneVisibilityToggleButton".Translate(), TexButton.ShowZones, PlaySettings_Type.GetField(nameof(PlaySettings.showZones))));
            Items.Add(CreateOptionVanilla("Label_ShowBeauty".Translate(), "ShowBeautyToggleButton".Translate(), TexButton.ShowBeauty, PlaySettings_Type.GetField(nameof(PlaySettings.showBeauty))));
            Items.Add(CreateOptionVanilla("Label_ShowRoomStats".Translate(), "ShowRoomStatsToggleButton".Translate(), TexButton.ShowRoomStats, PlaySettings_Type.GetField(nameof(PlaySettings.showRoomStats))));
            Items.Add(CreateOptionVanilla("Label_ShowRoofOverlay".Translate(), "ShowRoofOverlayToggleButton".Translate(), TexButton.ShowRoofOverlay, PlaySettings_Type.GetField(nameof(PlaySettings.showRoofOverlay))));
            Items.Add(CreateOptionVanilla("Label_ShowFertilityOverlay".Translate(), "ShowFertilityOverlayToggleButton".Translate(), TexButton.ShowFertilityOverlay, PlaySettings_Type.GetField(nameof(PlaySettings.showFertilityOverlay))));
            Items.Add(CreateOptionVanilla("Label_ShowTerrainAffordanceOverlay".Translate(), "ShowTerrainAffordanceOverlayToggleButton".Translate(), TexButton.ShowTerrainAffordanceOverlay, PlaySettings_Type.GetField(nameof(PlaySettings.showTerrainAffordanceOverlay))));
            Items.Add(CreateOptionVanilla("Label_ShowTemperatureOverlay".Translate(), "ShowTemperatureOverlayToggleButton".Translate(), TexButton.ShowTemperatureOverlay, PlaySettings_Type.GetField(nameof(PlaySettings.showTemperatureOverlay))));
            Items.Add(CreateOptionVanilla("Label_ShowPollutionOverlay".Translate(), "ShowPollutionOverlayToggleButton".Translate(), TexButton.ShowPollutionOverlay, PlaySettings_Type.GetField(nameof(PlaySettings.showPollutionOverlay))));

            Items.Add(CreateOptionVanilla("Label_AutoHomeArea".Translate(), "AutoHomeAreaToggleButton".Translate(), TexButton.AutoHomeArea, PlaySettings_Type.GetField(nameof(PlaySettings.autoHomeArea))));
            Items.Add(CreateOptionVanilla("Label_AutoRebuild".Translate(), "AutoRebuildButton".Translate(), TexButton.AutoRebuild, PlaySettings_Type.GetField(nameof(PlaySettings.autoRebuild))));
            Items.Add(CreateOptionVanilla("Label_ShowLearningHelperWhenEmpty".Translate(), "ShowLearningHelperWhenEmptyToggleButton".Translate(), TexButton.ShowLearningHelper, PlaySettings_Type.GetField(nameof(PlaySettings.showLearningHelper))));
            Items.Add(CreateOptionVanilla("Label_LockNorthUp".Translate(), "LockNorthUpToggleButton".Translate(), TexButton.LockNorthUp, PlaySettings_Type.GetField(nameof(PlaySettings.lockNorthUp))));
            Items.Add(CreateOptionVanilla("Label_UsePlanetDayNightSystem".Translate(), "UsePlanetDayNightSystemToggleButton".Translate(), TexButton.UsePlanetDayNightSystem, PlaySettings_Type.GetField(nameof(PlaySettings.usePlanetDayNightSystem))));
            Items.Add(CreateOptionVanilla("Label_ShowExpandingIcons".Translate(), "ShowExpandingIconsToggleButton".Translate(), TexButton.ShowExpandingIcons, PlaySettings_Type.GetField(nameof(PlaySettings.showExpandingIcons))));
            Items.Add(CreateOptionVanilla("Label_ShowWorldFeatures".Translate(), "ShowWorldFeaturesToggleButton".Translate(), TexButton.ShowExpandingIcons, PlaySettings_Type.GetField(nameof(PlaySettings.showWorldFeatures))));

            return Items;
        }

        /// <summary>
        /// Creates a Item option from vanilla.
        /// </summary>
        /// <param name="Text">Header Text.</param>
        /// <param name="Texture">Image Texture.</param>
        /// <param name="FieldInfo">Used to find the reference for the variable to toggle in <see cref="PlaySettings"/>, is added to Item Data.</param>
        /// <returns>Single Item ready to be added to a ListView.</returns>
        public ListViewItem CreateOptionVanilla(string Text, string ToolTip, Texture2D Texture, FieldInfo FieldInfo)
        {
            ListViewItem Item = new ListViewItem(Text);
            Item.SetStyle("Tray.ListViewItem");
            Item.Header.SetStyle("Tray.ListViewItem.Header");
            Item.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
            Item.Header.Label.Offset = new Vector2(25f, 0f);
            Item.Header.Data = FieldInfo;

            Item.ToolTipText = ToolTip;
            Item.IsSelected = (bool)FieldInfo.GetValue(Find.PlaySettings);

            Item.OnClick += (obj, e) =>
            {
                FieldInfo FieldInfo_Data = (FieldInfo)Item.Header.Data;
                bool ToggleState = !((bool)FieldInfo_Data.GetValue(Find.PlaySettings));
                FieldInfo_Data.SetValue(Find.PlaySettings, ToggleState);
                Item.IsSelected = ToggleState;
            };

            Item.Image = new Image(Texture);
            Item.Image.Size = new Vector2(20f, 20f);
            Item.Image.Style.Color = Color.white;
            Item.AddChild(Item.Image);

            return Item;
        }

        public override void Close(bool DoSound = false)
        {
            // Don't allow this Window to ever close.
            IsExpanded = false;
            ResetBounds();
        }
    }
}
