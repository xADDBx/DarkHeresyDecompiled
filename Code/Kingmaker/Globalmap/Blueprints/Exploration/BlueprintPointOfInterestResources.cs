using System;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[Obsolete]
[TypeId("cf8147101b6b49b2b3bfd5466175acbf")]
public class BlueprintPointOfInterestResources : BlueprintPointOfInterest
{
	[SerializeField]
	private ResourceData m_ExplorationResource;

	public ResourceData ExplorationResource => m_ExplorationResource;
}
