using Verse;

namespace BigMod.Entities.Windows
{
    public class ToolTip : WindowPanel, IDisposable
    {
        public Panel ToolTipHost;
        public Rect ToolTipHostRect;
        /// <summary>
        /// When the ToolTip is connected to the mouse and manually removed from it.
        /// </summary>
        public bool ManualDisposal;
        public bool AttachedToMouse;
        /// <summary>
        /// Additional Offset to be applied on render.
        /// </summary>
        public Vector2 Offset = Vector2.zero;
        public bool CloseOnMouseOutsideClick;

        public ToolTip()
        {
            // Make it appear above everything else.
            layer = WindowLayer.Super;

            IsDraggable = false;
            IsResizable = false;
            IsLockable = false;
            IgnoreMouseInput = true;
            CanReset = false;
            AttachedToMouse = true;
            AllowMultipleInstance = true;

            Root.Style.DrawBackground = true;
            Root.Style.DrawBorder = true;
            Root.Style.BorderThickness = 1;
        }

        public void Dispose()
        {
            WindowManager.Instance.RemoveToolTip(this);
        }

        public override void Update()
        {
            if (CloseOnMouseOutsideClick && !IsMouseOver && WindowManager.IsMouseUpAny() && (((ToolTipHost != null) && !ToolTipHost.IsMouseOver) || ((ToolTipHostRect != default) && !ToolTipHostRect.Contains(WindowManager.GetMousePosition()))))
            {
                Dispose();
            }
            if (AttachedToMouse)
            {
                Position = (WindowManager.GetMousePosition() + Offset);
            }
            if (!ManualDisposal && (((ToolTipHost != null) && !ToolTipHost.IsMouseOver) || ((ToolTipHostRect != default) && !ToolTipHostRect.Contains(WindowManager.GetMousePosition()))))
            {
                Dispose();
            }
            else
            {
                base.Update();
            }
        }
    }
}
