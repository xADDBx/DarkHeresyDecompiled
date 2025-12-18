using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

[AllowedOn(typeof(BlueprintStarSystemObject))]
[TypeId("41491fb156bc45d6b3efb0be1c73afde")]
public class MultiEntranceObject : BlueprintComponent
{
	[ValidateNotNull]
	[SerializeField]
	public BlueprintMultiEntranceReference Entrance;
}
