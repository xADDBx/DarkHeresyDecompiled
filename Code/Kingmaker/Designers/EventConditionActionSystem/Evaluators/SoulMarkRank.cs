using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("3fe34667d45f4ff2a1614dd885d8c2d3")]
public class SoulMarkRank : IntEvaluator
{
	public AlignmentAxis AlignmentMark;

	public override string GetCaption()
	{
		return $"Rank of alignment mark {AlignmentMark}";
	}

	protected override int GetValueInternal()
	{
		return AlignmentShiftExtension.GetMainCharacterAlignmentMark(AlignmentMark);
	}
}
