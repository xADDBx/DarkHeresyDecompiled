using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Pathfinding.Collections;
using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class GraphUpdateRouter
{
	private static GridLookup<NavmeshClipper> m_GridLookup;

	private static readonly List<Bounds> Disabled = new List<Bounds>();

	private static Dictionary<NavmeshClipper, Bounds> m_PrevBounds = new Dictionary<NavmeshClipper, Bounds>();

	private static readonly List<GraphUpdateObject> m_ScheduledGUOs = new List<GraphUpdateObject>();

	private static UpdateHook UpdateHook { get; set; }

	private static GraphTransform GraphTransform => AstarPath.active?.data.gridGraph?.transform ?? throw new InvalidOperationException();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init()
	{
		m_GridLookup = new GridLookup<NavmeshClipper>(new Vector2Int(1, 1));
		NavmeshClipper.AddEnableCallback(OnEnable, OnDisable);
		m_PrevBounds.Clear();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterLoad()
	{
		UpdateHook = new GameObject().AddComponent<UpdateHook>();
		UnityEngine.Object.DontDestroyOnLoad(UpdateHook.gameObject);
		UpdateHook.gameObject.hideFlags = HideFlags.HideAndDontSave;
		UpdateHook.OnUpdate += UpdateAll;
	}

	public static void OnLatePostScan()
	{
		m_ScheduledGUOs.Clear();
		foreach (NavmeshClipper item in NavmeshClipper.allEnabled)
		{
			item.ForceUpdate();
			GetGridLookupRoot(item).previousPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}
		UpdateAll();
	}

	private static void OnEnable(NavmeshClipper clipper)
	{
		if (AstarPath.active?.data.gridGraph != null)
		{
			UpdateCurrentPosImpl(clipper);
		}
	}

	private static void OnDisable(NavmeshClipper clipper)
	{
		if (AstarPath.active?.data.gridGraph != null && !UpdateCurrentPosImpl(clipper))
		{
			Bounds boundsInWorldSpace = GetBoundsInWorldSpace(clipper);
			if (float.IsNaN(boundsInWorldSpace.size.x) || float.IsNaN(boundsInWorldSpace.size.y) || float.IsNaN(boundsInWorldSpace.size.z))
			{
				PFLog.Default.Error(clipper, $"NavmeshClipper on {clipper.gameObject} has NaN bounds. Possible 0 scale in transform.");
				return;
			}
			Disabled.Add(boundsInWorldSpace);
			m_PrevBounds.Remove(clipper);
			m_GridLookup.Remove(clipper);
		}
	}

	private static bool UpdateCurrentPosImpl(NavmeshClipper clipper)
	{
		return QueueRect(GetBoundsInWorldSpace(clipper), clipper);
	}

	private static void UpdatePrevPosImpl(NavmeshClipper clipper)
	{
		Bounds valueOrDefault = m_PrevBounds.GetValueOrDefault(clipper);
		Bounds boundsInWorldSpace = GetBoundsInWorldSpace(clipper);
		if (float.IsNaN(boundsInWorldSpace.size.x) || float.IsNaN(boundsInWorldSpace.size.y) || float.IsNaN(boundsInWorldSpace.size.z))
		{
			if (Application.isPlaying)
			{
				clipper.enabled = false;
				PFLog.Default.Error(clipper, $"NavmeshClipper on {clipper.gameObject} has NaN bounds. Possible 0 scale in transform. Disabling it to avoid spam.");
			}
		}
		else
		{
			m_PrevBounds[clipper] = boundsInWorldSpace;
			if (!(valueOrDefault == default(Bounds)) && !(valueOrDefault == boundsInWorldSpace) && !QueueRect(valueOrDefault))
			{
				Disabled.Add(valueOrDefault);
			}
		}
	}

	private static bool IsWithinAreaBounds(Bounds bounds)
	{
		BlueprintAreaPart blueprintAreaPart = Game.Instance?.CurrentlyLoadedAreaPart;
		if (blueprintAreaPart == null || !blueprintAreaPart.Bounds)
		{
			return !Application.isPlaying;
		}
		if (blueprintAreaPart.Bounds.MechanicBounds.ContainsXZ(bounds.min))
		{
			return blueprintAreaPart.Bounds.MechanicBounds.ContainsXZ(bounds.max);
		}
		return false;
	}

	private static bool QueueRect(Bounds bounds)
	{
		return QueueRect(bounds, null);
	}

	private static bool QueueRect(Bounds bounds, [CanBeNull] NavmeshClipper notifyUpdated)
	{
		if (!AstarPath.active)
		{
			return false;
		}
		if (!IsWithinAreaBounds(bounds))
		{
			return false;
		}
		if (notifyUpdated != null)
		{
			notifyUpdated.NotifyUpdated(GetGridLookupRoot(notifyUpdated));
		}
		GraphUpdateObject item = new GraphUpdateObject(new Bounds(bounds.center, bounds.size * 1.1f))
		{
			updatePhysics = true
		};
		m_ScheduledGUOs.Add(item);
		return true;
	}

	private static Bounds GetBoundsInWorldSpace(NavmeshClipper clipper)
	{
		GraphTransform graphTransform = GraphTransform;
		Rect bounds = clipper.GetBounds(graphTransform, 0f);
		return graphTransform.Transform(new Bounds(bounds.center.To3D(), bounds.size.To3D(1000f)));
	}

	private static GridLookup<NavmeshClipper>.Root GetGridLookupRoot(NavmeshClipper clipper)
	{
		GridLookup<NavmeshClipper>.Root root = m_GridLookup.GetRoot(clipper);
		if (root == null)
		{
			m_GridLookup.Add(clipper, new IntRect(0, 0, 0, 0));
			root = m_GridLookup.GetRoot(clipper);
		}
		return root;
	}

	public static void Update(NavmeshClipper clipper)
	{
		if (clipper is NavmeshCut)
		{
			AdditionalGraphDataManager.Instance.GetGridDataOptional()?.NavmeshCuts.Invalidate();
		}
		UpdatePrevPosImpl(clipper);
		UpdateCurrentPosImpl(clipper);
	}

	public static void UpdateAll(List<NavmeshClipper> clippers, bool force = false)
	{
		AstarPath.active.batchGraphUpdates = true;
		foreach (NavmeshClipper clipper in clippers)
		{
			if (force || clipper.RequiresUpdate(GetGridLookupRoot(clipper)))
			{
				Update(clipper);
			}
		}
		Disabled.RemoveAll(QueueRect);
		FlushGraphUpdates();
	}

	public static void UpdateAll(NavmeshClipper[] clippers, bool force = false)
	{
		AstarPath.active.batchGraphUpdates = true;
		foreach (NavmeshClipper navmeshClipper in clippers)
		{
			if (force || navmeshClipper.RequiresUpdate(GetGridLookupRoot(navmeshClipper)))
			{
				Update(navmeshClipper);
			}
		}
		Disabled.RemoveAll(QueueRect);
		FlushGraphUpdates();
	}

	private static void UpdateAll()
	{
		UpdateAll(NavmeshClipper.allEnabled);
	}

	public static void FlushGraphUpdates()
	{
		if (m_ScheduledGUOs.Empty())
		{
			return;
		}
		try
		{
			foreach (GraphUpdateObject scheduledGUO in m_ScheduledGUOs)
			{
				AstarPath.active.UpdateGraphs(scheduledGUO);
			}
		}
		finally
		{
			m_ScheduledGUOs.Clear();
			AstarPath.active.FlushGraphUpdates();
		}
	}
}
