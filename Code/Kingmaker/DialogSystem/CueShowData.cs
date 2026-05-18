using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.DialogSystem;

public class CueShowData : IDialogShowData
{
	public readonly BlueprintCue BlueprintCue;

	private readonly BlueprintUnit m_SpeakerBlueprint;

	private readonly string m_SpeakerName;

	private SkillCheckResult[] m_SkillChecks;

	private AlignmentShift[] m_ConvictionShifts;

	public string SpeakerName => m_SpeakerName;

	public bool IsOverrideSpeakerColor
	{
		get
		{
			if (m_SpeakerBlueprint != null && !BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker)
			{
				return m_SpeakerBlueprint.OverrideColorInDialog;
			}
			return true;
		}
	}

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

	public IEnumerable<AlignmentShift> ConvictionShifts => m_ConvictionShifts;

	public CueShowData(BlueprintCue blueprintCue, IEnumerable<SkillCheckResult> skillChecks, IEnumerable<AlignmentShift> soulMarkShifts = null)
	{
		BlueprintCue = blueprintCue;
		m_SkillChecks = skillChecks.ToArray();
		if (soulMarkShifts != null)
		{
			m_ConvictionShifts = soulMarkShifts.ToArray();
		}
		else
		{
			m_ConvictionShifts = new AlignmentShift[0];
		}
	}

	public CueShowData(BlueprintCue blueprintCue, string speakerName = "", BlueprintUnit speakerBlueprint = null)
	{
		BlueprintCue = blueprintCue;
		m_SpeakerName = speakerName;
		m_SpeakerBlueprint = speakerBlueprint;
	}
}
