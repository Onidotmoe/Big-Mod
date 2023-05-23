using BigMod.Entities.Interface;
using RimWorld;
using System.Reflection;
using Verse;

namespace BigMod.Entities.Windows
{
    // TODO: Missing Clickthrough

    /// <summary>
    /// Replaces <see cref="WeatherManager.DoWeatherGUI"/> & <see cref="GlobalControls.TemperatureString"/> in the Bottom right of the Screen.
    /// </summary>
    public class DisplayWeather : WindowPanel, IOnRequest
    {
        public event EventHandler OnRequest;
        public int RequestCurrent { get; set; }
        // Vanilla update rate is every frame.
        public int RequestRate { get; set; } = 110;

        public override Rect DefaultBounds { get; set; } = new Rect((UI.screenWidth - 500f), (UI.screenHeight - 25f), 150f, 25f);

        /// <summary>
        /// Exposes the private static method in the GlobalControls.
        /// </summary>
        /// <remarks>Also available in <see cref="Globals.GlobalControls_TemperatureString"/>, here because we don't want to search for the method each time.</remarks>
        private static MethodInfo Method_TemperatureString = typeof(GlobalControls).GetMethod("TemperatureString", (BindingFlags.NonPublic | BindingFlags.Static));
        // TODO: GlobalControls.GlobalControlsOnGUI needs to be disabled but first all other stuff has to be extracted out
        public string Text
        {
            get
            {
                return Label.Text;
            }
            set
            {
                Label.Text = value;
            }
        }
        public Label Label = new Label();

        public DisplayWeather() : base()
        {
            Identifier = "Window_DisplayWeather";

            DrawBackground = false;
            DrawBorder = false;

            // Allow mouse passthrough.
            absorbInputAroundWindow = false;

            // Align the Text to the Right to prevent the changing numbers from moving the rest of the text around.
            Label.Style.TextAnchor = TextAnchor.MiddleRight;
            Label.InheritParentSize = true;
            Root.AddChild(Label);

            Root.OnMouseEnter += DoOnMouseEnter;

            IsLocked = true;
        }

        public override void Update()
        {
            base.Update();

            if (RequestCurrent >= RequestRate)
            {
                RequestCurrent = 0;

                if (IsVisible)
                {
                    DoOnRequest();
                }
            }
            else
            {
                RequestCurrent++;
            }
        }

        public void DoOnRequest()
        {
            // Do not raise the OnRequest Event here as this will run very often.

            Text = $"{GetTemperatureString()} {Find.CurrentMap.weatherManager.CurWeatherPerceived.LabelCap}";
        }

        public void DoOnMouseEnter(object Sender, MouseEventArgs MouseEventArgs)
        {
            Root.ToolTipText = Find.CurrentMap.weatherManager.CurWeatherPerceived.description;
        }

        public static string GetTemperatureString()
        {
            return (string)Method_TemperatureString.Invoke(null, null);
        }
    }
}
