using BigMod.Entities.Interface;
using Verse;

namespace BigMod.Entities.Windows.Overview
{
    public class ListView_Stats : ListView, IPawn, IPull
    {
        public Pawn Pawn { get; set; }

        public ListView_Stats() : base(50f, 50f)
        {
        }

        public ListView_Stats(float Width, float Height) : base(Width, Height)
        {
        }

        public ListView_Stats(Pawn Pawn, float Width, float Height) : base(Width, Height)
        {
            SetPawn(Pawn);
        }

        public ListView_Stats(Vector2 Size, Vector2 Offset, bool ShowScrollbarVertical = false) : this(Size.x, Size.y)
        {
            this.ShowScrollbarVertical = ShowScrollbarVertical;
            this.Offset = Offset;

            if (!ShowScrollbarVertical)
            {
                ScrollbarSize = 0f;
            }
        }

        /// <summary>
        /// Used to Pull data to this Entity.
        /// </summary>
        public virtual void Pull()
        { }

        public virtual void SetPawn(Pawn Pawn)
        {
            this.Pawn = Pawn;
        }
    }
}
