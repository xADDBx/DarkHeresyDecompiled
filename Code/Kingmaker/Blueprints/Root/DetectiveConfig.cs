using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class DetectiveConfig
{
	[field: SerializeField]
	public Sprite UnknownCluesIcon { get; private set; }

	[field: SerializeField]
	public Color ClueSelectedColor { get; private set; }

	[field: SerializeField]
	public Color ClueHighlightColor { get; private set; }

	[field: SerializeField]
	public Color ClassifiedDefaultColor { get; private set; }

	[field: SerializeField]
	public Color ClassifiedHighlightColor { get; private set; }

	[field: SerializeField]
	public Color OverrideAddendumAddition { get; private set; }

	[field: SerializeField]
	public bool ShowSolidLines { get; private set; } = true;


	[field: SerializeField]
	public bool ShowDescriptionWhenDrag { get; private set; } = true;


	[field: SerializeField]
	public bool UseDefaultConclusionWindow { get; private set; }

	[field: SerializeField]
	public float DelayBeforeMusicStateDetectiveBoard { get; private set; }

	[field: SerializeField]
	public List<EnumIconEntry<DetectiveCaseIssueType>> IssuingTypeIcons { get; private set; } = new List<EnumIconEntry<DetectiveCaseIssueType>>();


	[field: SerializeField]
	public AnswerDebugValues AnswerDebugValues { get; private set; }

	[field: SerializeField]
	public bool MoveToNewClue { get; private set; }

	public Sprite GetIssuingTypeIcon(DetectiveCaseIssueType type)
	{
		if (IssuingTypeIcons.FirstOrDefault((EnumIconEntry<DetectiveCaseIssueType> i) => i.Type == type) != null)
		{
			return IssuingTypeIcons.FirstOrDefault((EnumIconEntry<DetectiveCaseIssueType> i) => i.Type == type)?.Icon;
		}
		return IssuingTypeIcons.FirstOrDefault()?.Icon;
	}

	public void SetMoveToNewClue(bool value)
	{
		MoveToNewClue = value;
	}
}
