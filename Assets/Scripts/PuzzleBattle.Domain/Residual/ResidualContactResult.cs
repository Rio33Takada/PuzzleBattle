namespace PuzzleBattle.Domain.Residual
{
    public sealed class ResidualContactResult
    {
        internal ResidualContactResult(
            ResidualPiece residualPiece,
            bool contacted,
            bool destroyed,
            int playerDamage)
        {
            ResidualPiece = residualPiece;
            Contacted = contacted;
            Destroyed = destroyed;
            PlayerDamage = playerDamage;
        }

        public ResidualPiece ResidualPiece { get; }
        public bool Contacted { get; }
        public bool Destroyed { get; }
        public int PlayerDamage { get; }
    }
}
