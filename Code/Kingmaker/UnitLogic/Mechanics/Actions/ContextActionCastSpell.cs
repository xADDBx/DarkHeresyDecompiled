using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("2f9cfff2340b8c344ab4fd92c2eb61f2")]
public class ContextActionCastSpell : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Spell")]
	private BlueprintAbilityReference m_Spell;

	public bool CastByTarget;

	[Tooltip("Enables animation for casting and initiates full UseAbility command instead of simply triggering cast rule")]
	public bool UseFullAbilityCastCycle;

	public bool CastOnTargetPoint;

	[SerializeField]
	[SerializeReference]
	[ShowIf("CastOnTargetPoint")]
	public PositionEvaluator TargetPoint;

	[Tooltip("Append the source ability's modifiers to the cast spell's modifier list.")]
	public bool InheritModifiers;

	[Tooltip("Use the source ability's resolved scaling for the cast spell.")]
	public bool InheritScalings;

	public BlueprintAbility Spell => m_Spell?.Get();

	public override string GetCaption()
	{
		return $"Cast spell {Spell}";
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = (CastByTarget ? base.Target.Entity : base.Context.Caster);
		if (mechanicEntity == null)
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		TargetWrapper targetWrapper = (TargetPoint ? ((TargetWrapper)TargetPoint.GetValue()) : base.Target);
		AbilityData abilityData = mechanicEntity.GetAbilityDataWithUpgrades(Spell) ?? new AbilityData(Spell, mechanicEntity);
		AbilityData sourceAbility = base.Context.SourceAbility;
		ScalingInfo? scalingOverride = ((!InheritScalings) ? null : sourceAbility?.GetScaling());
		if (InheritModifiers && sourceAbility != null)
		{
			abilityData = abilityData.Clone(sourceAbility.Modifiers);
		}
		else if (scalingOverride.HasValue)
		{
			abilityData = abilityData.Clone();
		}
		if (scalingOverride.HasValue)
		{
			abilityData.ScalingOverride = scalingOverride;
		}
		if (UseFullAbilityCastCycle)
		{
			PartUnitCommands commandsOptional = base.Caster.GetCommandsOptional();
			if (commandsOptional != null)
			{
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(abilityData, targetWrapper)
				{
					IgnoreCooldown = true,
					FreeAction = true
				};
				commandsOptional.AddToQueue(cmdParams);
				return;
			}
		}
		RulePerformAbility obj = new RulePerformAbility(abilityData, targetWrapper, base.Context)
		{
			IgnoreCooldown = true,
			ForceFreeAction = true
		};
		Rulebook.Trigger(obj);
		obj.Context.RewindActionIndex();
	}
}
