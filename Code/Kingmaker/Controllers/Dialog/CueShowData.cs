using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public class CueShowData : IDialogShowData
{
	public readonly BlueprintCue BlueprintCue;

	private readonly BlueprintUnit m_SpeakerBlueprint;

	private readonly string m_SpeakerName;

	private IEnumerable<SkillCheckResult> m_SkillChecks;

	private IEnumerable<AlignmentShift> m_SoulMarkShifts;

	public string SpeakerName => m_SpeakerName;

	public Color SpeakerColor
	{
		get
		{
			if (m_SpeakerBlueprint == null)
			{
				return Color.black;
			}
			if (BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker)
			{
				return Color.red;
			}
			return m_SpeakerBlueprint.Color;
		}
	}

	public string Text => BlueprintCue.DisplayText;

	public IEnumerable<SkillCheckResult> SkillChecks => m_SkillChecks;

	public IEnumerable<AlignmentShift> ConvictionShifts => m_SoulMarkShifts;

	public CueShowData(BlueprintCue blueprintCue, IEnumerable<SkillCheckResult> skillChecks, IEnumerable<AlignmentShift> soulMarkShifts)
	{
		BlueprintCue = blueprintCue;
		m_SkillChecks = skillChecks.ToList();
		m_SoulMarkShifts = soulMarkShifts.ToList();
	}

	public CueShowData(BlueprintCue blueprintCue, string speakerName = "", BlueprintUnit speakerBlueprint = null)
	{
		BlueprintCue = blueprintCue;
		m_SpeakerName = speakerName;
		m_SpeakerBlueprint = speakerBlueprint;
	}
}
