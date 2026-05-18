using System;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;

namespace Kingmaker.RuleSystem.Rules;

[RuleRoles(Initiator = "ability caster", Target = "ability target")]
public class RulePerformAbility : RulebookEvent
{
	private EntityFactRef<MechanicEntityFact> m_ReplaceTargetSource;

	[NotNull]
	public readonly AbilityData Ability;

	public readonly AbilityExecutionContext Context;

	public bool Success { get; private set; }

	[CanBeNull]
	public AbilityExecutionProcess Result { get; private set; }

	[Obsolete]
	[CanBeNull]
	public MechanicsContext ExecutionActionContext { get; set; }

	public bool IsCutscene { get; set; }

	public bool IgnoreCooldown
	{
		get
		{
			return Context.IgnoreCooldown;
		}
		set
		{
			Context.IgnoreCooldown = value;
		}
	}

	public bool ForceFreeAction
	{
		get
		{
			return Context.FreeAction;
		}
		set
		{
			Context.FreeAction = value;
		}
	}

	[NotNull]
	public TargetWrapper AbilityTarget => Context.ClickedTarget;

	public override AbilityData MaybeAbility => Ability;

	[CanBeNull]
	public MechanicEntityFact ReplaceTargetSource => m_ReplaceTargetSource;

	public override MechanicEntity Target => AbilityTarget.Entity;

	public RulePerformAbility([NotNull] Ability spell, [NotNull] TargetWrapper abilityTarget, IEvalContext parentContext = null)
		: this(spell.Data, abilityTarget, parentContext)
	{
	}

	public RulePerformAbility([NotNull] AbilityData ability, [NotNull] TargetWrapper abilityTarget, IEvalContext parentContext = null)
		: base(ability.Caster)
	{
		Ability = ability;
		Context = ability.ClaimExecutionContext(abilityTarget, parentContext);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Context.IsForced = IsCutscene || base.ConcreteInitiator.IsCheater;
		if (!Context.IsForced && !Ability.IsValid(AbilityTarget))
		{
			PFLog.Default.ErrorWithReport($"Invalid target {Target} for spell '{Ability.Blueprint}'");
			return;
		}
		Success = true;
		using (ContextData<AbilityData.IgnoreCooldown>.RequestIf(IgnoreCooldown))
		{
			using (ContextData<AbilityData.ForceFreeAction>.RequestIf(ForceFreeAction))
			{
				Result = Ability.Cast(Context);
			}
		}
	}

	public void ReplaceTarget([NotNull] TargetWrapper replacement, [NotNull] MechanicEntityFact source)
	{
		m_ReplaceTargetSource = source;
		Context.ReplaceClickedTarget(replacement);
	}
}
