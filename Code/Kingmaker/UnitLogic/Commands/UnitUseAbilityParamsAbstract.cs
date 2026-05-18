using CodeGenerators.MemoryPackUnionGenerator;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Framework;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using MemoryPack;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackDynamicUnion]
public abstract class UnitUseAbilityParamsAbstract<T> : UnitCommandParams<T>, IUnitUseAbilityParamsAbstract, IOwlPackable<UnitUseAbilityParamsAbstract<T>> where T : AbstractUnitCommand
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	[MemoryPackInclude]
	protected bool? m_IgnoreCooldown;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public bool DisableLog { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public AttackHitPolicyType HitPolicy { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public DamagePolicyType DamagePolicy { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	public bool KillTarget { get; set; }

	[MemoryPackIgnore]
	public bool IgnoreAbilityUsingInThreateningArea { get; set; }

	[MemoryPackIgnore]
	public MechanicsContext ParentContext { get; set; }

	[MemoryPackIgnore]
	public AbilityData Ability { get; protected set; }

	[MemoryPackIgnore]
	public bool DisableCameraFollow { get; set; }

	[MemoryPackIgnore]
	public IAbilityCustomAnimation CustomAnimationOverride { get; set; }

	[MemoryPackIgnore]
	public bool IgnoreCooldown
	{
		get
		{
			return m_IgnoreCooldown ?? DefaultIgnoreCooldown;
		}
		set
		{
			m_IgnoreCooldown = value;
		}
	}

	protected override bool DefaultFreeAction => Ability.IsFreeAction;

	protected override bool DefaultNeedLoS => Ability.NeedLoS;

	public override int DefaultApproachRadius => Ability.RangeCells;

	[MemoryPackIgnore]
	public virtual bool DefaultIgnoreCooldown => false;

	[MemoryPackIgnore]
	public override bool IsDirectionCorrect => true;

	protected UnitUseAbilityParamsAbstract()
	{
	}

	[JsonConstructor]
	protected UnitUseAbilityParamsAbstract(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitUseAbilityParamsAbstract([NotNull] TargetWrapper target)
		: base(target)
	{
	}

	protected UnitUseAbilityParamsAbstract([NotNull] AbilityData ability, [NotNull] TargetWrapper target)
		: this(target)
	{
		Ability = ability;
		ParentContext = EvalContext.Current.AbilityExecution;
	}
}
