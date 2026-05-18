using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Gameplay.Blueprints.Root;
using Kingmaker.Pathfinding;
using Kingmaker.Settings;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GeometryExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Framework.Pathfinding;

public static class GridObstacleDebug
{
	public delegate Vector3? GetPositionDelegate(GridNodeIndex node);

	public readonly struct BoundsEntry
	{
		public readonly Bounds Bounds;

		public readonly GridObstacle Obstacle;

		public BoundsEntry(Bounds bounds, GridObstacle obstacle)
		{
			Bounds = bounds;
			Obstacle = obstacle;
		}
	}

	public delegate bool ShouldDrawInSceneViewDelegate();

	private const int InitialBufferSize = 4096;

	private static readonly List<BoundsEntry> _Bounds = new List<BoundsEntry>();

	private static long _CacheVersion = -1L;

	public static Matrix4x4[] ObstaclesBuffer = new Matrix4x4[4096];

	public static Matrix4x4[] CoversBuffer = new Matrix4x4[4096];

	public static Matrix4x4[] InvisibleCoversBuffer = new Matrix4x4[4096];

	public static Matrix4x4[] LosBlockersBuffer = new Matrix4x4[4096];

	public static Matrix4x4[] InvisibleLosBlockersBuffer = new Matrix4x4[4096];

	public static Matrix4x4[] InactiveObstaclesBuffer = new Matrix4x4[4096];

	public static Matrix4x4[] InactiveCoversBuffer = new Matrix4x4[4096];

	public static Matrix4x4[] InactiveLosBlockersBuffer = new Matrix4x4[4096];

	private static Mesh _QuadMesh;

	[NotNull]
	public static GetPositionDelegate GetPosition { get; set; } = DefaultGetPosition;


	public static int ObstaclesCount { get; private set; }

	public static int CoversCount { get; private set; }

	public static int InvisibleCoversCount { get; private set; }

	public static int LosBlockersCount { get; private set; }

	public static int InvisibleLosBlockersCount { get; private set; }

	public static int InactiveObstaclesCount { get; private set; }

	public static int InactiveCoversCount { get; private set; }

	public static int InactiveLosBlockersCount { get; private set; }

	public static ReadonlyList<BoundsEntry> Bounds => _Bounds;

	[NotNull]
	public static ShouldDrawInSceneViewDelegate ShouldDrawInSceneView { get; set; } = () => false;


	private static Mesh QuadMesh
	{
		get
		{
			if (!(_QuadMesh != null))
			{
				return _QuadMesh = CreateQuadMesh();
			}
			return _QuadMesh;
		}
	}

	public static void UpdateBounds()
	{
		GridObstacleCache instance = GridObstacleCache.Instance;
		if (instance == null || _CacheVersion == instance.Version)
		{
			return;
		}
		_CacheVersion = instance.Version;
		_Bounds.Clear();
		int index = 0;
		int index2 = 0;
		int index3 = 0;
		int index4 = 0;
		int index5 = 0;
		int index6 = 0;
		int index7 = 0;
		int index8 = 0;
		foreach (GridObstacleCache.Entry item in instance)
		{
			if (item.Direction.IsDiagonal())
			{
				continue;
			}
			GridNodeIndex node = item.Node;
			GridNodeIndex neighbourAlongDirection = item.Node.GetNeighbourAlongDirection(item.Direction);
			Vector3? vector = GetPosition(node);
			if (!vector.HasValue)
			{
				continue;
			}
			Vector3 valueOrDefault = vector.GetValueOrDefault();
			vector = GetPosition(neighbourAlongDirection);
			if (!vector.HasValue)
			{
				continue;
			}
			Vector3 valueOrDefault2 = vector.GetValueOrDefault();
			foreach (GridObstacle source in item.Sources)
			{
				float num = (item.Backward ? ((float)source.HeightBackward / 1000f) : ((float)source.Height / 1000f));
				float y = source.transform.position.y;
				Quaternion q = (item.ZAligned ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity);
				Vector3 s = new Vector3(1.Cells().Meters, Math.Max(0.25f, num), 1f);
				Vector3 vector2 = (valueOrDefault + (valueOrDefault2 - valueOrDefault) / 2f).ReplaceY(y);
				vector2 += ((!item.ZAligned) ? (item.Backward ? new Vector3(0f, 0f, -0.01f) : new Vector3(0f, 0f, 0.01f)) : (item.Backward ? new Vector3(-0.01f, 0f, 0f) : new Vector3(0.01f, 0f, 0f)));
				Matrix4x4 matrix = Matrix4x4.TRS(vector2, q, s);
				LosCalculations.CoverType coverType = (item.Backward ? source.TypeBackward : source.Type);
				bool flag = source == item.Source;
				bool hideArVisual = source._hideArVisual;
				if (hideArVisual && coverType == LosCalculations.CoverType.Cover)
				{
					Add(ref InvisibleCoversBuffer, ref index3, matrix);
				}
				else if (hideArVisual && coverType == LosCalculations.CoverType.LosBlocker)
				{
					Add(ref InvisibleLosBlockersBuffer, ref index5, matrix);
				}
				else if (flag)
				{
					switch (coverType)
					{
					case LosCalculations.CoverType.Obstacle:
						Add(ref ObstaclesBuffer, ref index, matrix);
						break;
					case LosCalculations.CoverType.Cover:
						Add(ref CoversBuffer, ref index2, matrix);
						break;
					case LosCalculations.CoverType.LosBlocker:
						Add(ref LosBlockersBuffer, ref index4, matrix);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				else
				{
					switch (coverType)
					{
					case LosCalculations.CoverType.Obstacle:
						Add(ref InactiveObstaclesBuffer, ref index6, matrix);
						break;
					case LosCalculations.CoverType.Cover:
						Add(ref InactiveCoversBuffer, ref index7, matrix);
						break;
					case LosCalculations.CoverType.LosBlocker:
						Add(ref InactiveLosBlockersBuffer, ref index8, matrix);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				Vector3 size = (item.ZAligned ? new Vector3(0.02f, num, 1.Cells().Meters) : new Vector3(1.Cells().Meters, num, 0.02f));
				Bounds bounds = new Bounds(vector2.ReplaceY(y + num / 2f), size);
				_Bounds.Add(new BoundsEntry(bounds, source));
			}
		}
		ObstaclesCount = index;
		CoversCount = index2;
		InvisibleCoversCount = index3;
		LosBlockersCount = index4;
		InvisibleLosBlockersCount = index5;
		InactiveObstaclesCount = index6;
		InactiveCoversCount = index7;
		InactiveLosBlockersCount = index8;
	}

	private static void Add(ref Matrix4x4[] buffer, ref int index, Matrix4x4 matrix)
	{
		if (index >= buffer.Length)
		{
			Array.Resize(ref buffer, buffer.Length * 2);
		}
		buffer[index++] = matrix;
	}

	private static Vector3? DefaultGetPosition(GridNodeIndex node)
	{
		return AstarPath.active?.data.gridGraph?.GetNearest(node).node?.Vector3Position();
	}

	[RuntimeInitializeOnLoadMethod]
	public static void InitializeOnLoad()
	{
		RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
	}

	private static void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		Camera camera2 = CameraRig.Instance.Or(null)?.Camera;
		if (camera.cameraType switch
		{
			CameraType.SceneView => ShouldDrawInSceneView(), 
			CameraType.Game => camera == camera2 && SettingsRoot.Initialized && (bool)SettingsRoot.Development.DrawObstaclesGizmo, 
			_ => false, 
		})
		{
			UpdateBounds();
			DrawObstacles(context);
		}
	}

	private static void DrawObstacles(ScriptableRenderContext context)
	{
		using (ProfileScope.New("RenderMeshInstanced"))
		{
			BlueprintCoverRoot cover = ConfigRoot.Instance.Cover;
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			commandBuffer.BeginSample("GridObstacleDebug");
			DrawMeshInstanced(commandBuffer, cover.ObstacleMaterial, ObstaclesBuffer, ObstaclesCount);
			DrawMeshInstanced(commandBuffer, cover.CoverMaterial, CoversBuffer, CoversCount);
			DrawMeshInstanced(commandBuffer, cover.InvisibleCoverMaterial, InvisibleCoversBuffer, InvisibleCoversCount);
			DrawMeshInstanced(commandBuffer, cover.LosBlockerMaterial, LosBlockersBuffer, LosBlockersCount);
			DrawMeshInstanced(commandBuffer, cover.InvisibleLosBlockerMaterial, InvisibleLosBlockersBuffer, InvisibleLosBlockersCount);
			DrawMeshInstanced(commandBuffer, cover.InactiveObstacleMaterial, InactiveObstaclesBuffer, InactiveObstaclesCount);
			DrawMeshInstanced(commandBuffer, cover.InactiveCoverMaterial, InactiveCoversBuffer, InactiveCoversCount);
			DrawMeshInstanced(commandBuffer, cover.InactiveLosBlockerMaterial, InactiveLosBlockersBuffer, InactiveLosBlockersCount);
			commandBuffer.EndSample("GridObstacleDebug");
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
			context.Submit();
		}
	}

	private static void DrawMeshInstanced(CommandBuffer cmd, Material material, Matrix4x4[] buffer, int length)
	{
		if (length > 0)
		{
			cmd.DrawMeshInstanced(QuadMesh, 0, material, material.FindPass("Waaagh Forward"), buffer, length);
		}
	}

	private static Mesh CreateQuadMesh()
	{
		Mesh mesh = new Mesh
		{
			name = "Double-sided Quad"
		};
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-0.5f, 0f, 0f),
			new Vector3(0.5f, 0f, 0f),
			new Vector3(0.5f, 1f, 0f),
			new Vector3(-0.5f, 1f, 0f)
		};
		int[] triangles = new int[12]
		{
			0, 1, 2, 2, 3, 0, 2, 1, 0, 0,
			3, 2
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mesh.RecalculateNormals();
		return mesh;
	}
}
