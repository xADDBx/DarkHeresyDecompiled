using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.Localization;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.Framework.TextTools;

public sealed class ScalingFormulaTemplate : TextTemplate
{
	private const string MissingFormulaText = "<b><color=#FF0000>!missing-scaling-formula!</color><b>";

	public override string Generate(bool capitalized, List<string> parameters)
	{
		AbilityData value = GameLogContext.DescriptionAbility.Value;
		if ((object)value != null)
		{
			LocalizedString localizedString = value.GetScaling()?.Description;
			if (localizedString == null)
			{
				return "<b><color=#FF0000>!missing-scaling-formula!</color><b>";
			}
			return localizedString;
		}
		BlueprintMechanicEntityFact value2 = GameLogContext.DescriptionFactBlueprint.Value;
		if (value2 != null)
		{
			MechanicEntity caster = (MechanicEntity)GameLogContext.DescriptionOwner.Value;
			LocalizedString localizedString = value2.GetScaling(caster)?.Description;
			if (localizedString == null)
			{
				return "<b><color=#FF0000>!missing-scaling-formula!</color><b>";
			}
			return localizedString;
		}
		return "<b><color=#FF0000>!missing-scaling-formula!</color><b>";
	}
}
