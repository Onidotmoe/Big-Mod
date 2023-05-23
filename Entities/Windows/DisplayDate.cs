using BigMod.Entities.Interface;
using RimWorld;
using System.Text;
using Verse;

namespace BigMod.Entities.Windows
{
    // TODO: Missing Clickthrough

    /// <summary>
    /// Replaces <see cref="DateReadout"/> in the Bottom right of the Screen.
    /// </summary>
    public class DisplayDate : WindowPanel, IOnRequest
    {
        public event EventHandler OnRequest;
        public int RequestCurrent { get; set; }
        // Vanilla update rate is every frame.
        public int RequestRate { get; set; } = 110;

        public override Rect DefaultBounds { get; set; } = new Rect((UI.screenWidth - 325f), (UI.screenHeight - 25f), 300f, 25f);

        public string Date;
        public int Hour;
        public int Day = -1;
        public int Year = -1;
        public Season Season = Season.Undefined;
        public Quadrum Quadrum = Quadrum.Undefined;

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

        public DisplayDate() : base()
        {
            Identifier = "Window_DisplayDate";

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

            Vector2 GlobalPosition = GetGlobalPosition();

            if (GlobalPosition != default)
            {
                IsVisible = true;

                long Ticks = Find.TickManager.TicksAbs;

                Hour = GenDate.HourInteger(Ticks, GlobalPosition.x);
                Day = GenDate.DayOfTwelfth(Ticks, GlobalPosition.x);
                Year = GenDate.Year(Ticks, GlobalPosition.x);
                Season = GenDate.Season(Ticks, GlobalPosition);
                Quadrum = GenDate.Quadrum(Ticks, GlobalPosition.x);

                int Minutes = (int)Math.Floor((Ticks % 2500f) / 60f);
                int DayOfSeason = (GenDate.DayOfSeason(Ticks, GlobalPosition.x) + 1);
                string Date = "DateReadout".Translate(Find.ActiveLanguageWorker.OrdinalNumber(DayOfSeason, Gender.None), Quadrum.Label(), Year, DayOfSeason);

                Text = $"{Hour:00}:{Minutes:00} {Date} {Season.LabelCap()}";
            }
            else
            {
                IsVisible = false;
            }
        }

        public Vector2 GetGlobalPosition()
        {
            Vector2 Vector = default;

            if (Find.WorldSelector.selectedTile >= 0)
            {
                Vector = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile);
            }
            else if (Find.WorldSelector.NumSelectedObjects > 0)
            {
                Vector = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
            }
            else if (Find.CurrentMap != null)
            {
                Vector = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
            }

            return Vector;
        }

        public void DoOnMouseEnter(object Sender, MouseEventArgs MouseEventArgs)
        {
            StringBuilder StringBuilder = new StringBuilder();

            Vector2 GlobalPosition = GetGlobalPosition();

            for (int i = 0; i < 4; i++)
            {
                Quadrum Quadrum = (Quadrum)i;

                StringBuilder.AppendLine($"{Quadrum.GetSeason(GlobalPosition.y).LabelCap()} - {Quadrum.Label()}");
            }

            Root.ToolTipText = "DateReadoutTip".Translate(GenDate.DaysPassed, 15, Season.LabelCap(), 15, Quadrum.Label(), StringBuilder.ToString());
        }
    }
}
