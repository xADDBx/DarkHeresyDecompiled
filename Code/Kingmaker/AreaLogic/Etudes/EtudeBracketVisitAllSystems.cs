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
[TypeId("404e5d9170fb45ceaa82439aa523d696")]
public class EtudeBracketVisitAllSystems : BlueprintComponent
{
	[SerializeField]
	private ActionList m_OnTriggerActions;

	[SerializeField]
	private BlueprintStarSystemMap.Reference[] m_ExcludeAreas;

	private List<BlueprintStarSystemMap> ExcludedAreas => m_ExcludeAreas?.Dereference().ToList();
}
