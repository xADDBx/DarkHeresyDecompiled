using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.Localization;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.Framework.TextTools;

public sealed class ScalingFormulaTemplate : TextTemplate
{
	private const string MissingFormulaText = "<b><color=#FF0000>!missing-scaling-formula!</color><b>";

	public override string Generate(bool capitalized, List<string> parameters)
	{
		LocalizedString @string = GetString();
		if (@string == null)
		{
			return "<b><color=#FF0000>!missing-scaling-formula!</color><b>";
		}
		return @string;
	}

	public static LocalizedString GetString()
	{
		AbilityData value = GameLogContext.DescriptionAbility.Value;
		if ((object)value != null)
		{
			return value.GetScaling()?.Description;
		}
		BlueprintMechanicEntityFact value2 = GameLogContext.DescriptionFactBlueprint.Value;
		if (value2 != null)
		{
			MechanicEntity owner = (MechanicEntity)GameLogContext.DescriptionOwner.Value;
			return ResolveFact(value2, owner);
		}
		return null;
	}

	private static LocalizedString ResolveFact(BlueprintMechanicEntityFact fact, MechanicEntity owner)
	{
		if (owner != null)
		{
			MechanicEntityFact mechanicEntityFact = owner.Facts.Get<MechanicEntityFact>(fact);
			if (mechanicEntityFact != null)
			{
				ScalingInfo? scaling = mechanicEntityFact.GetScaling();
				if (scaling.HasValue)
				{
					return scaling.Value.Description;
				}
			}
		}
		ScalingInfo? scaling2 = fact.GetScaling(owner);
		if (scaling2.HasValue)
		{
			return scaling2.Value.Description;
		}
		if (owner != null)
		{
			MechanicEntityFact mechanicEntityFact2 = owner.Facts.Get<MechanicEntityFact>(fact);
			AbilityData abilityData = mechanicEntityFact2?.MaybeContext?.SourceAbility;
			if (abilityData != null)
			{
				ScalingInfo? scaling3 = abilityData.GetScaling();
				if (scaling3.HasValue)
				{
					return scaling3.Value.Description;
				}
			}
			BlueprintMechanicEntityFact blueprintMechanicEntityFact = mechanicEntityFact2?.MaybeContext?.SourceFact?.Blueprint;
			if (blueprintMechanicEntityFact != null && blueprintMechanicEntityFact != fact)
			{
				scaling2 = blueprintMechanicEntityFact.GetScaling(owner);
				if (scaling2.HasValue)
				{
					return scaling2.Value.Description;
				}
			}
		}
		scaling2 = ScalingHelper.GetScalingFromDescriptionFact(fact, owner);
		if (scaling2.HasValue)
		{
			return scaling2.Value.Description;
		}
		return null;
	}
}
