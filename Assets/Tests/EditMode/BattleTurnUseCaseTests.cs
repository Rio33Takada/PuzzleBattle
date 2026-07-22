using System.Collections.Generic;
using NUnit.Framework;
using PuzzleBattle.Application.BattleFlow;
using PuzzleBattle.Domain.BattleResult;
using PuzzleBattle.Domain.Randomness;

namespace PuzzleBattle.Application.Tests
{
    public sealed class BattleTurnUseCaseTests
    {
        [Test]
        public void StartBattle_InitializesBattleThenFirstPlayerTurn()
        {
            var services = new RecordingServices();
            BattleTurnUseCase useCase = CreateUseCase(services);
            var random = new DeterministicRandomSource(new StageRandomSeed(10UL));
            var session = new BattleFlowSession(random);

            useCase.StartBattle(session);

            Assert.That(services.Calls, Is.EqualTo(new[] { "battle-start", "player-turn-start" }));
            Assert.That(session.Phase, Is.EqualTo(BattleFlowPhase.PlayerTurn));
            Assert.That(session.TurnNumber, Is.EqualTo(1));
            Assert.That(session.Random, Is.SameAs(random));
            Assert.That(random.GeneratedValueCount, Is.EqualTo(1UL));
        }

        [Test]
        public void ResolveEnemyTurn_UsesFixedOrderAndStartsNextPlayerTurn()
        {
            var services = new RecordingServices();
            BattleTurnUseCase useCase = CreateUseCase(services);
            var session = new BattleFlowSession(
                new DeterministicRandomSource(new StageRandomSeed(20UL)));
            useCase.StartBattle(session);
            services.Calls.Clear();

            BattleTurnResult result = useCase.ResolveEnemyTurn(session);

            Assert.That(services.Calls, Is.EqualTo(new[]
            {
                "stun",
                "movement",
                "damage",
                "destruction",
                "death",
                "outcome",
                "enemy-turn-end",
                "player-turn-start"
            }));
            Assert.That(result.ResolvedTurnNumber, Is.EqualTo(1));
            Assert.That(result.Outcome, Is.EqualTo(BattleResolutionOutcome.Continuing));
            Assert.That(result.NextPhase, Is.EqualTo(BattleFlowPhase.PlayerTurn));
            Assert.That(result.NextTurnNumber, Is.EqualTo(2));
        }

        [Test]
        public void ResolveEnemyTurn_DefeatStopsBeforeEndAndNextTurn()
        {
            var services = new RecordingServices
            {
                Outcome = BattleResolutionOutcome.Defeat
            };
            BattleTurnUseCase useCase = CreateUseCase(services);
            var session = new BattleFlowSession(
                new DeterministicRandomSource(new StageRandomSeed(30UL)));
            useCase.StartBattle(session);
            services.Calls.Clear();

            BattleTurnResult result = useCase.ResolveEnemyTurn(session);

            Assert.That(services.Calls, Is.EqualTo(new[]
            {
                "stun", "movement", "damage", "destruction", "death", "outcome"
            }));
            Assert.That(result.Outcome, Is.EqualTo(BattleResolutionOutcome.Defeat));
            Assert.That(session.Phase, Is.EqualTo(BattleFlowPhase.Finished));
            Assert.That(session.TurnNumber, Is.EqualTo(1));
        }

        [Test]
        public void ResolveEnemyTurn_AreaClearWaitsForAreaTransition()
        {
            var services = new RecordingServices
            {
                Outcome = BattleResolutionOutcome.AreaCleared
            };
            BattleTurnUseCase useCase = CreateUseCase(services);
            var session = new BattleFlowSession(
                new DeterministicRandomSource(new StageRandomSeed(40UL)));
            useCase.StartBattle(session);

            BattleTurnResult result = useCase.ResolveEnemyTurn(session);

            Assert.That(result.NextPhase, Is.EqualTo(BattleFlowPhase.AwaitingAreaTransition));
            Assert.That(session.TurnNumber, Is.EqualTo(1));
            Assert.That(services.Calls, Has.No.Member("enemy-turn-end"));
        }

        [Test]
        public void InvalidTransitions_AreRejectedWithoutCallingServices()
        {
            var services = new RecordingServices();
            BattleTurnUseCase useCase = CreateUseCase(services);
            var session = new BattleFlowSession(
                new DeterministicRandomSource(new StageRandomSeed(50UL)));

            Assert.Throws<System.InvalidOperationException>(() => useCase.ResolveEnemyTurn(session));
            Assert.That(services.Calls, Is.Empty);

            useCase.StartBattle(session);
            services.Calls.Clear();
            Assert.Throws<System.InvalidOperationException>(() => useCase.StartBattle(session));
            Assert.That(services.Calls, Is.Empty);
        }

        private static BattleTurnUseCase CreateUseCase(RecordingServices services)
        {
            return new BattleTurnUseCase(
                services,
                services,
                services,
                services,
                services,
                services,
                services,
                services,
                services);
        }

        private sealed class RecordingServices :
            IBattleStartService,
            IPlayerTurnStartService,
            IStunTurnService,
            IEnemyMovementTurnService,
            IDamageResolutionService,
            IDestructionResolutionService,
            IDeathEvaluationService,
            IBattleOutcomeEvaluationService,
            IEnemyTurnEndService
        {
            public List<string> Calls { get; } = new List<string>();
            public BattleResolutionOutcome Outcome { get; set; } = BattleResolutionOutcome.Continuing;

            void IBattleStartService.Execute(BattleFlowSession session) => Calls.Add("battle-start");

            void IPlayerTurnStartService.Execute(BattleFlowSession session)
            {
                Calls.Add("player-turn-start");
                session.Random.NextUInt32();
            }

            void IStunTurnService.Execute(BattleFlowSession session) => Calls.Add("stun");
            void IEnemyMovementTurnService.Execute(BattleFlowSession session) => Calls.Add("movement");
            void IDamageResolutionService.Execute(BattleFlowSession session) => Calls.Add("damage");
            void IDestructionResolutionService.Execute(BattleFlowSession session) => Calls.Add("destruction");
            void IDeathEvaluationService.Execute(BattleFlowSession session) => Calls.Add("death");

            BattleResolutionOutcome IBattleOutcomeEvaluationService.Evaluate(BattleFlowSession session)
            {
                Calls.Add("outcome");
                return Outcome;
            }

            void IEnemyTurnEndService.Execute(BattleFlowSession session) => Calls.Add("enemy-turn-end");
        }
    }
}
