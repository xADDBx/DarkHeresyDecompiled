using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.Gameplay.Features.Experience;

namespace Kingmaker.Items;

public static class UnitEquipmentCRHelper
{
	public static int GetEquipmentCR(BaseUnitEntity unit)
	{
		int cR = unit.CR;
		int num = ResolveOffset(unit);
		return Math.Max(0, cR + num);
	}

	private static int ResolveOffset(BaseUnitEntity unit)
	{
		UnitDifficultyType difficultyType = unit.OriginalBlueprint.DifficultyType;
		BlueprintEncounter blueprintEncounter = unit.GetOptional<PartEncounter>()?.Blueprint;
		if (blueprintEncounter != null)
		{
			int? num = blueprintEncounter.GetComponent<EncounterEquipmentCROffsetOverride>()?.GetOffset(difficultyType);
			if (num.HasValue)
			{
				return num.Value;
			}
		}
		return ConfigRoot.Instance.ItemProgressionRoot.GetDifficultyEquipmentCROffset(difficultyType);
	}
}
