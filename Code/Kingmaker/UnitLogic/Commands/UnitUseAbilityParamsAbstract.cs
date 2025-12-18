using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class UnitUseAbilityParamsAbstract<T> : UnitCommandParams<T>, IUnitUseAbilityParamsAbstract, IOwlPackable<UnitUseAbilityParamsAbstract<T>> where T : AbstractUnitCommand
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected bool? m_IgnoreCooldown;

	[JsonProperty]
	[OwlPackInclude]
	public bool DisableLog { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public AttackHitPolicyType HitPolicy { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public DamagePolicyType DamagePolicy { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool KillTarget { get; set; }

	public bool IgnoreAbilityUsingInThreateningArea { get; set; }

	public MechanicsContext ParentContext { get; set; }

	public AbilityData Ability { get; protected set; }

	public bool DisableCameraFollow { get; set; }

	public IAbilityCustomAnimation CustomAnimationOverride { get; set; }

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

	public virtual bool DefaultIgnoreCooldown => false;

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
		ParentContext = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current;
	}
}
