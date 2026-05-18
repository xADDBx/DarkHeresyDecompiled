using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("8eee9d45ddcfa614d99610c1892993e3")]
public class BlueprintCue : BlueprintCueBase, ILocalizedStringHolder, IAlignmentShiftProvider, IPossibleDialogEnding
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.ByProperty)]
	public LocalizedString Text;

	[Tooltip("Приоритет юнита SpeakerEvaluator > Blueprint. Приоритет войсовера SpeakerPortrait > SpeakerEvaluator > Blueprint")]
	public DialogSpeaker Speaker;

	public bool TurnSpeaker = true;

	public bool IsNarratorText;

	public DialogAnimation Animation;

	[ShowIf("m_IsCustomAnimation")]
	public AnimationClipWrapper CustomAnimation;

	[CanBeNull]
	[Tooltip("Listener portrait (main character by default)")]
	[SerializeField]
	[FormerlySerializedAs("Listener")]
	private BlueprintUnitReference m_Listener;

	public ActionList OnShow;

	public ActionList OnStop;

	public LocalizedString Description;

	public List<BlueprintAnswerBaseReference> Answers = new List<BlueprintAnswerBaseReference>();

	public CueSelection Continue;

	[SerializeField]
	[KDB("Set it to true, if dialogue can end on this Cue and it's a valid ending \n For example: dialog should break with cutscene and continue after that")]
	private bool IsPossibleDialogEnding;

	public string DisplayText => Text;

	private bool m_IsCustomAnimation => Animation == DialogAnimation.Custom;

	public BlueprintUnit Listener => m_Listener?.Get();

	public LocalizedString LocalizedStringText => Text;

	public IEnumerable<AlignmentShift> AlignmentShifts => from AddAlignmentRank a in OnShow.Actions.Where((GameAction a) => a is AddAlignmentRank).Concat(OnStop.Actions.Where((GameAction a) => a is AddAlignmentRank))
		select a.Shift;

	public AlignmentAxis AlignmentShiftAxis
	{
		get
		{
			if (!AlignmentShifts.Any())
			{
				return AlignmentAxis.None;
			}
			return AlignmentShifts.First().Axis;
		}
	}

	public int AlignmentShiftRank
	{
		get
		{
			if (!AlignmentShifts.Any())
			{
				return 0;
			}
			return AlignmentShifts.First().Value;
		}
	}

	public bool IsPossibleEnding => IsPossibleDialogEnding;

	public override bool CanShow(bool isFromSequence = false)
	{
		if (!base.CanShow(isFromSequence))
		{
			return false;
		}
		if (Speaker.NeedsEntity && !Speaker.TryGetSpeakerEntity(this, out var _, isFromSequence))
		{
			return false;
		}
		return true;
	}
}
