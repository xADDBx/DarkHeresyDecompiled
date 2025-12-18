using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("dfb7daf626ba4548a64215686e8a6187")]
public class CheckIsPlayerDominantAlignment : Condition
{
	[SerializeField]
	private AlignmentAxis m_Direction;

	protected override string GetConditionCaption()
	{
		return "Check if Soulmark is dominant";
	}

	protected override bool CheckCondition()
	{
		if (!AlignmentShiftExtension.TryGetMainCharacterDominantAxis(out var alignmentAxis))
		{
			PFLog.Default.Error("CheckIsPlayerDominantSoulmark: there're two equally dominant Soulmark. Cannot decide, return false");
			return false;
		}
		return alignmentAxis == m_Direction;
	}
}
