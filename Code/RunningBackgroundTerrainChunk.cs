using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RunningBackgroundTerrainChunk : MonoBehaviour
{
	private struct SpawnedEntry
	{
		public GameObject prefab;

		public GameObject instance;

		public Renderer[] renderers;
	}

	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	private Mesh mesh;

	private readonly List<SpawnedEntry> activeAssets = new List<SpawnedEntry>();

	private readonly Dictionary<GameObject, Queue<SpawnedEntry>> localPool = new Dictionary<GameObject, Queue<SpawnedEntry>>();

	public void Initialize()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		mesh = new Mesh();
		mesh.name = "TerrainChunk";
		mesh.indexFormat = IndexFormat.UInt32;
		meshFilter.sharedMesh = mesh;
	}

	public void Generate(int chunkIndex, RunningBackgroundSettings settings, RunningBackgroundBiomeConfig biome)
	{
		ReturnAllToLocalPool();
		GenerateMesh(chunkIndex, settings, biome);
		SpawnTrack(chunkIndex, settings);
		SpawnAssets(chunkIndex, settings, biome);
		meshRenderer.sharedMaterial = biome.terrainMaterial;
	}

	private void GenerateMesh(int chunkIndex, RunningBackgroundSettings s, RunningBackgroundBiomeConfig biome)
	{
		int resolutionX = s.resolutionX;
		int resolutionZ = s.resolutionZ;
		int num = resolutionX * resolutionZ;
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num];
		Vector2[] array3 = new Vector2[num];
		float num2 = (float)chunkIndex * s.chunkLength;
		for (int i = 0; i < resolutionZ; i++)
		{
			for (int j = 0; j < resolutionX; j++)
			{
				int num3 = i * resolutionX + j;
				float num4 = (float)j / (float)(resolutionX - 1);
				float num5 = (float)i / (float)(resolutionZ - 1);
				float num6 = num4 * s.chunkWidth - s.chunkWidth * 0.5f;
				float num7 = num5 * s.chunkLength;
				float num8 = num6;
				float num9 = num2 + num7;
				float y = s.SampleHeightWithTrack(num6, num8, num9);
				array[num3] = new Vector3(num6, y, num7);
				array2[num3] = new Vector2(num8 / s.textureWorldScale, num9 / s.textureWorldScale);
				array3[num3] = new Vector2(num4, num5);
			}
		}
		int[] array4 = new int[(resolutionX - 1) * (resolutionZ - 1) * 6];
		int num10 = 0;
		for (int k = 0; k < resolutionZ - 1; k++)
		{
			for (int l = 0; l < resolutionX - 1; l++)
			{
				int num11 = k * resolutionX + l;
				array4[num10++] = num11;
				array4[num10++] = num11 + resolutionX;
				array4[num10++] = num11 + resolutionX + 1;
				array4[num10++] = num11;
				array4[num10++] = num11 + resolutionX + 1;
				array4[num10++] = num11 + 1;
			}
		}
		mesh.Clear();
		mesh.vertices = array;
		mesh.triangles = array4;
		mesh.uv = array2;
		mesh.uv2 = array3;
		mesh.RecalculateNormals();
		GenerateVertexColors(s, biome, array, num2, resolutionX, resolutionZ);
		mesh.RecalculateTangents();
		mesh.RecalculateBounds();
	}

	private void GenerateVertexColors(RunningBackgroundSettings s, RunningBackgroundBiomeConfig biome, Vector3[] vertices, float worldZOffset, int resX, int resZ)
	{
		Vector3[] normals = mesh.normals;
		Color[] array = new Color[vertices.Length];
		for (int i = 0; i < resZ; i++)
		{
			for (int j = 0; j < resX; j++)
			{
				int num = i * resX + j;
				float t = 1f - Mathf.Abs(normals[num].y);
				float x = vertices[num].x;
				float num2 = worldZOffset + vertices[num].z;
				float baseWeight = biome.baseWeight;
				float num3 = Mathf.SmoothStep(biome.slopeThreshold - biome.slopeFalloff * 0.5f, biome.slopeThreshold + biome.slopeFalloff * 0.5f, t) * biome.slopeStrength;
				float t2 = Mathf.PerlinNoise((x + biome.patternOffset.x) * biome.patternScale, (num2 + biome.patternOffset.y) * biome.patternScale);
				float num4 = Mathf.SmoothStep(1f - biome.patternBrightness - biome.patternFalloff * 0.5f, 1f - biome.patternBrightness + biome.patternFalloff * 0.5f, t2) * biome.patternStrength;
				float num5 = ((s.embankmentHeight > 0f) ? (Mathf.InverseLerp(s.baseHeight, s.baseHeight + s.embankmentHeight, vertices[num].y) * biome.embankmentStrength) : 0f);
				float num6 = baseWeight + num3 + num4 + num5;
				if (num6 > 0f)
				{
					array[num] = new Color(baseWeight / num6, num3 / num6, num4 / num6, num5 / num6);
				}
				else
				{
					array[num] = new Color(1f, 0f, 0f, 0f);
				}
			}
		}
		mesh.colors = array;
	}

	private static void SetRenderersVisible(Renderer[] renderers, bool visible)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = visible;
		}
	}

	private GameObject GetFromLocalPool(GameObject prefab)
	{
		SpawnedEntry item;
		if (localPool.TryGetValue(prefab, out var value) && value.Count > 0)
		{
			item = value.Dequeue();
			SetRenderersVisible(item.renderers, visible: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, base.transform);
			SpawnedEntry spawnedEntry = default(SpawnedEntry);
			spawnedEntry.prefab = prefab;
			spawnedEntry.instance = gameObject;
			spawnedEntry.renderers = gameObject.GetComponentsInChildren<Renderer>();
			item = spawnedEntry;
		}
		activeAssets.Add(item);
		return item.instance;
	}

	private void ReturnAllToLocalPool()
	{
		for (int num = activeAssets.Count - 1; num >= 0; num--)
		{
			SpawnedEntry item = activeAssets[num];
			if (!(item.instance == null))
			{
				SetRenderersVisible(item.renderers, visible: false);
				if (!localPool.TryGetValue(item.prefab, out var value))
				{
					value = new Queue<SpawnedEntry>();
					localPool[item.prefab] = value;
				}
				value.Enqueue(item);
			}
		}
		activeAssets.Clear();
	}

	private void SpawnTrack(int chunkIndex, RunningBackgroundSettings s)
	{
		if (!(s.trackPrefab == null))
		{
			float trackSegmentLength = s.trackSegmentLength;
			float num = (float)chunkIndex * s.chunkLength;
			float num2 = num + s.chunkLength;
			float y = s.baseHeight + s.embankmentHeight;
			float num3 = Mathf.Ceil(num / trackSegmentLength) * trackSegmentLength;
			if (num3 > num)
			{
				float z = num3 - trackSegmentLength - num;
				GetFromLocalPool(s.trackPrefab).transform.localPosition = new Vector3(0f, y, z);
			}
			for (float num4 = num3; num4 < num2; num4 += trackSegmentLength)
			{
				float z2 = num4 - num;
				GetFromLocalPool(s.trackPrefab).transform.localPosition = new Vector3(0f, y, z2);
			}
		}
	}

	private void SpawnAssets(int chunkIndex, RunningBackgroundSettings s, RunningBackgroundBiomeConfig biome)
	{
		if (biome.spawnableAssets == null || biome.spawnableAssets.Length == 0)
		{
			return;
		}
		float num = s.chunkWidth * 0.5f;
		float num2 = (float)chunkIndex * s.chunkLength;
		for (int i = 0; i < biome.spawnableAssets.Length; i++)
		{
			RunningBackgroundSpawnableAsset runningBackgroundSpawnableAsset = biome.spawnableAssets[i];
			if (runningBackgroundSpawnableAsset.prefab == null)
			{
				continue;
			}
			System.Random random = new System.Random(chunkIndex * 7919 + i * 1301 + 31);
			int num3 = Mathf.Max(1, Mathf.FloorToInt(s.chunkWidth / runningBackgroundSpawnableAsset.cellSize));
			int num4 = Mathf.Max(1, Mathf.FloorToInt(s.chunkLength / runningBackgroundSpawnableAsset.cellSize));
			float num5 = s.chunkWidth / (float)num3;
			float num6 = s.chunkLength / (float)num4;
			for (int j = 0; j < num4; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					if ((float)random.NextDouble() > runningBackgroundSpawnableAsset.probability)
					{
						continue;
					}
					float num7 = (float)random.NextDouble() * num5;
					float num8 = (float)random.NextDouble() * num6;
					float num9 = (float)k * num5 - num + num7;
					float num10 = (float)j * num6 + num8;
					float num11 = s.trackHalfWidth + runningBackgroundSpawnableAsset.trackSafeDistance;
					if (!(num11 > 0f) || !(Mathf.Abs(num9) < num11))
					{
						float worldX = num9;
						float worldZ = num2 + num10;
						float num12 = s.SampleHeightWithTrack(num9, worldX, worldZ);
						GameObject fromLocalPool = GetFromLocalPool(runningBackgroundSpawnableAsset.prefab);
						fromLocalPool.transform.localPosition = new Vector3(num9, num12 + runningBackgroundSpawnableAsset.verticalOffset, num10);
						float num13 = Mathf.Lerp(runningBackgroundSpawnableAsset.minScale, runningBackgroundSpawnableAsset.maxScale, (float)random.NextDouble());
						fromLocalPool.transform.localScale = Vector3.one * num13;
						float x = ((float)random.NextDouble() * 2f - 1f) * 180f * runningBackgroundSpawnableAsset.randomRotationX;
						float y = ((float)random.NextDouble() * 2f - 1f) * 180f * runningBackgroundSpawnableAsset.randomRotationY;
						float z = ((float)random.NextDouble() * 2f - 1f) * 180f * runningBackgroundSpawnableAsset.randomRotationZ;
						fromLocalPool.transform.localRotation = Quaternion.Euler(x, y, z);
						if (runningBackgroundSpawnableAsset.alignToNormal)
						{
							Vector3 toDirection = s.SampleNormal(worldX, worldZ);
							Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, toDirection);
							fromLocalPool.transform.localRotation = quaternion * fromLocalPool.transform.localRotation;
						}
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (mesh != null)
		{
			UnityEngine.Object.Destroy(mesh);
		}
	}
}
