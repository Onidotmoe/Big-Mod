using Verse;

namespace BigMod.Entities.Interface
{
    /// <summary>
    /// A class with this Interface has a <see cref="Verse.Pawn"/> property.
    /// </summary>
    public interface IPawn
    {
        /// <summary>
        /// A RimWold Pawn.
        /// </summary>
        public Pawn Pawn { get; set; }

        /// <summary>
        /// Sets <see cref="Pawn"/>.
        /// </summary>
        /// <param name="Pawn">Pawn to set.</param>
        public void SetPawn(Pawn Pawn);
    }
}
