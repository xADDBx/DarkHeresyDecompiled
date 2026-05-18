using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintCaseAnswer))]
[TypeId("32c97c4161bc486a94cac122045e42d2")]
public class CaseAnswerUISettings : BlueprintComponent
{
	[HideIf("HideCaseAnswerRelatedItem")]
	[SerializeField]
	private LocalizedString OverrideAnswerFormat;

	[field: SerializeField]
	public bool HideCaseAnswerRelatedItem { get; private set; }

	public LocalizedString GetAnswerFormat()
	{
		return OverrideAnswerFormat;
	}
}
