using System.Collections.Generic;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class PathCursor
{
	public struct Waypoint
	{
		public Vector3 Position;

		public LinkNode LinkNode;
	}

	private AbstractUnitEntity m_Unit;

	private bool m_HasPath;

	private float m_AmplifyValue = 1f;

	private List<Waypoint> m_Waypoints = new List<Waypoint>();

	private int m_NextWaypointIndex;

	public int NextWaypointIndex => m_NextWaypointIndex;

	public bool HasPath => m_HasPath;

	public bool HasNodePath { get; private set; }

	public Vector3 NextWaypoint3D => m_Waypoints[m_NextWaypointIndex].Position;

	public Vector2 NextWaypoint2D_SizeAdjusted
	{
		get
		{
			if (m_Unit == null || !m_Unit.IsInCombat)
			{
				return m_Waypoints[m_NextWaypointIndex].Position.To2D();
			}
			return SizePathfindingHelper.FromMechanicsToViewPosition(m_Unit, m_Waypoints[m_NextWaypointIndex].Position).To2D();
		}
	}

	public Vector3 NextWaypoint3D_SizeAdjusted
	{
		get
		{
			if (m_Unit == null || !m_Unit.IsInCombat)
			{
				return m_Waypoints[m_NextWaypointIndex].Position;
			}
			return SizePathfindingHelper.FromMechanicsToViewPosition(m_Unit, m_Waypoints[m_NextWaypointIndex].Position);
		}
	}

	public Vector2 PrevWaypoint2D_SizeAdjusted
	{
		get
		{
			Vector3 vector = ((m_NextWaypointIndex == 0) ? m_Waypoints[0].Position : m_Waypoints[m_NextWaypointIndex - 1].Position);
			if (m_Unit == null || !m_Unit.IsInCombat)
			{
				return vector.To2D();
			}
			return SizePathfindingHelper.FromMechanicsToViewPosition(m_Unit, vector).To2D();
		}
	}

	public Vector3 PrevWaypoint3D_SizeAdjusted
	{
		get
		{
			Vector3 vector = ((m_NextWaypointIndex == 0) ? m_Waypoints[0].Position : m_Waypoints[m_NextWaypointIndex - 1].Position);
			if (m_Unit == null || !m_Unit.IsInCombat)
			{
				return vector;
			}
			return SizePathfindingHelper.FromMechanicsToViewPosition(m_Unit, vector);
		}
	}

	public Vector3 LastWaypoint3D
	{
		get
		{
			List<Waypoint> waypoints = m_Waypoints;
			return waypoints[waypoints.Count - 1].Position;
		}
	}

	public Vector3 LastWaypoint3D_SizeAdjusted
	{
		get
		{
			List<Waypoint> waypoints = m_Waypoints;
			Vector3 position = waypoints[waypoints.Count - 1].Position;
			if (m_Unit == null || !m_Unit.IsInCombat)
			{
				return position;
			}
			return SizePathfindingHelper.FromMechanicsToViewPosition(m_Unit, position);
		}
	}

	public LinkNode PrevWaypointLinkNode
	{
		get
		{
			if (!HasNodePath)
			{
				return null;
			}
			if (m_NextWaypointIndex <= 0)
			{
				return null;
			}
			return m_Waypoints[m_NextWaypointIndex - 1].LinkNode;
		}
	}

	public LinkNode NextWaypointLinkNode
	{
		get
		{
			if (!HasNodePath)
			{
				return null;
			}
			return m_Waypoints[m_NextWaypointIndex].LinkNode;
		}
	}

	public bool OnFirstSegment
	{
		get
		{
			if (HasPath && m_Waypoints.Count > 0)
			{
				return m_NextWaypointIndex == 1;
			}
			return false;
		}
	}

	public bool OnLastSegment
	{
		get
		{
			if (HasPath)
			{
				return m_NextWaypointIndex >= m_Waypoints.Count - 1;
			}
			return false;
		}
	}

	public int Count => m_Waypoints.Count;

	public Vector3 GetWaypoint(int index)
	{
		return m_Waypoints[index].Position;
	}

	public bool IsLinkWaypoint(int index)
	{
		return m_Waypoints[index].LinkNode != null;
	}

	public bool AdvanceWaypoint()
	{
		if (m_NextWaypointIndex >= m_Waypoints.Count - 1)
		{
			return false;
		}
		m_NextWaypointIndex++;
		return true;
	}

	public bool AdvanceWaypointAfterTraversal()
	{
		if (NextWaypointLinkNode == null)
		{
			PFLog.Pathfinding.Error("AdvanceWaypointAfterTraversal called when NextWaypointLinkNode is null. This should not happen.");
			return false;
		}
		return AdvanceWaypoint();
	}

	public float GetPathLength(int? waypointsToCount = null)
	{
		int num = m_Waypoints.Count - 1;
		if (waypointsToCount.HasValue)
		{
			num = ((waypointsToCount.Value < 0) ? Mathf.Max(m_Waypoints.Count + waypointsToCount.Value - 1, 0) : Mathf.Min(waypointsToCount.Value, m_Waypoints.Count - 1));
		}
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			Vector2 a = m_Waypoints[i].Position.To2D();
			Vector2 b = m_Waypoints[i + 1].Position.To2D();
			num2 += Vector2.Distance(a, b);
		}
		return num2;
	}

	public float RemainingLength(int? waypointsToCount = null)
	{
		int num = m_Waypoints.Count - 1;
		if (waypointsToCount.HasValue)
		{
			num = ((waypointsToCount.Value < 0) ? Mathf.Max(m_Waypoints.Count + waypointsToCount.Value - 1, m_NextWaypointIndex) : Mathf.Min(m_NextWaypointIndex + waypointsToCount.Value, m_Waypoints.Count - 1));
		}
		float num2 = 0f;
		for (int i = m_NextWaypointIndex; i < num; i++)
		{
			Vector2 a = m_Waypoints[i].Position.To2D();
			Vector2 b = m_Waypoints[i + 1].Position.To2D();
			num2 += Vector2.Distance(a, b);
		}
		return num2;
	}

	public void SetPath(AbstractUnitEntity unit, Path path)
	{
		m_Unit = unit;
		HasNodePath = path.path != null;
		if (HasNodePath && path.vectorPath.Count != path.path.Count)
		{
			PFLog.Pathfinding.Error("SetPath: path.vectorPath.Count != path.path.Count");
			m_HasPath = false;
			return;
		}
		m_Waypoints.Clear();
		for (int i = 0; i < path.vectorPath.Count; i++)
		{
			m_Waypoints.Add(new Waypoint
			{
				Position = path.vectorPath[i],
				LinkNode = (HasNodePath ? (path.path[i] as LinkNode) : null)
			});
		}
		m_NextWaypointIndex = 1;
		if (m_Waypoints.Count > 1 && m_Waypoints[1].LinkNode != null && (m_Waypoints[0].Position - m_Waypoints[1].Position).sqrMagnitude < 0.0001f)
		{
			m_NextWaypointIndex = 2;
		}
		PathfindingService.Instance.TraversalProviderWithBusyLinkPenalties.RegisterPath(path, unit.View.GetInstanceID());
		m_HasPath = true;
	}

	public void ClearPath()
	{
		foreach (Waypoint waypoint in m_Waypoints)
		{
			if (waypoint.LinkNode != null)
			{
				PathfindingService.Instance.TraversalProviderWithBusyLinkPenalties.UnregisterNode(waypoint.LinkNode, m_Unit.View.GetInstanceID());
			}
		}
		m_Waypoints.Clear();
		m_HasPath = false;
		m_NextWaypointIndex = 0;
	}

	public void DrawGizmos()
	{
		if (HasPath)
		{
			for (int i = 0; i < m_Waypoints.Count - 1; i++)
			{
				Vector3 position = m_Waypoints[i].Position;
				Vector3 position2 = m_Waypoints[i + 1].Position;
				Debug.DrawLine(position, position2, Color.cyan, 0f, depthTest: false);
			}
			for (int j = 0; j < m_Waypoints.Count - 1; j++)
			{
				Vector3 start = SizePathfindingHelper.FromMechanicsToViewPosition(m_Unit, m_Waypoints[j].Position);
				Vector3 end = SizePathfindingHelper.FromMechanicsToViewPosition(m_Unit, m_Waypoints[j + 1].Position);
				Debug.DrawLine(start, end, Color.yellow, 0f, depthTest: false);
			}
		}
	}
}
