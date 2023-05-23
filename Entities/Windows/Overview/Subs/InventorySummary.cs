using BigMod.Entities.Interface;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigMod.Entities.Windows.Overview.Subs
{
    public class InventorySummary : Panel, IPawn, IPull
    {
        public Label Label = new Label();
        public Pawn Pawn { get; set; }
        public ProgressBar ProgressBar = new ProgressBar() {Anchor = Anchor.BottomLeft, DoRequest = false};
        public float Capacity
        {
            get => MassUtility.Capacity(Pawn);
        }
        public string CapacityExplanation
        {
            get
            {
                StringBuilder StringBuilder = new StringBuilder();
                MassUtility.Capacity(Pawn, StringBuilder);
                return StringBuilder.ToString();
            }
        }

        public float CarryMass
        {
            get => MassUtility.GearAndInventoryMass(Pawn);
        }
        public float FreeSpace
        {
            get => MassUtility.FreeSpace(Pawn);
        }
        public bool PawnCanCarry
        {
            get => MassUtility.CanEverCarryAnything(Pawn);
        }
        public InventorySummary(float Width, float Height)
        {
            Size = new Vector2(Width, Height);

            Style.DrawBackground = true;

            ProgressBar.Size = new Vector2(Width, (Height / 10f));
            Label.InheritParentSize = true;
            Label.RenderStyle = LabelStyle.GUI;
            Label.Style.TextAnchor = TextAnchor.MiddleRight;
            Label.Style.TextOffset = new Vector2(-2f, -(ProgressBar.Height / 2));

            // Colors are swapped in Sheet.xml
            // Having less space is negative, having more is positive.
            ProgressBar.ColorMax = Globals.GetColor("InventorySummary.ProgressBar.ColorMax");
            ProgressBar.ColorMin = Globals.GetColor("InventorySummary.ProgressBar.ColorMin");

            AddRange(Label, ProgressBar);
        }
        public InventorySummary(Vector2 Size, Vector2 Offset = default) : this(Size.x, Size.y)
        {
            this.Offset = Offset;
        }
        public void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }
        public void Pull()
        {
            if (!IsVisible)
            {
                return;
            }

            // Pawn can become unable to carry things after having been able.
            IsLocked = !PawnCanCarry;

            if (IsLocked)
            {
                ToolTipText = IsLocked ? "InventorySummary_CantCarry".Translate() : string.Empty;
                Label.Text = "InventorySummary_CantCarry_Label".Translate();
            }
            else
            {
                ToolTipText = $"{"InventorySummary_CarryCapacity".Translate()}\n\n{CapacityExplanation}";
                // Using FreeSpace so we don't get invalid values.
                ProgressBar.Percentage = (Capacity - FreeSpace) / Capacity;
                // We can exceed our capacity.
                Label.Text = $"{CarryMass}{"Kg".Translate()} / {Capacity}{"Kg".Translate()}";
            }
        }
    }
}
