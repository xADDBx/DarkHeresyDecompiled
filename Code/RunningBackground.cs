using System.Collections;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using UnityEngine;

public class RunningBackground : MonoBehaviour
{
	private struct ActiveEffect
	{
		public RunningBackgroundBiomeEffect config;

		public GameObject instance;
	}

	[SerializeField]
	[Tooltip("Train speed in km/h. Affects terrain scrolling rate and camera shake intensity")]
	private float speed = 150f;

	[SerializeField]
	[Tooltip("Shared settings asset: chunk dimensions, mesh resolution, noise, track, and camera shake parameters")]
	private RunningBackgroundSettings settings;

	[SerializeField]
	[Tooltip("Active biome config: terrain material, vertex paint, spawnable assets, and VFX")]
	private RunningBackgroundBiomeConfig currentBiome;

	[SerializeField]
	[Min(1f)]
	[Tooltip("Number of terrain chunks generated ahead of the camera (along movement direction)")]
	private int chunksAhead = 4;

	[SerializeField]
	[Min(1f)]
	[Tooltip("Number of terrain chunks kept behind the camera before recycling")]
	private int chunksBehind = 2;

	[SerializeField]
	[Tooltip("Force a full terrain rebuild on next frame (destroys and regenerates all chunks)")]
	private bool forceRebuild;

	[SerializeField]
	[Tooltip("Draw gizmo lines at chunk boundaries in Scene view for debugging layout")]
	private bool debugChunkBorders;

	[SerializeField]
	[BoolButton("GenerateEditorPreview", "Generate Preview")]
	private bool _btnGeneratePreview;

	[SerializeField]
	[BoolButton("ClearEditorPreview", "Delete Preview")]
	private bool _btnDeletePreview;

	private readonly List<RunningBackgroundTerrainChunk> chunks = new List<RunningBackgroundTerrainChunk>();

	private RunningBackgroundTerrainChunk editorPreviewChunk;

	private readonly List<ActiveEffect> activeEffects = new List<ActiveEffect>();

	private RunningBackgroundCameraShake cameraShake;

	private float offset;

	private int nextChunkIndex;

	private Coroutine effectsCoroutine;

	private float SpeedMs => speed / 3.6f;

	private void Start()
	{
		if (currentBiome == null)
		{
			Debug.LogError("RunningBackground: BiomeConfig is not assigned.", this);
			base.enabled = false;
			return;
		}
		int num = chunksBehind + 1 + chunksAhead;
		int num2 = -chunksBehind;
		for (int i = 0; i < num; i++)
		{
			int chunkIndex = num2 + i;
			RunningBackgroundTerrainChunk item = CreateChunk(chunkIndex);
			chunks.Add(item);
		}
		nextChunkIndex = num2 + num;
		offset = 0f;
		RepositionAll();
		SetupCameraShake();
	}

	private void OnDestroy()
	{
		DestroyCameraShake();
	}

	private void Update()
	{
		if (!(currentBiome == null))
		{
			if (forceRebuild || Input.GetKeyDown(KeyCode.End))
			{
				forceRebuild = false;
				Rebuild();
			}
			UpdateCameraShake();
			offset += SpeedMs * Time.deltaTime;
			float chunkLength = settings.chunkLength;
			while (offset >= chunkLength)
			{
				offset -= chunkLength;
				RecycleRear();
			}
			RepositionAll();
		}
	}

	private void RecycleRear()
	{
		RunningBackgroundTerrainChunk runningBackgroundTerrainChunk = chunks[0];
		chunks.RemoveAt(0);
		runningBackgroundTerrainChunk.Generate(nextChunkIndex, settings, currentBiome);
		nextChunkIndex++;
		chunks.Add(runningBackgroundTerrainChunk);
	}

	private void RepositionAll()
	{
		float chunkLength = settings.chunkLength;
		for (int i = 0; i < chunks.Count; i++)
		{
			float z = (float)(i - chunksBehind) * chunkLength - offset;
			chunks[i].transform.localPosition = new Vector3(0f, 0f, z);
		}
	}

	private RunningBackgroundTerrainChunk CreateChunk(int chunkIndex)
	{
		GameObject obj = new GameObject("TerrainChunk");
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		RunningBackgroundTerrainChunk runningBackgroundTerrainChunk = obj.AddComponent<RunningBackgroundTerrainChunk>();
		runningBackgroundTerrainChunk.Initialize();
		runningBackgroundTerrainChunk.Generate(chunkIndex, settings, currentBiome);
		return runningBackgroundTerrainChunk;
	}

	public void ChangeBiome(RunningBackgroundBiomeConfig newBiome)
	{
		currentBiome = newBiome;
		Rebuild();
	}

	private void Rebuild()
	{
		foreach (RunningBackgroundTerrainChunk chunk in chunks)
		{
			if (chunk != null)
			{
				Object.Destroy(chunk.gameObject);
			}
		}
		chunks.Clear();
		int num = chunksBehind + 1 + chunksAhead;
		int num2 = -chunksBehind;
		for (int i = 0; i < num; i++)
		{
			RunningBackgroundTerrainChunk item = CreateChunk(num2 + i);
			chunks.Add(item);
		}
		nextChunkIndex = num2 + num;
		offset = 0f;
		RepositionAll();
		RespawnEffects();
	}

	private void RespawnEffects()
	{
		if (effectsCoroutine != null)
		{
			StopCoroutine(effectsCoroutine);
		}
		DestroyAllEffects();
		if (currentBiome.effects == null)
		{
			return;
		}
		RunningBackgroundBiomeEffect[] effects = currentBiome.effects;
		foreach (RunningBackgroundBiomeEffect runningBackgroundBiomeEffect in effects)
		{
			if (!(runningBackgroundBiomeEffect.prefab == null))
			{
				activeEffects.Add(new ActiveEffect
				{
					config = runningBackgroundBiomeEffect,
					instance = null
				});
			}
		}
		effectsCoroutine = StartCoroutine(EffectsCheckLoop());
	}

	private IEnumerator EffectsCheckLoop()
	{
		while (true)
		{
			UpdateEffects();
			for (int f = 0; f < 10; f++)
			{
				yield return null;
			}
		}
	}

	private void UpdateEffects()
	{
		for (int i = 0; i < activeEffects.Count; i++)
		{
			ActiveEffect value = activeEffects[i];
			bool flag = value.config.minSpeed <= 0f || speed >= value.config.minSpeed;
			if (flag && value.instance == null)
			{
				value.instance = Object.Instantiate(value.config.prefab, base.transform);
				value.instance.transform.localPosition = value.config.offset;
				activeEffects[i] = value;
			}
			else if (!flag && value.instance != null)
			{
				Object.Destroy(value.instance);
				value.instance = null;
				activeEffects[i] = value;
			}
		}
	}

	private void DestroyAllEffects()
	{
		foreach (ActiveEffect activeEffect in activeEffects)
		{
			if (activeEffect.instance != null)
			{
				Object.Destroy(activeEffect.instance);
			}
		}
		activeEffects.Clear();
	}

	private void SetupCameraShake()
	{
		DestroyCameraShake();
		if (!(settings == null) && settings.enableCameraShake)
		{
			cameraShake = base.gameObject.AddComponent<RunningBackgroundCameraShake>();
			cameraShake.Setup(settings);
			cameraShake.SetSpeed(speed);
		}
	}

	private void DestroyCameraShake()
	{
		if (cameraShake != null)
		{
			Object.Destroy(cameraShake);
			cameraShake = null;
		}
	}

	private void UpdateCameraShake()
	{
		if (!(cameraShake == null))
		{
			cameraShake.SetSpeed(speed);
			cameraShake.enabled = settings != null && settings.enableCameraShake;
		}
	}
}
