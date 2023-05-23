using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BigMod.Entities.Windows
{
    /// <summary>
    /// <para>Replaces the TabPanel at the bottom of the ingame HUD in vanilla.</para>
    /// <para>Will automatically add MainButtonDef from other mods as buttons.</para>
    /// </summary>
    public class Hotbar : WindowPanel
    {
        public ListView ListView;

        public Hotbar() : base()
        {
            Identifier = "Window_Hotbar";

            DrawBorder = false;

            // Prevents this window from getting closed
            SetCloseOn();

            VisibleOnMouseOverOnly_ButtonCloseX = true;
            VisibleOnMouseOverOnly_ButtonResize = true;

            // 12/15 of all the default tabs : inspect(not visible in vanilla tabcontrol), architect, work, schedule, assign, animals, wildlife, mechs, research, quests, world, history, factions, ideoligions, menu(not part of this Hotbar)
            ListView = new ListView((55 * 13), 75, false, false);
            ListView.SetStyle("Hotbar.ListView");
            ListView.RenderStyle = ListViewStyle.Grid;
            ListView.ExtendItemsHorizontally = false;
            ListView.AllowSelection = true;
            ListView.IsMultiSelect = true;
            ListView.IgnoreGUIScroll = true;
            Root.AddChild(ListView);

            // These will not be added to the Hotbar
            List<MainButtonDef> Ignore = new List<MainButtonDef>() { MainButtonDefOf.Inspect, MainButtonDefOf.Menu };
            List<MainButtonDef> Tabs = (from F in DefDatabase<MainButtonDef>.AllDefs orderby F.order select F).ToList();

            foreach (MainButtonDef Tab in Tabs)
            {
                if (Ignore.Contains(Tab.TabWindow?.def))
                {
                    continue;
                }

                ListViewItem Item = new ListViewItem(Tab.label);
                Item.SetStyle("Hotbar.Item");
                Item.Header.SetStyle("Hotbar.Item.Header");
                Item.Size = new Vector2(50, 50);
                Item.MaxSize = new Vector2(50, 50);
                Item.Data = Tab;

                if (Tab.TabWindow?.def == MainButtonDefOf.Research)
                {
                    AddButtonResearch(ref Item);
                    AddButtonToggleVanilla(Tab.TabWindow.def, ref Item);
                }
                // Using string comparison as its TabWindow doesn't exist at this time
                else if (Tab.label == "world")
                {
                    AddButtonWorld(ref Item);
                }
                else if (Tab.TabWindow?.def == MainButtonDefOf.Architect)
                {
                    AddButtonToggle<Architect.Architect>(ref Item);
                }
                else if (Tab.tabWindowClass == typeof(MainTabWindow_Animals))
                {
                    // Animals doesn't have a MainButtonDefOf
                    AddButtonToggle<Animals.Animals>(ref Item);
                }
                else if (Tab.tabWindowClass == typeof(MainTabWindow_Assign))
                {
                    // Assign doesn't have a MainButtonDefOf
                    AddButtonToggle<Overview.Overview>(ref Item);
                }
                else if (Tab.tabWindowClass == typeof(MainTabWindow_Schedule))
                {
                    // Schedule doesn't have a MainButtonDefOf
                    AddButtonToggle<Schedule.Schedule>(ref Item);
                }
                else if (Tab.tabWindowClass == typeof(MainTabWindow_Work))
                {
                    // Work doesn't have a MainButtonDefOf
                    AddButtonToggle<Work.Work>(ref Item);
                }
                else if (Tab.tabWindowClass == typeof(MainTabWindow_Wildlife))
                {
                    // Wildlife doesn't have a MainButtonDefOf
                    AddButtonToggle<Wildlife.Wildlife>(ref Item);
                }
                else if (Tab.tabWindowClass == typeof(MainTabWindow_Mechs))
                {
                    // Mechs doesn't have a MainButtonDefOf
                    AddButtonToggle<Mechs.Mechs>(ref Item);
                }
                else if ((Tab.TabWindow != null) && (Tab.TabWindow.def != null))
                {
                    AddButtonToggleVanilla(Tab.TabWindow.def, ref Item);
                }

                // Add ToolTip
                if (Tab.TabWindow != null)
                {
                    Item.ToolTipText = Tab.TabWindow.def.LabelCap + "\n\n" + Tab.TabWindow.def.description;
                }

                string Alias = Globals.TryGetTexturePathFromAlias(Tab.label.CapitalizeFirst());

                if (Alias != Tab.label.CapitalizeFirst())
                {
                    Item.Header.RenderStyle = ButtonStyle.Invisible;
                    Item.Image = new Image(Alias, 50, 50);
                    Item.Image.SetStyle("Hotbar.Item.Image");
                    Item.Image.RenderStyle = ImageStyle.Fitted;
                    Item.Image.InheritParentSize = true;
                    Item.AddChild(Item.Image);
                }

                ListView.AddItem(Item);
            }

            AddButtonResize();
            // Pad the Height a bit to prevent the scrollbar from rendering.
            Size = new Vector2((ListView.Width + ButtonResize.Width), (ListView.Height - ButtonResize.Height + 3f));
            Position = new Vector2(((UI.screenWidth / 2) - (Width / 2)), (UI.screenHeight - Height));
            DefaultBounds = Bounds;

            ListView.InheritParentSize = true;

            BigMod.WindowStack.OnWindowClosed += OnWindowClosed;

            IsLocked = true;
        }

        // Moved out of main method for clarity
        public void AddButtonResearch(ref ListViewItem Item)
        {
            // Add Research ProgressBar
            ProgressBar ResearchBar = new ProgressBar();
            ResearchBar.Size = Item.Size;
            ResearchBar.InheritParentSize = true;
            ResearchBar.IsVertical = true;
            ResearchBar.SetStyle("Hotbar.Research.ProgressBar");
            ResearchBar.ColorOverride = Globals.GetColor("Hotbar.Research.ProgressBar.ColorOverride");

            ResearchBar.OnRequest += (obj, e) =>
            {
                ResearchProjectDef CurrentProject = Find.ResearchManager.currentProj;

                if (CurrentProject != null)
                {
                    ResearchBar.IsVisible = true;
                    ResearchBar.Parent.ToolTipText = CurrentProject.LabelCap.CapitalizeFirst().ToString() + " " + CurrentProject.CostApparent.ToString("F0") + " / " + CurrentProject.ProgressApparent.ToString("F0");
                    ResearchBar.Percentage = CurrentProject.ProgressPercent;
                }
                else
                {
                    ResearchBar.IsVisible = false;
                    ResearchBar.Percentage = 0;
                }
            };

            ResearchBar.OnEmpty += (obj, e) =>
            {
                ResearchBar.IsVisible = false;
            };

            Item.AddChild(ResearchBar);
        }

        public void AddButtonWorld(ref ListViewItem Item)
        {
            Item.OnClick += (obj, e) =>
            {
                if (Find.World.renderer.wantedMode == WorldRenderMode.None)
                {
                    Find.World.renderer.wantedMode = WorldRenderMode.Planet;
                }
                else if (WorldRendererUtility.WorldRenderedNow)
                {
                    Find.World.renderer.wantedMode = WorldRenderMode.None;
                }
            };

            // This is what it's called when you exit worldview.
            Item.Header.Data = typeof(WorldInspectPane);
        }

        /// <summary>
        /// Resets the bounds of the specified window.
        /// </summary>
        /// <param name="Window">Window to reset bounds of.</param>
        /// <returns>True if bounds have been reset, False if not or if window was null.</returns>
        public bool ResetBounds<T>(ref T Window) where T : Window
        {
            if (Window != null)
            {
                // Shift+Alt-clicking resets the bounds
                if (WindowManager.IsShiftDown() && WindowManager.IsAltDown())
                {
                    Window.windowRect.size = Window.InitialSize;
                    return true;
                }
            }

            return false;
        }

        public void AddButtonToggle<T>(ref ListViewItem Item) where T : WindowPanel
        {
            // TODO: Windows have to be moved if there's one already in its default position in the way
            Item.OnClick += (obj, e) =>
            {
                WindowManager.TryGetWindow<T>(out T Window);

                if (!ResetBounds(ref Window))
                {
                    WindowManager.TryToggleWindow<T>();
                }
            };

            // Storing it in the Header's Data property is the easiest and cleanest thing to do.
            Item.Header.Data = typeof(T);
        }

        public void AddButtonToggleVanilla(MainButtonDef Tab, ref ListViewItem Item)
        {
            Item.OnClick += (obj, e) =>
            {
                Window Window = Tab.TabWindow;

                if (!ResetBounds(ref Window))
                {
                    WindowManager.ToggleWindow(Window);
                }
            };

            Item.Header.Data = Tab.tabWindowClass;
        }

        public void OnWindowClosed(object Sender, EventArgs EventArgs)
        {
            foreach (ListViewItem Item in ListView.Items)
            {
                if ((Type)Item.Header.Data == Sender.GetType())
                {
                    // Ensure Item isn't selected while its associated Window isn't Open. Supports Vanilla Windows too.
                    Item.Deselect();
                    break;
                }
            }
        }
    }
}
