using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Framework.Utility;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Framework;

public sealed class EvalContext : IEvalContext
{
	public struct Builder
	{
		private readonly IEvalContext? _context;

		private MechanicEntityFact? _fact;

		private TargetWrapper? _target;

		private RulebookEvent? _rule;

		private MechanicEntity? _currentEntity;

		private AreaEffectEntity? _areaEffect;

		private OrientedPatternData? _pattern;

		private MechanicEntity? _caster;

		private MechanicEntity? _owner;

		private TargetWrapper? _clickedTarget;

		private BlueprintScriptableObject? _blueprint;

		private AbilityData? _ability;

		private AbilityData? _sourceAbility;

		private ItemEntityWeapon? _weapon;

		public Builder(IEvalContext? context = null)
		{
			_context = context;
			_fact = null;
			_target = null;
			_rule = null;
			_currentEntity = null;
			_areaEffect = null;
			_pattern = null;
			_caster = null;
			_owner = null;
			_clickedTarget = null;
			_blueprint = null;
			_ability = null;
			_sourceAbility = null;
			_weapon = null;
		}

		public Builder Fact(MechanicEntityFact? fact)
		{
			_fact = fact;
			return this;
		}

		public Builder Target(TargetWrapper? target)
		{
			_target = target;
			return this;
		}

		public Builder Rule(RulebookEvent? rule)
		{
			_rule = rule;
			return this;
		}

		public Builder CurrentEntity(MechanicEntity? entity)
		{
			_currentEntity = entity;
			return this;
		}

		public Builder AreaEffect(AreaEffectEntity? areaEffect)
		{
			_areaEffect = areaEffect;
			return this;
		}

		public Builder Pattern(OrientedPatternData? pattern)
		{
			_pattern = pattern;
			return this;
		}

		public Builder Caster(MechanicEntity? caster)
		{
			_caster = caster;
			return this;
		}

		public Builder Owner(MechanicEntity? owner)
		{
			_owner = owner;
			return this;
		}

		public Builder ClickedTarget(TargetWrapper? clickedTarget)
		{
			_clickedTarget = clickedTarget;
			return this;
		}

		public Builder Blueprint(BlueprintScriptableObject? blueprint)
		{
			_blueprint = blueprint;
			return this;
		}

		public Builder Ability(AbilityData? ability)
		{
			_ability = ability;
			return this;
		}

		public Builder SourceAbility(AbilityData? ability)
		{
			_sourceAbility = ability;
			return this;
		}

		public Builder Weapon(ItemEntityWeapon? weapon)
		{
			_weapon = weapon;
			return this;
		}

		public StackFrameHandle Push(out EvalContext frame)
		{
			frame = EvalContext.Push(_context ?? _fact?.MaybeContext);
			if (_fact != null)
			{
				frame.SetFact(_fact);
			}
			if (_blueprint != null)
			{
				frame.SetBlueprint(_blueprint);
			}
			if (_clickedTarget != null)
			{
				frame.SetClickedTarget(_clickedTarget);
			}
			if (_caster != null)
			{
				frame.SetCaster(_caster);
			}
			if (_owner != null)
			{
				frame.SetOwner(_owner);
			}
			if (_currentEntity != null)
			{
				frame.SetCurrentEntity(_currentEntity);
			}
			if (_areaEffect != null)
			{
				frame.SetAreaEffect(_areaEffect);
			}
			if (_pattern.HasValue)
			{
				frame.SetPattern(_pattern.Value);
			}
			if (_rule != null)
			{
				frame.SetRule(_rule);
			}
			TargetWrapper targetWrapper = _target ?? _clickedTarget;
			if (targetWrapper != null)
			{
				frame.SetTarget(targetWrapper);
			}
			if (_ability != null)
			{
				frame.SetAbility(_ability);
			}
			if (_sourceAbility != null)
			{
				frame.SetSourceAbility(_sourceAbility);
			}
			if (_weapon != null)
			{
				frame.SetWeapon(_weapon);
			}
			return PrepareCurrentFrame(frame);
		}

		public StackFrameHandle Push()
		{
			EvalContext frame;
			return Push(out frame);
		}
	}

	public readonly struct StackFrameHandle : IDisposable
	{
		private readonly int _expectedIndex;

		internal StackFrameHandle(int expectedIndex)
		{
			_expectedIndex = expectedIndex;
		}

		public StackFrameHandle Get(out IEvalContext ctx)
		{
			ctx = Current;
			return this;
		}

		public void Dispose()
		{
			if (_expectedIndex >= 1)
			{
				if (_expectedIndex != _CurrentIndex)
				{
					throw new InvalidOperationException($"EvalContext: scope dispose mismatch (expected {_expectedIndex}, actual {_CurrentIndex})");
				}
				Pop();
			}
		}
	}

	private ScopedStack<TargetWrapper> _scopeTarget;

	private ScopedStack<RulebookEvent> _scopeRule;

	private ScopedStack<MechanicEntity> _scopeCurrentEntity;

	private ScopedStack<AreaEffectEntity> _scopeAreaEffect;

	private ScopedStack<AbilityData> _scopeSourceAbility;

	private static readonly List<EvalContext> _Stack = new List<EvalContext>
	{
		new EvalContext()
	};

	private static int _CurrentIndex;

	private static readonly Dictionary<string, ContextPropertyName> _NameToProperty = BuildNameToPropertyMap();

	public TargetWrapper? Target => _scopeTarget.Peek();

	public RulebookEvent? Rule => _scopeRule.Peek();

	public MechanicEntity? CurrentEntity => _scopeCurrentEntity.Peek();

	public AreaEffectEntity? AreaEffect => _scopeAreaEffect.Peek();

	public AbilityData? SourceAbility => _scopeSourceAbility.Peek();

	public static EvalContext Current => _Stack[_CurrentIndex];

	public static EvalContext Root
	{
		get
		{
			if (_Stack.Count < 2)
			{
				return _Stack[0];
			}
			return _Stack[1];
		}
	}

	public MechanicEntity? Caster { get; private set; }

	public MechanicEntity? Owner { get; private set; }

	public TargetWrapper? ClickedTarget { get; private set; }

	public AbilityData? Ability { get; private set; }

	public ItemEntityWeapon? Weapon { get; private set; }

	public BlueprintScriptableObject? Blueprint { get; private set; }

	public MechanicEntityFact? Fact { get; private set; }

	public EntityFactComponent? FactComponent { get; private set; }

	public OrientedPatternData Pattern { get; private set; }

	public MechanicEntity? SourceCaster { get; private set; }

	public AbilityExecutionContext? AbilityExecution { get; private set; }

	public BlueprintAbility? SourceAbilityBlueprint { get; private set; }

	public BlueprintScriptableObject? SourceBlueprint { get; private set; }

	public TargetWrapper? SourceClickedTarget { get; private set; }

	public Vector3? SourceCastPosition { get; private set; }

	public MechanicEntityFact? SourceFact { get; private set; }

	public bool DisableFx { get; private set; }

	public Vector3 Direction { get; private set; }

	public int[]? PropertiesArray { get; private set; }

	public ItemEntityWeapon? AbilityWeapon
	{
		get
		{
			ItemEntityWeapon weapon;
			if (!(Rule is RuleCalculateStatsWeapon ruleCalculateStatsWeapon))
			{
				weapon = Weapon;
				if (weapon == null)
				{
					return Ability?.Weapon;
				}
			}
			else
			{
				weapon = ruleCalculateStatsWeapon.Weapon;
			}
			return weapon;
		}
	}

	public BlueprintAbility? AbilityBlueprint => Ability?.Blueprint.OriginalBlueprint;

	public MechanicEntity? RuleInitiator => Rule?.Initiator;

	public MechanicEntity? RuleTarget => Rule?.Target;

	public MechanicEntity? CurrentTargetEntity => Target?.Entity;

	public TargetWrapper? ContextMainTarget => ClickedTarget;

	public Vector3 ContextMainTargetPosition => ClickedTarget?.Point ?? default(Vector3);

	public MechanicEntity? ContextOwner => Owner;

	public MechanicEntity? ContextCaster => Caster;

	public LosDescription LosToClickedTarget => GetLosToClickedTarget();

	public NodeList SourcePatternNodes => Pattern.Nodes;

	public int this[ContextPropertyName key]
	{
		get
		{
			if (PropertiesArray == null)
			{
				return 0;
			}
			return PropertiesArray[(int)key];
		}
		set
		{
			if (PropertiesArray != null)
			{
				PropertiesArray[(int)key] = value;
			}
		}
	}

	public int this[string key]
	{
		get
		{
			if (!_NameToProperty.TryGetValue(key, out var value))
			{
				return 0;
			}
			return this[value];
		}
		set
		{
			if (_NameToProperty.TryGetValue(key, out var value2))
			{
				this[value2] = value;
			}
		}
	}

	public static Builder Build(IEvalContext? context = null)
	{
		return new Builder(context);
	}

	private void SetTarget(TargetWrapper target)
	{
		PushTarget(target);
	}

	private void SetRule(RulebookEvent rule)
	{
		PushRule(rule);
	}

	private void SetCurrentEntity(MechanicEntity entity)
	{
		PushCurrentEntity(entity);
	}

	private void SetAbility(AbilityData ability)
	{
		Ability = ability;
	}

	private void SetSourceAbility(AbilityData ability)
	{
		PushSourceAbility(ability);
	}

	private void SetWeapon(ItemEntityWeapon weapon)
	{
		Weapon = weapon;
	}

	private void SetAreaEffect(AreaEffectEntity ae)
	{
		PushAreaEffect(ae);
	}

	private void SetPattern(OrientedPatternData pattern)
	{
		Pattern = pattern;
	}

	private void SetCaster(MechanicEntity caster)
	{
		Caster = caster;
	}

	private void SetOwner(MechanicEntity owner)
	{
		Owner = owner;
	}

	private void SetClickedTarget(TargetWrapper clickedTarget)
	{
		ClickedTarget = clickedTarget;
	}

	private void SetBlueprint(BlueprintScriptableObject blueprint)
	{
		Blueprint = blueprint;
	}

	private void SetFact(MechanicEntityFact fact)
	{
		Fact = fact;
	}

	public static MechanicsContext? ClaimMechanicsContext(IEvalContext? context)
	{
		if (context == null)
		{
			return null;
		}
		if (context is MechanicsContext result)
		{
			return result;
		}
		if (context.Blueprint == null)
		{
			return null;
		}
		return MechanicsContext.Claim(context.Blueprint, context.Caster, context.Owner, context, context.ClickedTarget, context.Fact, context.Ability);
	}

	private static StackFrameHandle PrepareCurrentFrame(EvalContext frame)
	{
		if (frame != Current)
		{
			throw new InvalidOperationException();
		}
		frame.Prepare();
		return new StackFrameHandle(_CurrentIndex);
	}

	public static StackFrameHandle PushContext(IEvalContext context, TargetWrapper? target = null, RulebookEvent? rule = null)
	{
		EvalContext evalContext = Push(context);
		evalContext.PushPropertiesIfNotNull(target, rule);
		return PrepareCurrentFrame(evalContext);
	}

	public static StackFrameHandle PushContextMaybe(IEvalContext? ctx, TargetWrapper? target = null, RulebookEvent? rule = null)
	{
		if (ctx == null)
		{
			return default(StackFrameHandle);
		}
		return PushContext(ctx, target, rule);
	}

	public static StackFrameHandle PushFact(MechanicEntityFact fact, TargetWrapper? target = null, RulebookEvent? rule = null)
	{
		if (fact.MaybeContext != null)
		{
			return PushContext(fact.Context, target, rule);
		}
		EvalContext evalContext = Push(null);
		evalContext.Fact = fact;
		evalContext.Caster = fact.Caster;
		evalContext.Blueprint = fact.Blueprint;
		evalContext.ClickedTarget = fact.Owner;
		evalContext.PushPropertiesIfNotNull(target ?? ((TargetWrapper)fact.Owner), rule);
		return PrepareCurrentFrame(evalContext);
	}

	public static StackFrameHandle PushFactComponent(EntityFactComponent component)
	{
		EvalContext evalContext = Push(component.Fact?.MaybeContext);
		evalContext.FactComponent = component;
		return PrepareCurrentFrame(evalContext);
	}

	public static StackFrameHandle PushAbility(AbilityData ability, TargetWrapper clickedTarget, TargetWrapper? target = null, Vector3? casterPosition = null)
	{
		EvalContext evalContext = Push(ability.Fact?.MaybeContext);
		evalContext.Caster = ability.Caster;
		evalContext.Ability = ability;
		evalContext.Blueprint = ability.Blueprint.OriginalBlueprint;
		evalContext.ClickedTarget = clickedTarget;
		evalContext.Fact = ability.Fact;
		if (casterPosition.HasValue)
		{
			evalContext.SourceCastPosition = casterPosition;
		}
		evalContext.PushPropertiesIfNotNull(target ?? clickedTarget);
		return PrepareCurrentFrame(evalContext);
	}

	public static StackFrameHandle PushPropertyContext(IEvalContext? context, MechanicEntity? currentEntity = null, TargetWrapper? currentTarget = null, RulebookEvent? rule = null, AbilityData? ability = null)
	{
		if (currentEntity == null)
		{
			currentEntity = context?.ClickedTarget?.Entity ?? context?.Owner ?? throw new InvalidOperationException("CurrentEntity is missing");
		}
		EvalContext evalContext = Push(context);
		evalContext.PushPropertiesIfNotNull(currentTarget, rule, currentEntity);
		evalContext.Ability = ability ?? rule?.MaybeAbility ?? evalContext.Ability ?? evalContext.SourceAbility;
		return PrepareCurrentFrame(evalContext);
	}

	public static StackFrameHandle PushPropertyContext(MechanicEntityFact fact, MechanicEntity? currentEntity = null, TargetWrapper? currentTarget = null, RulebookEvent? rule = null, AbilityData? ability = null)
	{
		return PushPropertyContext(fact.Context, currentEntity, currentTarget, rule, ability);
	}

	public static StackFrameHandle PushAsksContext(MechanicEntity caster, MechanicEntity target)
	{
		return Build().Blueprint(target.View?.Asks?.Blueprint).Caster(caster).ClickedTarget(target)
			.Push();
	}

	public ScopedStackFrame PushTarget(TargetWrapper target)
	{
		return _scopeTarget.Push(target);
	}

	public ScopedStackFrame PushRule(RulebookEvent rule)
	{
		return _scopeRule.Push(rule);
	}

	public ScopedStackFrame PushCurrentEntity(MechanicEntity entity)
	{
		return _scopeCurrentEntity.Push(entity);
	}

	public ScopedStackFrame PushAreaEffect(AreaEffectEntity ae)
	{
		return _scopeAreaEffect.Push(ae);
	}

	public ScopedStackFrame PushSourceAbility(AbilityData ability)
	{
		return _scopeSourceAbility.Push(ability);
	}

	private void PushPropertiesIfNotNull(TargetWrapper? target = null, RulebookEvent? rule = null, MechanicEntity? currentEntity = null, AreaEffectEntity? areaEffect = null, AbilityData? sourceAbility = null)
	{
		if (target != null)
		{
			_scopeTarget.Push(target);
		}
		if (rule != null)
		{
			_scopeRule.Push(rule);
		}
		if (currentEntity != null)
		{
			_scopeCurrentEntity.Push(currentEntity);
		}
		if (areaEffect != null)
		{
			_scopeAreaEffect.Push(areaEffect);
		}
		if (sourceAbility != null)
		{
			_scopeSourceAbility.Push(sourceAbility);
		}
	}

	private void ClearScopedStacks()
	{
		_scopeTarget.Clear();
		_scopeRule.Clear();
		_scopeCurrentEntity.Clear();
		_scopeAreaEffect.Clear();
		_scopeSourceAbility.Clear();
	}

	private static EvalContext Push(IEvalContext? parent)
	{
		_CurrentIndex++;
		if (_CurrentIndex >= _Stack.Count)
		{
			_Stack.Add(new EvalContext());
		}
		EvalContext evalContext = _Stack[_CurrentIndex];
		evalContext.ClearScopedStacks();
		evalContext.Weapon = null;
		EvalContext evalContext2 = null;
		EvalContext evalContext3 = _Stack[_CurrentIndex - 1];
		if (evalContext3 != null && evalContext3 != parent)
		{
			evalContext2 = evalContext3;
			evalContext.Inherit(evalContext2);
		}
		if (parent != null)
		{
			evalContext.Inherit(parent);
		}
		evalContext.PushPropertiesIfNotNull(parent?.Target ?? evalContext2?.Target, currentEntity: parent?.CurrentEntity ?? evalContext2?.CurrentEntity, rule: parent?.Rule ?? evalContext2?.Rule, areaEffect: null, sourceAbility: parent?.SourceAbility ?? evalContext2?.SourceAbility);
		return evalContext;
	}

	private static void Pop()
	{
		if (_CurrentIndex <= 0)
		{
			throw new InvalidOperationException("EvalContext: stack underflow");
		}
		_CurrentIndex--;
	}

	private static Dictionary<string, ContextPropertyName> BuildNameToPropertyMap()
	{
		Dictionary<string, ContextPropertyName> dictionary = new Dictionary<string, ContextPropertyName>();
		foreach (ContextPropertyName value in Enum.GetValues(typeof(ContextPropertyName)))
		{
			dictionary[value.ToString()] = value;
		}
		return dictionary;
	}

	private void Prepare()
	{
		if (Caster == null)
		{
			MechanicEntity mechanicEntity = (Caster = Owner);
		}
		if (Owner == null)
		{
			MechanicEntity mechanicEntity = (Owner = Caster);
		}
		if ((object)ClickedTarget == null)
		{
			TargetWrapper targetWrapper2 = (ClickedTarget = Owner);
		}
		if (Ability == null && Fact is Ability ability)
		{
			Ability = ability.Data;
		}
		if (Target == null && ClickedTarget != null)
		{
			PushTarget(ClickedTarget);
		}
		if (AreaEffect == null && Owner is AreaEffectEntity ae)
		{
			PushAreaEffect(ae);
		}
		if (SourceAbility == null && Ability != null)
		{
			PushSourceAbility(Ability);
		}
		if (!(ClickedTarget != null))
		{
			return;
		}
		if (ClickedTarget.IsOrientationSpecified)
		{
			Direction = (Quaternion.Euler(0f, ClickedTarget.Orientation, 0f) * Vector3.forward).normalized;
			return;
		}
		Vector3? sourceCastPosition = SourceCastPosition;
		if (sourceCastPosition.HasValue)
		{
			Vector3 valueOrDefault = sourceCastPosition.GetValueOrDefault();
			Direction = (ClickedTarget.Point - valueOrDefault).normalized;
		}
	}

	public MechanicEntity? GetEntityByType(PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => CurrentEntity, 
			PropertyTargetType.CurrentTarget => CurrentTargetEntity, 
			PropertyTargetType.ContextCaster => ContextCaster, 
			PropertyTargetType.ContextOwner => ContextOwner, 
			PropertyTargetType.ContextMainTarget => ContextMainTarget?.Entity, 
			PropertyTargetType.RuleInitiator => RuleInitiator, 
			PropertyTargetType.RuleTarget => RuleTarget, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public Vector3 GetEntityPositionByType(PropertyTargetType type)
	{
		MechanicEntity entityByType = GetEntityByType(type);
		if (entityByType != null)
		{
			return entityByType.Position;
		}
		if (type == PropertyTargetType.ContextMainTarget)
		{
			return ContextMainTargetPosition;
		}
		return default(Vector3);
	}

	public IntRect GetEntityRectByType(PropertyTargetType type)
	{
		return GetEntityByType(type)?.SizeRect ?? default(IntRect);
	}

	public T? Get<T>() where T : ContextData<T>, new()
	{
		return ContextData<T>.Current;
	}

	private void Inherit(IEvalContext other)
	{
		FactComponent = (other as EvalContext)?.FactComponent ?? FactComponent;
		Weapon = (other as EvalContext)?.Weapon ?? Weapon;
		Caster = other.Caster;
		Owner = other.Owner;
		ClickedTarget = other.ClickedTarget;
		Ability = other.Ability;
		Blueprint = other.Blueprint;
		Fact = other.Fact;
		Pattern = other.Pattern;
		DisableFx = other.DisableFx;
		Direction = other.Direction;
		PropertiesArray = other.PropertiesArray;
		SourceCaster = other.SourceCaster;
		AbilityExecution = other.AbilityExecution ?? AbilityExecution;
		SourceAbilityBlueprint = other.SourceAbilityBlueprint;
		SourceBlueprint = other.SourceBlueprint;
		SourceClickedTarget = other.SourceClickedTarget;
		SourceCastPosition = other.SourceCastPosition;
		SourceFact = other.SourceFact;
	}

	public T TriggerRule<T>(T rule) where T : RulebookEvent
	{
		rule.Reason = new RuleReason(this);
		return Rulebook.Trigger(rule);
	}

	private LosDescription GetLosToClickedTarget()
	{
		if (Caster == null || ClickedTarget == null)
		{
			return default(LosDescription);
		}
		if (!Game.HasInstance)
		{
			return default(LosDescription);
		}
		VirtualPositionController virtualPositionController = Game.Instance.Controllers.VirtualPositionController;
		if (virtualPositionController == null)
		{
			return default(LosDescription);
		}
		return LosCalculations.GetWarhammerLos(virtualPositionController.GetDesiredPosition(Caster).GetNearestNodeXZUnwalkable(), Caster.SizeRect, ClickedTarget.NearestNode, ClickedTarget.SizeRect);
	}
}
