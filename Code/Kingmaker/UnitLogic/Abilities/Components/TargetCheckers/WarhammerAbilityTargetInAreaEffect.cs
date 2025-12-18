using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Target Restriction/WarhammerAbilityTargetInAreaEffect")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("245c8bbfd839454cb3e6df4b1c12932e")]
public class WarhammerAbilityTargetInAreaEffect : BlueprintComponent, IAbilityTargetRestriction
{
	[SerializeField]
	private BlueprintAreaEffectReference m_AreaEffect;

	public bool AnyStrategistZone;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		BlueprintAreaEffect blueprintAreaEffect = m_AreaEffect?.Get();
		if (blueprintAreaEffect == null && !AnyStrategistZone)
		{
			PFLog.Default.Error("Area effect not set");
			return true;
		}
		Vector3 point = target.Point;
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Blueprint == blueprintAreaEffect && areaEffect.Contains(point))
			{
				return true;
			}
			BlueprintAreaEffect blueprint = areaEffect.Blueprint;
			if (blueprint != null && blueprint.IsStrategistAbility && AnyStrategistZone && areaEffect.Contains(point))
			{
				return true;
			}
		}
		return (target.Entity?.GetOptional<UnitPartSpawnedAreaEffects>()?.Contains(blueprintAreaEffect)).GetValueOrDefault();
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return ConfigRoot.Instance.LocalizedTexts.Reasons.TargetNotInAreaEffect.ToString(delegate
		{
			GameLogContext.Text = m_AreaEffect?.Get().Name;
		});
	}
}
