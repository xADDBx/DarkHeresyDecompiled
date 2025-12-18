using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("2f983d0b114b4a9e9a83f3d3452315fe")]
public class EtudeBracketAllPlanetsVisited : BlueprintComponent
{
	[SerializeField]
	private ActionList m_OnTriggerActions;

	[SerializeField]
	private BlueprintStarSystemMap.Reference[] m_ExcludeAreas;

	private List<BlueprintStarSystemMap> ExcludedAreas => m_ExcludeAreas?.Dereference().ToList();
}
