using System.Collections.Generic;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.DialogSystem;

public interface IDialogShowData
{
	string SpeakerName { get; }

	bool IsOverrideSpeakerColor { get; }

	Color SpeakerColor { get; }

	string Text { get; }

	IEnumerable<SkillCheckResult> SkillChecks { get; }

	IEnumerable<AlignmentShift> ConvictionShifts { get; }
}
