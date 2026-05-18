using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.UI;
using UnityEngine;
using WebSocketSharp;

namespace Kingmaker.Code.UI.MVVM;

public class CueVM : ViewModel, IHasBlueprintInfo
{
	private readonly string m_Text;

	private readonly BlueprintCue m_BlueprintCue;

	public readonly bool IsSpecial;

	public readonly List<SkillCheckResult> SkillChecks;

	public readonly List<AlignmentShift> SoulMarkShifts;

	private DialogType DialogType => DialogController.Dialog.Type;

	private DialogController DialogController => Game.Instance.Controllers.DialogController;

	public BlueprintScriptableObject Blueprint => m_BlueprintCue;

	public string RawText
	{
		get
		{
			if (!m_BlueprintCue)
			{
				return m_Text;
			}
			return m_BlueprintCue.DisplayText;
		}
	}

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

	public string SpeakerName
	{
		get
		{
			if (!m_BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker)
			{
				return DialogController.CurrentSpeakerName;
			}
			return "Error Speaker";
		}
	}

	public bool IsOverrideSpeakerColor
	{
		get
		{
			if (!m_BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker && !DialogController.CurrentSpeakerName.IsNullOrEmpty())
			{
				if (DialogController.CurrentSpeaker != null)
				{
					return DialogController.CurrentSpeaker.Blueprint.OverrideColorInDialog;
				}
				return false;
			}
			return true;
		}
	}

	public Color SpeakerColor
	{
		get
		{
			if (DialogController.CurrentSpeaker == null)
			{
				return Color.black;
			}
			if (m_BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker)
			{
				return Color.red;
			}
			return DialogController.CurrentSpeaker.Blueprint.Color;
		}
	}

	public bool IsErrorSpeaker => m_BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker;

	public bool HasSkillchecks => SkillChecks?.Any() ?? false;

	public bool HasSoulMarkShift => SoulMarkShifts?.Any() ?? false;

	public bool IsShowDevComment => BuildModeUtility.IsShowDevComment;

	public string DevComment => DialogController.CurrentCueShowData.BlueprintCue.Comment;

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public CueVM(string cueText, IEnumerable<SkillCheckResult> skillChecks, IEnumerable<AlignmentShift> soulMarkShifts, bool isSpecial = false)
	{
		m_Text = cueText;
		SkillChecks = skillChecks.ToList();
		if (SkillChecks.Any())
		{
			EventBus.RaiseEvent(delegate(IDialogCueWithSkillcheckSetupHandler h)
			{
				h.HandleDialogCueWithSkillcheckSetup();
			});
		}
		SoulMarkShifts = soulMarkShifts.ToList();
		IsSpecial = isSpecial;
		IsNarratorText = DialogController.Dialog.IsNarratorText || (DialogController.CurrentCueShowData?.BlueprintCue?.IsNarratorText).GetValueOrDefault();
	}

	public CueVM(BlueprintCue cue, IEnumerable<SkillCheckResult> skillChecks, IEnumerable<AlignmentShift> soulMarkShifts, bool isSpecial = false)
		: this(string.Empty, skillChecks, soulMarkShifts, isSpecial)
	{
		m_BlueprintCue = cue;
	}

	public string GetMechanicText(SkillCheckColors skillCheckColors, SoulMarkShiftColors soulMarkShiftColors, string skillCheckSuffix = " ")
	{
		string text = "";
		if (SoulMarkShifts.Count > 0)
		{
			text += UIUtilityUnit.SoulMarkShiftsText(SoulMarkShifts, soulMarkShiftColors);
		}
		return text + UtilitySkillcheck.SkillCheckText(SkillChecks, skillCheckColors, skillCheckSuffix);
	}

	public string GetNarrativeText(DialogCueColors dialogCueColors)
	{
		return RawText;
	}
}
