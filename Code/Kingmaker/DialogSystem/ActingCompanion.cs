using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem;

[Obsolete]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("0eeae5d8661f0a648972f78edc348d80")]
public class ActingCompanion : BlueprintComponent
{
	[SerializeField]
	[FormerlySerializedAs("Companion")]
	private BlueprintUnitReference m_Companion;

	public BlueprintUnit Companion => m_Companion?.Get();
}
