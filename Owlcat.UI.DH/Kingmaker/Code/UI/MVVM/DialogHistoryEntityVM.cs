using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DialogHistoryEntityVM : ViewModel
{
	private readonly string m_Text;

	public readonly string SpeakerName;

	public readonly Color SpeakerColor;

	public readonly List<SkillCheckResult> SkillChecks;

	public readonly List<AlignmentShift> SoulMarkShifts;

	public DialogType DialogType => DialogController.Dialog.Type;

	private DialogController DialogController => Game.Instance.Controllers.DialogController;

	public string Text => m_Text;

	public bool IsNarratorText { get; private set; }

	public bool HasSpeaker
	{
		get
		{
			if (DialogType == DialogType.Common && !IsNarratorText)
			{
				return DialogController.CurrentSpeakerBlueprint != null;
			}
			return false;
		}
	}

	public bool HasSkillchecks => SkillChecks?.Any() ?? false;

	public bool HasSoulMarkShift => SoulMarkShifts?.Any() ?? false;

	public DialogHistoryEntityVM(IDialogShowData dialogShowData)
	{
		m_Text = dialogShowData.Text;
		SpeakerName = dialogShowData.SpeakerName;
		SpeakerColor = dialogShowData.SpeakerColor;
		SkillChecks = dialogShowData.SkillChecks?.ToList();
		SoulMarkShifts = dialogShowData.ConvictionShifts?.ToList();
	}
}
