using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Alignments;

public static class AlignmentShiftExtension
{
	public static void ApplyShiftToMainCharacter(AlignmentShift shift, BlueprintScriptableObject source = null)
	{
		BaseUnitEntity mainCharacterOriginalEntity = Game.Instance.Player.MainCharacterOriginalEntity;
		ApplyShiftTo(shift, mainCharacterOriginalEntity, source);
	}

	public static void ApplyShiftTo(AlignmentShift shift, BaseUnitEntity unit, BlueprintScriptableObject source = null)
	{
		if (shift.Value != 0 && shift.Axis != 0)
		{
			unit.Alignment.ApplyShift(shift, source);
		}
	}

	public static int GetMainCharacterAlignmentMark(AlignmentAxis axis)
	{
		return Game.Instance.Player.MainCharacterOriginalEntity.Alignment.GetAlignmentMark(axis);
	}

	public static int GetMainCharacterAlignmentRank(AlignmentAxis axis)
	{
		return Game.Instance.Player.MainCharacterOriginalEntity.Alignment.GetAlignmentRank(axis);
	}

	public static int GetAlignmentMarkRankIndex(AlignmentAxis alignmentAxis, int currentRank)
	{
		return ConfigRoot.Instance.AlignmentMarksRoot.GetMarkForRank(alignmentAxis, currentRank);
	}

	private static IEnumerable<AlignmentShiftHistoryEntry> AppliedShifts(AlignmentAxis direction)
	{
		BaseUnitEntity mainCharacterOriginalEntity = Game.Instance.Player.MainCharacterOriginalEntity;
		foreach (AlignmentShiftHistoryEntry item in mainCharacterOriginalEntity.Alignment.ShiftHistory)
		{
			if (item.Axis == direction)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<AlignmentShiftHistoryEntry> AppliedShifts()
	{
		BaseUnitEntity mainCharacterOriginalEntity = Game.Instance.Player.MainCharacterOriginalEntity;
		foreach (AlignmentShiftHistoryEntry item in mainCharacterOriginalEntity.Alignment.ShiftHistory)
		{
			yield return item;
		}
	}

	public static bool TryGetMainCharacterDominantAxis(out AlignmentAxis alignmentAxis)
	{
		int num = 0;
		alignmentAxis = AlignmentAxis.None;
		foreach (AlignmentAxis item in Enum.GetValues(typeof(AlignmentAxis)).Cast<AlignmentAxis>())
		{
			int mainCharacterAlignmentRank = GetMainCharacterAlignmentRank(item);
			if (mainCharacterAlignmentRank > num)
			{
				num = mainCharacterAlignmentRank;
				alignmentAxis = item;
			}
			else if (mainCharacterAlignmentRank == num)
			{
				alignmentAxis = AlignmentAxis.None;
			}
		}
		return alignmentAxis != AlignmentAxis.None;
	}
}
