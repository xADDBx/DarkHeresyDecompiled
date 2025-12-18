using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public static class XPBDMeshUtils
{
	private static Mesh s_CubeMeshWithUvAndNormals;

	public static Mesh CubeMeshWithUvAndNormals
	{
		get
		{
			if (s_CubeMeshWithUvAndNormals != null)
			{
				return s_CubeMeshWithUvAndNormals;
			}
			float num = 1f;
			float num2 = 1f;
			float num3 = 1f;
			Vector3[] array = new Vector3[8]
			{
				new Vector3((0f - num) * 0.5f, (0f - num2) * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, (0f - num2) * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, (0f - num2) * 0.5f, (0f - num3) * 0.5f),
				new Vector3((0f - num) * 0.5f, (0f - num2) * 0.5f, (0f - num3) * 0.5f),
				new Vector3((0f - num) * 0.5f, num2 * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, num2 * 0.5f, num3 * 0.5f),
				new Vector3(num * 0.5f, num2 * 0.5f, (0f - num3) * 0.5f),
				new Vector3((0f - num) * 0.5f, num2 * 0.5f, (0f - num3) * 0.5f)
			};
			Vector3[] vertices = new Vector3[24]
			{
				array[0],
				array[1],
				array[2],
				array[3],
				array[7],
				array[4],
				array[0],
				array[3],
				array[4],
				array[5],
				array[1],
				array[0],
				array[6],
				array[7],
				array[3],
				array[2],
				array[5],
				array[6],
				array[2],
				array[1],
				array[7],
				array[6],
				array[5],
				array[4]
			};
			Vector3 up = Vector3.up;
			Vector3 down = Vector3.down;
			Vector3 forward = Vector3.forward;
			Vector3 back = Vector3.back;
			Vector3 left = Vector3.left;
			Vector3 right = Vector3.right;
			Vector3[] normals = new Vector3[24]
			{
				down, down, down, down, left, left, left, left, forward, forward,
				forward, forward, back, back, back, back, right, right, right, right,
				up, up, up, up
			};
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(1f, 0f);
			Vector2 vector3 = new Vector2(0f, 1f);
			Vector2 vector4 = new Vector2(1f, 1f);
			Vector2[] uv = new Vector2[24]
			{
				vector4, vector3, vector, vector2, vector4, vector3, vector, vector2, vector4, vector3,
				vector, vector2, vector4, vector3, vector, vector2, vector4, vector3, vector, vector2,
				vector4, vector3, vector, vector2
			};
			int[] triangles = new int[36]
			{
				3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
				6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
				12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
				23, 21, 20, 23, 22, 21
			};
			s_CubeMeshWithUvAndNormals = new Mesh();
			s_CubeMeshWithUvAndNormals.name = "Owlcat XPBD Cube Mesh With Uv And Normals";
			s_CubeMeshWithUvAndNormals.vertices = vertices;
			s_CubeMeshWithUvAndNormals.uv = uv;
			s_CubeMeshWithUvAndNormals.normals = normals;
			s_CubeMeshWithUvAndNormals.triangles = triangles;
			return s_CubeMeshWithUvAndNormals;
		}
	}
}
