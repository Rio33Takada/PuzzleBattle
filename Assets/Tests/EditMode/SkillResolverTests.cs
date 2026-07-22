using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleBattle.Domain.Character;
using PuzzleBattle.Domain.Combat;
using PuzzleBattle.Domain.Enemy;
using PuzzleBattle.Domain.Grid;
using PuzzleBattle.Domain.Piece;
using PuzzleBattle.Domain.Placement;
using PuzzleBattle.Domain.Skill;
using BattleBoard = PuzzleBattle.Domain.Board.Board;
using BattleCharacter = PuzzleBattle.Domain.Character.Character;
using BattleField = PuzzleBattle.Domain.Field.Field;
using BattlePiece = PuzzleBattle.Domain.Piece.Piece;

namespace PuzzleBattle.Domain.Tests
{
    public sealed class SkillResolverTests
    {
        private static readonly CharacterId Hero = new CharacterId("hero");
        private static readonly ElementAttribute Fire = new ElementAttribute("fire");
        private static readonly ElementAttribute Water = new ElementAttribute("water");
        private readonly SkillResolver resolver = new SkillResolver();

        [TestCase(2, SkillExecutionStatus.ConditionNotMet, 100)]
        [TestCase(3, SkillExecutionStatus.Applied, 70)]
        public void ExecuteActive_UsesTurnCondition(
            int turn,
            SkillExecutionStatus expectedStatus,
            int expectedEnemyHp)
        {
            var enemy = new EnemyBattleState(new EnemyId("enemy"), new EnemyStats(100, 1, 0));
            SkillDefinition skill = Skill(
                "active",
                SkillActivationKind.Active,
                0,
                new MinimumTurnCondition(3),
                new DamageEnemyEffect(enemy.Id, 30));

            SkillExecutionResult result = resolver.ExecuteActive(skill, Context(turn, enemy));

            Assert.That(result.Status, Is.EqualTo(expectedStatus));
            Assert.That(enemy.CurrentHitPoints, Is.EqualTo(expectedEnemyHp));
        }

        [Test]
        public void ExecuteActive_CanGrantPiece()
        {
            BattlePiece granted = CreatePiece("granted", Hero, Fire);
            SkillExecutionContext context = Context(1);
            SkillDefinition skill = Skill(
                "grant", SkillActivationKind.Active, 0,
                new AlwaysSkillCondition(), new GrantPieceEffect(granted));

            resolver.ExecuteActive(skill, context);

            Assert.That(context.GrantedPieces.Pieces.Single(), Is.SameAs(granted));
        }

        [TestCase("fire", 2, SkillExecutionStatus.Applied)]
        [TestCase("fire", 3, SkillExecutionStatus.ConditionNotMet)]
        [TestCase("water", 1, SkillExecutionStatus.ConditionNotMet)]
        public void Passive_UsesCurrentTurnOverwriteAttributeAndCount(
            string attribute,
            int requiredCount,
            SkillExecutionStatus expectedStatus)
        {
            TurnOverwriteSummary summary = CreateOverwriteSummary();
            SkillExecutionContext context = Context(1, overwrites: summary);
            SkillDefinition skill = Skill(
                "passive", SkillActivationKind.Passive, 0,
                new OverwriteAttributeCountCondition(
                    new ElementAttribute(attribute), requiredCount),
                new IncreaseAttackEffect(5));

            SkillExecutionResult result = resolver.ApplyPassivesAfterPlacement(
                new[] { skill }, context).Single();

            Assert.That(result.Status, Is.EqualTo(expectedStatus));
        }

        [Test]
        public void Passive_IncreasesAttackAndHealingDoesNotExceedMaximum()
        {
            SkillExecutionContext context = Context(1);
            context.Player.ReceiveDamage(10);
            SkillDefinition skill = new SkillDefinition(
                new SkillId("buff-heal"), Hero, SkillActivationKind.Passive, 0,
                new AlwaysSkillCondition(),
                new ISkillEffect[] { new IncreaseAttackEffect(7), new HealPlayerEffect(999) });

            SkillExecutionResult result = resolver.ApplyPassivesAfterPlacement(
                new[] { skill }, context).Single();

            Assert.That(context.Modifiers.GetAttackBonus(Hero), Is.EqualTo(7));
            Assert.That(context.Player.CurrentHitPoints, Is.EqualTo(context.Player.MaximumHitPoints));
            Assert.That(result.Effects.Last().AppliedAmount, Is.EqualTo(10));
        }

        [Test]
        public void Passives_AreAppliedByPriorityThenSkillId()
        {
            var order = new List<string>();
            SkillDefinition later = Skill(
                "z", SkillActivationKind.Passive, 20,
                new AlwaysSkillCondition(), new RecordingEffect("z", order));
            SkillDefinition firstById = Skill(
                "a", SkillActivationKind.Passive, 10,
                new AlwaysSkillCondition(), new RecordingEffect("a", order));
            SkillDefinition secondById = Skill(
                "b", SkillActivationKind.Passive, 10,
                new AlwaysSkillCondition(), new RecordingEffect("b", order));

            resolver.ApplyPassivesAfterPlacement(
                new[] { later, secondById, firstById }, Context(1));

            Assert.That(order, Is.EqualTo(new[] { "a", "b", "z" }));
        }

        private static TurnOverwriteSummary CreateOverwriteSummary()
        {
            GridShape range = GridShape.CreateRectangle(2, 1);
            var board = new BattleBoard(range, new BattleField(range));
            var service = new PiecePlacementService();
            service.TryPlace(board, new PiecePlacementRequest(
                CreateTwoCellPiece("old", new CharacterId("other"), Fire, Fire),
                new GridPosition(0, 0)));
            PiecePlacementResult overwrite = service.TryPlace(board, new PiecePlacementRequest(
                CreateTwoCellPiece("new", Hero, Water, Water),
                new GridPosition(0, 0)));
            var summary = new TurnOverwriteSummary();
            summary.Add(overwrite);
            return summary;
        }

        private static SkillExecutionContext Context(
            int turn,
            EnemyBattleState enemy = null,
            TurnOverwriteSummary overwrites = null)
        {
            BattleCharacter character = CreateCharacter();
            var player = new PlayerBattleState(new PartyFormation(new[] { character }));
            return new SkillExecutionContext(
                turn,
                overwrites ?? new TurnOverwriteSummary(),
                player,
                enemy == null ? Array.Empty<EnemyBattleState>() : new[] { enemy },
                new CharacterCombatModifiers(),
                new GrantedPiecePool());
        }

        private static SkillDefinition Skill(
            string id,
            SkillActivationKind kind,
            int priority,
            ISkillCondition condition,
            ISkillEffect effect)
        {
            return new SkillDefinition(
                new SkillId(id), Hero, kind, priority, condition, new[] { effect });
        }

        private static BattleCharacter CreateCharacter()
        {
            var levels = new CharacterLevelTable(new[]
            {
                new KeyValuePair<int, CharacterLevelStats>(1, new CharacterLevelStats(100, 10))
            });
            return new BattleCharacter(
                Hero, 1, levels, new[] { CreatePiece("owned", Hero, Fire) });
        }

        private static BattlePiece CreatePiece(
            string id,
            CharacterId owner,
            ElementAttribute attribute)
        {
            var position = new GridPosition(0, 0);
            return new BattlePiece(
                new PieceId(id), owner,
                new Dictionary<GridPosition, int> { [position] = 1 },
                new Dictionary<GridPosition, ElementAttribute> { [position] = attribute });
        }

        private static BattlePiece CreateTwoCellPiece(
            string id,
            CharacterId owner,
            ElementAttribute first,
            ElementAttribute second)
        {
            return new BattlePiece(
                new PieceId(id), owner,
                new Dictionary<GridPosition, int>
                {
                    [new GridPosition(0, 0)] = 1,
                    [new GridPosition(1, 0)] = 1
                },
                new Dictionary<GridPosition, ElementAttribute>
                {
                    [new GridPosition(0, 0)] = first,
                    [new GridPosition(1, 0)] = second
                });
        }

        private sealed class RecordingEffect : ISkillEffect
        {
            private readonly string value;
            private readonly ICollection<string> output;

            public RecordingEffect(string value, ICollection<string> output)
            {
                this.value = value;
                this.output = output;
            }

            public SkillEffectResult Apply(SkillDefinition skill, SkillExecutionContext context)
            {
                output.Add(value);
                return new SkillEffectResult(nameof(RecordingEffect), 0);
            }
        }
    }
}
