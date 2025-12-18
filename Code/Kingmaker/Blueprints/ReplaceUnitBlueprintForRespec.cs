using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("e54ac94ff517aaf45b4ee9c09d77d8c0")]
public class ReplaceUnitBlueprintForRespec : BlueprintComponent
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Blueprint;

	public BlueprintUnit Blueprint => m_Blueprint;
}
