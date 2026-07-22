using System.Collections.Generic;
using NUnit.Framework;
using PuzzleBattle.Application.GimmickField;
using PuzzleBattle.Domain.Grid;

namespace PuzzleBattle.Application.Tests
{
    public sealed class GimmickFieldBoundaryTests
    {
        [Test]
        public void Activate_SuspendsNormalStateAndAllowsDifferentGridRange()
        {
            var normal = new FakeField("normal", GridShape.CreateRectangle(3, 3), 7);
            var gimmick = new FakeGimmickField("gimmick", GridShape.CreateRectangle(1, 4), 20);
            var coordinator = new BattleFieldSwitchCoordinator(normal, new RecordingEffectApplier());

            coordinator.Activate(gimmick);

            Assert.That(coordinator.ActiveField, Is.SameAs(gimmick));
            Assert.That(coordinator.IsGimmickFieldActive, Is.True);
            Assert.That(gimmick.Entry.NormalRange, Is.EqualTo(normal.Range));
            Assert.That(((FakeState)gimmick.Entry.SuspendedNormalState).Value, Is.EqualTo(7));
            Assert.That(gimmick.Range, Is.Not.EqualTo(normal.Range));
        }

        [Test]
        public void AdvanceTurn_OnlyAdvancesActiveField()
        {
            var normal = new FakeField("normal", GridShape.CreateRectangle(2, 2), 0);
            var gimmick = new FakeGimmickField("gimmick", GridShape.CreateRectangle(1, 1), 0);
            var coordinator = new BattleFieldSwitchCoordinator(normal, new RecordingEffectApplier());

            coordinator.AdvanceActiveFieldTurn();
            coordinator.Activate(gimmick);
            coordinator.AdvanceActiveFieldTurn();
            coordinator.AdvanceActiveFieldTurn();

            Assert.That(normal.AdvancedTurns, Is.EqualTo(1));
            Assert.That(gimmick.AdvancedTurns, Is.EqualTo(2));
        }

        [Test]
        public void Restore_ReturnsToCapturedNormalStateThenAppliesAdditionalEffect()
        {
            var calls = new List<string>();
            var normal = new FakeField("normal", GridShape.CreateRectangle(2, 2), 5, calls);
            var effect = new FakeEffect();
            var gimmick = new FakeGimmickField("gimmick", GridShape.CreateRectangle(1, 1), 9)
            { ExitEffect = effect };
            var applier = new RecordingEffectApplier(calls);
            var coordinator = new BattleFieldSwitchCoordinator(normal, applier);
            coordinator.Activate(gimmick);
            normal.Value = 99;

            FieldRestorationResult result = coordinator.RestoreNormalField();

            Assert.That(coordinator.ActiveField, Is.SameAs(normal));
            Assert.That(coordinator.IsGimmickFieldActive, Is.False);
            Assert.That(normal.Value, Is.EqualTo(5));
            Assert.That(result.FinalGimmickState.OwnerId, Is.EqualTo(gimmick.Id));
            Assert.That(result.AppliedEffect, Is.SameAs(effect));
            Assert.That(calls, Is.EqualTo(new[] { "restore", "effect" }));
        }

        [Test]
        public void InvalidNestedActivationAndRestoreWithoutActivationAreRejected()
        {
            var normal = new FakeField("normal", GridShape.CreateRectangle(1, 1), 0);
            var coordinator = new BattleFieldSwitchCoordinator(normal, new RecordingEffectApplier());
            var first = new FakeGimmickField("first", GridShape.CreateRectangle(1, 1), 0);
            var second = new FakeGimmickField("second", GridShape.CreateRectangle(1, 1), 0);

            Assert.Throws<System.InvalidOperationException>(() => coordinator.RestoreNormalField());
            coordinator.Activate(first);
            Assert.Throws<System.InvalidOperationException>(() => coordinator.Activate(second));
        }

        private sealed class FakeState : IFieldRuntimeState
        {
            public FakeState(FieldRuntimeId ownerId, int value) { OwnerId = ownerId; Value = value; }
            public FieldRuntimeId OwnerId { get; }
            public int Value { get; }
        }

        private class FakeField : IBattleFieldRuntime
        {
            private readonly IList<string> calls;
            public FakeField(string id, GridShape range, int value, IList<string> calls = null)
            { Id = new FieldRuntimeId(id); Range = range; Value = value; this.calls = calls; }
            public FieldRuntimeId Id { get; }
            public GridShape Range { get; }
            public int Value { get; set; }
            public int AdvancedTurns { get; private set; }
            public IFieldRuntimeState CaptureState() => new FakeState(Id, Value);
            public void RestoreState(IFieldRuntimeState state)
            { Value = ((FakeState)state).Value; calls?.Add("restore"); }
            public void AdvanceTurn() { AdvancedTurns++; Value++; }
        }

        private sealed class FakeGimmickField : FakeField, IGimmickFieldRuntime
        {
            public FakeGimmickField(string id, GridShape range, int value) : base(id, range, value) { }
            public GimmickFieldEntryContext Entry { get; private set; }
            public IFieldReturnEffect ExitEffect { get; set; }
            public void Enter(GimmickFieldEntryContext context) { Entry = context; }
            public GimmickFieldExitContext Exit() => new GimmickFieldExitContext(CaptureState(), ExitEffect);
        }

        private sealed class FakeEffect : IFieldReturnEffect { }

        private sealed class RecordingEffectApplier : IFieldReturnEffectApplier
        {
            private readonly IList<string> calls;
            public RecordingEffectApplier(IList<string> calls = null) { this.calls = calls; }
            public void Apply(IBattleFieldRuntime restoredNormalField, IFieldReturnEffect effect)
            { calls?.Add("effect"); }
        }
    }
}
