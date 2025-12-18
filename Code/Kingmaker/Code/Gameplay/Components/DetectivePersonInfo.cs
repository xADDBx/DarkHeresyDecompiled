using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintClue))]
[TypeId("d3a2f7585f544025a0dce4ab13c3618f")]
public class DetectivePersonInfo : BlueprintComponent
{
	[field: SerializeField]
	public BodyType BodyType { get; private set; } = BodyType.Other;


	[field: SerializeField]
	public SpriteLink IconInProfile { get; private set; }

	[field: SerializeField]
	public int Height { get; private set; }

	[field: SerializeField]
	public int Weight { get; private set; }

	[field: SerializeField]
	public int Age { get; private set; }

	[field: SerializeField]
	public LocalizedString HairColor { get; private set; }

	[field: SerializeField]
	public LocalizedString EyesColor { get; private set; }
}
