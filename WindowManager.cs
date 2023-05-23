using BigMod.Entities.Windows;
using BigMod.Entities.Windows.Inspect;
using BigMod.Entities.Windows.Orders;
using RimWorld;
using Verse;

namespace BigMod.Entities
{
    [StaticConstructorOnStartup]
    public class WindowManager : MonoBehaviour
    {
        public List<WindowPanel> Windows = new List<WindowPanel>();
        public List<ToolTip> ToolTips = new List<ToolTip>();
        public DisplayWeather DisplayWeather;
        public DisplayDate DisplayDate;
        public Media Media;
        public Tray Tray;
        public Hotbar Hotbar;
        public Inspect Inspect;
        public Orders Orders;
        /// <summary>
        /// Used by <see cref="ListWindow.Search"/> and other TextInputs to only update filtering when the user has stopped typing.
        /// Only when the users hasn't typed something for this amount of time will the TextInput do its filtering.
        /// </summary>
        public static float TextInputDelay = 250f;
        public static float ScrollWheelScale = 1.0f;
        /// <summary>
        /// Double Clicks can only happen in this threshold after a Single Click.
        /// </summary>
        public static float DoubleClickThreshold = 0.35f;

        public static bool Last_MouseDownLeft;
        public static bool Last_MouseDownRight;
        /// <summary>
        /// The Mouse Position in the last frame, used by <see cref="MousePositionDelta"/>.
        /// </summary>
        public static Vector2 Last_MousePosition;
        /// <summary>
        /// If not null will move the camera whenever the target moves.
        /// </summary>
        public static RimWorld.Planet.GlobalTargetInfo AttachedTarget;
        /// <summary>
        /// Controls the Smoothness when the camera is moving.
        /// </summary>
        public static float SmoothTime = 0.3f;
        /// <summary>
        /// Velocity of the moving <see cref="AttachedTarget"/>, changes at runtime.
        /// </summary>
        private Vector3 Velocity = Vector3.zero;
        /// <summary>
        /// Last Position of <see cref="AttachedTarget"/>, used for Delta.
        /// </summary>
        private static Vector3 Last = Vector3.zero;
        public static GUISkin GUISkin;
        public static WindowManager Instance;
        public new static GameObject gameObject;

        public static WindowManager Create()
        {
            gameObject = new GameObject(nameof(WindowManager));
            DontDestroyOnLoad(gameObject);

            Instance = gameObject.AddComponent<WindowManager>();

            gameObject.SetActive(true);
            Instance.enabled = true;

            return Instance;
        }

        public void Initiate()
        {
            GUISkin = ScriptableObject.CreateInstance<GUISkin>();
            GUISkin.verticalScrollbar.normal.background = Globals.GetTextureGenerated("Skin.verticalScrollbar.normal.background");
            GUISkin.verticalScrollbar.active.background = Globals.GetTextureGenerated("Skin.verticalScrollbar.active.background");
            GUISkin.verticalScrollbarThumb.normal.background = Globals.GetTextureGenerated("Skin.verticalScrollbarThumb.normal.background");
            GUISkin.verticalScrollbarThumb.active.background = Globals.GetTextureGenerated("Skin.verticalScrollbarThumb.active.background");
            GUISkin.horizontalScrollbar.normal.background = Globals.GetTextureGenerated("Skin.horizontalScrollbar.normal.background");
            GUISkin.horizontalScrollbar.active.background = Globals.GetTextureGenerated("Skin.horizontalScrollbar.active.background");
            GUISkin.horizontalScrollbarThumb.normal.background = Globals.GetTextureGenerated("Skin.horizontalScrollbarThumb.normal.background");
            GUISkin.horizontalScrollbarThumb.active.background = Globals.GetTextureGenerated("Skin.horizontalScrollbarThumb.active.background");

            GUISkin.verticalScrollbar.fixedWidth = 14f;
            GUISkin.verticalScrollbarThumb.fixedWidth = 14f;

            GUISkin.horizontalScrollbar.fixedHeight = 14f;
            GUISkin.horizontalScrollbarThumb.fixedHeight = 14f;

            // Based on original, otherwise we get UnityEngine.GUI.HandleTextFieldEventForDesktop exception
            GUISkin.textField = new GUIStyle(Text.fontStyles[(int)FontStyle.Normal]);
            GUISkin.textField.normal.background = Globals.GetTextureGenerated("Skin.textField.normal.background");
            GUISkin.textField.active.background = Globals.GetTextureGenerated("Skin.textField.active.background");
            GUISkin.textField.focused.background = Globals.GetTextureGenerated("Skin.textField.focused.background");
            GUISkin.textField.normal.textColor = Globals.GetColor("Skin.textField.normal.textColor");
            GUISkin.textField.active.textColor = Globals.GetColor("Skin.textField.active.textColor");
            GUISkin.textField.focused.textColor = Globals.GetColor("Skin.textField.focused.textColor");

            GUISkin.textField.alignment = TextAnchor.MiddleLeft;
            GUISkin.textField.wordWrap = false;

            GUISkin.label.clipping = TextClipping.Clip;
            GUISkin.button.clipping = TextClipping.Clip;

            DisplayWeather = new DisplayWeather();
            DisplayDate = new DisplayDate();
            Media = new Media();
            Tray = new Tray();
            Hotbar = new Hotbar();
            Inspect = new Inspect();
            Orders = new Orders();

            foreach (MainButtonDef Tab in DefDatabase<MainButtonDef>.AllDefs)
            {
                // Hide the original TabButtons
                Tab.buttonVisible = false;
            }

            // Remove vanilla keybind
            MainButtonDefOf.Architect.defaultHotKey = KeyCode.None;
            MainButtonDefOf.Architect.hotKey = null;

            Find.WindowStack.TryRemove(Find.MainButtonsRoot.GetType());
            Find.WindowStack.TryRemove(Find.ColonistBar.GetType());
            // Removing the bar as above doesn't always hide it
            Find.PlaySettings.showColonistBar = false;
            // Activates the disabling override in BigMod.ResourceReadout_RootThingCategories
            Prefs.ResourceReadoutCategorized = true;

            // Always use Work priority numbers
            Find.PlaySettings.useWorkPriorities = true;

            // Disable Force to Normal Speed
            DebugViewSettings.neverForceNormalSpeed = true;

            // Allow closer zoom in, previously 11
            Find.CameraDriver.config.sizeRange.min = 5f;
            // Allow further zoom out, previously 60
            Find.CameraDriver.config.sizeRange.max = 180f;

            OpenWindow(DisplayWeather);
            OpenWindow(DisplayDate);
            OpenWindow(Media);
            OpenWindow(Tray);
            OpenWindow(Hotbar);
            OpenWindow(Inspect);
            OpenWindow(new Pawns());
            OpenWindow(Orders);
            OpenWindow(new Windows.Resources.Resources());

            // TODO: Debugging
            OpenWindow(new Windows.Overview.Overview());
        }

        /// <summary>
        /// Updates the WindowManager, Windows are not updated here, they're updated by the vanilla WindowStack somewhere.
        /// </summary>
        public void Update()
        {
            Last_MouseDownLeft = Input.GetKey(KeyCode.Mouse0);
            Last_MouseDownRight = Input.GetKey(KeyCode.Mouse1);
            Last_MousePosition = GetMousePosition();

            if (AttachedTarget != default)
            {
                // TODO: Cancel buttons have to be hotkey'd instead with WASD also allowing it to stop the follow, and a follow hotkey and a button to follow
                if ((Input.mouseScrollDelta == Vector2.zero) && !KeyBindingDefOf.Cancel.KeyDownEvent && !Input.GetMouseButtonDown(2) && !Input.GetKeyDown(KeyCode.Escape))
                {
                    Find.CameraDriver.transform.position = Vector3.SmoothDamp(Last, new Vector3(AttachedTarget.Thing.DrawPos.x, Find.CameraDriver.transform.position.y, AttachedTarget.Thing.DrawPos.z), ref Velocity, SmoothTime);
                    Last = new Vector3(AttachedTarget.Thing.DrawPos.x, Find.CameraDriver.transform.position.y, AttachedTarget.Thing.DrawPos.z);
                }
                else
                {
                    Find.CameraDriver.JumpToCurrentMapLoc(Find.CameraDriver.transform.position);
                    DetachCamera();
                }
            }
        }

        /// <summary>
        /// Attaches the Camera to the specified target, making the Camera follow it smoothly.
        /// </summary>
        /// <param name="GlobalTargetInfo">Target to attach camera to.</param>
        /// <param name="TryJumpAndSelect">Optional should use <see cref="CameraJumper.TryJumpAndSelect()"/> instead of <see cref="CameraJumper.TryJump()"/>.</param>
        public static void AttachCameraToTarget(RimWorld.Planet.GlobalTargetInfo GlobalTargetInfo, bool TryJumpAndSelect = true)
        {
            AttachedTarget = GlobalTargetInfo;
            // Have to use the existing Camera y to get us into the right layer depth.
            Last = new Vector3(AttachedTarget.Thing.DrawPos.x, Find.CameraDriver.transform.position.y, AttachedTarget.Thing.DrawPos.z);

            if (TryJumpAndSelect)
            {
                CameraJumper.TryJumpAndSelect(AttachedTarget);
            }
            else
            {
                CameraJumper.TryJump(AttachedTarget);
            }
        }

        /// <summary>
        /// Removes the attached target, stopping the camera follow.
        /// </summary>
        public static void DetachCamera()
        {
            AttachedTarget = default;
        }

        #region "Window Manipulation"

        /// <summary>
        /// Toggles the specified Window.
        /// </summary>
        /// <param name="Window">Window to Toggle.</param>
        public static void ToggleWindow(Window Window)
        {
            if (!Find.WindowStack.TryRemove(Window))
            {
                Find.WindowStack.Add(Window);
            }
        }

        /// <summary>
        /// Tries to Toggle the specified WindowPanel Type, if it doesn't exist it will be created, if it does exist it will be closed.
        /// </summary>
        /// <typeparam name="T">WindowPanel Type to Toggle.</typeparam>
        /// <returns>True if Window existed and was closed, false if Window was created and opened.</returns>
        public static bool TryToggleWindow<T>() where T : WindowPanel
        {
            T Window = Instance.Windows.OfType<T>().FirstOrDefault();

            if (Window != null)
            {
                CloseWindow(Window);
                return true;
            }

            OpenWindow(Activator.CreateInstance<T>());

            return false;
        }

        public static void OpenWindow(WindowPanel Window)
        {
            Instance.Windows.Add(Window);
            Find.WindowStack.Add(Window);
        }

        public static T OpenWindow<T>() where T : WindowPanel
        {
            T Window = Activator.CreateInstance<T>();

            OpenWindow(Window);

            return Window;
        }

        public static void CloseWindow(WindowPanel Window, bool DoSound = false)
        {
            Instance.Windows.Remove(Window);
            Find.WindowStack.TryRemove(Window, DoSound);
        }

        public static void CloseWindow<T>() where T : WindowPanel
        {
            Instance.Windows.OfType<T>().FirstOrDefault()?.Close();
        }

        /// <summary>
        /// Prevents Windows from lingering in the <see cref="Windows"/> list when closed by the vanilla game or other mods.
        /// </summary>
        /// <param name="Window">Window to open.</param>
        public static void OpenWindowVanilla(Window Window)
        {
            Find.WindowStack.Add(Window);
        }

        /// <summary>
        /// Prevents Windows from lingering in the <see cref="Windows"/> list when closed by the vanilla game or other mods.
        /// </summary>
        /// <param name="Window">Window to close.</param>
        /// <param name="DoSound">Should play closing sound.</param>
        public static void CloseWindowVanilla(Window Window, bool DoSound = false)
        {
            Find.WindowStack.TryRemove(Window, DoSound);
        }

        /// <summary>
        /// Returns the first WindowPanel of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of window to find.</typeparam>
        /// <param name="Window">Window instance.</param>
        /// <returns>True if Window if was found.</returns>
        public static bool TryGetWindow<T>(out T Window) where T : WindowPanel
        {
            Window = Instance.Windows.OfType<T>().FirstOrDefault();

            if (Window != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the first window of the specified type.
        /// </summary>
        /// <param name="Type">Type of window to find.</typeparam>
        /// <param name="Window">Window instance.</param>
        /// <returns>True if Window if was found.</returns>
        public static bool TryGetWindowVanilla(Type Type, out Window Window)
        {
            Window = Find.WindowStack.Windows.FirstOrDefault((F) => F.GetType() == Type);

            if (Window != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to Toggle the specified WindowPanel Type, if it doesn't exist it will be created, if it does it will be closed.
        /// </summary>
        /// <param name="Type">WindowPanel Type to Toggle.</typeparam>
        /// <param name="Parameters">Optional Parameters to use when a new window needs to be created.</param>
        /// <returns>True if Window existed and was closed, false if Window was created and opened.</returns>
        public static bool TryToggleWindowVanilla(Type Type, params object[] Parameters)
        {
            TryGetWindowVanilla(Type, out Window Window);

            if (Window != null)
            {
                CloseWindowVanilla(Window);
                return true;
            }

            OpenWindowVanilla((Window)Activator.CreateInstance(Type, Parameters));

            return false;
        }

        /// <summary>
        /// Tries to Toggle the specified WindowPanel Type, if it doesn't exist it will be created, if it does it will be closed.
        /// </summary>
        /// <typeparam name="T">>WindowPanel Type to Toggle.</typeparam>
        /// <returns>True if Window existed and was closed, false if Window was created and opened.</returns>
        /// <param name="Parameters">Optional Parameters to use when a new window needs to be created.</param>
        public static bool TryToggleWindowVanilla<T>(params object[] Parameters)
        {
            return TryToggleWindowVanilla(typeof(T), Parameters);
        }

        /// <summary>
        /// Gets a list of all WindowPanels of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of Windows to retrieve.</typeparam>
        /// <returns>List of all WindowPanels of type <typeparamref name="T"/>.</returns>
        public static List<T> GetWindows<T>() where T : WindowPanel
        {
            return Instance.Windows.OfType<T>().ToList();
        }

        /// <summary>
        /// Get all WindowPanels with the specified Identifier.
        /// </summary>
        /// <param name="Identifier">String Identifier to filter with.</param>
        /// <returns>List of WindowPanels with the specified Identifier.</returns>
        public static List<WindowPanel> GetWindowsWithIdentifier(string Identifier)
        {
            return Instance.Windows.Where((F) => F.Identifier == Identifier).ToList();
        }

        /// <summary>
        /// Gets a list of all Entities that are underneath the mouse cursor.
        /// </summary>
        /// <returns>A list of Entities underneath the mouse cursor through multiple windows, regardless of if can or cannot get mouse inputs.</returns>
        public static List<Panel> GetEntitiesUnderMouse()
        {
            List<Panel> Entities = new List<Panel>();

            foreach (WindowPanel Window in BigMod.WindowManager.Windows)
            {
                Window.Root.GetMouseOver(GetMousePosition() - Window.Position);

                Entities.AddRange(Window.Root.GetChildrenFlatten().Where((F) => F.IsMouseOver));
            }

            return Entities;
        }

        /// <summary>
        /// If the game is currently Forced Pause because of a Window.
        /// </summary>
        /// <returns>True if a Window exists that Force Pauses the game.</returns>
        public static bool IsForcedPause()
        {
            return Find.WindowStack.WindowsForcePause;
        }


        #endregion "Window Manipulation"

        #region "ToolTip Manipulation"

        public void AddToolTip(ToolTip ToolTip)
        {
            ToolTips.Add(ToolTip);
            Find.WindowStack.Add(ToolTip);
        }

        public void RemoveToolTip(ToolTip ToolTip)
        {
            ToolTips.Remove(ToolTip);
            Find.WindowStack.TryRemove(ToolTip, false);
        }

        public void RemoveToolTips(string ID)
        {
            for (int i = ToolTips.Count; i-- > 0;)
            {
                if (ToolTips[i].Identifier == ID)
                {
                    RemoveToolTip(ToolTips[i]);
                }
            }
        }

        public void ClearToolTips()
        {
            foreach (ToolTip ToolTip in ToolTips)
            {
                RemoveToolTip(ToolTip);
            }
        }

        public bool ToolTipExists(string ID)
        {
            return ToolTips.Any((F) => F.Identifier == ID);
        }

        public bool ToolTipExists(ToolTip ToolTip)
        {
            return ToolTips.Contains(ToolTip);
        }

        public ToolTip GetToolTip(string ID)
        {
            return ToolTips.FirstOrDefault((F) => F.Identifier == ID);
        }

        public ToolTip GetToolTipFromHost(Rect Host)
        {
            foreach (ToolTip ToolTip in ToolTips)
            {
                if (ToolTip.ToolTipHostRect == Host)
                {
                    return ToolTip;
                }
            }

            return null;
        }

        public ToolTip GetToolTipFromHost(Panel Host)
        {
            foreach (ToolTip ToolTip in ToolTips)
            {
                if (ToolTip.ToolTipHost == Host)
                {
                    return ToolTip;
                }
            }

            return null;
        }

        #endregion "ToolTip Manipulation"

        #region "Input"

        /// <summary>
        /// Is the Left Mouse Button freshly pressed down?
        /// </summary>
        /// <returns>True if the Left Mouse Button is pressed down but was up in the previous frame.</returns>
        public static bool IsMouseClick()
        {
            return Input.GetMouseButtonDown(0);
        }

        /// <summary>
        /// Is the Right Mouse Button freshly pressed down?
        /// </summary>
        /// <returns>True when the Right Mouse Button is pressed down but was up in the previous frame.</returns>
        public static bool IsMouseClickRight()
        {
            return Input.GetMouseButtonDown(1);
        }

        /// <summary>
        /// Is the Middle Mouse Button freshly pressed down?
        /// </summary>
        /// <returns>True when the Middle Mouse Button is pressed down but was up in the previous frame.</returns>
        public static bool IsMouseClickMiddle()
        {
            return Input.GetMouseButtonDown(2);
        }

        /// <summary>
        /// Is the Middle Mouse Button currently down?
        /// </summary>
        /// <returns>True if the Middle Mouse Button is pressed down.</returns>
        public static bool IsMouseDownMiddle()
        {
            return Input.GetMouseButton(2);
        }

        /// <summary>
        /// Is the Left Mouse Button freshly pressed down?
        /// </summary>
        /// <returns>True when the Left Mouse Button is pressed down but was up in the previous frame.</returns>
        public static bool IsMouseDown()
        {
            return Input.GetKeyDown(KeyCode.Mouse0);
        }

        /// <summary>
        /// Is the Left Mouse Button released after being down?
        /// </summary>
        /// <returns>True when the Left Mouse Button has been released.</returns>
        public static bool IsMouseUp()
        {
            return Input.GetKeyUp(KeyCode.Mouse0);
        }

        /// <summary>
        /// Is the Right Mouse Button freshly pressed down?
        /// </summary>
        /// <returns>True when the Right Mouse Button is pressed down but was up in the previous frame.</returns>
        public static bool IsMouseDownRight()
        {
            return Input.GetKeyDown(KeyCode.Mouse1);
        }

        /// <summary>
        /// Is the Right Mouse Button released after being down?
        /// </summary>
        /// <returns>True when the Right Mouse Button has been released.</returns>
        public static bool IsMouseUpRight()
        {
            return Input.GetKeyUp(KeyCode.Mouse1);
        }

        /// <summary>
        /// Is the Middle Mouse Button released after being down?
        /// </summary>
        /// <returns>True when the Middle Mouse Button has been released.</returns>
        public static bool IsMouseUpMiddle()
        {
            return Input.GetKeyUp(KeyCode.Mouse2);
        }

        /// <summary>
        /// Is Left Mouse Button currently held down?
        /// </summary>
        /// <returns>True while the Left Mouse Button is pressed Down.</returns>
        public static bool IsMouseDownCurrently()
        {
            return Input.GetKey(KeyCode.Mouse0);
        }

        /// <summary>
        /// Is Right Mouse Button currently held down?
        /// </summary>
        /// <returns>True while the Right Mouse Button is pressed Down.</returns>
        public static bool IsMouseDownRightCurrently()
        {
            return Input.GetKey(KeyCode.Mouse1);
        }

        /// <summary>
        /// Is Left Mouse Button pressed down and was it down in the previous frame?
        /// </summary>
        /// <returns>True if Left Mouse Button is currently down and was down in the previous frame.</returns>
        public static bool IsMouseDownContinuous()
        {
            return (Last_MouseDownLeft && Input.GetMouseButton(0));
        }

        /// <summary>
        /// Is Any Mouse Button freshly pressed down?
        /// </summary>
        /// <returns>True when the Any Mouse Button is pressed down but was up in the previous frame.</returns>
        public static bool IsMouseDownAny()
        {
            return (IsMouseDown() || IsMouseDownMiddle() || IsMouseDownRight());
        }

        /// <summary>
        /// Is Any Mouse Button released after being down?
        /// </summary>
        /// <returns>True when the Any Mouse Button is pressed down but was up in the previous frame.</returns>
        /// <returns>True when Any Mouse Button has been released.</returns>
        public static bool IsMouseUpAny()
        {
            return (IsMouseUp() || IsMouseUpMiddle() || IsMouseUpRight());
        }

        /// <summary>
        /// Is Shift pressed down?
        /// </summary>
        /// <returns>True if Shift is currently pressed down.</returns>
        public static bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift);
        }

        /// <summary>
        /// Is Ctrl pressed down?
        /// </summary>
        /// <returns>True if Ctrl is currently pressed down.</returns>
        public static bool IsCtrlDown()
        {
            return Input.GetKey(KeyCode.LeftControl);
        }

        /// <summary>
        /// Is Alt pressed down?
        /// </summary>
        /// <returns>True if Alt is currently pressed down.</returns>
        public static bool IsAltDown()
        {
            return Input.GetKey(KeyCode.LeftAlt);
        }

        /// <summary>
        /// Get Scroll Wheel Delta, non-zero values mean the Wheel has been turned.
        /// </summary>
        /// <returns>Scroll Wheel Delta with Scaling applied.</returns>
        public static float MouseWheelDelta()
        {
            return (Input.mouseScrollDelta.y * ScrollWheelScale);
        }

        /// <summary>
        /// Get the Mouse movement delta between this frame and the last frame. Uses <see cref="UI.MousePositionOnUIInverted"/>.
        /// </summary>
        /// <returns>Delta Mouse movement.</returns>
        public static Vector2 MousePositionDelta()
        {
            return (Last_MousePosition - GetMousePosition());
        }

        /// <summary>
        /// Get the Mouse position. Uses <see cref="UI.MousePositionOnUIInverted"/>.
        /// </summary>
        /// <remarks>Use this instead of using <see cref="UI.MousePositionOnUIInverted"/> directly, so it can be changed easily throughout the mod if need be.</remarks>
        /// <returns>Absolute mouse position on screen from Top-Left corner.</returns>
        public static Vector2 GetMousePosition()
        {
            return UI.MousePositionOnUIInverted;
        }

        #endregion "Input"
    }

    [DefOf]
    public static class KeyBindings
    {
        public static KeyBindingDef WindowDragging;
        public static KeyBindingDef WindowLocking;
    }

    public enum MouseInput
    {
        Empty,
        IsMouseClick,
        IsMouseClickDouble,
        IsMouseClickRight,
        IsMouseDown,
        IsMouseUp,
        IsMouseDownCurrently,
        IsMouseDownContinuous,
        IsMouseWheel
    }

    public class MouseEventArgs : EventArgs
    {
        public MouseInput MouseInput = MouseInput.Empty;
        public Vector2 DeltaVector;
        public float Delta;

        public MouseEventArgs(MouseInput MouseInput)
        {
            this.MouseInput = MouseInput;
        }

        public MouseEventArgs(MouseInput MouseInput, Vector2 DeltaVector, float Delta)
        {
            this.MouseInput = MouseInput;
            this.DeltaVector = DeltaVector;
            this.Delta = Delta;
        }

        public static MouseEventArgs GetMouseWheel()
        {
            Vector2 DeltaVector;
            float Delta = WindowManager.MouseWheelDelta();

            if (!WindowManager.IsShiftDown())
            {
                DeltaVector = new Vector2(0f, WindowManager.MouseWheelDelta());
            }
            else
            {
                // Shift Changes MouseWheel Delta from Being Y based to X based.
                DeltaVector = new Vector2(WindowManager.MouseWheelDelta(), 0f);
            }

            return new MouseEventArgs(MouseInput.IsMouseWheel, DeltaVector, Delta);
        }

        // Instead of creating new ones each time, just use these
        public new static readonly MouseEventArgs Empty = new MouseEventArgs(MouseInput.Empty);
        public static readonly MouseEventArgs IsMouseClick = new MouseEventArgs(MouseInput.IsMouseClick);
        public static readonly MouseEventArgs IsMouseClickDouble = new MouseEventArgs(MouseInput.IsMouseClickDouble);
        public static readonly MouseEventArgs IsMouseClickRight = new MouseEventArgs(MouseInput.IsMouseClickRight);
        public static readonly MouseEventArgs IsMouseDown = new MouseEventArgs(MouseInput.IsMouseDown);
        public static readonly MouseEventArgs IsMouseUp = new MouseEventArgs(MouseInput.IsMouseUp);
        public static readonly MouseEventArgs IsMouseDownCurrently = new MouseEventArgs(MouseInput.IsMouseDownCurrently);
        public static readonly MouseEventArgs IsMouseDownContinuous = new MouseEventArgs(MouseInput.IsMouseDownContinuous);
        public static readonly MouseEventArgs IsMouseWheel = new MouseEventArgs(MouseInput.IsMouseWheel);
    }
}
