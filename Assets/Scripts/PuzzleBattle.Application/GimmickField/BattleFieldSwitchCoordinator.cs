using System;
using PuzzleBattle.Application.BattleFlow;

namespace PuzzleBattle.Application.GimmickField
{
    public sealed class FieldRestorationResult
    {
        internal FieldRestorationResult(
            IFieldRuntimeState restoredNormalState,
            IFieldRuntimeState finalGimmickState,
            IFieldReturnEffect appliedEffect)
        {
            RestoredNormalState = restoredNormalState;
            FinalGimmickState = finalGimmickState;
            AppliedEffect = appliedEffect;
        }
        public IFieldRuntimeState RestoredNormalState { get; }
        public IFieldRuntimeState FinalGimmickState { get; }
        public IFieldReturnEffect AppliedEffect { get; }
    }

    public sealed class BattleFieldSwitchCoordinator
    {
        private readonly IBattleFieldRuntime normalField;
        private readonly IFieldReturnEffectApplier effectApplier;
        private IFieldRuntimeState suspendedNormalState;
        private IGimmickFieldRuntime gimmickField;

        public BattleFieldSwitchCoordinator(
            IBattleFieldRuntime normalField,
            IFieldReturnEffectApplier effectApplier)
        {
            this.normalField = normalField ?? throw new ArgumentNullException(nameof(normalField));
            this.effectApplier = effectApplier ?? throw new ArgumentNullException(nameof(effectApplier));
            ActiveField = normalField;
        }

        public IBattleFieldRuntime NormalField => normalField;
        public IBattleFieldRuntime ActiveField { get; private set; }
        public bool IsGimmickFieldActive => gimmickField != null;

        public void Activate(IGimmickFieldRuntime field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (IsGimmickFieldActive) throw new InvalidOperationException("A gimmick field is already active.");
            if (field.Id.Equals(normalField.Id)) throw new ArgumentException("Field runtime IDs must be distinct.", nameof(field));

            IFieldRuntimeState snapshot = normalField.CaptureState()
                ?? throw new InvalidOperationException("Normal field did not provide a state snapshot.");
            if (!snapshot.OwnerId.Equals(normalField.Id))
                throw new InvalidOperationException("Normal field snapshot owner does not match the field.");

            field.Enter(new GimmickFieldEntryContext(normalField.Id, normalField.Range, snapshot));
            suspendedNormalState = snapshot;
            gimmickField = field;
            ActiveField = field;
        }

        public void AdvanceActiveFieldTurn() => ActiveField.AdvanceTurn();

        public FieldRestorationResult RestoreNormalField()
        {
            if (!IsGimmickFieldActive) throw new InvalidOperationException("No gimmick field is active.");
            GimmickFieldExitContext exit = gimmickField.Exit()
                ?? throw new InvalidOperationException("Gimmick field did not provide an exit context.");
            if (!exit.FinalState.OwnerId.Equals(gimmickField.Id))
                throw new InvalidOperationException("Gimmick field snapshot owner does not match the field.");

            IFieldRuntimeState normalState = suspendedNormalState;
            normalField.RestoreState(normalState);
            ActiveField = normalField;
            gimmickField = null;
            suspendedNormalState = null;
            if (exit.AdditionalEffect != null)
                effectApplier.Apply(normalField, exit.AdditionalEffect);

            return new FieldRestorationResult(normalState, exit.FinalState, exit.AdditionalEffect);
        }
    }

    public sealed class ActiveFieldTurnEndService : IEnemyTurnEndService
    {
        private readonly BattleFieldSwitchCoordinator coordinator;
        public ActiveFieldTurnEndService(BattleFieldSwitchCoordinator coordinator) =>
            this.coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        public void Execute(BattleFlowSession session) => coordinator.AdvanceActiveFieldTurn();
    }
}
