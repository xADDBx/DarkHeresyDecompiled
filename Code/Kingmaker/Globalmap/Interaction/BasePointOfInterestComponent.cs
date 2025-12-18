using System;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Obsolete]
[TypeId("8cc11777ee7a46c9a395221dc8753cbb")]
public abstract class BasePointOfInterestComponent : BlueprintComponent
{
	[SerializeField]
	public BlueprintPointOfInterestReference m_PointBlueprint;

	public virtual BlueprintPointOfInterest PointBlueprint => m_PointBlueprint?.Get();
}
