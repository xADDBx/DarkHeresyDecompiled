using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public class AnswerShowData : IDialogShowData
{
	private readonly BlueprintAnswer m_Answer;

	private readonly BlueprintUnit m_CharacterBlueprint;

	public string SpeakerName => m_CharacterBlueprint?.CharacterName;

	public Color SpeakerColor => m_CharacterBlueprint.Color;

	public string Text => m_Answer.DisplayText;

	public IEnumerable<SkillCheckResult> SkillChecks => null;

	public IEnumerable<AlignmentShift> ConvictionShifts => null;

	public AnswerShowData(BlueprintAnswer answer, BlueprintUnit characterBlueprint = null)
	{
		m_Answer = answer;
		m_CharacterBlueprint = characterBlueprint;
	}
}
