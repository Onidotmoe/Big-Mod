global using System;
global using System.Collections.Generic;
global using System.Linq;
global using UnityEngine;
using BigMod.Entities;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using RimWorld.Planet;
using System.Reflection.Emit;
using Verse;

namespace BigMod
{
    public class BigMod : ModBase
    {
        public SettingHandle<bool> Toggle_GUI;
        public static WindowManager WindowManager;
        public static BigMod Instance;
        public static ModMetaData ModMetaData;
        public static string Directory;

        public override string ModIdentifier
        {
            get
            {
                return "Onidotmoe.BigMod";
            }
        }

        public override void DefsLoaded()
        {
            Toggle_GUI = Settings.GetHandle<bool>(
                // Name inside Config File, Spaces not allowed!
                "NewGUI",
                // Menu Name
                "NewGUI".Translate(),
                // Menu Description
                "NewGUI_Description".Translate(),
                // Value
                false);

            // Allow all objects to be multi-selectable
            foreach (ThingDef ThingDef in DefDatabase<ThingDef>.AllDefs)
            {
                ThingDef.neverMultiSelect = false;
            }
        }

        public BigMod()
        {
            Instance = this;
            ModMetaData = ModsConfig.ActiveModsInLoadOrder.First((F) => F.PackageIdPlayerFacing == ModIdentifier);
            Directory = ModMetaData.RootDir.FullName;
        }

        public override void MapLoaded(Map map)
        {
            WindowManager = WindowManager.Create();
            WindowManager.Initiate();
        }

        #region "Patches"


        // The Mechs tab is handled slightly differently than the other tabs, it's initially hidden and becomes visible when the Player has Mechs, make it always hidden.
        [HarmonyPatch(typeof(MainButtonWorker_ToggleMechTab), nameof(MainButtonWorker_ToggleMechTab.Visible), MethodType.Getter)]
        public static class MainButtonWorker_ToggleMechTab_Visible
        {
            [HarmonyPrefix]
            public static bool Visible(ref bool __result)
            {
                __result = false;

                return false;
            }
        }

        [HarmonyPatch(typeof(RimWorld.Selector))]
        public static class Selector
        {
            // Removes selection limit
            [HarmonyPatch("SelectInternal")]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions)
            {
                List<CodeInstruction> Codes = Instructions.ToList();

                foreach (CodeInstruction Code in Codes)
                {
                    // Casting Instruction.operand to int cause a NullReferenceException
                    if ((Code.opcode == OpCodes.Ldc_I4) && (Code.operand?.ToString() == "200"))
                    {
                        Code.operand = int.MaxValue;
                        break;
                    }
                }

                return Codes;
            }

            // Hook-ins for the Orders WindowPanel
            public static EventHandler Event_Select;

            public static EventHandler Event_Deselect;
            public static EventHandler Event_ClearSelection;

            [HarmonyPatch("Select")]
            [HarmonyPostfix]
            public static void Select(object obj, bool playSound = true, bool forceDesignatorDeselect = true)
            {
                Event_Select?.Invoke(obj, EventArgs.Empty);
            }

            [HarmonyPatch("Deselect")]
            [HarmonyPostfix]
            public static void Deselect(object obj)
            {
                Event_Deselect?.Invoke(obj, EventArgs.Empty);
            }

            [HarmonyPatch("ClearSelection")]
            [HarmonyPostfix]
            public static void ClearSelection()
            {
                Event_ClearSelection?.Invoke(null, EventArgs.Empty);
            }
        }

        // Removes all items from the ResourceReadout list, making it not render anything when it is in Prefs.ResourceReadoutCategorized mode
        [HarmonyPatch(typeof(ResourceReadout), MethodType.Constructor)]
        public static class ResourceReadout_RootThingCategories
        {
            [HarmonyPostfix]
            public static void ResourceReadout(ref List<ThingCategoryDef> ___RootThingCategories)
            {
                ___RootThingCategories.Clear();
            }
        }

        // Removes Right-click to open Architect TabWindow
        [HarmonyPatch(typeof(MainButtonWorker), nameof(MainButtonWorker.InterfaceTryActivate))]
        public static class MainButtonWorker_InterfaceTryActivate
        {
            [HarmonyPrefix]
            private static bool Prefix(MainButtonWorker __instance)
            {
                if (__instance.def == MainButtonDefOf.Architect)
                {
                    return false;
                }

                return true;
            }
        }

        // Sets Right-Click drag to move the camera instead of Middle-Click drag
        [HarmonyPatch(typeof(CameraDriver), "CameraDriverOnGUI")]
        public static class CameraDriver_CameraDriverOnGUI
        {
            [HarmonyPatch]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions)
            {
                List<CodeInstruction> Codes = Instructions.ToList();

                // There are 4 instances where 2 has to be replaced with 1 in the beginning of the method.
                int i = 4;

                foreach (CodeInstruction Code in Codes)
                {
                    if (Code.opcode == OpCodes.Ldc_I4_2)
                    {
                        // Change from integer 2 to integer 1
                        Code.opcode = OpCodes.Ldc_I4_1;

                        i--;

                        if (i <= 0)
                        {
                            break;
                        }
                    }
                }

                return Codes;
            }
        }

        // Sets Right-Click drag to move the camera instead of Middle-Click drag
        [HarmonyPatch(typeof(WorldCameraDriver), "WorldCameraDriverOnGUI")]
        public static class WorldCameraDriver_WorldCameraDriverOnGUI
        {
            [HarmonyPatch]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions)
            {
                List<CodeInstruction> Codes = Instructions.ToList();

                // There are 4 instances where 2 has to be replaced with 1 in the beginning of the method.
                int i = 4;

                foreach (CodeInstruction Code in Codes)
                {
                    if (Code.opcode == OpCodes.Ldc_I4_2)
                    {
                        // Change from integer 2 to integer 1
                        Code.opcode = OpCodes.Ldc_I4_1;

                        i--;

                        if (i <= 0)
                        {
                            break;
                        }
                    }
                }

                return Codes;
            }
        }

        // Right-Click drag sensitivity is too high, sensitivity options are not low enough in the settings menu
        [HarmonyPatch(typeof(Dialog_Options), "DoControlsOptions")]
        public static class Dialog_Options_DoWindowContents
        {
            [HarmonyPatch]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions)
            {
                List<CodeInstruction> Codes = Instructions.ToList();

                foreach (CodeInstruction Code in Codes)
                {
                    if ((Code.opcode == OpCodes.Ldc_R4) && (Code.operand?.ToString() == "0.8"))
                    {
                        Code.operand = 0f;
                        break;
                    }
                }

                return Codes;
            }
        }

        // Override the Vanilla Trading Screen
        [HarmonyPatch(typeof(Dialog_Trade))]
        public static class Dialog_Trade_PreOpen
        {
            [HarmonyPatch("PreOpen")]
            [HarmonyPrefix]
            public static bool PreOpen(Dialog_Trade __instance)
            {
                WindowManager.OpenWindow(new Entities.Windows.Trade.Trade(__instance));

                return false;
            }

            [HarmonyPatch("DoWindowContents")]
            [HarmonyPrefix]
            public static bool DoWindowContents()
            {
                return false;
            }
        }

        // Override the Vanilla PlaySettings buttons.
        [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
        public static class PlaySettings_DoPlaySettingsGlobalControls
        {
            [HarmonyPrefix]
            public static bool DoPlaySettingsGlobalControls(WidgetRow row, bool worldView)
            {
                return false;
            }
        }

        // Override the Vanilla TimeControls buttons.
        [HarmonyPatch(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoTimespeedControls))]
        public static class GlobalControlsUtility_DoTimespeedControls
        {
            [HarmonyPrefix]
            public static bool DoTimespeedControls(float leftX, float width, ref float curBaseY)
            {
                return false;
            }
        }

        // Override the DateReadout display in the bottom right.
        [HarmonyPatch(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate))]
        public static class GlobalControlsUtility_DoDate
        {
            [HarmonyPrefix]
            public static bool DoDate(float leftX, float width, ref float curBaseY)
            {
                return false;
            }
        }

        // Override the Weather display in the bottom right.
        [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.DoWeatherGUI))]
        public static class WeatherManager_DoWeatherGUI
        {
            [HarmonyPrefix]
            public static bool DoWeatherGUI(UnityEngine.Rect rect)
            {
                return false;
            }
        }

        // Override the Gizmo Grid in the bottom middle.
        [HarmonyPatch(typeof(GizmoGridDrawer), nameof(GizmoGridDrawer.DrawGizmoGrid))]
        public static class GizmoGridDrawer_DrawGizmoGrid
        {
            [HarmonyPrefix]
            public static bool DrawGizmoGrid(IEnumerable<Gizmo> gizmos, float startX, out Gizmo mouseoverGizmo, Func<Gizmo, bool> customActivatorFunc = null, Func<Gizmo, bool> highlightFunc = null, Func<Gizmo, bool> lowlightFunc = null)
            {
                mouseoverGizmo = null;

                return false;
            }
        }

        // Override the Inspect panel in the bottom left, the MainTabWindow_Inspect.
        [HarmonyPatch(typeof(MainTabsRoot), nameof(MainTabsRoot.SetCurrentTab))]
        public static class MainTabsRoot_SetCurrentTab
        {
            [HarmonyPrefix]
            public static bool SetCurrentTab(MainButtonDef tab, bool playSound = true)
            {
                if (tab == MainButtonDefOf.Inspect)
                {
                    return false;
                }

                return true;
            }
        }
        // Tabs aren't going to be valid and will never render correctly while the vanilla Inspect window isn't open.
        [HarmonyPatch(typeof(RimWorld.ITab))]
        public static class ITab
        {
            [HarmonyPatch("StillValid", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool StillValid(ref bool __result)
            {
                __result = true;

                return false;
            }
            [HarmonyPatch("SelPawn", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool SelPawn(ref Pawn __result)
            {
                __result = (Entities.Windows.Inspect.Inspect.TempTarget as Pawn);

                return false;
            }
            [HarmonyPatch("SelThing", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool SelThing(ref Thing __result)
            {
                __result = Entities.Windows.Inspect.Inspect.TempTarget;

                return false;
            }
        }
        // Removed references to WITab.InspectPane.CurTabs
        [HarmonyPatch(typeof(WITab), "StillValid", MethodType.Getter)]
        public static class WITab_StillValid
        {
            [HarmonyPrefix]
            public static bool StillValid(ref bool __result)
            {
                __result = WorldRendererUtility.WorldRenderedNow && Find.WindowStack.IsOpen<WorldInspectPane>();

                return false;
            }
        }

        [HarmonyPatch(typeof(Verse.WindowStack))]
        public static class WindowStack
        {
            public static event EventHandler OnWindowOpened;
            public static event EventHandler OnWindowClosed;

            [HarmonyPatch(nameof(Verse.WindowStack.Add))]
            [HarmonyPostfix]
            public static void Add(Window window)
            {
                OnWindowOpened?.Invoke(window, EventArgs.Empty);
            }

            [HarmonyPatch(nameof(Verse.WindowStack.TryRemove), new Type[] { typeof(Window), typeof(bool) })]
            [HarmonyPostfix]
            public static void TryRemove(ref bool __result, Window window, bool doCloseSound = true)
            {
                if (__result)
                {
                    OnWindowClosed?.Invoke(window, EventArgs.Empty);
                }
            }
        }

        [HarmonyPatch(typeof(Verse.Pawn_EquipmentTracker))]
        public static class Pawn_EquipmentTracker
        {
            public static event EventHandler<EquipmentEventArgs> EquipmentAdded;
            public static event EventHandler<EquipmentEventArgs> EquipmentRemoved;

            [HarmonyPatch(nameof(Verse.Pawn_EquipmentTracker.Notify_EquipmentAdded))]
            [HarmonyPostfix]
            public static void Notify_EquipmentAdded(Verse.Pawn_EquipmentTracker __instance, ThingWithComps eq)
            {
                EquipmentAdded?.Invoke(__instance, new EquipmentEventArgs(eq));

            }
            [HarmonyPatch(nameof(Verse.Pawn_EquipmentTracker.Notify_EquipmentAdded))]
            [HarmonyPostfix]
            public static void Notify_EquipmentRemoved(Verse.Pawn_EquipmentTracker __instance, ThingWithComps eq)
            {
                EquipmentRemoved?.Invoke(__instance, new EquipmentEventArgs(eq));
            }

            public class EquipmentEventArgs : EventArgs
            {
                public ThingWithComps Thing;

                public EquipmentEventArgs(ThingWithComps Thing)
                {
                    this.Thing = Thing;
                }
            }
        }
        [HarmonyPatch(typeof(RimWorld.Pawn_ApparelTracker))]
        public static class Pawn_ApparelTracker
        {
            public static event EventHandler<ApparelEventArgs> ApparelAdded;
            public static event EventHandler<ApparelEventArgs> ApparelRemoved;
            public static event EventHandler<ApparelEventArgs> ApparelChanged;

            [HarmonyPatch(nameof(RimWorld.Pawn_ApparelTracker.Notify_ApparelAdded))]
            [HarmonyPostfix]
            public static void Notify_ApparelAdded(RimWorld.Pawn_ApparelTracker __instance, Apparel apparel)
            {
                ApparelAdded?.Invoke(__instance, new ApparelEventArgs(apparel));
            }
            [HarmonyPatch(nameof(RimWorld.Pawn_ApparelTracker.Notify_ApparelRemoved))]
            [HarmonyPostfix]
            public static void Notify_ApparelRemoved(RimWorld.Pawn_ApparelTracker __instance, Apparel apparel)
            {
                ApparelRemoved?.Invoke(__instance, new ApparelEventArgs(apparel));
            }
            [HarmonyPatch(nameof(RimWorld.Pawn_ApparelTracker.Notify_ApparelChanged))]
            [HarmonyPostfix]
            public static void Notify_ApparelChanged(RimWorld.Pawn_ApparelTracker __instance)
            {
                ApparelChanged?.Invoke(__instance, null);
            }

            public class ApparelEventArgs : EventArgs
            {
                public Apparel Apparel;

                public ApparelEventArgs(Apparel Apparel)
                {
                    this.Apparel = Apparel;
                }
            }
        }
        [HarmonyPatch(typeof(Verse.Pawn_InventoryTracker))]
        public static class Pawn_InventoryTracker
        {
            public static event EventHandler<ItemEventArgs> ItemRemoved;

            [HarmonyPatch(nameof(Verse.Pawn_InventoryTracker.Notify_ItemRemoved))]
            [HarmonyPostfix]
            public static void Notify_ItemRemoved(Verse.Pawn_InventoryTracker __instance, Thing item)
            {
                ItemRemoved?.Invoke(__instance, new ItemEventArgs(item));
            }
            public class ItemEventArgs : EventArgs
            {
                public Thing Thing;

                public ItemEventArgs(Thing Thing)
                {
                    this.Thing = Thing;
                }
            }
        }

        [HarmonyPatch(typeof(Verse.ThingOwner))]
        public static class ThingOwner
        {
            public static event EventHandler<ItemEventArgs> Added;
            public static event EventHandler<ItemEventArgs> Removed;

            [HarmonyPatch("NotifyAdded")]
            [HarmonyPostfix]
            public static void NotifyAdded(Verse.ThingOwner __instance, Thing item)
            {
                Added?.Invoke(__instance, new ItemEventArgs(item));
            }

            [HarmonyPatch("NotifyRemoved")]
            [HarmonyPostfix]
            public static void NotifyRemoved(Verse.ThingOwner __instance, Thing item)
            {
                Removed?.Invoke(__instance, new ItemEventArgs(item));
            }
            public class ItemEventArgs : EventArgs
            {
                public Thing Thing;

                public ItemEventArgs(Thing Thing)
                {
                    this.Thing = Thing;
                }
            }
        }

        #endregion "Patches"

        /*
        UI color should be based on Dark Theme :
        https://docs.microsoft.com/en-us/visualstudio/extensibility/ux-guidelines/color-value-reference-for-visual-studio?view=vs-2019

        TODO:
        SceneLoaded
        https://github.com/UnlimitedHugs/RimworldHugsLib/wiki/ModBase-Reference

            GenUI.ThingsUnderMouse

            Add option to only clean inside rooms

            Add wealth overlay
            Add impressiveness overlay
            Add dirty overlay

            Shift Clicking a zone fill the mouseover room
            Shift+Ctrl clicking a zone unfill the mouseover room
            Holding alt makes the zone go into remove mode

            shift-dragging will create multiple of that item per tile
            shift-alt-dragging will create a box of that item per tile
            shift-ctrl dragging will create a hollow box of that item per tile

            Home button on world map to get back to base

            MiddleMouse click Copy single selected object like oxygen not included

            Allow doors to be locked so people can't just walk through them like dwarf fortress

            ordering pawns to attack something will make them go into range and shot at the thing

            Selecting pawns from alerts will make the camera follow them, clicking on pawns in the pawn overview menu or the pinned previews will also follow them
            Ctrl+clicking on a pawn will attach the camera to them too
            Selecting pawns should highlight them in the menu like vanilla

            like oxygen not included add a bias to a single specific desired skill to get a random pawn with a higher stats in that skill

            Shift-clicking on a target in combat will make your pawns shot at them when they get into range but will not move themselves
            Ctrl-cliking force attacks anything underneath the cursor, could be a tree or just the ground

            Add drag-select functionality to all ui elements

            Shelves will automatically inherit settings from the stockpile zone they're in or when a new zone is applied to them
            can be individually toggled on each piece of furniture

            Remove zone dragging limit size, seems to be 51 tiles in either direction -- Seems to work now?
            Rightclicking on a zone will open a context menu with its designator options and also a color picker and naming tool

            Allow TAB and arrow key navigation -- not happening :s

            Consider moving our GUI to its own thread

            Clicking on a research table or surgery table shows the penalty for dirtiness somewhere

            Zoomed out hostiles : use unicode icons to show angry face over enemies

            Add support for item optional textures

            Character "Quirks" that change the way they behave

            Add hotkey for copying stuff

            Can't find Damage for primary weapon in ListViewItem_Resource, and accuracy

            Add rebindable keybind for F1 for architect window, currently TAB and doesn't seem to work

            Add option to switch to right-mouse drag window only

            Right-click drag makes it hard to align drafted pawns --Can't find where in the code this is done

            TopLeft Notifications have to be merged/grouped together and inserted into its own window

            Precept for "Eating Without a Table"

            "plant cut" needs to be changed in localization to something more accurate

            default behavior should be attack, when target is out of range, go closer and attack
            shift-clicking would be hold your ground and shot if the target gets in range
            ctrl-clicking would be target the ground or anything

            // Missing: assign, animals, wildlife, ideoligions
            // Consider unicode symbols instead

            //http://veli.ee/colorpedia/

            //Corn
            //# E0A400

            Description :
                All changes are disabled by default, except for the xml patches because i don't know how to make those optional.

                Changes Mouse Camera Drag from Mouse MiddleClick to Mouse RightClick.
                Increase the range of drag sensitivity in the menus from 0.8 to 0.0, allowing a wider sensitivity selection. Right-Click drag set to 12% seems to be good.

                Patches Steel, Uranium, Gold, Jade, Plasteel, and Silver to nolonger be flammable, to disable any of these delete the corrisponding file inside the "Patches" folder.

            Known Issues :
                    The Architect menu's subitems that have Sub-ListViews steal mousewheel scrolling from the parent.

                    Right-Click dragging feels floaty, 16% seems like a good value for scroll speed in the Options -> Controls -> Map Mouse Drag Sensitivity

                    Sometimes Items in the Thoughts ListView doesn't have a correct size and Text is cutoff.


            GameConditionManager
            GlobalControls.GlobalControlsOnGUI
            Letter
            LetterStack

            Can't replace vanilla string to from "one drink per day" to "Once Per Day"
            <Defs><DrugPolicyDefs><DrugPolicyDef><defName>OneDrinkPerDay.label
         */
    }
}
