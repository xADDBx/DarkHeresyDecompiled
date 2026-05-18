using Code.View.UI.MVVM;
using Dreamteck.Splines;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class GlobalMapZoneView : View<Unit>
{
	[Header("Values")]
	[SerializeField]
	private bool m_HasPaths;

	[field: SerializeField]
	public GlobalTransitionMapEntityView EntityView { get; private set; }

	[field: ShowIf("m_HasPaths")]
	[field: SerializeField]
	public Transform ShipAnchor { get; private set; }

	[field: ShowIf("m_HasPaths")]
	[field: SerializeField]
	public EnumToObjectSelector<BlueprintMultiEntranceMap, SplineComputer> Paths { get; private set; }
}
