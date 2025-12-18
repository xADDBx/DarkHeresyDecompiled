using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Pathfinding;

[ExecuteInEditMode]
public class NavmeshMasks : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	private struct MaskColor
	{
		public NavmeshMask Mask;

		public Color Color;
	}

	[SerializeField]
	public float CellSize = 0.2f;

	[SerializeField]
	public string[] ForbidNavmeshMasks = new string[1] { "mountains" };

	[SerializeField]
	public string[] IgnoredMeshes;

	[HideInInspector]
	[SerializeField]
	[Range(0f, 1f)]
	public float MaskAlpha = 0.5f;

	[SerializeField]
	[HideInInspector]
	public Bounds Bounds;

	[Obsolete("Unused, waiting for convert to m_Mask")]
	[SerializeField]
	[FormerlySerializedAs("m_Data")]
	private Texture2D m_DataOld;

	[SerializeField]
	[HideInInspector]
	private NavmeshMask[] m_Mask = new NavmeshMask[0];

	[SerializeField]
	private MaskColor[] m_GizmoColors = new MaskColor[4]
	{
		new MaskColor
		{
			Mask = NavmeshMask.TerrainObstacle,
			Color = Color.blue
		},
		new MaskColor
		{
			Mask = NavmeshMask.ForbidArea,
			Color = Color.red
		},
		new MaskColor
		{
			Mask = NavmeshMask.AllowArea,
			Color = Color.green
		},
		new MaskColor
		{
			Mask = NavmeshMask.LosBlockingFloor,
			Color = Color.yellow
		}
	};

	public bool UpdateMasksWhenCollidersMove;

	private bool? m_WasUpdateMasksWhenCollidersMove;

	private Dictionary<object, Bounds> m_PrevColliderBounds = new Dictionary<object, Bounds>();

	private Rect m_UpdateRect = new Rect
	{
		xMin = float.MaxValue,
		yMin = float.MaxValue,
		xMax = float.MinValue,
		yMax = float.MinValue
	};

	private double m_LastMoveTime;

	private bool m_ColliderTransformChanged;

	public int MaxX => Mathf.CeilToInt(Bounds.size.x / CellSize);

	public int MaxZ => Mathf.CeilToInt(Bounds.size.z / CellSize);

	public NavmeshMasksGeneration ConvertData()
	{
		if (m_Mask == null || m_Mask.Length != MaxX * MaxZ)
		{
			return default(NavmeshMasksGeneration);
		}
		return new NavmeshMasksGeneration(m_Mask, MaxX, MaxZ);
	}

	protected void OnEnable()
	{
	}

	protected void OnDisable()
	{
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	[Obsolete("Convert for obsolete data")]
	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		Texture2D dataOld = m_DataOld;
		if (dataOld == null)
		{
			return;
		}
		NavmeshMask[] mask = m_Mask;
		if (mask != null && mask.Length > 0)
		{
			return;
		}
		int num = dataOld.width * dataOld.height;
		m_Mask = new NavmeshMask[num];
		for (int i = 0; i < dataOld.width; i++)
		{
			for (int j = 0; j < dataOld.height; j++)
			{
				Color pixel = dataOld.GetPixel(i, j);
				int num2 = j * dataOld.width + i;
				NavmeshMask navmeshMask = (NavmeshMask)0;
				if (pixel.b > 0.5f)
				{
					navmeshMask |= NavmeshMask.TerrainObstacle;
				}
				if (pixel.r > 0.5f)
				{
					navmeshMask |= NavmeshMask.ForbidArea;
				}
				else if (pixel.g > 0.5f)
				{
					navmeshMask |= NavmeshMask.AllowArea;
				}
				m_Mask[num2] = navmeshMask;
			}
		}
		m_DataOld = null;
	}
}
