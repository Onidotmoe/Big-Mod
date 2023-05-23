using Verse;

namespace BigMod.Entities
{
    // TODO: Possibly add option to anchor to a specific control
    public enum Anchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public class Panel
    {
        #region "Positioning & Sizing"

        /// <summary>
        /// <para>Ignores all other requirements except for <see cref="IsVisible"/> when trying to <see cref="DrawMouseOver"/>.</para>
        /// <para>Used when mimicking MouseOver from another Entity.</para>
        /// </summary>
        public bool ForceDrawMouseOver;
        private Anchor _Anchor = Anchor.TopLeft;
        /// <summary>
        /// Used along with <see cref="Offset"/> to position the Entity relative to its Parent.
        /// </summary>
        public Anchor Anchor
        {
            get
            {
                return _Anchor;
            }
            set
            {
                if (_Anchor != value)
                {
                    _Anchor = value;
                    DoOnPositionChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Toggle the use of Anchor for positioning.
        /// </summary>
        /// <remarks>Does not propagate to its children.</remarks>
        public bool UseAnchoring = true;
        /// <summary>
        /// Public to allow it to be used by ref in <see cref="Button.Draw"/>.
        /// </summary>
        public Rect _Bounds = Rect.zero;
        /// <summary>
        /// <para>Used along with <see cref="SizeWithParent"/> to resize this Entity when its parents changes size.</para>
        /// <para>Or along with <see cref="PositionWithParent"/> to reposition this Entity when parent size changes.</para>
        /// <para>Or along with <see cref="LimitToParent"/> to limit this Entity's size to inside its parent's size.</para>
        /// </summary>
        public Rect _OldBounds;
        /// <summary>
        /// Should this Entity scale its size when its parent changes size?
        /// </summary>
        /// TODO: Needs to use Scale instead and store origin size for comparison.
        public bool SizeWithParent;
        /// <summary>
        /// Will change its position with its parent, when its parent's size changes this Entity will relocate to be in the same relative position.
        /// </summary>
        public bool PositionWithParent;
        /// <summary>
        /// Prevents the Size of this Entity from overflow its Parents size.
        /// </summary>
        public bool LimitToParent;
        private Vector2 _Offset = Vector2.zero;
        /// <summary>
        /// Offset to be applied along with <see cref="Anchor"/> or with <see cref="InheritParentPosition"/>.
        /// </summary>
        public virtual Vector2 Offset
        {
            get
            {
                return _Offset;
            }
            set
            {
                if (_Offset != value)
                {
                    _Offset = value;
                    DoOnPositionChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Property for <see cref="InheritParentPositionX"/> and <see cref="InheritParentPositionY"/>.
        /// </summary>
        /// <remarks><para>Returns True if <see cref="InheritParentPositionX"/> or <see cref="InheritParentPositionY"/> is active.</para>
        /// <para>Does not update the UI.</para>
        public bool InheritParentPosition
        {
            get
            {
                return (InheritParentPositionX || InheritParentPositionY);
            }
            set
            {
                if ((InheritParentPositionX != value) || (InheritParentPositionY != value))
                {
                    InheritParentPositionX = value;
                    InheritParentPositionY = value;
                }
            }
        }
        /// <summary>
        /// Property for <see cref="InheritParentSizeWidth"/> and <see cref="InheritParentSizeHeight"/>.
        /// </summary>
        /// <remarks><para>Returns True if either <see cref="InheritParentSizeWidth"/> or <see cref="InheritParentSizeHeight"/> is active.</para>
        /// <para>Does not update the UI.</para>
        /// </remarks>
        public bool InheritParentSize
        {
            get
            {
                return (InheritParentSizeWidth || InheritParentSizeHeight);
            }
            set
            {
                if ((InheritParentSizeWidth != value) || (InheritParentSizeHeight != value))
                {
                    InheritParentSizeWidth = value;
                    InheritParentSizeHeight = value;
                }
            }
        }

        /// <summary>
        /// Will copy its Parent's Position X whenever it changes.
        /// </summary>
        public bool InheritParentPositionX;
        /// <summary>
        /// Will copy its Parent's Position Y whenever it changes.
        /// </summary>
        public bool InheritParentPositionY;
        /// <summary>
        /// Will copy its Parent's Width whenever it changes.
        /// </summary>
        public bool InheritParentSizeWidth;
        /// <summary>
        /// Will copy its Parent's Height whenever it changes.
        /// </summary>
        public bool InheritParentSizeHeight;
        /// <summary>
        /// Used along with <see cref="InheritParentSize"/> will modify the final size after Inheriting from Parent.
        /// </summary>
        public Vector2 InheritParentSize_Modifier = Vector2.zero;
        /// <summary>
        /// Property for <see cref="InheritChildrenSizeWidth"/> and <see cref="InheritChildrenSizeHeight"/>.
        /// </summary>
        /// <remarks>
        /// <para>Returns XOR whether either <see cref="InheritChildrenSizeWidth"/> or <see cref="InheritChildrenSizeHeight"/> is active.</para>
        /// <para>Does not update the UI automatically.</para>
        /// </remarks>
        public bool InheritChildrenSize
        {
            get
            {
                return (InheritChildrenSizeWidth ^ InheritChildrenSizeHeight);
            }
            set
            {
                if ((InheritChildrenSizeWidth != value) || (InheritChildrenSizeHeight != value))
                {
                    InheritChildrenSizeWidth = value;
                    InheritChildrenSizeHeight = value;

                    DoOnSizeChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Will Set its Size based on the Size of its Children's Width.
        /// </summary>
        /// <remarks>
        /// Does not update the UI automatically.
        /// </remarks>
        public bool InheritChildrenSizeWidth;
        /// <summary>
        /// Will Set its Size based on the Size of its Children's Height.
        /// </summary>
        /// <remarks>
        /// Does not update the UI automatically.
        /// </remarks>
        public bool InheritChildrenSizeHeight;
        /// <summary>
        /// Used along with <see cref="InheritChildrenSize"/> will modify the final size after Inheriting from Children.
        /// </summary>
        public Vector2 InheritChildrenSize_Modifier = Vector2.zero;
        /// <summary>
        /// Min size this Entity will ever shrink to.
        /// </summary>
        public virtual Vector2 MinSize { get; set; } = Vector2.zero;
        /// <summary>
        /// Max size this Entity will ever expand to.
        /// </summary>
        public virtual Vector2 MaxSize { get; set; } = Vector2.zero;
        /// <summary>
        /// Current Size.
        /// </summary>
        public virtual Vector2 Size
        {
            get
            {
                return _Bounds.size;
            }
            set
            {
                if (_Bounds.size != value)
                {
                    _OldBounds = _Bounds;
                    // When MinSize is unset, prevents negative values
                    _Bounds.size = Vector2.Max(MinSize, value);

                    if (MaxSize.x != 0)
                    {
                        _Bounds.width = Mathf.Min(MaxSize.x, _Bounds.width);
                    }
                    if (MaxSize.y != 0)
                    {
                        _Bounds.height = Mathf.Min(MaxSize.y, _Bounds.height);
                    }

                    DoOnSizeChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Some Entities can be selected.
        /// </summary>
        public virtual bool IsSelected { get; set; }

        /// <summary>
        /// Can this Entity be Selected?
        /// </summary>
        public virtual bool Selectable { get; set; }
        /// <summary>
        /// Bounding Box of this Entity.
        /// </summary>
        /// <remarks>Does not trigger <see cref="OnSizeChanged"/> and <see cref="OnBoundsChanged"/> when changed.</remarks>
        public virtual Rect Bounds
        {
            get
            {
                return _Bounds;
            }
            set
            {
                _Bounds = value;
            }
        }
        /// <summary>
        /// Current position X.
        /// </summary>
        public virtual float X
        {
            get
            {
                return _Bounds.x;
            }
            set
            {
                if (_Bounds.x != value)
                {
                    _OldBounds = _Bounds;
                    _Bounds.x = value;
                    DoOnPositionChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Current position Y.
        /// </summary>
        public virtual float Y
        {
            get
            {
                return _Bounds.y;
            }
            set
            {
                if (_Bounds.y != value)
                {
                    _OldBounds = _Bounds;
                    _Bounds.y = value;
                    DoOnPositionChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Width of this Entity.
        /// </summary>
        public virtual float Width
        {
            get
            {
                return _Bounds.width;
            }
            set
            {
                // Don't update anything if the value didn't change
                if (_Bounds.width != value)
                {
                    _OldBounds = _Bounds;
                    _Bounds.width = Mathf.Max(MinSize.x, value);

                    if (MaxSize.x != 0)
                    {
                        _Bounds.width = Mathf.Min(MaxSize.x, _Bounds.width);
                    }

                    DoOnSizeChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Height of this Entity.
        /// </summary>
        public virtual float Height
        {
            get
            {
                return _Bounds.height;
            }
            set
            {
                if (_Bounds.height != value)
                {
                    _OldBounds = _Bounds;
                    _Bounds.height = Mathf.Max(MinSize.y, value);

                    if (MaxSize.y != 0)
                    {
                        _Bounds.height = Mathf.Min(MaxSize.y, _Bounds.height);
                    }

                    DoOnSizeChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Current Position vector.
        /// </summary>
        public virtual Vector2 Position
        {
            get
            {
                return _Bounds.position;
            }
            set
            {
                if (_Bounds.position != value)
                {
                    _OldBounds = _Bounds;
                    _Bounds.position = value;
                    DoOnPositionChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Bottom-most Y.
        /// </summary>
        public virtual float Bottom
        {
            get
            {
                return (Y + Height);
            }
        }
        /// <summary>
        /// Right-most X.
        /// </summary>
        public virtual float Right
        {
            get
            {
                return (X + Width);
            }
        }
        /// <summary>
        /// Bottom-most Y relative to Offset.
        /// </summary>
        public virtual float OffsetBottom
        {
            get
            {
                return (Offset.y + Height);
            }
        }
        /// <summary>
        /// Right-most X relative to Offset.
        /// </summary>
        public virtual float OffsetRight
        {
            get
            {
                return (Offset.x + Width);
            }
        }
        /// <summary>
        /// Current Offset X.
        /// </summary>
        public virtual float OffsetX
        {
            get
            {
                return _Offset.x;
            }
            set
            {
                if (_Offset.x != value)
                {
                    _Offset.x = value;
                    DoOnPositionChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Current Offset Y.
        /// </summary>
        public virtual float OffsetY
        {
            get
            {
                return _Offset.y;
            }
            set
            {
                if (_Offset.y != value)
                {
                    _Offset.y = value;
                    DoOnPositionChanged(this, EventArgs.Empty);
                    DoOnBoundsChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Increases Childrens Position based on this Entity's Size change.
        /// </summary>
        public virtual void DoWithParentPosition()
        {
            if (_OldBounds.size != Size)
            {
                Vector2 Delta = (Size - _OldBounds.size);

                foreach (Panel Child in Children)
                {
                    if (Child.PositionWithParent)
                    {
                        // Reposition according to parent size change
                        Child.Position += Delta;
                    }
                }
            }
        }

        /// <summary>
        /// Increases Childrens Size based on this Entity's Size change.
        /// </summary>
        public virtual void DoWithParentSize()
        {
            if (_OldBounds.size != Size)
            {
                Vector2 Delta = (Size - _OldBounds.size);

                foreach (Panel Child in Children)
                {
                    if (Child.SizeWithParent)
                    {
                        // Resize according to parent size change
                        Child.Size += Delta;
                    }
                }
            }
        }

        /// <summary>
        /// Bequeaths Position to its Children.
        /// </summary>
        public virtual void DoInheritParentPosition()
        {
            if (_OldBounds.position != Position)
            {
                foreach (Panel Child in Children)
                {
                    if (Child.InheritParentPosition)
                    {
                        // Doing it like this to only have it update the UI once.
                        Child.Position = new Vector2((Child.InheritParentPositionX ? (Position.x + Child.Offset.x) : Child.Position.x), (Child.InheritParentPositionY ? (Position.y + Child.Offset.y) : Child.Position.y));
                    }
                }
            }
        }

        /// <summary>
        /// Bequeaths Size to its Children and bubbles its change event to its Parent.
        /// </summary>
        public virtual void DoInheritParentSize()
        {
            // TODO: should be more like UpdateAnchorPosition
            if (_OldBounds.size != Size)
            {
                foreach (Panel Child in Children)
                {
                    if (Child.InheritParentSize)
                    {
                        // Doing it like this to only have it update the UI once.
                        Child.Size = new Vector2((Child.InheritParentSizeWidth ? (Size.x + Child.InheritParentSize_Modifier.x) : Child.Size.x), (Child.InheritParentSizeHeight ? (Size.y + Child.InheritParentSize_Modifier.y) : Child.Size.y));
                    }
                }
            }
        }

        /// <summary>
        /// Set Size based on its Children's size.
        /// </summary>
        public virtual void DoInheritChildrenSize()
        {
            if (InheritChildrenSize)
            {
                if (Children.Any())
                {
                    float New_Width = Width;
                    float New_Height = Height;

                    switch (Anchor)
                    {
                        // Good enough for now...
                        case Anchor.TopRight:
                        case Anchor.CenterRight:
                        case Anchor.BottomRight:
                            // If the current Anchor is set to the Right, then this Entity needs to grow towards the Left.
                            if (InheritChildrenSizeWidth)
                            {
                                New_Width = Math.Abs(Children.Min((F) => F.IsVisible ? F.X : 0f));
                            }
                            if (InheritChildrenSizeHeight)
                            {
                                New_Height = Children.Max((F) => F.IsVisible ? F.Bottom : 0f);
                            }
                            break;

                        default:
                            if (InheritChildrenSizeWidth)
                            {
                                New_Width = Children.Max((F) => F.IsVisible ? F.Right : 0f);
                            }
                            if (InheritChildrenSizeHeight)
                            {
                                New_Height = Children.Max((F) => F.IsVisible ? F.Bottom : 0f);
                            }
                            break;
                    }

                    Size = new Vector2(New_Width + InheritChildrenSize_Modifier.x, New_Height + InheritChildrenSize_Modifier.y);
                }
                else
                {
                    Size = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// Restricts the Size of its Children.
        /// </summary>
        public virtual void DoLimitToParent()
        {
            // Only handle if the size was changed
            if (_OldBounds.size != Size)
            {
                foreach (Panel Child in Children)
                {
                    if (Child.LimitToParent)
                    {
                        // Cap overflow to its position inside its parent minus its parent's size
                        Child.Size = Vector2.Min(Child.Size, (Size - (Position - Child.Position)));
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Current position to the current Anchor and applies Offset.
        /// </summary>
        public virtual void UpdateAnchorPosition()
        {
            if (UseAnchoring)
            {
                _Bounds.position = (GetAnchorPosition(Anchor) + Offset);
            }

            foreach (Panel Child in Children)
            {
                // Even tho the parent might have anchoring disabled, its children might not
                Child.UpdateAnchorPosition();
            }
        }

        /// <summary>
        /// Get the current position of the Anchor.
        /// </summary>
        /// <param name="Anchor">Optional Anchor to get position of, defaults to <see cref="Anchor.TopLeft"/>.</param>
        /// <param name="Bounds">Optional Bounds to use, default to Parent bounds.</param>
        /// <returns>Position of the Anchor with this Entity's size taking into account.</returns>
        public Vector2 GetAnchorPosition(Anchor Anchor = Anchor.TopLeft, Rect Bounds = default)
        {
            Vector2 Position = Vector2.zero;

            if (Bounds == default)
            {
                if (Parent != null)
                {
                    Bounds = Parent.Bounds;
                    Position = Parent.Position;
                }
                else
                {
                    Bounds = Rect.zero;
                }
            }

            switch (Anchor)
            {
                case Anchor.TopLeft:
                    break;

                case Anchor.TopCenter:
                    Position.x += (Bounds.width / 2) - (Width / 2);
                    break;

                case Anchor.TopRight:
                    Position.x += Bounds.width - Width;
                    break;

                case Anchor.CenterLeft:
                    Position.y += (Bounds.height / 2) - (Height / 2);
                    break;

                case Anchor.Center:
                    Position.x += (Bounds.width / 2) - (Width / 2);
                    Position.y += (Bounds.height / 2) - (Height / 2);
                    break;

                case Anchor.CenterRight:
                    Position.x += Bounds.width - Height;
                    Position.y += (Bounds.height / 2) - (Height / 2);
                    break;

                case Anchor.BottomLeft:
                    Position.y += Bounds.height - Height;
                    break;

                case Anchor.BottomCenter:
                    Position.x += (Bounds.width / 2) - (Width / 2);
                    Position.y += Bounds.height - Height;
                    break;

                case Anchor.BottomRight:
                    Position.x += Bounds.width - Width;
                    Position.y += Bounds.height - Height;
                    break;
            }

            return Position;
        }

        #endregion "Positioning & Sizing"

        #region "Events & Callbacks"

        /// <summary>
        /// Used for detecting Double-Click. Threshold is define in <see cref="WindowManager.DoubleClickThreshold"/>.
        /// </summary>
        public float LastClickTime;
        /// <summary>
        /// Called when mouse is Clicked over this Entity.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnClick;
        /// <summary>
        /// Called when mouse is Clicked over this Entity twice in rapid succession.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnClickDouble;
        /// <summary>
        /// Called when mouse is Right Clicked over this Entity.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnClickRight;
        /// <summary>
        /// Called when <see cref="Bounds"/> has changed.
        /// </summary>
        public virtual event EventHandler OnBoundsChanged;
        /// <summary>
        /// Called when <see cref="Size"/> has changed.
        /// </summary>
        public virtual event EventHandler OnSizeChanged;
        /// <summary>
        /// Called when <see cref="Position"/> has changed.
        /// </summary>
        public virtual event EventHandler OnPositionChanged;
        /// <summary>
        /// Called when <see cref="Children"/> has changed.
        /// </summary>
        public virtual event EventHandler OnChildrenChanged;
        /// <summary>
        /// Called while <see cref="IsMouseOver"/> is true.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnWhileMouseOver;
        /// <summary>
        /// Called when <see cref="IsMouseOver"/> is true but was false.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnMouseEnter;
        /// <summary>
        /// Called when <see cref="IsMouseOver"/> was true but is now false.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnMouseLeave;
        /// <summary>
        /// Called each update cycle while mouse is down over this Entity.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnWhileMouseDown;
        /// <summary>
        /// Called once when mouse is down over this Entity.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnMouseDown;
        /// <summary>
        /// Called once when mouse is up after it has been down over this Entity.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnMouseUp;
        /// <summary>
        /// Called whenever the mousewheel has been used.
        /// </summary>
        public virtual event EventHandler<MouseEventArgs> OnMouseWheel;
        /// <summary>
        /// Called when <see cref="IsVisible"/> has changed.
        /// </summary>
        public virtual event EventHandler OnVisibilityChanged;

        /// <summary>
        /// Is this Entity currently capable of passing Mouse Inputs to its Children?
        /// </summary>
        /// <returns>True if can pass Mouse Inputs right now.</returns>
        public virtual bool CanCascadeMouseInput()
        {
            return (IsMouseOver && IsVisible && !IgnoreMouse && !IsLocked);
        }

        /// <summary>
        /// Called when Left Mouse Button is pressed down but was up in the previous frame over this Entity.
        /// </summary>
        /// <remarks>Also checks and initiates DoubleClick.</remarks>
        public virtual void DoOnClick(object Sender, MouseEventArgs EventArgs)
        {
            OnClick?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);

            if ((Time.time - LastClickTime) < WindowManager.DoubleClickThreshold)
            {
                DoOnClickDouble(this, MouseEventArgs.IsMouseClickDouble);
            }

            LastClickTime = Time.time;
        }

        /// <summary>
        /// Called when mouse is Clicked over this Entity twice in rapid succession.
        /// </summary>
        public virtual void DoOnClickDouble(object Sender, MouseEventArgs EventArgs)
        {
            OnClickDouble?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called when mouse is Right Clicked over this Entity.
        /// </summary>
        public virtual void DoOnClickRight(object Sender, MouseEventArgs EventArgs)
        {
            OnClickRight?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called once when mouse is down over this Entity.
        /// </summary>
        public virtual void DoOnMouseDown(object Sender, MouseEventArgs EventArgs)
        {
            IsMouseDown = true;

            OnMouseDown?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called once when mouse is down over this Entity.
        /// </summary>
        public virtual void DoOnMouseUp(object Sender, MouseEventArgs EventArgs)
        {
            IsMouseDown = false;

            OnMouseUp?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called each update cycle while mouse is down over this Entity.
        /// </summary>
        public virtual void DoOnWhileMouseDown(object Sender, MouseEventArgs EventArgs)
        {
            OnWhileMouseDown?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called while <see cref="IsMouseOver"/> is true.
        /// </summary>
        public virtual void DoOnWhileMouseOver(object Sender, MouseEventArgs EventArgs)
        {
            OnWhileMouseOver?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called when <see cref="IsMouseOver"/> is true but was false.
        /// </summary>
        public virtual void DoOnMouseEnter(object Sender, MouseEventArgs EventArgs)
        {
            OnMouseEnter?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called when <see cref="IsMouseOver"/> was true but is now false.
        /// </summary>
        public virtual void DoOnMouseLeave(object Sender, MouseEventArgs EventArgs)
        {
            IsMouseDown = false;
            IsMouseOver = false;

            if (IsVisible && !IgnoreMouse)
            {
                OnMouseLeave?.Invoke(this, EventArgs);
            }
        }

        /// <summary>
        /// Called on MouseWheel change.
        /// </summary>
        public virtual void DoOnMouseWheel(object Sender, MouseEventArgs EventArgs)
        {
            OnMouseWheel?.Invoke(this, EventArgs);

            CascadeMouseInput(this, EventArgs);
        }

        /// <summary>
        /// Called when <see cref="Size"/> has changed.
        /// </summary>
        public virtual void DoOnSizeChanged(object Sender, EventArgs EventArgs)
        {
            DoInheritParentSize();
            DoWithParentSize();
            DoLimitToParent();

            OnSizeChanged?.Invoke(this, EventArgs);
        }

        public virtual void DoOnPositionChanged(object Sender, EventArgs EventArgs)
        {
            DoInheritParentPosition();
            DoWithParentPosition();

            OnPositionChanged?.Invoke(this, EventArgs);
        }

        /// <summary>
        /// Called when <see cref="Bounds"/> has changed.
        /// </summary>
        public virtual void DoOnBoundsChanged(object Sender, EventArgs EventArgs)
        {
            UpdateAnchorPosition();

            OnBoundsChanged?.Invoke(this, EventArgs);
        }

        /// <summary>
        /// Called when <see cref="IsVisible"/> has changed.
        /// </summary>
        public virtual void DoOnVisibilityChanged(object Sender, EventArgs EventArgs)
        {
            OnVisibilityChanged?.Invoke(this, EventArgs);
        }

        /// <summary>
        /// Cascades MouseInput to first Child that has <see cref="CanCascadeMouseInput"/> true.
        /// </summary>
        /// <param name="Sender">Sender Object.</param>
        /// <param name="MouseEventArgs">MouseInput to cascade.</param>
        /// <remarks>Going down the Children's Tree and branching off whenever there's a Child that can capture MouseInput, then that Child will pass it on to its Children and so on.</remarks>
        public virtual void CascadeMouseInput(object Sender, MouseEventArgs MouseEventArgs)
        {
            if (!BlockMouseCascade)
            {
                // Allows Children to be modified as we iterate over them.
                foreach (Panel Child in Children.AsEnumerable().ToList())
                {
                    if (Child.CanCascadeMouseInput())
                    {
                        Child.HandleMouseInput(Child, MouseEventArgs);
                    }
                }
            }
        }

        /// <summary>
        /// Pushes Mouse Event to the correct method.
        /// </summary>
        /// <param name="Sender">Sender Object.</param>
        /// <param name="MouseEventArgs">MouseInput to handle.</param>
        public virtual void HandleMouseInput(object Sender, MouseEventArgs MouseEventArgs)
        {
            switch (MouseEventArgs.MouseInput)
            {
                case MouseInput.Empty:
                    break;

                case MouseInput.IsMouseClick:
                    DoOnClick(Sender, MouseEventArgs);
                    break;

                case MouseInput.IsMouseClickRight:
                    DoOnClickRight(Sender, MouseEventArgs);
                    break;

                case MouseInput.IsMouseDown:
                case MouseInput.IsMouseDownCurrently:
                    DoOnMouseDown(Sender, MouseEventArgs);
                    break;

                case MouseInput.IsMouseUp:
                    DoOnMouseUp(Sender, MouseEventArgs);
                    break;

                case MouseInput.IsMouseDownContinuous:
                    DoOnWhileMouseDown(Sender, MouseEventArgs);
                    break;

                case MouseInput.IsMouseWheel:
                    DoOnMouseWheel(Sender, MouseEventArgs);
                    break;
            }
        }

        public virtual void DoOnChildrenChanged(object Sender, EventArgs EventArgs)
        {
            DoInheritChildrenSize();

            OnChildrenChanged?.Invoke(this, EventArgs);
        }

        #endregion "Events & Callbacks"

        private bool _IsVisible = true;
        /// <summary>
        /// Set Visibility.
        /// </summary>
        public virtual bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
            set
            {
                if (_IsVisible != value)
                {
                    _IsVisible = value;

                    DoOnVisibilityChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Toggle if Enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        /// <summary>
        /// Toggle if Locked.
        /// </summary>
        /// <remarks>Locked controls are a intermediary state between Enabled and Disabled, allowing MouseOver ToolTip but not Click events.</remarks>
        public bool IsLocked { get; set; }
        /// <summary>
        /// Scale to apply to texture when rendering.
        /// </summary>
        public float TextureScale { get; set; } = 1;
        /// <summary>
        /// Generic Texture.
        /// </summary>
        public Texture2D Texture { get; set; }
        /// <summary>
        /// Path to the Texture resource.
        /// </summary>
        public string TexturePath { get; set; }
        /// <summary>
        /// Text to display in Tooltip box when MouseOver.
        /// </summary>
        public virtual string ToolTipText { get; set; }
        public Panel Parent;
        private WindowPanel _ParentWindow;
        /// <summary>
        /// Because we're effectively dealing with 2 different UI systems, sometimes our parent isn't a Entity.
        /// </summary>
        public virtual WindowPanel ParentWindow
        {
            get
            {
                return _ParentWindow;
            }
            set
            {
                if (_ParentWindow != value)
                {
                    _ParentWindow = value;

                    // Update ParentWindow on all children reclusively
                    foreach (Panel Child in GetChildrenFlatten())
                    {
                        Child.ParentWindow = value;
                    }
                }
            }
        }
        private List<Panel> _Children = new List<Panel>();
        public List<Panel> Children
        {
            get
            {
                return _Children;
            }
            set
            {
                // Check if list has changed before applying the incoming value.
                bool ShouldDoOnChildrenChanged = ((_Children != value) || (_Children.Count != value.Count));

                _Children = value;

                if (ShouldDoOnChildrenChanged)
                {
                    // Fire the Callback after the value has been applied.
                    DoOnChildrenChanged(this, EventArgs.Empty);
                }
            }
        }
        private GUIStyle _GUIStyle;
        /// <summary>
        /// Style of this specific Entity.
        /// </summary>
        /// <remarks>TODO: Might wanna remove.</remarks>
        public virtual GUIStyle GUIStyle
        {
            get
            {
                // Create a new one when requesting first time, this allows the rest of the Panel to be handled in parallel.
                return _GUIStyle ??= GetStyle();
            }
            set
            {
                _GUIStyle = value;
            }
        }
        public string ID;
        /// <summary>
        /// Arbitrary data attached to this Entity.
        /// </summary>
        public virtual object Data { get; set; }
        private bool _IgnoreMouse;
        /// <summary>
        /// Will ignore MouseOver & Click Events.
        /// </summary>
        public virtual bool IgnoreMouse
        {
            get
            {
                // Also ignore when Disabled.
                return (_IgnoreMouse || !IsEnabled);
            }
            set
            {
                _IgnoreMouse = value;
            }
        }
        /// <summary>
        /// Prevents MouseInput from Cascading to its Children.
        /// </summary>
        public virtual bool BlockMouseCascade { get; set; }
        /// <summary>
        /// Style used when rendering.
        /// </summary>
        public virtual Style Style { get; set; } = new Style(nameof(Panel));

        /// <summary>
        /// Get a new instance of the GUIStyle used by this Entity.
        /// </summary>
        /// <returns>New style based on styles inside <see cref="WindowManager.GUISkin"/>.</returns>
        public virtual GUIStyle GetStyle()
        {
            return new GUIStyle(WindowManager.GUISkin.box);
        }

        public Panel()
        {
            _Bounds.size = Vector2.Max(MinSize, Size);
            _OldBounds = Bounds;

            SetStyle(GetType().Name);
        }

        public Panel(Vector2 Size = default) : this()
        {
            this.Size = Size;
        }

        public Panel(Rect Bounds = default) : this()
        {
            this.Bounds = Bounds;
        }

        public Panel(Vector2 Size = default, Vector2 Offset = default) : this(Size)
        {
            this.Offset = Offset;
        }

        /// <summary>
        /// Updates this Entity and its children;
        /// </summary>
        public virtual void Update()
        {
            foreach (Panel Child in Children)
            {
                Child.Update();
            }
        }

        /// <summary>
        /// Draws this Entity.
        /// </summary>
        public virtual void Draw()
        {
            if (IsVisible)
            {
                DrawUnderlays();

                DrawChildren();
            }
        }

        /// <summary>
        /// Draws all Children.
        /// </summary>
        public virtual void DrawChildren()
        {
            foreach (Panel Child in Children)
            {
                Child.Draw();
            }

            DrawOverlays();
        }

        /// <summary>
        /// Draw the various Underlays.
        /// </summary>
        public virtual void DrawUnderlays()
        {
            DrawBackground();
            DrawToolTip();
        }

        /// <summary>
        /// Draw the various Overlays.
        /// </summary>
        public virtual void DrawOverlays()
        {
            DrawBorder();
            DrawSelected();
            DrawMouseOver();
            DrawDisabledOverlay();
        }

        /// <summary>
        /// Draws a overlay color based on <see cref="Style.MouseOverColor"/>.
        /// </summary>
        /// <remarks>MouseOverColor has to be applied after all children or it'll be rendered below everything</remarks>
        public virtual void DrawMouseOver()
        {
            if (IsVisible && (IsMouseOver && !IgnoreMouse && Style.DrawMouseOver && (Style.MouseOverColor != Color.clear)) || ForceDrawMouseOver)
            {
                GUI.DrawTexture(Bounds, BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 1f, Style.MouseOverColor, 0f, 0f);
            }
        }

        /// <summary>
        /// Draws a overlay Border based on <see cref="Style.BorderColor"/>.
        /// </summary>
        public virtual void DrawBorder()
        {
            if (IsVisible && Style.DrawBorder && (Style.BorderThickness > 0) && (Style.BorderColor != Color.clear))
            {
                Widgets.DrawBoxSolidWithOutline(Bounds, Color.clear, Style.BorderColor, Style.BorderThickness);
            }
        }

        /// <summary>
        /// Draws a rectangle filling it with the <see cref="Style.BackgroundColor"/> color when <see cref="Style.DrawBackground"/> is True and this Entity is visible.
        /// </summary>
        public virtual void DrawBackground()
        {
            if (IsVisible && Style.DrawBackground && (Style.BackgroundColor != Color.clear))
            {
                GUI.DrawTexture(Bounds, BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 1f, Style.BackgroundColor, 0f, 0f);
            }
        }

        /// <summary>
        /// Draws a overlay ontop of everything else if this Entity is disabled and has a <see cref="Style.DisabledColor"/>.
        /// </summary>
        public virtual void DrawDisabledOverlay()
        {
            if (IsVisible && (!IsEnabled || IsLocked) && (Style.DisabledColor != Color.clear))
            {
                GUI.DrawTexture(Bounds, BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 1, Style.DisabledColor, 0f, 0f);
            }
        }

        public virtual void DrawSelected()
        {
            if (IsVisible && IsSelected && (Style.SelectedColor != Color.clear))
            {
                GUI.DrawTexture(Bounds, BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 1f, Style.SelectedColor, 0f, 0f);
            }
        }

        /// <summary>
        /// Draws the vanilla ToolTip panel with the <see cref="ToolTipText"/>.
        /// </summary>
        public virtual void DrawToolTip()
        {
            if (IsVisible && IsMouseOver && !IgnoreMouse && !string.IsNullOrWhiteSpace(ToolTipText))
            {
                TooltipHandler.TipRegion(Bounds, ToolTipText);
            }
        }

        /// <summary>
        /// Inserts the given Child at the specified Index in the Children List.
        /// </summary>
        /// <remarks>Children List order is rendering order.</remarks>
        /// <param name="Index">Index to insert to.</param>
        /// <param name="Child">Child to Add.</param>
        public virtual void InsertChild(int Index, Panel Child)
        {
            Register(Child);
            Children.Insert(Index, Child);

            if (Child.UseAnchoring)
            {
                Child.UpdateAnchorPosition();
            }

            DoOnChildrenChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds the Child to the Top of the Children List.
        /// </summary>
        /// <remarks>Children List order is rendering order.</remarks>
        /// <param name="Child"></param>
        public virtual void PrependChild(Panel Child)
        {
            InsertChild(0, Child);
        }

        /// <summary>
        /// Appends the given Child to the Bottom of the Children List.
        /// </summary>
        /// <remarks>Children List order is rendering order.</remarks>
        /// <param name="Child">Child to Add.</param>
        public virtual void AddChild(Panel Child)
        {
            Register(Child);
            Children.Add(Child);

            // Update Parent's children's Bounds.
            DoInheritParentPosition();
            DoInheritParentSize();

            if (Child.UseAnchoring)
            {
                Child.UpdateAnchorPosition();
            }

            DoOnChildrenChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a list of Entities to this Entity's Children list.
        /// </summary>
        /// <param name="Children">Children to add.</param>
        public virtual void AddRange(IEnumerable<Panel> Children)
        {
            foreach (Panel Child in Children)
            {
                AddChild(Child);
            }
        }

        /// <summary>
        /// Add a range of Entities to this Entity's Children list.
        /// </summary>
        /// <param name="Children">Children to add.</param>
        public virtual void AddRange(params Panel[] Children)
        {
            foreach (Panel Child in Children)
            {
                AddChild(Child);
            }
        }

        /// <summary>
        /// Removes a list of Entities from this Entity's Children list.
        /// </summary>
        /// <param name="Children">Children to remove.</param>
        public virtual void RemoveRange(IEnumerable<Panel> Children)
        {
            foreach (Panel Child in Children)
            {
                RemoveChild(Child);
            }
        }

        /// <summary>
        /// Removes a range of Entities from this Entity's Children list.
        /// </summary>
        /// <param name="Children">Children to remove.</param>
        public virtual void RemoveRange(params Panel[] Children)
        {
            foreach (Panel Child in Children)
            {
                RemoveChild(Child);
            }
        }

        /// <summary>
        /// Removes the given Child from this Parent.
        /// </summary>
        /// <param name="Child">Child to Remove.</param>
        public virtual void RemoveChild(Panel Child)
        {
            UnRegister(Child);
            Children.Remove(Child);

            DoOnChildrenChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Removes all child Entities from this Entity.
        /// </summary>
        public virtual void ClearChildren()
        {
            for (int i = Children.Count; i-- > 0;)
            {
                RemoveChild(Children[i]);
            }
        }

        /// <summary>
        /// Remove this Entity from its Parent.
        /// </summary>
        public virtual void RemoveFromParent()
        {
            Parent?.RemoveChild(this);
        }

        public void Register(Panel Child)
        {
            Child.Parent = this;
            Child.ParentWindow = ParentWindow;

            OnBoundsChanged += Child.DoOnBoundsChanged;
            OnSizeChanged += Child.DoOnSizeChanged;
            OnPositionChanged += Child.DoOnPositionChanged;
            OnMouseLeave += Child.DoOnMouseLeave;
        }

        public void UnRegister(Panel Child)
        {
            Child.Parent = null;
            Child.ParentWindow = null;

            OnBoundsChanged -= Child.DoOnBoundsChanged;
            OnSizeChanged -= Child.DoOnSizeChanged;
            OnPositionChanged -= Child.DoOnPositionChanged;
            OnMouseLeave -= Child.DoOnMouseLeave;
        }

        /// <summary>
        /// Get the first immediate Child of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to filter by.</typeparam>
        /// <returns>First Child of type <typeparamref name="T"/> or Null if no such Child exists.</returns>
        public virtual T GetChild<T>() where T : Panel
        {
            return Children.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Get all immediate Children of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to filter by.</typeparam>
        /// <returns>List of Children of type <typeparamref name="T"/>.</returns>
        public virtual List<T> GetChildren<T>() where T : Panel
        {
            return Children.OfType<T>().ToList();
        }

        /// <summary>
        /// Get the first immediate child with the specified ID.
        /// </summary>
        /// <param name="ID">ID of child to search for.</param>
        /// <returns>Child that has the specified ID or null if not found.</returns>
        public virtual Panel GetChildWithID(string ID)
        {
            return Children.FirstOrDefault((F) => F.ID == ID);
        }

        /// <summary>
        /// Get a list of immediate children with the specified ID.
        /// </summary>
        /// <param name="ID">ID to filter with-</param>
        /// <returns>List of Children with the specified ID.</returns>
        public virtual List<Panel> GetChildrenWithID(string ID)
        {
            return Children.Where((F) => F.ID == ID).ToList();
        }

        /// <summary>
        /// Gets all Children reclusively through the Children's tree.
        /// </summary>
        /// <returns>List of all Children in Entity tree.</returns>
        public virtual List<Panel> GetChildrenFlatten()
        {
            List<Panel> Descendants = new List<Panel>();

            foreach (Panel Child in Children)
            {
                Descendants.Add(Child);
                Descendants.AddRange(Child.GetChildrenFlatten());
            }

            return Descendants;
        }

        /// <summary>
        /// Get the first Ancestor with the specified ID.
        /// </summary>
        /// <param name="ID">ID of the Ancestor to search for.</param>
        /// <returns>Ancestor that has the specified ID or null if not found.</returns>
        public virtual Panel GetAncestor(string ID)
        {
            Panel Ancestor = Parent;

            while (Ancestor != null)
            {
                if (Ancestor.ID == ID)
                {
                    return Ancestor;
                }

                Ancestor = Ancestor.Parent;
            }

            return null;
        }

        /// <summary>
        /// Get the first Ancestor with the specified Type.
        /// </summary>
        /// <typeparam name="T">Type of the Ancestor to search for.</typeparam>
        /// <returns>Ancestor that has the specified Type or null if not found.</returns>
        public virtual T GetAncestor<T>() where T : Panel
        {
            Panel Ancestor = Parent;
            Type Type = typeof(T);

            while (Ancestor != null)
            {
                if (Ancestor.GetType() == Type)
                {
                    return (T)Ancestor;
                }

                Ancestor = Ancestor.Parent;
            }

            return null;
        }

        /// <summary>
        /// Gets All Ancestors.
        /// </summary>
        /// <returns>List of All Ancestors.</returns>
        public virtual List<Panel> GetAncestors()
        {
            List<Panel> Ancestors = new List<Panel>();
            Panel Ancestor = Parent;

            while (Ancestor != null)
            {
                Ancestors.Add(Ancestor);
                Ancestor = Ancestor.Parent;
            }

            return Ancestors;
        }

        /// <summary>
        /// Moves the specified Child to the Top of the Children's list, this will make it draw first.
        /// </summary>
        /// <param name="Child">Child to Move.</param>
        public virtual void BringToFront(Panel Child)
        {
            Children.Remove(Child);
            Children.Insert(0, Child);
        }

        /// <summary>
        /// Moves the specified Child to the Back of the Children's list, this will make it draw last.
        /// </summary>
        /// <param name="Child">Child to Move.</param>
        public virtual void BringToBack(Panel Child)
        {
            Children.Remove(Child);
            Children.Add(Child);
        }

        /// <summary>
        /// Move this Entity to the front of its Parent's Children list, this will make it draw first.
        /// </summary>
        public virtual void MoveToFront()
        {
            Parent.BringToFront(this);
        }

        /// <summary>
        /// Move this Entity to the back of its Parent's Children list, this will make it draw last.
        /// </summary>
        public virtual void MoveToBack()
        {
            Parent.BringToBack(this);
        }

        /// <summary>
        /// Sets and loads the specified Texture, will not update Texture if <paramref name="Path"/> is the same as <see cref="TexturePath"/>.
        /// </summary>
        /// <param name="Path">Path to Texture file as a string.</param>
        public void SetTexture(string Path)
        {
            if (Path != TexturePath)
            {
                TexturePath = Path;
                Texture = ContentFinder<Texture2D>.Get(TexturePath);
            }
        }

        /// <summary>
        /// Clears the <see cref="Texture"/> and the <see cref="TexturePath"/>.
        /// </summary>
        public void ClearTexture()
        {
            TexturePath = null;
            Texture = null;
        }

        private bool _IsMouseOver;
        /// <summary>
        /// If mouse is currently over this Entity.
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return _IsMouseOver;
            }
            set
            {
                if (!IgnoreMouse)
                {
                    // Don't trigger mouseover if not visible
                    if (IsVisible)
                    {
                        if (_IsMouseOver != value)
                        {
                            bool Old_IsMouseOver = _IsMouseOver;
                            // Need to apply it because it might be used in below Callback Events.
                            _IsMouseOver = value;

                            if (value && !Old_IsMouseOver)
                            {
                                DoOnMouseEnter(this, MouseEventArgs.Empty);
                            }
                            else if (!value && Old_IsMouseOver)
                            {
                                DoOnMouseLeave(this, MouseEventArgs.Empty);
                            }
                        }

                        if (value)
                        {
                            DoOnWhileMouseOver(this, MouseEventArgs.Empty);
                        }
                    }
                }
                else
                {
                    _IsMouseOver = false;
                }
            }
        }
        /// <summary>
        /// If mouse is currently pressed down over this Entity.
        /// </summary>
        public virtual bool IsMouseDown { get; set; }

        /// <summary>
        /// Current color for text, handles switching between color states like MouseOver and Disabled.
        /// </summary>
        /// <returns>Current color that text should be rendered with.</returns>
        public virtual Color GetActiveTextColor()
        {
            return ((!IsMouseOver || !IsEnabled) ? Style.TextColor : Style.MouseOverTextColor);
        }

        /// <summary>
        /// Checks if mouse is over.
        /// </summary>
        /// <param name="MousePosition">Position of the mouse relative to the TopLeft corner of this Entity's WindowPanel.</param>
        /// <param name="ChildOffset">Optional Offset to apply when checking Children MouseOver.</param>
        /// <returns>True if mouse is currently inside this Entity's bounds.</returns>
        public virtual bool GetMouseOver(Vector2 MousePosition, Vector2 ChildOffset = default)
        {
            if (!IgnoreMouse && Bounds.Contains(MousePosition))
            {
                IsMouseOver = true;

                if (CanCascadeMouseInput())
                {
                    MousePosition += ChildOffset;

                    foreach (Panel Child in Children)
                    {
                        Child.GetMouseOver(MousePosition);
                    }
                }
            }
            else
            {
                IsMouseOver = false;
            }

            return IsMouseOver;
        }

        /// <summary>
        /// Gets the actual position of this Entity's TopLeft Corner on the screen.
        /// </summary>
        /// <returns>This Entity's position on the screen.</returns>
        public virtual Vector2 GetAbsolutePosition()
        {
            return ParentWindow.Position + Position;
        }

        /// <summary>
        /// Gets the cumulative Offset of this Entity in its Parent tree.
        /// </summary>
        /// <returns>This Entity's total Offset in its Parent tree.</returns>
        public virtual Vector2 GetAbsoluteOffset()
        {
            Vector2 AbsoluteOffset = Offset;
            Panel Ancestor = Parent;

            while (Ancestor != null)
            {
                AbsoluteOffset += Ancestor.Offset;
                Ancestor = Ancestor.Parent;
            }

            return AbsoluteOffset;
        }

        /// <summary>
        /// Used for Debugging to get its Position in the Entity Tree, all the way up to its ParentWindow, if it has one.
        /// </summary>
        /// <returns>String Path of Ancestors.</returns>
        public string GetAbsolutePath()
        {
            string Path = GetType().Name;
            Panel Ancestor = Parent;

            while (Ancestor != null)
            {
                Path = (Ancestor.GetType().Name + "." + Path);
                Ancestor = Ancestor.Parent;
            }

            if (ParentWindow != null)
            {
                Path = (ParentWindow.GetType().Name + "." + Path);
            }

            return Path;
        }

        /// <summary>
        /// Sets the Style Palette of this Entity.
        /// </summary>
        /// <param name="Palette">Name of Style Palette to apply.</param>
        /// <remarks>Use to apply additionally coloring changes to derived entities.</remarks>
        public virtual void SetStyle(string Palette)
        {
            Style.SetHierarchy(Palette);
        }
    }
}
