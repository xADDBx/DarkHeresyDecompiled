using System;
using System.Diagnostics;
using Plawius.NonConvexCollider;
using UnityEngine;

public class RuntimeTest : MonoBehaviour
{
	private class StopwatchScoped : IDisposable
	{
		private readonly string name;

		private readonly Stopwatch stopwatch;

		public StopwatchScoped(string name)
		{
			this.name = name;
			stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
			if (elapsedMilliseconds > 1000)
			{
				UnityEngine.Debug.LogFormat("[{0}] took {1} seconds", name, (double)elapsedMilliseconds / 1000.0);
			}
			else
			{
				UnityEngine.Debug.LogFormat("[{0}] took {1} msec", name, elapsedMilliseconds);
			}
		}
	}

	private void Start()
	{
		base.gameObject.AddComponent<Rigidbody>();
		Mesh mesh = base.gameObject.AddComponent<MeshFilter>().mesh;
		using (new StopwatchScoped("Generate mesh"))
		{
			GenerateTorusMesh(mesh);
		}
		base.gameObject.AddComponent<MeshRenderer>();
		Mesh[] meshes;
		using (new StopwatchScoped("NonConvexCollider generate meshes"))
		{
			meshes = API.GenerateConvexMeshes(mesh, Parameters.Default());
		}
		using (new StopwatchScoped("NonConvexCollider generate add to gameobject"))
		{
			NonConvexColliderAsset physicsCollider = NonConvexColliderAsset.CreateAsset(meshes);
			base.gameObject.AddComponent<NonConvexColliderComponent>().SetPhysicsCollider(physicsCollider);
		}
	}

	private static void GenerateTorusMesh(Mesh mesh)
	{
		mesh.Clear();
		Vector3[] array = new Vector3[475];
		for (int i = 0; i <= 24; i++)
		{
			float num = (float)((i != 24) ? i : 0) / 24f * (MathF.PI * 2f);
			Vector3 vector = new Vector3(Mathf.Cos(num) * 1f, 0f, Mathf.Sin(num) * 1f);
			for (int j = 0; j <= 18; j++)
			{
				float f = (float)((j != 18) ? j : 0) / 18f * (MathF.PI * 2f);
				Vector3 vector2 = Quaternion.AngleAxis((0f - num) * 57.29578f, Vector3.up) * new Vector3(Mathf.Sin(f) * 0.3f, Mathf.Cos(f) * 0.3f);
				array[j + i * 19] = vector + vector2;
			}
		}
		Vector3[] array2 = new Vector3[array.Length];
		for (int k = 0; k <= 24; k++)
		{
			float f2 = (float)((k != 24) ? k : 0) / 24f * (MathF.PI * 2f);
			Vector3 vector3 = new Vector3(Mathf.Cos(f2) * 1f, 0f, Mathf.Sin(f2) * 1f);
			for (int l = 0; l <= 18; l++)
			{
				array2[l + k * 19] = (array[l + k * 19] - vector3).normalized;
			}
		}
		Vector2[] array3 = new Vector2[array.Length];
		for (int m = 0; m <= 24; m++)
		{
			for (int n = 0; n <= 18; n++)
			{
				array3[n + m * 19] = new Vector2((float)m / 24f, (float)n / 18f);
			}
		}
		int[] array4 = new int[array.Length * 2 * 3];
		int num2 = 0;
		for (int num3 = 0; num3 <= 24; num3++)
		{
			for (int num4 = 0; num4 <= 17; num4++)
			{
				int num5 = num4 + num3 * 19;
				int num6 = num4 + ((num3 < 24) ? ((num3 + 1) * 19) : 0);
				if (num2 < array4.Length - 6)
				{
					array4[num2++] = num5;
					array4[num2++] = num6;
					array4[num2++] = num6 + 1;
					array4[num2++] = num5;
					array4[num2++] = num6 + 1;
					array4[num2++] = num5 + 1;
				}
			}
		}
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.uv = array3;
		mesh.triangles = array4;
		mesh.RecalculateBounds();
	}
}
