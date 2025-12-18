using System.Collections.Generic;

namespace Kingmaker.UnitLogic.Alignments;

public interface IAlignmentShiftProvider
{
	IEnumerable<AlignmentShift> AlignmentShifts { get; }

	AlignmentAxis AlignmentShiftAxis { get; }

	int AlignmentShiftRank { get; }
}
