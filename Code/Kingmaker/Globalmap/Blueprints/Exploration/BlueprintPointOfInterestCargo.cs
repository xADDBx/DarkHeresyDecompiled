using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[Obsolete]
[TypeId("99a3644cfd7fbef43b5eea4bf3eac4db")]
public class BlueprintPointOfInterestCargo : BlueprintPointOfInterest
{
	[SerializeField]
	private List<BlueprintCargoReference> m_ExplorationCargo = new List<BlueprintCargoReference>();

	public List<BlueprintCargo> ExplorationCargo => m_ExplorationCargo?.Dereference().ToList();
}
