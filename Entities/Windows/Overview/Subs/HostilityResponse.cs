using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    /// <summary>
    /// Created its own Class to reduce complexity of <see cref="OverviewPawn"/>.
    /// </summary>
    public class HostilityResponse : DropDown, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public HostilityResponseMode Mode;

        public HostilityResponse(float Width, float Height) : base(ButtonStyle.Image, Globals.TryGetTexturePathFromAlias("Medicine"))
        {
            Size = new Vector2(Width, Height);

            Style.DrawBackground = true;

            ToolTipText = "CarryMedicineQuality".Translate();

            Populate();
        }

        public void Populate()
        {
            ListView.Clear();

            string[] Names = Enum.GetNames(typeof(HostilityResponseMode));

            for (int i = 0; i < Names.Length; i++)
            {
                string Name = Names[i];
                HostilityResponseMode HostilityResponseMode = (HostilityResponseMode)Enum.Parse(typeof(HostilityResponseMode), Name);

                ListViewItem Item = new ListViewItem(HostilityResponseMode.GetLabel());
                Item.Data = HostilityResponseMode;

                Item.Header.Label.Style.TextAnchor = TextAnchor.MiddleLeft;
                Item.Header.Label.Style.FontType = GameFont.Tiny;

                Item.Image = new Image();
                Item.Image.Size = new Vector2(Item.Height, Item.Height);
                Item.Image.Texture = HostilityResponseMode.GetIcon();
                Item.Image.Style.Color = Color.white;
                Item.AddChild(Item.Image);

                Item.Header.Label.OffsetX = (Item.Image.Width + 5f);

                AddItem(Item);
            }

            if (ListView.Items.Any())
            {
                // Ensure Items are large enough to show all text without clipping.
                SizeExpanded.x = Mathf.Max(SizeExpanded.x, (ListView.Items.Max((F) => F.Header.Label.GetTextSize().x) + ListView.Items.First().Height + 5f));
            }
        }

        public void Pull()
        {
            Mode = Pawn.playerSettings.hostilityResponse;

            Image.Texture = HostilityResponseModeUtility.GetIcon(Mode);
            ToolTipText = "HostilityReponseTip".Translate() + "\n\n" + "HostilityResponseCurrentMode".Translate() + " : " + HostilityResponseModeUtility.GetLabel(Mode);
        }

        public void Push()
        {
            Pawn.playerSettings.hostilityResponse = Mode;
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }

        public override void DoOnClickedItem(object Sender, EventArgs EventArgs)
        {
            base.DoOnClickedItem(Sender, EventArgs);

            Mode = (HostilityResponseMode)((ListViewItem)Sender).Data;
            Push();
            Pull();
        }
    }
}
