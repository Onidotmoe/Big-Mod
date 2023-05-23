using RimWorld;
using System.Reflection;
using Verse;

namespace BigMod.Entities
{
    /// <summary>
    /// A intermediary class to allow our Entity system to interact with the game's GUI.
    /// </summary>
    public class WindowPanel : Window
    {
        #region "Positioning & Sizing"

        public virtual Rect DefaultBounds { get; set; } = new Rect(100, 100, 50, 50);

        /// <summary>
        /// Bounding Box of this Entity.
        /// </summary>
        /// <remarks>Does not trigger <see cref="Root.OnSizeChanged"/> and <see cref="Root.DoOnBoundsChanged"/> when changed.</remarks>
        public Rect Bounds
        {
            get
            {
                return windowRect;
            }
            set
            {
                windowRect = value;

                Root.Size = Size;
            }
        }
        public virtual float X
        {
            get
            {
                return windowRect.x;
            }
            set
            {
                windowRect.x = value;

                Root.DoOnPositionChanged(Root, EventArgs.Empty);
                Root.DoOnBoundsChanged(Root, EventArgs.Empty);
            }
        }
        public virtual float Y
        {
            get
            {
                return windowRect.y;
            }
            set
            {
                windowRect.y = value;

                Root.DoOnPositionChanged(Root, EventArgs.Empty);
                Root.DoOnBoundsChanged(Root, EventArgs.Empty);
            }
        }
        public virtual float Width
        {
            get
            {
                return windowRect.width;
            }
            set
            {
                windowRect.width = value;

                Root.Width = value;
            }
        }
        public virtual float Height
        {
            get
            {
                return Bounds.height;
            }
            set
            {
                windowRect.height = value;

                Root.Height = value;
            }
        }
        public Vector2 Size
        {
            get
            {
                return windowRect.size;
            }
            set
            {
                windowRect.size = value;

                Root.Size = value;
            }
        }
        public virtual Vector2 Position
        {
            get
            {
                return windowRect.position;
            }
            set
            {
                windowRect.position = value;

                Root.DoOnPositionChanged(Root, EventArgs.Empty);
                Root.DoOnBoundsChanged(Root, EventArgs.Empty);
            }
        }
        public virtual float Bottom
        {
            get
            {
                return (Y + Height);
            }
        }
        public virtual float Right
        {
            get
            {
                return (X + Width);
            }
        }
        private bool _IsResizable = true;
        /// <summary>
        /// Use this instead of <see cref="Window.resizeable"/>.
        /// </summary>
        public virtual bool IsResizable
        {
            get
            {
                return (_IsResizable && !IsLocked);
            }
            set
            {
                _IsResizable = value;
            }
        }
        /// <summary>
        /// Is this window currently being resized? Used to block mouse cascading to children.
        /// </summary>
        public virtual bool IsResizing { get; set; }
        /// <summary>
        /// Is this window currently being dragged? Used to block mouse cascading to children.
        /// </summary>
        public virtual bool IsDragging { get; set; }
        private bool _IsLocked;
        /// <summary>
        /// Is this window currently locked? Locked windows can't be moved or resized and have their additional buttons hidden.
        /// </summary>
        public virtual bool IsLocked
        {
            get
            {
                return _IsLocked;
            }
            set
            {
                if (_IsLocked != value)
                {
                    _IsLocked = value;
                    UpdateLocking();
                }
            }
        }
        /// <summary>
        /// Can this window be locked? Locked windows can't be moved or resized and have their additional buttons hidden.
        /// </summary>
        public bool IsLockable;
        /// <summary>
        /// Used to store the start rectangle when resizing.
        /// </summary>
        public Rect ResizeStart;

        #endregion "Positioning & Sizing"

        public Panel Root;
        /// <summary>
        /// Set by <see cref="Window.IsOpen"/> in <see cref="Update"/>.
        /// </summary>
        public virtual bool IsVisible { get; set; }
        private bool _IsMouseOver;
        public virtual bool IsMouseOver
        {
            get
            {
                return _IsMouseOver;
            }
            set
            {
                _IsMouseOver = value;
                preventCameraMotion = (value && !CameraMotion);
            }
        }
        /// <summary>
        /// Won't monitor mouse position or update related methods.
        /// </summary>
        public virtual bool IgnoreMouseInput { get; set; }
        /// <summary>
        /// Toggle Passthrough, checks if any Events in <see cref="Passthrough_EventTypes"/> match the current Event, if it does it will Use it, preventing other stuff from using it. Only used by <see cref="InnerWindowOnGUI"/>.
        /// </summary>
        public virtual bool Passthrough_Active { get; set; }
        /// <summary>
        /// Used to Block Passthrough on the current Update cycle, is reset each Update.
        /// </summary>
        public virtual bool Passthrough_Blocked { get; set; }
        /// <summary>
        /// HashSet of EventTypes to consume Passthroughs.
        /// </summary>
        /// <remarks>HashSet insures no duplicate entries.</remarks>
        public virtual HashSet<EventType> Passthrough_EventTypes { get; set; }
        /// <summary>
        /// Usually located in the BottomCenter of the window.
        /// </summary>
        public virtual Button ButtonClose { get; set; }
        /// <summary>
        /// Usually located in the TopRight corner of the window.
        /// </summary>
        public virtual Button ButtonCloseX { get; set; }
        /// <summary>
        /// Usually located in the BottomRight corner of the window.
        /// </summary>
        public virtual Button ButtonResize { get; set; }
        /// <summary>
        /// Usually located in the BottomRight corner of the window.
        /// </summary>
        public virtual Image ToolTipIcon { get; set; }
        /// <summary>
        /// Use this instead of <see cref="Window.doWindowBackground"/>.
        /// </summary>
        /// <remarks>Sugarcoat to access <see cref="Root.Style.DrawBackground"/>.</remarks>
        public virtual bool DrawBackground
        {
            get
            {
                return Root.Style.DrawBackground;
            }
            set
            {
                Root.Style.DrawBackground = value;
            }
        }
        public virtual bool DrawBorder
        {
            get
            {
                return Root.Style.DrawBorder;
            }
            set
            {
                Root.Style.DrawBorder = value;
            }
        }
        public virtual bool CurrentlyOpen { get; set; }

        /// <summary>
        /// Show <see cref="ButtonCloseX"/> be visible only when the mouse is over this Window?
        /// </summary>
        public virtual bool VisibleOnMouseOverOnly_ButtonCloseX { get; set; }
        /// <summary>
        /// Show <see cref="ButtonResize"/> be visible only when the mouse is over this Window?
        /// </summary>
        public virtual bool VisibleOnMouseOverOnly_ButtonResize { get; set; }
        /// <summary>
        /// String identifier.
        /// </summary>
        public string Identifier;
        private bool _IsDraggable;
        /// <summary>
        /// Sets whether this window can be dragged around. Use this instead of <see cref="Window.draggable"/>.
        /// </summary>
        public virtual bool IsDraggable
        {
            get
            {
                return (_IsDraggable && !IsLocked);
            }
            set
            {
                _IsDraggable = value;
            }
        }
        private bool _CanReset;
        /// <summary>
        /// Can this Window have its Bounds Reset when click with Shift+Ctrl+LeftMouse
        /// </summary>
        public virtual bool CanReset
        {
            get
            {
                return _CanReset;
            }
            set
            {
                if (_CanReset != value)
                {
                    _CanReset = value;

                    if (value)
                    {
                        Root.OnClick += OnClick_ResetBounds;
                    }
                    else
                    {
                        Root.OnClick -= OnClick_ResetBounds;
                    }
                }
            }
        }
        private bool _CanCloseRightClick;
        /// <summary>
        /// Can this Window be closed when using Ctrl+Alt+RightMouse
        /// </summary>
        public virtual bool CanCloseRightClick
        {
            get
            {
                return _CanCloseRightClick;
            }
            set
            {
                if (_CanCloseRightClick != value)
                {
                    _CanCloseRightClick = value;

                    if (value)
                    {
                        Root.OnClickRight += OnClickRight_Close;
                    }
                    else
                    {
                        Root.OnClickRight -= OnClickRight_Close;
                    }
                }
            }
        }
        private Vector2 MouseLast = Vector2.zero;
        private bool _CameraMotion = true;
        /// <summary>
        /// Should the Camera be moveable while MouseOver?
        /// </summary>
        /// <remarks>Use this instead of <see cref="preventCameraMotion"/>.</remarks>
        public bool CameraMotion
        {
            get
            {
                return _CameraMotion;
            }
            set
            {
                _CameraMotion = value;
                preventCameraMotion = !value;
            }
        }
        /// <summary>
        /// Toggle whether using Mouse Scroll over this Window will cause the camera to zoom in or if that behavior should be blocked.
        /// </summary>
        public bool CameraMouseOverZooming = true;
        /// <summary>
        /// Allows overriding the vanilla Window innerWindowOnGUICached, use <see cref="Override_InnerWindowOnGuiCached"/> to initiate the override.
        /// </summary>
        public GUI.WindowFunction InnerWindowOnGUICached;
        public WindowPanel(Rect DefaultBounds = default)
        {
            // Don't draw the vanilla window
            doWindowBackground = false;
            layer = WindowLayer.GameUI;
            preventCameraMotion = false;
            // The vanilla close button will often be in a bad position or on the wrong layer, use AddButtonCloseX() to add the button correctly.
            doCloseX = false;
            // We won't be using the vanilla resizer
            resizeable = false;
            // We won't be using the vanilla dragging
            draggable = false;
            // Disable the background shadow by default
            DrawBackgroundShadow = false;

            AllowMultipleInstance = false;
            IsDraggable = true;
            IsLockable = true;

            this.DefaultBounds = ((DefaultBounds != default) ? DefaultBounds : this.DefaultBounds);
            Root = new Panel(this.DefaultBounds.size);
            windowRect = this.DefaultBounds;

            Root.ParentWindow = this;
            Root.MinSize = new Vector2(50, 50);
            Root.UseAnchoring = false;

            Root.Style.PaletteFallback = "WindowPanel";
            // Will get the Name of the derived classes too!
            Root.SetStyle(GetType().Name);
            Root.Style.BorderThickness = 3;

            DrawBackground = true;
            DrawBorder = true;

            CanReset = true;
        }

        /// <summary>
        /// Updates the Bounds and Handles input of itself and its children.
        /// </summary>
        /// <remarks>Called each frame before <see cref="Draw"/>.</remarks>
        public virtual void Update()
        {
            if (IsVisible)
            {
                Passthrough_Blocked = false;

                if (!IgnoreMouseInput)
                {
                    if (IsResizing)
                    {
                        DoResize();
                    }
                    else if (IsDragging)
                    {
                        DoDragging();
                    }
                    else if (GetMouseOver())
                    {
                        if (IsDraggable && KeyBindings.WindowDragging.IsDown)
                        {
                            IsDragging = true;
                            // Prevent the camera from moving around while dragging windows
                            preventCameraMotion = true;
                            MouseLast = WindowManager.GetMousePosition();
                            return;
                        }

                        if (IsLockable && KeyBindings.WindowLocking.JustPressed)
                        {
                            IsLocked = !IsLocked;
                        }

                        // Has to cascade MouseOver to its children
                        Root.GetMouseOver(WindowManager.GetMousePosition() - Position);

                        if (WindowManager.IsMouseDown())
                        {
                            Root.DoOnMouseDown(this, MouseEventArgs.IsMouseDown);
                        }

                        if (WindowManager.MouseWheelDelta() != 0)
                        {
                            Root.DoOnMouseWheel(this, MouseEventArgs.GetMouseWheel());
                        }

                        if (WindowManager.IsMouseClick())
                        {
                            Root.DoOnClick(this, MouseEventArgs.IsMouseClick);
                        }
                        else if (WindowManager.IsMouseDownContinuous())
                        {
                            Root.DoOnWhileMouseDown(this, MouseEventArgs.IsMouseDownContinuous);
                        }
                        else if (WindowManager.IsMouseClickRight())
                        {
                            Root.DoOnClickRight(this, MouseEventArgs.IsMouseClickRight);
                        }
                        else if (WindowManager.IsMouseUp() || WindowManager.IsMouseUpRight())
                        {
                            Root.DoOnMouseUp(this, MouseEventArgs.IsMouseUp);
                        }
                    }
                    else
                    {
                        IsResizing = false;
                        IsDragging = false;
                        IsMouseOver = false;
                        Root.IsMouseOver = false;
                    }
                }
                else
                {
                    // Restore settings for Zooming and Camera Movement while not mouseover
                    preventCameraMotion = CameraMotion;
                }

                Root.Update();
            }
        }

        /// <summary>
        /// Draws itself and its children.
        /// </summary>
        /// <remarks>Called each frame after <see cref="Update"/>.</remarks>
        public virtual void Draw()
        {
            // Unity updates OnGUI twice every frame, here we only want the part where it draws the layout, which is EventType.Repaint.
            if (Event.current.type == EventType.Layout)
            {
                return;
            }

            if (IsVisible)
            {
                GUI.skin = WindowManager.GUISkin;

                Root.Draw();

                // Prevent scrolling from zooming in and out while MouseOver.
                if (IsMouseOver && !CameraMouseOverZooming && (Event.current.type == EventType.ScrollWheel))
                {
                    Event.current.Use();
                }
            }
        }

        public bool GetMouseOver()
        {
            if (Find.WindowStack.GetWindowAt(WindowManager.GetMousePosition()) == this)
            {
                IsMouseOver = true;
                return true;
            }

            IsMouseOver = false;
            return false;
        }

        #region "Optional Buttons"

        /// <summary>
        /// Adds the vanilla close button x to the TopRight of the window.
        /// </summary>
        /// <remarks>Call this after you're done adding all Entities.</remarks>
        public virtual void AddButtonCloseX()
        {
            ButtonCloseX = new Button(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("CloseX"));
            ButtonCloseX.Size = new Vector2(18f, 18f);
            ButtonCloseX.Anchor = Anchor.TopRight;
            ButtonCloseX.IsVisible = !VisibleOnMouseOverOnly_ButtonCloseX;
            ButtonCloseX.OnClick += (obj, e) => Close(true);

            if (VisibleOnMouseOverOnly_ButtonCloseX)
            {
                Root.OnMouseEnter += (obj, e) => ButtonCloseX.IsVisible = true;
                Root.OnMouseLeave += (obj, e) => ButtonCloseX.IsVisible = false;
            }

            Root.AddChild(ButtonCloseX);
        }

        /// <summary>
        /// Adds the vanilla close button to the BottomCenter of the window.
        /// </summary>
        /// <remarks>Call this after you're done adding all Entities.</remarks>
        public virtual void AddButtonClose()
        {
            ButtonClose = new Button();
            ButtonClose.Size = new Vector2(100f, 20f);
            ButtonClose.Text = "CloseButton".Translate();
            ButtonClose.Anchor = Anchor.BottomCenter;
            ButtonClose.Style.DrawBackground = true;

            ButtonClose.OnClick += (obj, e) => Close(true);

            Root.AddChild(ButtonClose);
        }

        /// <summary>
        /// Adds the vanilla resize button to the BottomRight of the window.
        /// </summary>
        /// <remarks>Call this after you're done adding all Entities.</remarks>
        public virtual void AddButtonResize()
        {
            ButtonResize = new Button(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("Resize"));
            ButtonResize.Size = new Vector2(24f, 24f);
            ButtonResize.Anchor = Anchor.BottomRight;
            ButtonResize.IsVisible = !VisibleOnMouseOverOnly_ButtonResize;

            if (VisibleOnMouseOverOnly_ButtonResize)
            {
                Root.OnMouseEnter += (obj, e) => ButtonResize.IsVisible = !IsLocked;
                Root.OnMouseLeave += (obj, e) => ButtonResize.IsVisible = false;
            }

            ButtonResize.OnMouseDown += (obj, e) =>
            {
                if (!IsResizing && IsResizable)
                {
                    IsResizing = true;
                    ResizeStart = new Rect(WindowManager.GetMousePosition().x, WindowManager.GetMousePosition().y, Width, Height);
                }
            };

            bool _Cache_DrawBorder = DrawBorder;
            Color _Cache_BorderColor = Root.Style.BorderColor;
            int _Cache_BorderThickness = Root.Style.BorderThickness;

            ButtonResize.OnMouseEnter += (obj, e) =>
            {
                _Cache_DrawBorder = DrawBorder;
                _Cache_BorderColor = Root.Style.BorderColor;
                _Cache_BorderThickness = Root.Style.BorderThickness;
            };

            // Prevent clicks and whatnots from overriding our temporary settings
            ButtonResize.OnWhileMouseOver += (obj, e) =>
            {
                DrawBorder = true;
                Root.Style.BorderColor = Globals.GetColor("WindowPanel.MouseOverBorderColor");
                Root.Style.BorderThickness = 3;
            };

            EventHandler<MouseEventArgs> OnMouseLeave = (obj, e) =>
            {
                DrawBorder = _Cache_DrawBorder;
                Root.Style.BorderColor = _Cache_BorderColor;
                Root.Style.BorderThickness = _Cache_BorderThickness;
            };

            ButtonResize.OnMouseLeave += OnMouseLeave;
            Root.OnMouseLeave += OnMouseLeave;

            Root.AddChild(ButtonResize);
        }

        public virtual void DoResize()
        {
            if (WindowManager.IsMouseUp())
            {
                IsResizing = false;
            }
            else
            {
                Rect WinRect = Bounds;
                Vector2 mousePosition = WindowManager.GetMousePosition();

                WinRect.width = (ResizeStart.width + (mousePosition.x - ResizeStart.x));
                WinRect.height = (ResizeStart.height + (mousePosition.y - ResizeStart.y));

                WinRect.width = Mathf.Max(WinRect.width, Root.MinSize.x);
                WinRect.height = Mathf.Max(WinRect.height, Root.MinSize.y);

                if (Root.MaxSize.x > 0f)
                {
                    WinRect.width = Mathf.Min(WinRect.width, Root.MaxSize.x);
                }
                if (Root.MaxSize.y > 0f)
                {
                    WinRect.height = Mathf.Min(WinRect.height, Root.MaxSize.y);
                }

                WinRect.xMax = Mathf.Min((float)UI.screenWidth, WinRect.xMax);
                WinRect.yMax = Mathf.Min((float)UI.screenHeight, WinRect.yMax);

                Bounds = new Rect(WinRect.x, WinRect.y, (float)((int)WinRect.width), (float)((int)WinRect.height));
            }
        }

        #endregion "Optional Buttons"

        public virtual void UpdateLocking()
        {
            if (ButtonResize != null)
            {
                ButtonResize.IsVisible = !IsLocked;
            }
            if (ToolTipIcon != null)
            {
                ToolTipIcon.IsVisible = IsLocked;
            }
        }

        public virtual void AddLockingToolTipIcon()
        {
            if (ToolTipIcon == null)
            {
                ToolTipIcon = new Image(Globals.TryGetTexturePathFromAlias("Locked"), 14f, 14f);
                ToolTipIcon.SetStyle("WindowPanel.ToolTipIcon");
                ToolTipIcon.Anchor = Anchor.BottomRight;
                ToolTipIcon.Offset = new Vector2(-3f, -3f);
                ToolTipIcon.Style.Color = Color.white;
                ToolTipIcon.ToolTipText = "WindowPanelLocked_ToolTipText".Translate(KeyBindings.WindowLocking.MainKey.ToStringReadable());
                ToolTipIcon.IsVisible = false;
                Root.AddChild(ToolTipIcon);

                Root.OnMouseEnter += (obj, e) => ToolTipIcon.IsVisible = IsLocked;
                Root.OnMouseLeave += (obj, e) => ToolTipIcon.IsVisible = false;
            }
        }

        public virtual void DoDragging()
        {
            if (!KeyBindings.WindowDragging.IsDown)
            {
                IsDragging = false;
                preventCameraMotion = false;
                MouseLast = Vector2.zero;
            }
            else
            {
                // Add delta
                Position += (WindowManager.GetMousePosition() - MouseLast);
                MouseLast = WindowManager.GetMousePosition();
            }
        }

        public virtual void OnClick_ResetBounds(object Sender, MouseEventArgs EventArgs)
        {
            // Avoid stealing Shift Modifiers
            if (!WindowManager.IsShiftDown() && WindowManager.IsCtrlDown() && WindowManager.IsAltDown())
            {
                ResetBounds();
            }
        }

        public virtual void OnClickRight_Close(object Sender, MouseEventArgs EventArgs)
        {
            // Avoid stealing Shift Modifiers
            if (!WindowManager.IsShiftDown() && WindowManager.IsCtrlDown() && WindowManager.IsAltDown())
            {
                Close();
            }
        }

        public virtual void ResetBounds()
        {
            Bounds = DefaultBounds;
        }

        /// <summary>
        /// Adds this Window to the <see cref="WindowManager"/> instance.
        /// </summary>
        public void Open()
        {
            WindowManager.OpenWindow(this);
        }

        /// <summary>
        /// Removes this Window from the <see cref="WindowManager"/> instance.
        /// </summary>
        /// <param name="DoSound">Should Closing sound be played?</param>
        public override void Close(bool DoSound = false)
        {
            WindowManager.CloseWindow(this, DoSound);
        }

        /// <summary>
        /// Toggle if only one instance of this window class is allowed at anytime. Use this instead of <see cref="Window.onlyOneOfTypeAllowed"/>.
        /// </summary>
        /// <remarks>Defaults to false in the <see cref="WindowPanel(Rect)"/> constructor.</remarks>
        public bool AllowMultipleInstance
        {
            get
            {
                return !onlyOneOfTypeAllowed;
            }
            set
            {
                onlyOneOfTypeAllowed = !value;
            }
        }

        #region "Overrides"

        public override void PreOpen()
        {
            base.PreOpen();
            IsVisible = true;
        }

        public override void PreClose()
        {
            base.PreClose();
            IsVisible = false;
        }

        /// <summary>
        /// <para>Use this instead of <see cref="Window.closeOnAccept"/>, <see cref="Window.closeOnCancel"/>, <see cref="Window.closeOnClickedOutside"/>.</para>
        /// <para>Disabling all prevents the window from closing.</para>
        /// </summary>
        /// <param name="CloseOnAccept">Window will close if it receives a accept notification.</param>
        /// <param name="CloseOnCancel">Window will close if it receives a cancel notification.</param>
        /// <param name="CloseOnClickedOutside">Window will close if mouse was pressed down outside this window.</param>
        public virtual void SetCloseOn(bool CloseOnAccept = false, bool CloseOnCancel = false, bool CloseOnClickedOutside = false)
        {
            this.closeOnAccept = CloseOnAccept;
            this.closeOnCancel = CloseOnCancel;
            this.closeOnClickedOutside = CloseOnClickedOutside;
        }

        /// <summary>
        /// Called by Base class, Do not call directly, use <see cref="Draw"/> instead.
        /// </summary>
        public override void DoWindowContents(Rect inRect)
        {
            Draw();
        }

        /// <summary>
        /// Called by Base class, Do not call directly, use <see cref="Update"/> instead.
        /// </summary>
        public override void WindowUpdate()
        {
            Update();
        }

        /// <summary>
        /// Called on <see cref="Window.PreOpen"/> and on screen size change.
        /// </summary>
        /// <remarks>Do not use this for positioning and sizing, use <see cref="Bounds"/> instead.</remarks>
        protected override void SetInitialSizeAndPosition()
        { }

        /// <summary>
        /// We set Margin to 0 to avoid it messing up our Entities.
        /// </summary>
        protected override float Margin { get => 0; }

        public override void PostOpen()
        {
            CurrentlyOpen = true;
        }

        public override void PostClose()
        {
            CurrentlyOpen = false;
        }

        /// <summary>
        /// Draws a Shadow background, filling the entirety of the background of this Window and some extra overflow around it.
        /// </summary>
        /// <remarks>Needed to add documentation here to not forget what these did.</remarks>
        public bool DrawBackgroundShadow
        {
            get
            {
                return drawShadow;
            }
            set
            {
                drawShadow = value;
            }
        }

        /// <summary>
        /// The Opacity of the background shadow when <see cref="drawShadow"/> is not zero.
        /// </summary>
        /// <remarks>Needed to add documentation here to not forget what these did.</remarks>
        public float BackgroundShadowAlpha
        {
            get
            {
                return shadowAlpha;
            }
            set
            {
                shadowAlpha = value;
            }
        }

        public void Override_InnerWindowOnGuiCached()
        {
            FieldInfo Field = typeof(Window).GetField("innerWindowOnGUICached", BindingFlags.NonPublic | BindingFlags.Instance);
            InnerWindowOnGUICached = (GUI.WindowFunction)Field.GetValue(this);
            Field.SetValue(this, new GUI.WindowFunction(InnerWindowOnGUI));
        }

        /// <summary>
        /// Completely replaces the vanilla game's private InnerWindowOnGUI method.
        /// </summary>
        /// <remarks>Use <see cref="Override_InnerWindowOnGuiCached"/> to enable it.</remarks>
		public virtual void InnerWindowOnGUI(int x)
        {
            if (!IsVisible)
            {
                return;
            }

            // Modified from Window.InnerWindowOnGUI

            UnityGUIBugsFixer.OnGUI();
            Verse.Steam.SteamDeck.WindowOnGUI();
            OriginalEventUtility.RecordOriginalEvent(Event.current);
            Find.WindowStack.currentlyDrawnWindow = this;

            if (KeyBindingDefOf.Cancel.KeyDownEvent)
            {
                Find.WindowStack.Notify_PressedCancel();
            }
            if (KeyBindingDefOf.Accept.KeyDownEvent)
            {
                Find.WindowStack.Notify_PressedAccept();
            }
            if (Event.current.type == EventType.MouseDown)
            {
                Find.WindowStack.Notify_ClickedInsideWindow(this);
            }
            if ((Event.current.type == EventType.KeyDown) && !Find.WindowStack.GetsInput(this))
            {
                Event.current.Use();
            }

            Widgets.BeginGroup(Bounds.AtZero());

            try
            {
                Draw();
            }
            catch (Exception Exception)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Exception filling window for ",
                    base.GetType(),
                    ": ",
                    Exception
                }));
            }

            Widgets.EndGroup();

            LateWindowOnGUI(Bounds.AtZero());

            if (KeyBindingDefOf.Cancel.KeyDownEvent && IsOpen)
            {
                OnCancelKeyPressed();
            }

            if (Passthrough_Active && !Passthrough_Blocked)
            {
                foreach (EventType EventType in Passthrough_EventTypes)
                {
                    if (Event.current.type == EventType)
                    {
                        Event.current.Use();
                        break;
                    }
                }
            }

            Find.WindowStack.currentlyDrawnWindow = null;
            OriginalEventUtility.Reset();
        }

        #endregion "Overrides"
    }
}
