using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.AR;

public class ShipPathManager : MonoBehaviour
{
	public enum MovementPhase
	{
		One,
		Two,
		Three
	}

	private struct PathNodeMarkerEntity
	{
		public readonly GameObject GameObject;

		public readonly int XCoordinateInGrid;

		public readonly int ZCoordinateInGrid;

		public readonly Renderer Renderer;

		public readonly MovementPhase MovementPhase;

		public PathNodeMarkerEntity(GameObject gameObject, int xCoordinateInGrid, int zCoordinateInGrid, Renderer renderer, MovementPhase movementPhase)
		{
			GameObject = gameObject;
			XCoordinateInGrid = xCoordinateInGrid;
			ZCoordinateInGrid = zCoordinateInGrid;
			Renderer = renderer;
			MovementPhase = movementPhase;
		}
	}

	public GameObject PathNodeMarker;

	public GameObject VantagePointMarker;

	public Material PassThroughCellMaterial;

	public Material SpeedDownCellMaterial;

	public Material SteadySpeedCellMaterial;

	public Material SpeedUpCellMaterial;

	public Material SpeedUp2CellMaterial;

	[Range(0f, 255f)]
	public int PassThroughCellBaseAlpha;

	[Range(0f, 255f)]
	public int SpeedDownCellBaseAlpha;

	[Range(0f, 255f)]
	public int SteadySpeedCellBaseAlpha;

	[Range(0f, 1f)]
	public float NeighbourCellAlphaMultiplier;

	private readonly List<PathNodeMarkerEntity> m_PathNodeMarkers = new List<PathNodeMarkerEntity>();

	private readonly List<GameObject> m_VantagePointMarkers = new List<GameObject>();

	private readonly List<GridNodeBase> m_MovementAreaPhaseOneNodes = new List<GridNodeBase>();

	private readonly List<GridNodeBase> m_MovementAreaPhaseTwoNodes = new List<GridNodeBase>();

	private readonly List<GridNodeBase> m_MovementAreaPhaseThreeNodes = new List<GridNodeBase>();

	private readonly Quaternion[] m_DirectionToRotationMap = new Quaternion[8]
	{
		Quaternion.LookRotation(Vector3.back, Vector3.up),
		Quaternion.LookRotation(Vector3.right, Vector3.up),
		Quaternion.LookRotation(Vector3.forward, Vector3.up),
		Quaternion.LookRotation(Vector3.left, Vector3.up),
		Quaternion.LookRotation(Vector3.back + Vector3.right, Vector3.up),
		Quaternion.LookRotation(Vector3.right + Vector3.forward, Vector3.up),
		Quaternion.LookRotation(Vector3.forward + Vector3.left, Vector3.up),
		Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up)
	};

	public static ShipPathManager Instance { get; private set; }

	private bool AnyAbilitySelected => Game.Instance.CursorController.SelectedAbility != null;

	public void ClearPathMarkers()
	{
		ClearPathMarkersInternal();
		ClearMovementAreaNodes();
	}

	public void ClearPathMarkersInternal()
	{
		foreach (PathNodeMarkerEntity pathNodeMarker in m_PathNodeMarkers)
		{
			Object.Destroy(pathNodeMarker.GameObject);
		}
		m_PathNodeMarkers.Clear();
		HideVantagePointsMarkers();
	}

	public void UpdatePathNodeMarkers(GridNodeBase currentNode)
	{
		DisablePathNodeMarkers();
		if (AnyAbilitySelected)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		foreach (PathNodeMarkerEntity pathNodeMarker in m_PathNodeMarkers)
		{
			if (pathNodeMarker.XCoordinateInGrid == currentNode.XCoordinateInGrid && pathNodeMarker.ZCoordinateInGrid == currentNode.ZCoordinateInGrid)
			{
				num = pathNodeMarker.XCoordinateInGrid;
				num2 = pathNodeMarker.ZCoordinateInGrid;
				pathNodeMarker.GameObject.SetActive(value: true);
				SetPathNodeMarkerColor(pathNodeMarker, isNeighbour: false);
				break;
			}
		}
		if (num < 0 && num2 < 0)
		{
			return;
		}
		foreach (PathNodeMarkerEntity pathNodeMarker2 in m_PathNodeMarkers)
		{
			int xCoordinateInGrid = pathNodeMarker2.XCoordinateInGrid;
			int zCoordinateInGrid = pathNodeMarker2.ZCoordinateInGrid;
			if (xCoordinateInGrid != num || zCoordinateInGrid != num2)
			{
				bool num3 = num - 1 <= xCoordinateInGrid && xCoordinateInGrid <= num + 1;
				bool flag = num2 - 1 <= zCoordinateInGrid && zCoordinateInGrid <= num2 + 1;
				if (num3 && flag)
				{
					pathNodeMarker2.GameObject.SetActive(value: true);
					SetPathNodeMarkerColor(pathNodeMarker2, isNeighbour: true);
				}
			}
		}
	}

	public void DisablePathNodeMarkers()
	{
		foreach (PathNodeMarkerEntity pathNodeMarker in m_PathNodeMarkers)
		{
			pathNodeMarker.GameObject.SetActive(value: false);
		}
	}

	private void SetPathNodeMarkerColor(PathNodeMarkerEntity pathNodeMarker, bool isNeighbour)
	{
		Renderer renderer = pathNodeMarker.Renderer;
		if (!(renderer == null))
		{
			Color color = renderer.material.color;
			int num = Mathf.CeilToInt((float)GetMarkerMaterialAlpha(pathNodeMarker.MovementPhase) * (isNeighbour ? NeighbourCellAlphaMultiplier : 1f));
			renderer.material.color = new Color(color.r, color.g, color.b, (float)num / 255f);
		}
	}

	private void ClearMovementAreaNodes()
	{
		m_MovementAreaPhaseOneNodes.Clear();
		m_MovementAreaPhaseTwoNodes.Clear();
		m_MovementAreaPhaseThreeNodes.Clear();
	}

	private List<GridNodeBase> GetMovementAreaNodeContainer(MovementPhase phase)
	{
		return phase switch
		{
			MovementPhase.One => m_MovementAreaPhaseOneNodes, 
			MovementPhase.Two => m_MovementAreaPhaseTwoNodes, 
			MovementPhase.Three => m_MovementAreaPhaseThreeNodes, 
			_ => m_MovementAreaPhaseThreeNodes, 
		};
	}

	private Material GetMarkerMaterial(MovementPhase phase)
	{
		return phase switch
		{
			MovementPhase.One => SpeedDownCellMaterial, 
			MovementPhase.Two => PassThroughCellMaterial, 
			MovementPhase.Three => SteadySpeedCellMaterial, 
			_ => SteadySpeedCellMaterial, 
		};
	}

	private int GetMarkerMaterialAlpha(MovementPhase phase)
	{
		return phase switch
		{
			MovementPhase.One => SpeedDownCellBaseAlpha, 
			MovementPhase.Two => PassThroughCellBaseAlpha, 
			MovementPhase.Three => SteadySpeedCellBaseAlpha, 
			_ => SteadySpeedCellBaseAlpha, 
		};
	}

	private void HideVantagePointsMarkers()
	{
		foreach (GameObject vantagePointMarker in m_VantagePointMarkers)
		{
			Object.Destroy(vantagePointMarker);
		}
		m_VantagePointMarkers.Clear();
	}

	private Quaternion ConvertDirection(int direction)
	{
		if (direction >= 0 && direction < m_DirectionToRotationMap.Length)
		{
			return m_DirectionToRotationMap[direction];
		}
		return Quaternion.identity;
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		Instance = null;
	}
}
