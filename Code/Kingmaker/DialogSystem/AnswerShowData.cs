using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.DialogSystem;

public class AnswerShowData : IDialogShowData
{
	private readonly BlueprintAnswer m_Answer;

	private readonly BlueprintUnit m_CharacterBlueprint;

	public string SpeakerName { get; }

	public bool IsOverrideSpeakerColor => m_CharacterBlueprint?.OverrideColorInDialog ?? false;

	public Color SpeakerColor => m_CharacterBlueprint?.Color ?? Color.black;

	public string Text => m_Answer.DisplayText;

	public IEnumerable<SkillCheckResult> SkillChecks => null;

	public IEnumerable<AlignmentShift> ConvictionShifts => null;

	public AnswerShowData(BlueprintAnswer answer, string speakerName, BlueprintUnit characterBlueprint = null)
	{
		m_Answer = answer;
		SpeakerName = speakerName;
		m_CharacterBlueprint = characterBlueprint;
	}
}
