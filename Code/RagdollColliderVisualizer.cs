using System.Collections.Generic;
using UnityEngine;

public class RagdollColliderVisualizer : MonoBehaviour
{
	[SerializeField]
	private bool m_ShowColliders = true;

	[SerializeField]
	private Color m_BoxColliderColor = Color.red;

	[SerializeField]
	private Color m_SphereColliderColor = Color.green;

	[SerializeField]
	private Color m_CapsuleColliderColor = Color.cyan;

	[SerializeField]
	[Range(1f, 5f)]
	private float m_LineThickness = 2f;

	public ColliderVisualizationMode VisualizationMode = ColliderVisualizationMode.Billboard;

	public bool ShowOverlaps = true;

	[SerializeField]
	private Color m_OverlapColor = new Color(1f, 0.1f, 0.1f, 1f);

	private Collider m_HighlightedCollider;

	private readonly HashSet<Collider> m_OverlappingColliders = new HashSet<Collider>();
}
