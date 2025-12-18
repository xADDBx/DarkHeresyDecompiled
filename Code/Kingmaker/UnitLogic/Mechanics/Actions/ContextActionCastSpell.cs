using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
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

	public BlueprintAbility Spell => m_Spell?.Get();

	public override string GetCaption()
	{
		return $"Cast spell {Spell}";
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = (CastByTarget ? base.Target.Entity : base.Context.MaybeCaster);
		if (mechanicEntity == null)
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		TargetWrapper targetWrapper = (TargetPoint ? ((TargetWrapper)TargetPoint.GetValue()) : base.Target);
		AbilityData ability = mechanicEntity.GetAbilityDataWithUpgrades(Spell) ?? new AbilityData(Spell, mechanicEntity);
		if (UseFullAbilityCastCycle)
		{
			PartUnitCommands commandsOptional = base.Caster.GetCommandsOptional();
			if (commandsOptional != null)
			{
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability, targetWrapper)
				{
					IgnoreCooldown = true,
					FreeAction = true
				};
				commandsOptional.AddToQueue(cmdParams);
				return;
			}
		}
		RulePerformAbility obj = new RulePerformAbility(ability, targetWrapper, base.Context)
		{
			IgnoreCooldown = true,
			ForceFreeAction = true
		};
		Rulebook.Trigger(obj);
		obj.Context.RewindActionIndex();
	}
}
