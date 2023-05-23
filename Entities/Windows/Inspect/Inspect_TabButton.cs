using BigMod.Entities.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigMod.Entities.Windows.Inspect
{
    public class Inspect_TabButton : Button
    {
        public InspectTabBase TabBase;
        public static MethodInfo ExtraOnGUI = typeof(InspectTabBase).GetMethod("ExtraOnGUI", BindingFlags.Instance | BindingFlags.NonPublic);
        public static MethodInfo FillTab = typeof(InspectTabBase).GetMethod("FillTab", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo size = typeof(InspectTabBase).GetField("size", BindingFlags.Instance | BindingFlags.NonPublic);
        public static PropertyInfo TabRect = typeof(InspectTabBase).GetProperty("TabRect", BindingFlags.Instance | BindingFlags.NonPublic);
        public static PropertyInfo StillValid = typeof(InspectTabBase).GetProperty("StillValid", BindingFlags.Instance | BindingFlags.NonPublic);
        public Thing Target;
        public Rect TabWindow_Bounds;
        public Inspect_TabButton(InspectTabBase TabBase) : base(TabBase.labelKey.Translate(), true)
        {
            this.TabBase = TabBase;

            Label.SetStyle("Inspect_TabButton.Label");
            Style.DrawBackground = true;
            Label.Width = Label.GetTextSize().x;
            Width = (Label.Width + 8f);
            Height = 20f;

            TabWindow_Bounds = (Rect)TabRect.GetValue(TabBase);
        }
        public override void Draw()
        {
            base.Draw();

            if (ToggleState)
            {
                // Modified from InspectTabBase.DoTabGUI
                Find.WindowStack.ImmediateWindow(235086, TabWindow_Bounds, WindowLayer.Super, () =>
                {
                    if (!(bool)StillValid.GetValue(TabBase) || !TabBase.IsVisible)
                    {
                        ToggleState = false;
                        return;
                    }
                    if (Widgets.CloseButtonFor(TabWindow_Bounds.AtZero()))
                    {
                        ToggleState = false;
                        return;
                    }

                    try
                    {
                        FillTab.Invoke(TabBase, null);
                    }
                    catch (Exception Exception)
                    {
                        Log.ErrorOnce($"Exception filling tab {TabBase.GetType()} : {Exception}", 49827);
                    }

                }, true, false, 1f, () => 
                {
                    TabBase.Notify_ClickOutsideWindow();

                    // Would otherwise toggle twice if mouse over.
                    if (!IsMouseOver)
                    {
                        ToggleState = false;
                    }
                });

                ExtraOnGUI.Invoke(TabBase, null);
            }
        }
        public override void Update()
        {
            base.Update();

            // Don't Pull if not expanded.
            if (ToggleState)
            {
                TabBase.TabUpdate();
            }
        }
        public override void DoOnToggleStateChanged()
        {
            base.DoOnToggleStateChanged();

            if (ToggleState)
            {
                // Update Position
                DoOnPositionChanged(this, EventArgs.Empty);

                Inspect.TempTarget = Target;

                // Deselect all other Tabs. As only one is valid to be displayed at a time.
                if (Parent is ListViewItem_Inspect Item)
                {
                    foreach (Inspect_TabButton Tab in Item.Tabs)
                    {
                        if (Tab != this)
                        {
                            Tab.ToggleState = false;
                        }
                    }
                }
            }
        }
        public override void DoOnPositionChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnPositionChanged(Sender, EventArgs);

            if (ParentWindow != null)
            {
                Vector2 AbsolutePosition = Parent.GetAbsolutePosition();
                
                TabWindow_Bounds = new Rect(AbsolutePosition.x, (AbsolutePosition.y - TabWindow_Bounds.height), TabWindow_Bounds.width, TabWindow_Bounds.height);
            }
        }
    }
}
