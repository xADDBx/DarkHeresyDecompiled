using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSoulMarkShiftRecordVM : ViewModel
{
	private readonly AlignmentShiftHistoryEntry m_AlignmentShift;

	public LocalizedString Description => m_AlignmentShift.Description;

	public AlignmentAxis Axis => m_AlignmentShift.Axis;

	public int Amount => m_AlignmentShift.Rank;

	public List<BlueprintMechanicEntityFact> NewFacts => m_AlignmentShift.NewFacts;

	public CharInfoSoulMarkShiftRecordVM(AlignmentShiftHistoryEntry alignmentShift)
	{
		m_AlignmentShift = alignmentShift;
	}
}
