using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[ClassInfoBox("Проверяет, что блюпринт контекста (или блюпринт исходной абилки) имеет компонент WarpTag или является абилкой с AbilityParamsSource=PsychicPower")]
[TypeId("f61bd6d74e2a4727b8c8b9e1d9960ca9")]
public sealed class CheckIsWarpEffect : BoolPropertyGetter, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is warp effect";
	}

	protected override bool GetBaseValue()
	{
		MechanicsContext mechanicContext = this.GetMechanicContext();
		if (mechanicContext != null)
		{
			if (!IsWarpEffect(mechanicContext.Blueprint))
			{
				return IsWarpEffect(mechanicContext.SourceAbilityBlueprint);
			}
			return true;
		}
		return false;
	}

	private static bool IsWarpEffect(BlueprintScriptableObject blueprint)
	{
		if (blueprint.GetComponent<WarpTag>() == null)
		{
			if (blueprint is BlueprintAbility blueprintAbility)
			{
				return blueprintAbility.AbilityParamsSource == WarhammerAbilityParamsSource.PsychicPower;
			}
			return false;
		}
		return true;
	}
}
