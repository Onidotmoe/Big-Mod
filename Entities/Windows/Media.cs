using RimWorld;
using Verse;

namespace BigMod.Entities.Windows
{
    public class Media : WindowPanel
    {
        public Dictionary<TimeSpeed, Button> Speeds = new Dictionary<TimeSpeed, Button>();
        public override Rect DefaultBounds { get; set; } = new Rect((UI.screenWidth - 165f), (UI.screenHeight - 100f), 150f, 30f);

        public Color Button_Previous = Globals.GetColor("Media.Button.Previous.BackgroundColor");
        public Color Image_NotSelected = Globals.GetColor("Media.Button.Image.Color");
        public Color Image_Selected = Globals.GetColor("Media.Button.Image.Selected.Color");

        public Media()
        {
            Identifier = "Window_Media";

            CameraMouseOverZooming = false;
            DrawBorder = false;

            SetCloseOn();

            Populate();

            Root.OnMouseWheel += DoOnMouseWheel;

            // Check if should Pause OnLoad.
            if (Prefs.PauseOnLoad)
            {
                Speeds[TimeSpeed.Paused].ToggleState = true;
                SetCurrent(TimeSpeed.Paused);
            }
            else
            {
                Speeds[Previous].ToggleState = true;
                SetCurrent(Previous);
            }

            if ((Current == TimeSpeed.Paused) && (Previous == TimeSpeed.Paused))
            {
                // If Current & Previous is Paused, set Previous to Normal.
                SetPrevious(TimeSpeed.Normal);
            }

            UpdateColors();

            IsLocked = true;
        }

        public override void ExtraOnGUI()
        {
            // Key Presses in Event.current is Handled by WindowStack.WindowStackOnGUI but for the Keys to be registered when the Window isn't active, the Keys have to be checked in ExtraOnGUI which runs before WindowOnGUI.

            // Some Windows prevent unpausing the game.
            if ((Event.current.type == EventType.KeyDown) && !WindowManager.IsForcedPause())
            {
                // Keybinding work regardless of if its visible or not.
                // Game doesn't have a registering system for KeyBindings, they have to be checked manually.

                if (KeyBindingDefOf.TogglePause.KeyDownEvent)
                {
                    SetSpeed(TimeSpeed.Paused);
                }
                else if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
                {
                    SetSpeed(TimeSpeed.Normal);
                }
                else if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
                {
                    SetSpeed(TimeSpeed.Fast);
                }
                else if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
                {
                    SetSpeed(TimeSpeed.Superfast);
                }
                else if (KeyBindingDefOf.TimeSpeed_Slower.KeyDownEvent && (Current != TimeSpeed.Paused))
                {
                    AddDelta(-1f);
                }
                else if (KeyBindingDefOf.TimeSpeed_Faster.KeyDownEvent && (Current < TimeSpeed.Ultrafast))
                {
                    AddDelta(1f);
                }
                else if (KeyBindingDefOf.TimeSpeed_Ultrafast.KeyDownEvent)
                {
                    SetSpeed(TimeSpeed.Ultrafast);
                }
                else if (KeyBindingDefOf.Dev_TickOnce.KeyDownEvent && (Current == TimeSpeed.Paused))
                {
                    Find.TickManager.DoSingleTick();
                }
            }
        }

        public void Populate()
        {
            string[] Names = Enum.GetNames(typeof(TimeSpeed));

            for (int i = 0; i < Names.Length; i++)
            {
                string Name = Names[i];
                TimeSpeed TimeSpeed = (TimeSpeed)Enum.Parse(typeof(TimeSpeed), Name);

                Button Button = new Button(ButtonStyle.Image, new Vector2(30f, 30f), Globals.TryGetTexturePathFromAlias(Name), false);
                Button.SetStyle("Media.Button");
                Button.Style.DrawBackground = true;
                Button.ID = Name;
                Button.ToolTipText = Name.Translate();
                Button.Data = TimeSpeed;
                Button.OnClick += DoOnClick;
                Button.OffsetX = (Button.Width * i);
                Button.Image.Style.Color = Image_NotSelected;
                Speeds.Add(TimeSpeed, Button);
                Root.AddChild(Button);
            }
        }

        public void UpdateColors()
        {
            foreach (var Speed in Speeds)
            {
                // Sets Selected Colors.
                Speed.Value.ToggleState = (Speed.Key == Current);

                if (Speed.Key == Current)
                {
                    Speed.Value.Image.Style.Color = Image_Selected;
                }
                else if ((Speed.Key == Previous) && (Speed.Key != TimeSpeed.Paused) && (Current == TimeSpeed.Paused))
                {
                    // TimeSpeed.Paused should never be Previous speed.
                    // Previous Color is only relevant when TimeSpeed is Paused.
                    Speed.Value.Style.BackgroundColor = Button_Previous;
                    Speed.Value.Image.Style.Color = Image_NotSelected;
                }
                else
                {
                    Speed.Value.Style.BackgroundColor = Speed.Value.ColorToggleOff;
                    Speed.Value.Image.Style.Color = Image_NotSelected;
                }
            }
        }

        public void SetSpeed(TimeSpeed TimeSpeed)
        {
            if (TimeSpeed == TimeSpeed.Paused)
            {
                if (Current != TimeSpeed.Paused)
                {
                    // Previous should never be TimeSpeed.Paused.
                    SetPrevious(Current);
                    SetCurrent(TimeSpeed);
                }
                else
                {
                    // If Current is Paused, set the Previous one.
                    SetCurrent(Previous);
                }
            }
            else if (TimeSpeed == Current)
            {
                // Clicking on the Active speed should pause the game.
                SetPrevious(Current);
                SetCurrent(TimeSpeed.Paused);
            }
            else
            {
                SetPrevious(Current);
                SetCurrent(TimeSpeed);
            }

            UpdateColors();

            Event.current.Use();
        }

        public void AddDelta(float Delta)
        {
            int Index = (int)Current;
            Delta = Mathf.Clamp((Index + Delta), GetMin(), GetMax());
            TimeSpeed TimeSpeed = (TimeSpeed)Delta;

            if (TimeSpeed != Current)
            {
                SetSpeed(TimeSpeed);
            }
        }

        public int GetMin()
        {
            return Enum.GetValues(typeof(TimeSpeed)).Cast<byte>().Min();
        }

        public int GetMax()
        {
            return Enum.GetValues(typeof(TimeSpeed)).Cast<byte>().Max();
        }

        public void SetPrevious(TimeSpeed TimeSpeed)
        {
            Find.TickManager.prePauseTimeSpeed = TimeSpeed;
        }

        public void SetCurrent(int TimeSpeed)
        {
            SetCurrent((TimeSpeed)TimeSpeed);
        }

        public void SetCurrent(TimeSpeed TimeSpeed)
        {
            Find.TickManager.CurTimeSpeed = TimeSpeed;
        }

        public TimeSpeed Previous
        {
            get
            {
                return Find.TickManager.prePauseTimeSpeed;
            }
        }

        public TimeSpeed Current
        {
            get
            {
                return Find.TickManager.CurTimeSpeed;
            }
        }

        public void DoOnClick(object Sender, EventArgs MouseEventArgs)
        {
            SetSpeed((TimeSpeed)((Button)Sender).Data);
        }

        public void DoOnMouseWheel(object Sender, MouseEventArgs MouseEventArgs)
        {
            AddDelta(MouseEventArgs.Delta);
        }
    }
}
