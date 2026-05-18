using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.AR;

public sealed class PathServiceRequest
{
	public IAreaSource source;

	public Material material;

	public Vector3 positionOffset;

	public Transform progressTrackingTransform;

	public GridSettings GridSettings;

	public PathLineSettings PathLineSettings;

	public GridGraph Graph;

	public CombatHudPathRenderer.LineMaterialSettingsPerCreatureSize? FadeSettings;

	public void Clear()
	{
		source = null;
		material = null;
		positionOffset = default(Vector3);
		progressTrackingTransform = null;
		GridSettings = default(GridSettings);
		PathLineSettings = default(PathLineSettings);
		Graph = null;
		FadeSettings = null;
	}
}
