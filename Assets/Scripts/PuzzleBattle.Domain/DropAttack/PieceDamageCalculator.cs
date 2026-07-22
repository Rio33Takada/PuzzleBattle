using System;

namespace PuzzleBattle.Domain.DropAttack
{
    public interface IPieceDamageCalculator
    {
        int Calculate(int cellPower, int ownerAttack, int targetDefense, decimal stunMultiplier);
    }

    /// <summary>
    /// Owns the provisional FLW-013 formula so it can be replaced without changing hit resolution.
    /// </summary>
    public sealed class PieceDamageCalculator : IPieceDamageCalculator
    {
        public int Calculate(int cellPower, int ownerAttack, int targetDefense, decimal stunMultiplier)
        {
            if (cellPower < 0)
                throw new ArgumentOutOfRangeException(nameof(cellPower), cellPower, "Cell power cannot be negative.");
            if (ownerAttack < 0)
                throw new ArgumentOutOfRangeException(nameof(ownerAttack), ownerAttack, "Attack cannot be negative.");
            if (targetDefense < 0)
                throw new ArgumentOutOfRangeException(nameof(targetDefense), targetDefense, "Defense cannot be negative.");
            if (stunMultiplier <= 0m)
                throw new ArgumentOutOfRangeException(nameof(stunMultiplier), stunMultiplier, "Stun multiplier must be positive.");

            decimal damageBeforeStun = Math.Max(0m, (decimal)cellPower * ownerAttack - targetDefense);
            return checked((int)decimal.Floor(damageBeforeStun * stunMultiplier));
        }
    }
}
