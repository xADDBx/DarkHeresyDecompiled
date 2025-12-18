using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker;

public class NodeLinkVisual : MonoBehaviour
{
	private bool m_Initialized;

	public WarhammerNodeLink NodeLink { get; private set; }

	[field: SerializeField]
	public LineRenderer FirstLineRenderer { get; private set; }

	[field: SerializeField]
	public LineRenderer SecondLineRenderer { get; private set; }

	[field: SerializeField]
	public GameObject FirstDecal { get; private set; }

	[field: SerializeField]
	public GameObject SecondDecal { get; private set; }

	public void Init(WarhammerNodeLink nodeLink)
	{
		if (!m_Initialized)
		{
			NodeLink = nodeLink;
		}
	}
}
