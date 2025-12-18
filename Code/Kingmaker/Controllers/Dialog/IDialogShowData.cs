using System.Collections.Generic;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public interface IDialogShowData
{
	string SpeakerName { get; }

	Color SpeakerColor { get; }

	string Text { get; }

	IEnumerable<SkillCheckResult> SkillChecks { get; }

	IEnumerable<AlignmentShift> ConvictionShifts { get; }
}
