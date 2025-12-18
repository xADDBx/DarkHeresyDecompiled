using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SurfaceCombatActionVM : ViewModel
{
	private Sprite m_Icon;

	private string m_Name;

	private string m_SecondaryText;

	public Buff ActionAbility { get; }

	public TooltipTemplateBuff ActionAbilityTooltip { get; }

	public Sprite Icon
	{
		get
		{
			if (m_Icon == null)
			{
				UpdateFields();
			}
			return m_Icon;
		}
	}

	public SurfaceCombatActionVM(Buff actionAbilityAbility)
	{
		ActionAbility = actionAbilityAbility;
		UpdateFields();
		ActionAbilityTooltip = new TooltipTemplateBuff(ActionAbility, null, isConcentration: true, Icon, m_Name, m_SecondaryText);
	}

	private void UpdateFields()
	{
		if (ActionAbility == null)
		{
			return;
		}
		AbilityAttackDelivery abilityAttackDelivery = ActionAbility.GetComponent<ChannelingLogic>()?.Ability?.Blueprint.GetComponent<AbilityAttackDelivery>();
		if (abilityAttackDelivery != null && abilityAttackDelivery.IsPrecise)
		{
			MechanicsContext context = ActionAbility.Context;
			AbilityData sourceAbility = context.SourceAbility;
			if (sourceAbility != null)
			{
				MechanicEntity entity = context.ClickedTarget.Entity;
				BlueprintBodyPart preciseBodyPart = sourceAbility.PreciseBodyPart;
				PartLifeState lifeStateOptional = entity.GetLifeStateOptional();
				if (lifeStateOptional != null && preciseBodyPart != null)
				{
					int stage = Mathf.Clamp(lifeStateOptional.Health.GetCriticalStage(preciseBodyPart) + 1, 0, preciseBodyPart.CriticalEffectStagesCount);
					BlueprintBuff criticalEffectStageBuff = preciseBodyPart.GetCriticalEffectStageBuff(stage);
					m_Icon = criticalEffectStageBuff.Icon;
					m_Name = sourceAbility.Name + ": " + preciseBodyPart.Name.Text;
					m_SecondaryText = GetSecondaryText(preciseBodyPart, entity);
				}
			}
		}
		if ((object)m_Icon == null)
		{
			m_Icon = ActionAbility?.Icon;
		}
		if (m_Name == null)
		{
			m_Name = ActionAbility?.Name;
		}
	}

	private static string GetSecondaryText(BlueprintBodyPart bodyPart, MechanicEntity target)
	{
		if (bodyPart == null)
		{
			return null;
		}
		string result = null;
		if (bodyPart.CanBreakTargetConcentrationIfHit(target, checkTargetHasConcentration: false))
		{
			result = LocalizedTexts.Instance.PreciseAttack.CanBreakTargetConcentrationIfHit;
		}
		else if (bodyPart.CanChangeTargetTurnOrderIfHit())
		{
			result = LocalizedTexts.Instance.PreciseAttack.CanChangeTargetTurnOrderIfHit;
		}
		return result;
	}
}
