using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("ed0e545d0f3c91c42bba1e4dceb1b6e6")]
public class AbilityTargetInAreaEffectGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAreaEffectReference m_Area;

	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		BlueprintAreaEffect blueprintAreaEffect = m_Area.Get();
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (areaEffect.Blueprint == blueprintAreaEffect && areaEffect.Contains(this.GetTargetPositionByType(Target)))
			{
				return true;
			}
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " is in Area Effect";
	}
}
