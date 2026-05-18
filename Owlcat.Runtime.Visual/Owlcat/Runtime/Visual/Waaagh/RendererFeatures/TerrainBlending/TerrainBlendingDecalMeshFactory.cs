using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;

internal static class TerrainBlendingDecalMeshFactory
{
	public static Mesh Create()
	{
		Mesh mesh = new Mesh
		{
			name = "TerrainDecal",
			hideFlags = HideFlags.DontSave
		};
		mesh.vertices = new Vector3[8]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(1f, 0f, 1f),
			new Vector3(0f, 1f, 1f),
			new Vector3(1f, 1f, 1f)
		};
		int[] triangles = new int[36];
		int trianglesIdx = 0;
		PushTriangle(0, 1, 2, 3);
		PushTriangle(5, 4, 7, 6);
		PushTriangle(4, 0, 6, 2);
		PushTriangle(1, 5, 3, 7);
		PushTriangle(2, 3, 6, 7);
		PushTriangle(4, 5, 0, 1);
		mesh.triangles = triangles;
		mesh.UploadMeshData(markNoLongerReadable: true);
		return mesh;
		void PushTriangle(int v0, int v1, int v2, int v3)
		{
			triangles[trianglesIdx++] = v0;
			triangles[trianglesIdx++] = v2;
			triangles[trianglesIdx++] = v1;
			triangles[trianglesIdx++] = v1;
			triangles[trianglesIdx++] = v2;
			triangles[trianglesIdx++] = v3;
		}
	}
}
