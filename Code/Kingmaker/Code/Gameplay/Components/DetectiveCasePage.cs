using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBookPage))]
[TypeId("7f7605f3bc4c4967ae9db2ae4ab754fd")]
public class DetectiveCasePage : BlueprintComponent
{
	[SerializeField]
	public LocalizedString TrueAnswerFlavorText;

	[SerializeField]
	public LocalizedString TrueAnswerText;

	[field: SerializeField]
	public BpRef<BlueprintCase> BlueprintCase { get; private set; }
}
