using BigMod.Entities;
using RimWorld;
using RimWorld.Planet;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Verse;

namespace BigMod
{
    public static class Globals
    {
        public static Dictionary<string, Color> Colors = new Dictionary<string, Color>();
        public static Dictionary<string, Color> Assigns = new Dictionary<string, Color>();
        public static Dictionary<string, Sheet.Style.Proxy> Palettes = new Dictionary<string, Sheet.Style.Proxy>();
        public static Dictionary<string, Texture2D> TextureGenerated = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Texture2D> AssignedTextureGenerated = new Dictionary<string, Texture2D>();

        /// <summary>
        /// TexturePaths here can be overriden in the config files, the game will automatically load them.
        /// </summary>
        public static Dictionary<string, string> TextureAliases = new Dictionary<string, string>();

        static Globals()
        {
            Load();
        }

        public static void Load()
        {
            string SheetPath = Path.Combine(BigMod.Directory, "Resources/Sheet.xml");

            Sheet Sheet = Read<Sheet>(SheetPath);

            Load_Colors(Sheet);
            Load_Textures(Sheet);
            Load_Assigns(Sheet);
            Load_Palettes(Sheet);
            Load_TextureGenerated(Sheet);
        }

        public static void Load_Colors(Sheet Sheet)
        {
            foreach (Sheet.Color Color in Sheet.Colors)
            {
                if (!string.IsNullOrWhiteSpace(Color.RGBA))
                {
                    List<int> RGBA = Array.ConvertAll(Color.RGBA.Split(','), int.Parse).ToList();

                    if (RGBA.Count == 3)
                    {
                        // Add transparency if it's missing
                        RGBA.Add(255);
                    }

                    // Still has to be converted to float values.
                    AddColor(Color.Key, new Color(((float)RGBA[0] / 255), ((float)RGBA[1] / 255), ((float)RGBA[2] / 255), ((float)RGBA[3] / 255)));
                }
                else if (!string.IsNullOrWhiteSpace(Color.RGBA_float))
                {
                    List<float> RGBA = Array.ConvertAll(Color.RGBA_float.Split(','), float.Parse).ToList();

                    if (RGBA.Count == 3)
                    {
                        RGBA.Add(1);
                    }

                    AddColor(Color.Key, new Color(RGBA[0], RGBA[1], RGBA[2], RGBA[3]));
                }
                else if (!string.IsNullOrWhiteSpace(Color.HTML))
                {
                    AddColor(Color.Key, Color.HTML);
                }

                // Generate a colored texture from this color
                if (Color.Generate)
                {
                    TextureGenerate(Color.Key, Colors[Color.Key]);
                }
            }
        }

        public static void Load_Textures(Sheet Sheet)
        {
            foreach (Sheet.Texture Texture in Sheet.Textures)
            {
                if (string.IsNullOrWhiteSpace(Texture.Folder))
                {
                    TextureAliases.Add(Texture.Key, Texture.Path);
                }
                else
                {
                    string TextureFolderPath = Path.Combine(BigMod.Directory, "Textures");
                    string FolderPath = Path.Combine(TextureFolderPath, Texture.Folder);

                    foreach (string File in Directory.GetFiles(FolderPath, "*.png"))
                    {
                        // Format the string into something the game's content manager accepts.
                        TextureAliases.Add(Path.GetFileNameWithoutExtension(File), File.ReplaceFirst(TextureFolderPath + "\\", string.Empty).ReplaceFirst(".png", string.Empty).Replace("\\", "/"));
                    }
                }
            }
        }

        public static void Load_Palettes(Sheet Sheet)
        {
            List<FieldInfo> Fields = typeof(Sheet.Style.Proxy).GetFields().Where((F) => (F.FieldType == typeof(Color?))).ToList();

            foreach (Sheet.Palette Palette in Sheet.Palettes)
            {
                Sheet.Style.Proxy Proxy = new Sheet.Style.Proxy();

                if (!string.IsNullOrWhiteSpace(Palette.Parent))
                {
                    if (Palettes.TryGetValue(Palette.Parent, out Sheet.Style.Proxy Parent))
                    {
                        Parent.Apply(Proxy);
                    }
                    else
                    {
                        WriteLineError($"Globals.Load_Palettes : Palette '{Palette.Key}' had Parent '{Palette.Parent}' but Parent didn't exist.");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(Palette.Hierarchy))
                {
                    // Collapse the Hierarchy.
                    foreach (string Hierarch in Palette.Hierarchy.Split(','))
                    {
                        if (Palettes.TryGetValue(Hierarch, out Sheet.Style.Proxy Parent))
                        {
                            Parent.Apply(Proxy);
                        }
                        else
                        {
                            // If Hierarch doesn't exist then we have to manually fetch the colors.
                            foreach (string Key in Assigns.Keys.Where((F) => F.StartsWith(Hierarch + ".")))
                            {
                                // Remove the Hierarch prefix and get the value for the property name.
                                string PropertyName = Key.ReplaceFirst((Hierarch + "."), string.Empty);

                                FieldInfo FieldInfo = Fields.FirstOrDefault((F) => (F.Name == PropertyName));

                                if (FieldInfo != null)
                                {
                                    if (Assigns.TryGetValue(Key, out Color Color))
                                    {
                                        FieldInfo.SetValue(Proxy, Color);
                                    }
                                    else
                                    {
                                        WriteLineError($"Globals.Load_Palettes : Hierarch '{Hierarch}' had a property of Name '{FieldInfo.Name}' but Color of Assign '{Key}' didn't exist.");
                                    }
                                }
                            }
                        }
                    }
                }

                Palette.Style?.ToProxy().Apply(Proxy);

                Palettes.Add(Palette.Key, Proxy);
            }
        }

        public static void Load_Assigns(Sheet Sheet)
        {
            foreach (Sheet.Assign Assign in Sheet.Assigns)
            {
                if (string.IsNullOrWhiteSpace(Assign.Parent))
                {
                    if (Colors.TryGetValue(Assign.Color, out Color Color))
                    {
                        Assigns.Add(Assign.Key, Color);
                    }
                    else
                    {
                        WriteLineError($"Globals.Load_Assigns : Assign '{Assign.Key}' had Color '{Assign.Color}' but Color didn't exist.");
                    }
                }
                else
                {
                    if (Assigns.TryGetValue(Assign.Parent, out Color Color))
                    {
                        Assigns.Add(Assign.Key, Color);
                    }
                    else
                    {
                        WriteLineError($"Globals.Load_Assigns : Assign '{Assign.Key}' had Parent '{Assign.Parent}' but Parent didn't exist.");
                    }
                }
            }
        }

        public static void Load_TextureGenerated(Sheet Sheet)
        {
            foreach (Sheet.Assign Assign in Sheet.TexturesGenerated)
            {
                AssignedTextureGenerated.Add(Assign.Key, TextureGenerated[Assign.Color]);
            }
        }

        public static Texture2D GetTextureGenerated(string Name)
        {
            return AssignedTextureGenerated[Name];
        }

        public static Texture2D TextureGenerate(string Name, Color Color, int Width = 1, int Height = 1)
        {
            Texture2D Texture2D = CreateTextureColor(Color, Width, Height);
            TextureGenerated.Add(Name, Texture2D);
            return Texture2D;
        }

        public static void AddColor(string Key, string HTMLColor)
        {
            if (ColorUtility.TryParseHtmlString(HTMLColor, out Color Color))
            {
                Colors.Add(Key, Color);
            }
        }

        public static void AddColor(string Key, Color Color)
        {
            Colors.Add(Key, Color);
        }

        /// <summary>
        /// Get the Color Assigned to the associated string.
        /// </summary>
        /// <param name="Assign">Assigned string.</param>
        /// <returns>Color assigned to string.</returns>
        public static Color GetColor(string Assign)
        {
            return Assigns[Assign];
        }

        /// <summary>
        /// Try to get the Color associated with the given Key.
        /// </summary>
        /// <param name="Key">Key paired with the desired color.</param>
        /// <param name="Color">Color from the Kay.</param>
        /// <returns>True if Key exists.</returns>
        public static bool TryGetColorFromKey(string Key, out Color Color)
        {
            if (Colors.TryGetValue(Key, out Color))
            {
                return true;
            }

            WriteLineError($"Globals.TryGetColorFromKey : Tried to get Color from Key '{Key}' but Color didn't exist.");

            return false;
        }

        /// <summary>
        /// Get the Colors Assigned to the associated strings.
        /// </summary>
        /// <param name="Assigns">Assigned strings.</param>
        /// <returns>List of Colors assigned to the strings.</returns>
        public static List<Color> GetColorRange(string[] Assigns)
        {
            List<Color> Colors = new List<Color>();

            foreach (string Assign in Assigns)
            {
                Colors.Add(GetColor(Assign));
            }

            return Colors;
        }

        /// <summary>
        /// Tries to get the Color from the given Key, if it doesn't exist, returns Fallback entry.
        /// </summary>
        /// <param name="Key">Color to lookup.</param>
        /// <param name="FallBack">Fallback color to return. Must be valid.</param>
        /// <returns>Color from the given <paramref name="Key"/> or using <paramref name="FallBack"/> if Key doesn't exist..</returns>
        public static Color TryGetColor(string Key, string FallBack)
        {
            if (!Assigns.TryGetValue(Key, out Color Color))
            {
                return GetColor(FallBack);
            }

            return Color;
        }

        /// <summary>
        /// Tries to get the Color from the given Key.
        /// </summary>
        /// <param name="Key">Color to lookup.</param>
        /// <returns>Color if it exists or default.</returns>
        public static Color TryGetColor(string Key)
        {
            if (Assigns.TryGetValue(Key, out Color Color))
            {
                return Color;
            }

            return default;
        }

        public static bool TryGetColor(string Key, out Color Color)
        {
            if (Assigns.TryGetValue(Key, out Color))
            {
                return true;
            }

            WriteLineError($"Globals.TryGetColor : Tried to get Color from Assign Key '{Key}' but Color didn't exist.");

            return false;
        }

        /// <summary>
        /// Get the TexturePath associated with the Alias.
        /// </summary>
        /// <param name="Alias">Key of the Alias.</param>
        /// <returns>TexturePath or <paramref name="Alias"/> if Alias doesn't exist in the <see cref="TextureAliases"/> Dictionary.</returns>
        public static string TryGetTexturePathFromAlias(string Alias)
        {
            if (!TextureAliases.TryGetValue(Alias, out string TexturePath))
            {
                return Alias;
            }

            return TexturePath;
        }

        /// <summary>
        /// Get the Texture associated with the <paramref name="Key"/>, Texture must exist.
        /// </summary>
        /// <param name="Alias">Texture Alias of a texture in <see cref="TextureAliases"/>.</param>
        /// <returns>A texture based on the Key's Alias.</returns>
        public static Texture2D GetTextureFromAlias(string Alias)
        {
            return GetTexture(TryGetTexturePathFromAlias(Alias));
        }

        // TODO: This needs to be pulled and store temporarily instead of creating the list each time.
        /// <summary>
        /// Gets all resources on the current map that can build the specified Thing.
        /// </summary>
        /// <param name="BuildableDef">Thing that items should be able to build.</param>
        /// <returns>List of resources that can build the item or null if Item cannot be built using resources.</returns>
        public static List<ThingDef> GetMapResources_CanMake(BuildableDef BuildableDef)
        {
            if (!BuildableDef.MadeFromStuff)
            {
                return null;
            }

            return GetMapResources().Where(F => (F.IsStuff && F.stuffProps.CanMake(BuildableDef) && (Globals.GodMode || Find.CurrentMap.listerThings.ThingsOfDef(F).Any()))).ToList();
        }

        public static List<ThingDef> GetMapResources_InCategory(ThingCategoryDef Category)
        {
            return GetMapResources().Where(F => F.IsWithinCategory(Category)).ToList();
        }

        // Modified from Designator_Build.ProcessInput
        public static List<ThingDef> GetMapResources()
        {
            return Find.CurrentMap.resourceCounter.AllCountedAmounts.Keys.OrderByDescending((ThingDef ThingDef) =>
            {
                if (ThingDef.stuffProps == null)
                {
                    return float.PositiveInfinity;
                }

                return ThingDef.stuffProps.commonality;
            }).ThenBy((ThingDef ThingDef) => ThingDef.BaseMarketValue).ToList();
        }

        /// <summary>
        /// Selects all matching items of <paramref name="ThingDef"/> made from <paramref name="StuffDef"/> on screen.
        /// </summary>
        /// <param name="ThingDef">Base thing item.</param>
        /// <param name="StuffDef">Stuff material that <paramref name="ThingDef"/> is made from.</param>
        public static void SelectMatchingItemsOnScreen(ThingDef ThingDef, ThingDef StuffDef)
        {
            // Select all Items matching this one currently on screen
            // Based on DoorsDebugDrawer.DrawDebug
            CellRect CurrentViewRect = Find.CameraDriver.CurrentViewRect;

            List<Thing> List = Find.CurrentMap.listerThings.ThingsOfDef(ThingDef);
            Selector Selector = Find.Selector;

            List.Where((F) => !F.PositionHeld.Fogged(F.MapHeld) && CurrentViewRect.Contains(F.Position) && (F.Stuff == StuffDef)).ToList().ForEach((F) => Selector.Select(F));
        }

        /// <summary>
        /// Selects all matching items of <paramref name="ThingDef"/> on screen.
        /// </summary>
        /// <param name="ThingDef">ThingDef to select.</param>
        public static void SelectMatchingItemsOnScreen(ThingDef ThingDef)
        {
            CellRect CurrentViewRect = Find.CameraDriver.CurrentViewRect;

            List<Thing> List = Find.CurrentMap.listerThings.ThingsOfDef(ThingDef);
            Selector Selector = Find.Selector;

            List.Where((F) => !F.PositionHeld.Fogged(F.MapHeld) && CurrentViewRect.Contains(F.Position)).ToList().ForEach((F) => Selector.Select(F));
        }

        /// <summary>
        /// Selects all matching items of <paramref name="ThingDef"/> made from <paramref name="StuffDef"/> on the entire map.
        /// </summary>
        /// <param name="ThingDef">Base thing item.</param>
        /// <param name="StuffDef">Stuff material that <paramref name="ThingDef"/> is made from.</param>
        public static void SelectMatchingItemsOnMap(ThingDef ThingDef, ThingDef StuffDef)
        {
            List<Thing> List = Find.CurrentMap.listerThings.ThingsOfDef(ThingDef);
            Selector Selector = Find.Selector;

            List.Where((F) => (F.Stuff == StuffDef) && !F.PositionHeld.Fogged(F.MapHeld)).ToList().ForEach((F) => Selector.Select(F));
        }

        /// <summary>
        /// Selects all matching items of <paramref name="ThingDef"/> on the entire map.
        /// </summary>
        /// <param name="ThingDef">Base thing item.</param>
        public static void SelectMatchingItemsOnMap(ThingDef ThingDef)
        {
            List<Thing> List = Find.CurrentMap.listerThings.ThingsOfDef(ThingDef);
            Selector Selector = Find.Selector;

            List.Where((F) => !F.PositionHeld.Fogged(F.MapHeld)).ToList().ForEach((F) => Selector.Select(F));
        }

        /// <summary>
        /// Selects the first matching item of <paramref name="ThingDef"/> made from <paramref name="StuffDef"/> on the entire map.
        /// </summary>
        /// <param name="ThingDef">Base thing item.</param>
        /// <param name="StuffDef">Stuff material that <paramref name="ThingDef"/> is made from.</param>
        public static void SelectMatchingItemFirstOnMap(ThingDef ThingDef, ThingDef StuffDef)
        {
            // Center on the first building of this type on the map
            List<Thing> List = Find.CurrentMap.listerThings.ThingsOfDef(ThingDef);
            // Does not return any indication of success
            CameraJumper.TryJumpAndSelect(List.FirstOrDefault((F) => (F.Stuff == StuffDef) && !F.PositionHeld.Fogged(F.MapHeld)));
        }

        /// <summary>
        /// Selects the first matching item of <paramref name="ThingDef"/> on the entire map.
        /// </summary>
        /// <param name="ThingDef">ThingDef to select.</param>
        public static void SelectMatchingItemFirstOnMap(ThingDef ThingDef)
        {
            List<Thing> List = Find.CurrentMap.listerThings.ThingsOfDef(ThingDef);
            // Shouldn't jump to unknown items
            CameraJumper.TryJumpAndSelect(List.FirstOrDefault((F) => !F.PositionHeld.Fogged(F.MapHeld)));
        }

        /// <summary>
        /// Get all haulable items on the current map.
        /// </summary>
        /// <param name="OnlyKnown">Get only items we know of.</param>
        /// <returns>List of all haulable items on the current map.</returns>
        public static List<Thing> GetAllItemsOnMap(bool OnlyKnown = false)
        {
            List<Thing> Items = new List<Thing>();

            ThingOwnerUtility.GetAllThingsRecursively(Find.CurrentMap, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), Items, false);

            if (OnlyKnown)
            {
                // Only return items that we know of
                Items = Items.Where((F) => !F.PositionHeld.Fogged(F.MapHeld)).ToList();
            }

            return Items;
        }

        /// <summary>
        /// Standardizes Selection jumping from UI elements to gameworld objects.
        /// </summary>
        /// <remarks>This should be prefered over making individual methods in each class that requires this functionality to ensure keybind consistency.</remarks>
        /// <param name="ThingDef">Base Thing to search for.</param>
        /// <param name="StuffDef">Material of thing to search for.</param>
        /// <returns>True if handled, false if not.</returns>
        public static bool HandleModifierSelection(ThingDef ThingDef, ThingDef StuffDef = null)
        {
            // TODO: isn't smart enough to get minified items, doesn't search pawns for items
            if (WindowManager.IsShiftDown() && WindowManager.IsCtrlDown() && WindowManager.IsAltDown())
            {
                if (StuffDef != null)
                {
                    SelectMatchingItemsOnMap(ThingDef, StuffDef);
                }
                else
                {
                    SelectMatchingItemsOnMap(ThingDef);
                }

                return true;
            }
            else if (WindowManager.IsShiftDown() && WindowManager.IsAltDown())
            {
                if (StuffDef != null)
                {
                    SelectMatchingItemsOnScreen(ThingDef, StuffDef);
                }
                else
                {
                    SelectMatchingItemsOnScreen(ThingDef);
                }

                return true;
            }
            else if (WindowManager.IsShiftDown() && WindowManager.IsCtrlDown())
            {
                if (StuffDef != null)
                {
                    SelectMatchingItemFirstOnMap(ThingDef, StuffDef);
                }
                else
                {
                    SelectMatchingItemFirstOnMap(ThingDef);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all identical items grouped by their definition.
        /// </summary>
        /// <param name="PawnHolder">If Items carried by Pawns should be retrieved too.</param>
        /// <returns>Dictionary of items grouped by their definition with count for how many there are, sorted by rarity and condition.</returns>
        public static Dictionary<ThingDef, (List<Thing> Things, int Count)> GetAllCommodities(bool PawnHolder = false)
        {
            var Commodities = new Dictionary<ThingDef, (List<Thing> Things, int Count)>();
            List<Thing> Items = GetAllItemsOnMap(true);

            if (!PawnHolder)
            {
                // Do not get items carried by Pawns
                Items.RemoveAll(Item => Item.holdingOwner.Owner.ParentHolder is Pawn);
            }

            Commodities = Items.GroupBy(Item => Item.GetInnerIfMinified().def).ToDictionary(Group => Group.Key, Group => (Group.ToList().OrderBy((F) => F.TryGetComp<CompQuality>()?.Quality).ThenBy((F) => (F.HitPoints / F.MaxHitPoints)).ToList(), Group.Sum((F) => F.stackCount)));

            return Commodities;
        }

        /// <summary>
        /// Retrieves all Items of the specified ThingDef on the current Map.
        /// </summary>
        /// <param name="ThingDef">ThingDef to search for.</param>
        /// <param name="OnlyKnown">Get only items we know of.</param>
        /// <param name="PawnHolder">If Items carried by Pawns should be retrieved too.</param>
        /// <returns>List of all K</returns>
        public static List<Thing> FindAllOf(ThingDef ThingDef, bool OnlyKnown = false, bool PawnHolder = false)
        {
            List<Thing> Items = GetAllItemsOnMap(OnlyKnown);

            if (!PawnHolder)
            {
                // Do not get items carried by Pawns
                Items.RemoveAll(Item => Item.holdingOwner.Owner.ParentHolder is Pawn);
            }

            Items.RemoveAll(Item => Item.def != ThingDef);

            return Items;
        }

        public static Texture2D ToTexture2D(this RenderTexture RenderTexture)
        {
            Texture2D Texture2D = new Texture2D(RenderTexture.width, RenderTexture.height, TextureFormat.RGBA32, false);
            Texture2D.Apply(false);
            Graphics.CopyTexture(RenderTexture, Texture2D);
            return Texture2D;
        }

        /// <summary>
        /// Attempts to retrieve the Texture from <see cref="TextureGenerated"/> first, then tries to load it from the game's contentmanager if it doesn't exist.
        /// </summary>
        /// <param name="TexturePath">Path to the texture in the game's directory.</param>
        /// <returns>The Loaded Texture.</returns>
        public static Texture2D GetTexture(string TexturePath)
        {
            TextureGenerated.TryGetValue(TexturePath, out Texture2D Texture2D);

            if (Texture2D == null)
            {
                return ContentFinder<Texture2D>.Get(TexturePath);
            }

            return Texture2D;
        }

        public static Texture2D CreateTextureColor(Color Color, int Width = 1, int Height = 1)
        {
            Texture2D Texture2D = new Texture2D(Width, Height);
            Color[] Pixels = Enumerable.Repeat(Color, (Width * Height)).ToArray();
            Texture2D.SetPixels(Pixels);
            Texture2D.Apply();

            return Texture2D;
        }

        public static string GetGenderUnicodeSymbol(Gender Gender)
        {
            switch (Gender)
            {
                case Gender.None:
                    return "⚪";

                case Gender.Male:
                    return "♂";

                case Gender.Female:
                    return "♀";
            }

            return string.Empty;
        }

        public static ColorDef GetClosestColorDef(Color Color)
        {
            float Difference(Color A, Color B) => Mathf.Abs(A.r - B.r) + Mathf.Abs(A.g - B.g) + Mathf.Abs(A.b - B.b) + Mathf.Abs(A.a - B.a);

            return DefDatabase<ColorDef>.AllDefs.MinBy(colorDef => Difference(Color, colorDef.color));
        }

        /// <summary>
        /// Modified from ColonistBarColonistDrawer.DrawIcons because it was private and didn't return the texture path
        /// </summary>
        public static (string IconPath, string StatusDescription) GetStatus(Pawn Pawn)
        {
            if (Pawn.Dead)
            {
                return (TryGetTexturePathFromAlias("Skull"), "DiedOn".Translate(GenDate.DateFullStringAt((long)GenDate.TickGameToAbs(Pawn.Corpse.timeOfDeath), Find.WorldGrid.LongLatOf(Pawn.Corpse.Tile))));
            }
            else if (Pawn.ShouldBeSlaughtered())
            {
                return (TryGetTexturePathFromAlias("Status_Slaughter"), "Slaughter_ToolTipText".Translate());
            }
            else if (Pawn.Downed)
            {
                return (TryGetTexturePathFromAlias("Downed"), "Downed".Translate());
            }
            else if (Pawn.guilt?.awaitingExecution == true)
            {
                return (TryGetTexturePathFromAlias("Execute"), "Execute_ToolTipText_Marked".Translate());
            }
            else if (Pawn.IsFormingCaravan())
            {
                return (TryGetTexturePathFromAlias("FormingCaravan"), "ActivityIconFormingCaravan".Translate());
            }
            else if (Pawn.InAggroMentalState)
            {
                return (TryGetTexturePathFromAlias("MentalStateAggro"), Pawn.MentalStateDef.LabelCap);
            }
            else if (Pawn.InMentalState)
            {
                return (TryGetTexturePathFromAlias("MentalStateNonAggro"), Pawn.MentalStateDef.LabelCap);
            }
            else if (Pawn.InBed() && Pawn.CurrentBed().Medical)
            {
                return (TryGetTexturePathFromAlias("MedicalRest"), "ActivityIconMedicalRest".Translate());
            }
            else if ((Pawn.CurJob != null) && (Pawn.jobs.curDriver.asleep))
            {
                return (TryGetTexturePathFromAlias("Sleeping"), "ActivityIconSleeping".Translate());
            }
            else if ((Pawn.CurJob != null) && (Pawn.CurJob.def == JobDefOf.FleeAndCower))
            {
                return (TryGetTexturePathFromAlias("Fleeing"), "ActivityIconFleeing".Translate());
            }
            else if (Pawn.IsFighting())
            {
                return (TryGetTexturePathFromAlias("Attacking"), "ActivityIconAttacking".Translate());
            }
            else if (Pawn.mindState.IsIdle && (GenDate.DaysPassed >= 1))
            {
                return (TryGetTexturePathFromAlias("Idle"), "ActivityIconIdle".Translate());
            }
            else if (Pawn.IsBurning())
            {
                return (TryGetTexturePathFromAlias("Burning"), "ActivityIconBurning".Translate());
            }
            else if (Pawn.Inspired)
            {
                return (TryGetTexturePathFromAlias("Inspired"), Pawn.InspirationDef.LabelCap);
            }
            else if ((Pawn.IsSlaveOfColony) && (Pawn.guest.guestStatusInt == GuestStatus.Slave))
            {
                return (TryGetTexturePathFromAlias("Slavery"), "Slave".Translate());
            }

            return (null, null);
        }

        /// <summary>
        /// Modified from Pawn_PlayerSettings.ResetMedicalCare because we want to get the current Medical Group and not set it.
        /// </summary>
        /// <param name="Pawn">Pawn to get Medical Group for.</param>
        /// <returns>Untranslated String for Medical Group.</returns>
        public static string GetMedicalGroup(Pawn Pawn)
        {
            string MedicalGroup;

            if (Pawn.Faction == Faction.OfPlayer)
            {
                if (Pawn.RaceProps.Animal)
                {
                    MedicalGroup = "MedGroupColonyAnimal";
                }
                else if (Pawn.IsPrisoner)
                {
                    MedicalGroup = "MedGroupImprisonedColonist";
                }
                else if (Pawn.IsSlave)
                {
                    MedicalGroup = "MedGroupEnslavedColonist";
                }
                else
                {
                    MedicalGroup = "MedGroupColonist";
                }
            }
            else
            {
                if ((Pawn.Faction == null) && Pawn.RaceProps.Animal)
                {
                    MedicalGroup = "MedGroupNeutralAnimal";
                }
                else if ((Pawn.Faction == null) || !Pawn.Faction.HostileTo(Faction.OfPlayer))
                {
                    MedicalGroup = "MedGroupNeutralFaction";
                }
                else
                {
                    MedicalGroup = "MedGroupHostileFaction";
                }
            }

            return MedicalGroup;
        }

        #region "Exposed"

        /// <summary>
        /// Exposes the private method inside Widgets.
        /// </summary>
        public static void Widgets_ResolveParseNow<T>(string Edited, ref T Value, ref string Buffer, float Min, float Max, bool Force = false)
        {
            MethodInfo Method = typeof(Widgets).GetMethod("ResolveParseNow", (BindingFlags.NonPublic | BindingFlags.Static));
            MethodInfo Generic = Method.MakeGenericMethod(typeof(T));

            object[] Parameters = new object[] { Edited, Value, Buffer, Min, Max, Force };

            Generic.Invoke(null, Parameters);

            // Return the values
            Value = (T)Parameters[1];
            Buffer = (string)Parameters[2];
        }

        /// <summary>
        /// Exposes the private method inside SkillUI.
        /// </summary>
        public static string SkillUI_GetSkillDescription(SkillRecord Skill)
        {
            MethodInfo Method = typeof(SkillUI).GetMethod("GetSkillDescription", (BindingFlags.NonPublic | BindingFlags.Static));
            object[] Parameters = new object[] { Skill };

            return (string)Method.Invoke(null, Parameters);
        }

        /// <summary>
        /// Exposes the private method inside CharacterCardUtility.
        /// </summary>
        public static List<object> CharacterCardUtility_GetWorkTypeDisableCauses(Pawn Pawn, WorkTags WorkTag)
        {
            MethodInfo Method = typeof(CharacterCardUtility).GetMethod("GetWorkTypeDisableCauses", (BindingFlags.NonPublic | BindingFlags.Static));
            object[] Parameters = new object[] { Pawn, WorkTag };

            return (List<object>)Method.Invoke(null, Parameters);
        }

        /// <summary>
        /// Exposes the private method inside CharacterCardUtility.
        /// </summary>
        public static Color CharacterCardUtility_GetDisabledWorkTagLabelColor(Pawn Pawn, WorkTags WorkTag)
        {
            MethodInfo Method = typeof(CharacterCardUtility).GetMethod("GetDisabledWorkTagLabelColor", (BindingFlags.NonPublic | BindingFlags.Static));
            object[] Parameters = new object[] { Pawn, WorkTag };

            return (Color)Method.Invoke(null, Parameters);
        }

        /// <summary>
        /// Exposes the private method inside CharacterCardUtility.
        /// </summary>
        public static string CharacterCardUtility_GetWorkTypeDisabledCausedBy(Pawn Pawn, WorkTags WorkTag)
        {
            MethodInfo Method = typeof(CharacterCardUtility).GetMethod("GetWorkTypeDisabledCausedBy", (BindingFlags.NonPublic | BindingFlags.Static));
            object[] Parameters = new object[] { Pawn, WorkTag };

            return (string)Method.Invoke(null, Parameters);
        }

        /// <summary>
        /// Exposes the private method inside CharacterCardUtility.
        /// </summary>
        public static string CharacterCardUtility_GetWorkTypesDisabledByWorkTag(WorkTags WorkTag)
        {
            MethodInfo Method = typeof(CharacterCardUtility).GetMethod("GetWorkTypesDisabledByWorkTag", (BindingFlags.NonPublic | BindingFlags.Static));
            object[] Parameters = new object[] { WorkTag };

            return (string)Method.Invoke(null, Parameters);
        }

        /// <summary>
        /// Exposes the protected method inside PawnColumnWorker_LifeStage.
        /// </summary>
        public static string PawnColumnWorker_LifeStage_GetIconTip(Pawn Pawn)
        {
            MethodInfo Method = typeof(PawnColumnWorker_LifeStage).GetMethod("GetIconTip", (BindingFlags.NonPublic | BindingFlags.Instance));
            Func<Pawn, string> Function = (Func<Pawn, string>)Delegate.CreateDelegate(typeof(Func<Pawn, string>), null, Method);

            return Function.Invoke(Pawn);
        }

        /// <summary>
        /// Exposes the private field of <see cref="MedicalCareUtility.careTextures"/>.
        /// </summary>
        /// <returns>Texture array of the different Medicines that can be applied.</returns>
        public static Texture2D[] MedicalCareUtility_CareTextures()
        {
            FieldInfo Field = typeof(MedicalCareUtility).GetField("careTextures", (BindingFlags.NonPublic | BindingFlags.Static));

            return (Texture2D[])Field.GetValue(null);
        }

        /// <summary>
        /// Exposes the private static method in the MedicalCareUtility.
        /// </summary>
        public static Texture2D MedicalCareUtility_MedicalCareIcon(MedicalCareCategory Category)
        {
            MethodInfo Method = typeof(MedicalCareUtility).GetMethod("MedicalCareIcon", (BindingFlags.NonPublic | BindingFlags.Static));
            object[] Parameters = new object[] { Category };

            return (Texture2D)Method.Invoke(null, Parameters);
        }

        /// <summary>
        /// Exposes the private static method in the GlobalControls.
        /// </summary>
        public static string GlobalControls_TemperatureString()
        {
            MethodInfo Method = typeof(GlobalControls).GetMethod("TemperatureString", (BindingFlags.NonPublic | BindingFlags.Static));

            return (string)Method.Invoke(null, null);
        }

        /// <summary>
        /// Exposes the protected field in the supplied Need Instance.
        /// </summary>
        /// <returns>List of Float Thresholds, null if list isn't set.</returns>
        public static List<float> Get_Need_threshPercents(Need Instance)
        {
            FieldInfo Field = typeof(Need).GetField("threshPercents", (BindingFlags.NonPublic | BindingFlags.Instance));

            object Value = Field.GetValue(Instance);

            if (Value != null)
            {
                return ((List<float>)Value);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the protected field in the supplied Need Instance.
        /// </summary>
        /// <param name="Instance">Instance to set value on.</param>
        /// <param name="threshPercents">Value to set.</param>
        public static void Set_Need_threshPercents(Need Instance, List<float> threshPercents)
        {
            FieldInfo Field = typeof(Need).GetField("threshPercents", (BindingFlags.NonPublic | BindingFlags.Instance));

            Field.SetValue(Instance, threshPercents);
        }

        /// <summary>
        /// Exposes the private method in HealthCardUtility to get Visible Hediffs.
        /// </summary>
        /// <param name="Pawn">Pawn to get Hediffs from.</param>
        /// <param name="ShowBloodLoss">If Blood Loss should be shown.</param>
        /// <returns>IEnumerable with BodyPartRecords and Hediffs grouped together.</returns>
        public static IEnumerable<IGrouping<BodyPartRecord, Hediff>> HealthCardUtility_VisibleHediffGroupsInOrder(Pawn Pawn, bool ShowBloodLoss)
        {
            MethodInfo Method = typeof(HealthCardUtility).GetMethod("VisibleHediffGroupsInOrder", (BindingFlags.NonPublic | BindingFlags.Static));
            object[] Parameters = new object[] { Pawn, ShowBloodLoss };

            return (IEnumerable<IGrouping<BodyPartRecord, Hediff>>)Method.Invoke(null, Parameters);
        }


        #endregion "Exposed"

        /// <summary>
        /// Used for simplifying debugging.
        /// </summary>
        public static void WriteLine(params object[] Parameters)
        {
            System.Text.StringBuilder StringBuilder = new System.Text.StringBuilder();

            foreach (object Object in Parameters)
            {
                if (Object != null)
                {
                    StringBuilder.Append(Object.ToString());
                }

                StringBuilder.Append(" _ ");
            }

            Log.Message(StringBuilder.ToString());
        }

        /// <summary>
        /// Clears the Log before Printing stuff to it.
        /// </summary>
        public static void WriteLineReset(params object[] Parameters)
        {
            Log.Clear();
            WriteLine(Parameters);
        }

        /// <summary>
        /// Resets the Log Message limit each time.
        /// </summary>
        public static void WriteLineNoLimit(params object[] Parameters)
        {
            Log.ResetMessageCount();
            WriteLine(Parameters);
        }

        public static void WriteLineError(string Message)
        {
            Log.Error($"[{BigMod.Instance.ModIdentifier}] : " + Message);
        }

        /// <summary>
        /// Wrapper for Godmode.
        /// </summary>
        /// <returns>True if is in God Mode.</returns>
        public static bool GodMode
        {
            get
            {
                return DebugSettings.godMode;
            }
        }

        /// <summary>
        /// Reads a XML document from the given Path and deserializes it into an Object.
        /// </summary>
        /// <typeparam name="T">Type of Object to deserialize into.</typeparam>
        /// <param name="Path">Full path; including file name and extension.</param>
        /// <returns>Instance of the deserialized Object. Nothing if the specified file does not exist.</returns>
        public static T Read<T>(string Path)
        {
            if (System.IO.File.Exists(Path))
            {
                return Deserialize<T>(File.ReadAllText(Path));
            }

            return default;
        }

        /// <summary>
        /// Deserializes a XML String into an Object.
        /// </summary>
        /// <typeparam name="T">Type of Object represented by the String.</typeparam>
        /// <param name="Input">Input XML String to Deserialize.</param>
        /// <returns>Instance of the deserialized Object.</returns>
        public static T Deserialize<T>(string Input)
        {
            XmlSerializer XMLSerializer = new XmlSerializer(typeof(T));
            T Result;

            using (XmlTextReader Reader = new XmlTextReader(new StringReader(Input)))
            {
                Result = (T)XMLSerializer.Deserialize(Reader);
            }

            return Result;
        }

        public class Sheet
        {
            public List<Color> Colors;
            public List<Assign> Assigns;
            public List<Palette> Palettes;
            public List<Assign> TexturesGenerated;
            public List<Texture> Textures;

            public class Texture
            {
                [XmlAttribute]
                public string Key;
                [XmlAttribute]
                public string Path;
                [XmlAttribute]
                public string Folder;
            }

            public class Color
            {
                [XmlAttribute]
                public string Key;
                [XmlAttribute]
                public string HTML;
                [XmlAttribute]
                public string RGBA_float;
                [XmlAttribute]
                public string RGBA;
                [XmlAttribute]
                public bool Generate;
            }

            public class Assign
            {
                [XmlAttribute]
                public string Key;
                [XmlAttribute]
                public string Color;
                [XmlAttribute]
                public string Parent;
            }

            public class Palette
            {
                [XmlAttribute]
                public string Key;
                [XmlAttribute]
                public string Hierarchy;
                [XmlAttribute]
                public string Parent;
                public Style Style;
            }

            public class Style
            {
                [XmlAttribute]
                public string DisabledColor;
                [XmlAttribute]
                public string Color;
                [XmlAttribute]
                public string TextColor;
                [XmlAttribute]
                public string BorderColor;
                [XmlAttribute]
                public string BackgroundColor;
                [XmlAttribute]
                public string MouseOverColor;
                [XmlAttribute]
                public string MouseDownColor;
                [XmlAttribute]
                public string MouseOverTextColor;
                [XmlAttribute]
                public string TextOutlineColor;
                [XmlAttribute]
                public string SelectedColor;

                public Proxy ToProxy()
                {
                    return new Proxy(this);
                }

                /// <summary>
                /// Proxy Class of <see cref="Entities.Style"/> that has Nullable Colors.
                /// </summary>
                public class Proxy
                {
                    public UnityEngine.Color? DisabledColor;
                    public UnityEngine.Color? Color;
                    public UnityEngine.Color? TextColor;
                    public UnityEngine.Color? BorderColor;
                    public UnityEngine.Color? BackgroundColor;
                    public UnityEngine.Color? MouseOverColor;
                    public UnityEngine.Color? MouseDownColor;
                    public UnityEngine.Color? MouseOverTextColor;
                    public UnityEngine.Color? TextOutlineColor;
                    public UnityEngine.Color? SelectedColor;

                    public void Apply(Entities.Style Style)
                    {
                        Style.DisabledColor = (DisabledColor ?? Style.DisabledColor);
                        Style.Color = (Color ?? Style.Color);
                        Style.TextColor = (TextColor ?? Style.TextColor);
                        Style.BorderColor = (BorderColor ?? Style.BorderColor);
                        Style.BackgroundColor = (BackgroundColor ?? Style.BackgroundColor);
                        Style.MouseOverColor = (MouseOverColor ?? Style.MouseOverColor);
                        Style.MouseDownColor = (MouseDownColor ?? Style.MouseDownColor);
                        Style.MouseOverTextColor = (MouseOverTextColor ?? Style.MouseOverTextColor);
                        Style.TextOutlineColor = (TextOutlineColor ?? Style.TextOutlineColor);
                        Style.SelectedColor = (SelectedColor ?? Style.SelectedColor);
                    }

                    public void Apply(Proxy Proxy)
                    {
                        Proxy.DisabledColor = (DisabledColor ?? Proxy.DisabledColor);
                        Proxy.Color = (Color ?? Proxy.Color);
                        Proxy.TextColor = (TextColor ?? Proxy.TextColor);
                        Proxy.BorderColor = (BorderColor ?? Proxy.BorderColor);
                        Proxy.BackgroundColor = (BackgroundColor ?? Proxy.BackgroundColor);
                        Proxy.MouseOverColor = (MouseOverColor ?? Proxy.MouseOverColor);
                        Proxy.MouseDownColor = (MouseDownColor ?? Proxy.MouseDownColor);
                        Proxy.MouseOverTextColor = (MouseOverTextColor ?? Proxy.MouseOverTextColor);
                        Proxy.TextOutlineColor = (TextOutlineColor ?? Proxy.TextOutlineColor);
                        Proxy.SelectedColor = (SelectedColor ?? Proxy.SelectedColor);
                    }

                    public Proxy()
                    {
                    }

                    public Proxy(Sheet.Style Style)
                    {
                        DisabledColor = TryGetColorNullable(Style.DisabledColor, DisabledColor);
                        Color = TryGetColorNullable(Style.Color, Color);
                        TextColor = TryGetColorNullable(Style.TextColor, TextColor);
                        BorderColor = TryGetColorNullable(Style.BorderColor, BorderColor);
                        BackgroundColor = TryGetColorNullable(Style.BackgroundColor, BackgroundColor);
                        MouseOverColor = TryGetColorNullable(Style.MouseOverColor, MouseOverColor);
                        MouseDownColor = TryGetColorNullable(Style.MouseDownColor, MouseDownColor);
                        MouseOverTextColor = TryGetColorNullable(Style.MouseOverTextColor, MouseOverTextColor);
                        TextOutlineColor = TryGetColorNullable(Style.TextOutlineColor, TextOutlineColor);
                        SelectedColor = TryGetColorNullable(Style.SelectedColor, SelectedColor);
                    }

                    public UnityEngine.Color? TryGetColorNullable(string Key, UnityEngine.Color? Fallback)
                    {
                        if (!string.IsNullOrWhiteSpace(Key))
                        {
                            if (Key.StartsWith("Assign:"))
                            {
                                return TryGetColor(Key.ReplaceFirst("Assign:", string.Empty));
                            }
                            else if (Key.StartsWith("Parent:"))
                            {
                                Key = Key.ReplaceFirst("Parent:", string.Empty);
                                string Parent = Key.Remove(Key.LastIndexOf('.'));

                                if (Globals.Palettes.ContainsKey(Parent))
                                {
                                    Proxy Proxy = Globals.Palettes[Parent];
                                    string Field = Key.ReplaceFirst(Parent + '.', string.Empty);
                                    FieldInfo FieldInfo = Proxy.GetType().GetField(Field);

                                    if (FieldInfo != null)
                                    {
                                        return (UnityEngine.Color?)FieldInfo.GetValue(Proxy);
                                    }
                                    else
                                    {
                                        WriteLineError($"Globals.Style.Proxy.TryGetColorNullable : Tried to get Color from Parent Palette '{Parent}' with Key '{Key}' but Proxy Field '{Field}' didn't exist.");
                                    }
                                }
                                else
                                {
                                    WriteLineError($"Globals.Style.Proxy.TryGetColorNullable : Tried to get Color from Parent Palette '{Parent}' with Key '{Key}' but Parent didn't exist.");
                                }
                            }
                            else if (Globals.Colors.TryGetValue(Key, out UnityEngine.Color Value))
                            {
                                return Value;
                            }
                        }

                        return Fallback;
                    }
                }
            }
        }
    }
}
