using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

[ExecuteAlways]
public sealed class TerrainStampingBrush : MonoBehaviour
{
	[Min(0f)]
	public float Radius = 0.05f;

	[Min(0f)]
	public float Strength = 1f;

	public Texture2D Texture;

	private void OnEnable()
	{
		TerrainStampingBrushContainer.Add(this);
	}

	private void OnDisable()
	{
		TerrainStampingBrushContainer.Remove(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (base.isActiveAndEnabled)
		{
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, Quaternion.identity, new Vector3(1f, 0f, 1f));
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(Vector3.zero, Radius);
			Gizmos.matrix = matrix;
		}
	}
}
