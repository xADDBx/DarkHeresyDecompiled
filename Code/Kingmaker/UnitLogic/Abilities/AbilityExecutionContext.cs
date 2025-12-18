using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

[OwlPackable(OwlPackableMode.Generate)]
public class AbilityExecutionContext : MechanicsContext, IHashable, IOwlPackable<AbilityExecutionContext>
{
	public class Data : ContextData<Data>
	{
		public RulePerformAttack AttackRule { get; private set; }

		public Projectile Projectile { get; private set; }

		public Data Setup([NotNull] RulePerformAttack rule, [NotNull] Projectile projectile)
		{
			AttackRule = rule;
			Projectile = projectile;
			return this;
		}

		protected override void Reset()
		{
			AttackRule = null;
			Projectile = null;
		}
	}

	private static readonly Stack<AbilityExecutionContext> Pool = new Stack<AbilityExecutionContext>();

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private Vector3 m_CastPosition;

	private bool m_PatternConfigured;

	private OrientedPatternData m_Pattern;

	[NotNull]
	private readonly Dictionary<GridNodeBase, (WarhammerSingleNodeBlocker Blocker, IntRect Rect, Vector3 Direction)> m_BlockedNodes = new Dictionary<GridNodeBase, (WarhammerSingleNodeBlocker, IntRect, Vector3)>();

	[JsonIgnore]
	public bool ExecutionFromPsychicPhenomena;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AbilityExecutionContext",
		OldNames = null,
		Fields = new FieldInfo[23]
		{
			new FieldInfo("m_OwnerRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_CasterRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_FactRef", typeof(EntityFactRef<MechanicEntityFact>)),
			new FieldInfo("m_Ability", typeof(AbilityData)),
			new FieldInfo("m_Source", typeof(SourceInfo)),
			new FieldInfo("m_ClickedTarget", typeof(TargetWrapper)),
			new FieldInfo("Blueprint", typeof(BlueprintScriptableObject)),
			new FieldInfo("Direction", typeof(Vector3)),
			new FieldInfo("DisableFx", typeof(bool)),
			new FieldInfo("PropertiesConverter", typeof(Dictionary<ContextPropertyName, int>)),
			new FieldInfo("ActionIndex", typeof(int)),
			new FieldInfo("DelayBetweenActions", typeof(TimeSpan?)),
			new FieldInfo("CastTime", typeof(TimeSpan)),
			new FieldInfo("HitPolicy", typeof(AttackHitPolicyType)),
			new FieldInfo("DamagePolicy", typeof(DamagePolicyType)),
			new FieldInfo("KillTarget", typeof(bool)),
			new FieldInfo("DisableLog", typeof(bool)),
			new FieldInfo("IsForced", typeof(bool)),
			new FieldInfo("ProjectileHitPositions", typeof(List<Vector3>)),
			new FieldInfo("FreeAction", typeof(bool)),
			new FieldInfo("IgnoreCooldown", typeof(bool)),
			new FieldInfo("m_CastPosition", typeof(Vector3)),
			new FieldInfo("TargetsInPatternCount", typeof(int))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int ActionIndex { get; private set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[OwlPackInclude]
	public TimeSpan? DelayBetweenActions { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan CastTime { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public AttackHitPolicyType HitPolicy { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public DamagePolicyType DamagePolicy { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool KillTarget { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool DisableLog { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool IsForced { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public List<Vector3> ProjectileHitPositions { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool FreeAction { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool IgnoreCooldown { get; set; }

	[OwlPackInclude]
	public int TargetsInPatternCount { get; set; } = 1;


	[NotNull]
	public new AbilityData Ability => base.Ability;

	[NotNull]
	public new BlueprintAbility AbilityBlueprint => base.AbilityBlueprint;

	public IEnumerable<AbilitySpawnFx> FxSpawners => AbilityBlueprint.GetComponents<AbilitySpawnFx>();

	public OrientedPatternData Pattern
	{
		get
		{
			TryConfigurePattern();
			return m_Pattern;
		}
	}

	[NotNull]
	public MechanicEntity Caster => base.MaybeCaster ?? throw new Exception("Caster is missing");

	public override Vector3? SourceCastPosition => m_CastPosition;

	public LosDescription LosToClickedTarget => GetLosToClickedTarget();

	private LosDescription GetLosToClickedTarget()
	{
		return LosCalculations.GetWarhammerLos(Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Caster).GetNearestNodeXZUnwalkable(), Caster.SizeRect, ClickedTarget.NearestNode, ClickedTarget.SizeRect);
	}

	[JsonConstructor]
	protected AbilityExecutionContext()
	{
	}

	private void Setup(AbilityData ability, TargetWrapper clickedTarget, Vector3 casterPosition, MechanicsContext parentContext)
	{
		m_CastPosition = casterPosition;
		BlueprintAbility originalBlueprint = ability.Blueprint.OriginalBlueprint;
		MechanicEntity caster = ability.Caster;
		MechanicEntityFact fact = ability.Fact;
		Setup(originalBlueprint, caster, null, parentContext, clickedTarget, fact, ability);
		if (ability.Blueprint.HasVariants)
		{
			throw new Exception("Can't cast variational ability");
		}
		base.Direction = (ClickedTarget.IsOrientationSpecified ? (Quaternion.Euler(0f, ClickedTarget.Orientation, 0f) * Vector3.forward).normalized : (ClickedTarget.Point - casterPosition).normalized);
	}

	public static AbilityExecutionContext Claim(AbilityData ability, TargetWrapper clickedTarget, Vector3 casterPosition, MechanicsContext parentContext = null)
	{
		if (!Pool.TryPop(out var result))
		{
			result = new AbilityExecutionContext();
		}
		result.Setup(ability, clickedTarget, casterPosition, parentContext);
		return result;
	}

	public override void Dispose()
	{
		Reset();
		ActionIndex = 0;
		DelayBetweenActions = null;
		CastTime = default(TimeSpan);
		HitPolicy = AttackHitPolicyType.Default;
		DamagePolicy = DamagePolicyType.Default;
		KillTarget = false;
		DisableLog = false;
		IsForced = false;
		m_PatternConfigured = false;
		m_Pattern = default(OrientedPatternData);
		m_CastPosition = default(Vector3);
		ExecutionFromPsychicPhenomena = false;
		m_BlockedNodes.Clear();
		Pool.Push(this);
	}

	public void TemporarilyBlockNode(Vector3 pos, BaseUnitEntity unit)
	{
		TemporarilyBlockNode(pos.GetNearestNodeXZ(), unit);
	}

	public void TemporarilyBlockLastPathNode(Path path, BaseUnitEntity unit)
	{
		TemporarilyBlockNode(path.vectorPath.Last(), unit);
	}

	public void TemporarilyBlockNode(GridNodeBase node, BaseUnitEntity unit)
	{
		if (!m_BlockedNodes.ContainsKey(node))
		{
			BlockType blockType = (unit.IsInvisible ? BlockType.Invisible : (unit.Faction.IsPlayerEnemy ? BlockType.Enemy : BlockType.Friend));
			WarhammerBlockManager.Instance.InternalBlock(node, unit.MovementAgent.Blocker, unit.SizeRect, unit.Forward, blockType);
			m_BlockedNodes.Add(node, (unit.MovementAgent.Blocker, unit.SizeRect, unit.Forward));
		}
	}

	public void ClearBlockedNodes()
	{
		foreach (KeyValuePair<GridNodeBase, (WarhammerSingleNodeBlocker, IntRect, Vector3)> blockedNode in m_BlockedNodes)
		{
			WarhammerBlockManager.Instance.InternalUnblock(blockedNode.Key, blockedNode.Value.Item1, blockedNode.Value.Item2, blockedNode.Value.Item3);
		}
	}

	public static Data GetAbilityDataScope(RulePerformAttack rule, Projectile projectile)
	{
		return ContextData<Data>.Request().Setup(rule, projectile);
	}

	private void TryConfigurePattern()
	{
		if (!m_PatternConfigured)
		{
			m_Pattern = Ability.GetPattern(ClickedTarget, m_CastPosition);
			m_PatternConfigured = true;
		}
	}

	public void ConfigureHaloPattern(int haloSize)
	{
		m_Pattern = Ability.GetHaloPattern(ClickedTarget, m_CastPosition, haloSize);
	}

	public override T TriggerRule<T>(T rule)
	{
		rule.Reason = this;
		return base.TriggerRule(rule);
	}

	public void NextAction()
	{
		ActionIndex++;
	}

	public void RewindActionIndex(TimeSpan? delay = null)
	{
		if (delay.HasValue && delay.GetValueOrDefault().TotalSeconds > 0.001)
		{
			DelayBetweenActions = delay;
		}
		else
		{
			ActionIndex = Ability.ActionsCount;
		}
	}

	public bool CanTargetBodyPart(BlueprintBodyPart bodyPart)
	{
		if (bodyPart.Restrictions.IsPassed(this))
		{
			if (bodyPart.ReplaceableByCover)
			{
				return (LosCalculations.CoverType)LosToClickedTarget == LosCalculations.CoverType.Obstacle;
			}
			return true;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = ActionIndex;
		result.Append(ref val2);
		if (DelayBetweenActions.HasValue)
		{
			TimeSpan val3 = DelayBetweenActions.Value;
			result.Append(ref val3);
		}
		TimeSpan val4 = CastTime;
		result.Append(ref val4);
		AttackHitPolicyType val5 = HitPolicy;
		result.Append(ref val5);
		DamagePolicyType val6 = DamagePolicy;
		result.Append(ref val6);
		bool val7 = KillTarget;
		result.Append(ref val7);
		bool val8 = DisableLog;
		result.Append(ref val8);
		bool val9 = IsForced;
		result.Append(ref val9);
		List<Vector3> projectileHitPositions = ProjectileHitPositions;
		if (projectileHitPositions != null)
		{
			for (int i = 0; i < projectileHitPositions.Count; i++)
			{
				Vector3 obj = projectileHitPositions[i];
				Hash128 val10 = UnmanagedHasher<Vector3>.GetHash128(ref obj);
				result.Append(ref val10);
			}
		}
		bool val11 = FreeAction;
		result.Append(ref val11);
		bool val12 = IgnoreCooldown;
		result.Append(ref val12);
		result.Append(ref m_CastPosition);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AbilityExecutionContext source = new AbilityExecutionContext();
		result = Unsafe.As<AbilityExecutionContext, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<AbilityExecutionContext>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_OwnerRef", ref m_OwnerRef, state);
		formatter.Field(1, "m_CasterRef", ref m_CasterRef, state);
		formatter.Field(2, "m_FactRef", ref m_FactRef, state);
		formatter.Field(3, "m_Ability", ref m_Ability, state);
		formatter.Field(4, "m_Source", ref m_Source, state);
		formatter.Field(5, "m_ClickedTarget", ref m_ClickedTarget, state);
		BlueprintScriptableObject value = base.Blueprint;
		formatter.Field(6, "Blueprint", ref value, state);
		Vector3 value2 = base.Direction;
		formatter.Field(7, "Direction", ref value2, state);
		bool value3 = base.DisableFx;
		formatter.UnmanagedField(8, "DisableFx", ref value3, state);
		Dictionary<ContextPropertyName, int> value4 = base.PropertiesConverter;
		formatter.Field(9, "PropertiesConverter", ref value4, state);
		int value5 = ActionIndex;
		formatter.UnmanagedField(10, "ActionIndex", ref value5, state);
		TimeSpan? value6 = DelayBetweenActions;
		formatter.NullableField(11, "DelayBetweenActions", ref value6, state);
		TimeSpan value7 = CastTime;
		formatter.Field(12, "CastTime", ref value7, state);
		AttackHitPolicyType value8 = HitPolicy;
		formatter.EnumField(13, "HitPolicy", ref value8, state);
		DamagePolicyType value9 = DamagePolicy;
		formatter.EnumField(14, "DamagePolicy", ref value9, state);
		bool value10 = KillTarget;
		formatter.UnmanagedField(15, "KillTarget", ref value10, state);
		bool value11 = DisableLog;
		formatter.UnmanagedField(16, "DisableLog", ref value11, state);
		bool value12 = IsForced;
		formatter.UnmanagedField(17, "IsForced", ref value12, state);
		List<Vector3> value13 = ProjectileHitPositions;
		formatter.Field(18, "ProjectileHitPositions", ref value13, state);
		bool value14 = FreeAction;
		formatter.UnmanagedField(19, "FreeAction", ref value14, state);
		bool value15 = IgnoreCooldown;
		formatter.UnmanagedField(20, "IgnoreCooldown", ref value15, state);
		formatter.Field(21, "m_CastPosition", ref m_CastPosition, state);
		int value16 = TargetsInPatternCount;
		formatter.UnmanagedField(22, "TargetsInPatternCount", ref value16, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AbilityExecutionContext>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_OwnerRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 1:
				m_CasterRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 2:
				m_FactRef = formatter.ReadPackable<EntityFactRef<MechanicEntityFact>>(state);
				break;
			case 3:
				m_Ability = formatter.ReadPackable<AbilityData>(state);
				break;
			case 4:
				m_Source = formatter.ReadPackable<SourceInfo>(state);
				break;
			case 5:
				m_ClickedTarget = formatter.ReadPackable<TargetWrapper>(state);
				break;
			case 6:
				base.Blueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 7:
				base.Direction = formatter.ReadPackable<Vector3>(state);
				break;
			case 8:
				base.DisableFx = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				base.PropertiesConverter = formatter.ReadPackable<Dictionary<ContextPropertyName, int>>(state);
				break;
			case 10:
				ActionIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 11:
				DelayBetweenActions = formatter.ReadNullablePackable<TimeSpan>(state);
				break;
			case 12:
				CastTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 13:
				HitPolicy = formatter.ReadEnum<AttackHitPolicyType>(state);
				break;
			case 14:
				DamagePolicy = formatter.ReadEnum<DamagePolicyType>(state);
				break;
			case 15:
				KillTarget = formatter.ReadUnmanaged<bool>(state);
				break;
			case 16:
				DisableLog = formatter.ReadUnmanaged<bool>(state);
				break;
			case 17:
				IsForced = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				ProjectileHitPositions = formatter.ReadPackable<List<Vector3>>(state);
				break;
			case 19:
				FreeAction = formatter.ReadUnmanaged<bool>(state);
				break;
			case 20:
				IgnoreCooldown = formatter.ReadUnmanaged<bool>(state);
				break;
			case 21:
				m_CastPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 22:
				TargetsInPatternCount = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
