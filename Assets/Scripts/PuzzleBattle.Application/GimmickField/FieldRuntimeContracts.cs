using System;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Application.GimmickField
{
    public readonly struct FieldRuntimeId : IEquatable<FieldRuntimeId>
    {
        public FieldRuntimeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A field runtime ID is required.", nameof(value));
            Value = value;
        }
        public string Value { get; }
        public bool Equals(FieldRuntimeId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is FieldRuntimeId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
    }

    public interface IFieldRuntimeState
    {
        FieldRuntimeId OwnerId { get; }
    }

    public interface IBattleFieldRuntime
    {
        FieldRuntimeId Id { get; }
        GridShape Range { get; }
        IFieldRuntimeState CaptureState();
        void RestoreState(IFieldRuntimeState state);
        void AdvanceTurn();
    }

    public sealed class GimmickFieldEntryContext
    {
        internal GimmickFieldEntryContext(
            FieldRuntimeId normalFieldId,
            GridShape normalRange,
            IFieldRuntimeState suspendedNormalState)
        {
            NormalFieldId = normalFieldId;
            NormalRange = normalRange ?? throw new ArgumentNullException(nameof(normalRange));
            SuspendedNormalState = suspendedNormalState ?? throw new ArgumentNullException(nameof(suspendedNormalState));
        }
        public FieldRuntimeId NormalFieldId { get; }
        public GridShape NormalRange { get; }
        public IFieldRuntimeState SuspendedNormalState { get; }
    }

    public interface IFieldReturnEffect { }

    public sealed class GimmickFieldExitContext
    {
        public GimmickFieldExitContext(IFieldRuntimeState finalState, IFieldReturnEffect additionalEffect = null)
        {
            FinalState = finalState ?? throw new ArgumentNullException(nameof(finalState));
            AdditionalEffect = additionalEffect;
        }
        public IFieldRuntimeState FinalState { get; }
        public IFieldReturnEffect AdditionalEffect { get; }
    }

    public interface IGimmickFieldRuntime : IBattleFieldRuntime
    {
        void Enter(GimmickFieldEntryContext context);
        GimmickFieldExitContext Exit();
    }

    public interface IFieldReturnEffectApplier
    {
        void Apply(IBattleFieldRuntime restoredNormalField, IFieldReturnEffect effect);
    }
}
