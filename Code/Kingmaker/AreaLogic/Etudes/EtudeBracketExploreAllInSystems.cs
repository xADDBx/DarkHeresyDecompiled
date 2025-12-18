using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("03a285e0b85a4b989bc9784990b62090")]
public class EtudeBracketExploreAllInSystems : BlueprintComponent
{
	[SerializeField]
	private ActionList m_OnTriggerActions;

	[SerializeField]
	private BlueprintStarSystemMap.Reference[] m_ExcludeAreas;

	[SerializeField]
	[Tooltip("For areas that are not on global map")]
	private BlueprintStarSystemMap.Reference[] m_AdditionalAreas;

	private IEnumerable<BlueprintStarSystemMap> ExcludedAreas => m_ExcludeAreas?.Dereference();

	private IEnumerable<BlueprintStarSystemMap> AdditionalAreas => m_AdditionalAreas?.Dereference();
}
