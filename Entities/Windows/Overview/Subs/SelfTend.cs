using BigMod.Entities.Interface;
using RimWorld;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class SelfTend : Button, IPawn, IPull
    {
        public Pawn Pawn { get; set; }
        public bool IsActive
        {
            get
            {
                return Pawn.playerSettings.selfTend;
            }
            set
            {
                Pawn.playerSettings.selfTend = value;
            }
        }

        public SelfTend(float Width, float Height) : base(ButtonStyle.Image)
        {
            Size = new Vector2(Width, Height);

            Style.DrawBackground = true;
            Style.BorderThickness = 2;

            Image.Texture = Globals.GetTextureFromAlias("SelfTend");
        }

        public void Pull()
        {
            // Check if it's Disabled elsewhere somehow.
            CanToggle = (Pawn.IsColonist && !Pawn.Dead && !Pawn.DevelopmentalStage.Baby() && !Pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) && (Pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor) > 0));
            IsLocked = !CanToggle;

            ToggleState = IsActive;

            if (!CanToggle)
            {
                if (Pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
                {
                    ToolTipText = "MessageCannotSelfTendEver".Translate(Pawn.LabelShort, Pawn);
                }
                else if (Pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor) == 0)
                {
                    ToolTipText = "MessageSelfTendUnsatisfied".Translate(Pawn.LabelShort, Pawn);
                }
            }
            else
            {
                ToolTipText = "SelfTendTip".Translate(RimWorld.Faction.OfPlayer.def.pawnsPlural, 0.7f.ToStringPercent()).CapitalizeFirst();
            }
        }

        public override void DoOnToggleStateChanged()
        {
            base.DoOnToggleStateChanged();

            if (CanToggle)
            {
                IsActive = ToggleState;
            }

            Style.DrawBorder = ToggleState;
        }

        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }
    }
}
