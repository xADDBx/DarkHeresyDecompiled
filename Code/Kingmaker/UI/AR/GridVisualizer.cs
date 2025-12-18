using System;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.AR;

public class GridVisualizer : MonoBehaviour, ITurnBasedModeHandler, ISubscriber
{
	private Renderer[] Renderers = new Renderer[0];

	private void OnEnable()
	{
		Renderers = GetComponentsInChildren<Renderer>();
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(Rebuild));
		EventBus.Subscribe(this);
		HandleTurnBasedModeSwitched(Game.Instance.Controllers.TurnController.TurnBasedModeActive);
	}

	private void OnDisable()
	{
		Renderers = Array.Empty<Renderer>();
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(Rebuild));
		EventBus.Unsubscribe(this);
		HandleTurnBasedModeSwitched(isTurnBased: false);
	}

	private void Rebuild(AstarPath path)
	{
		GridGraph gridGraph = AstarPath.active.graphs.OfType<GridGraph>().FirstOrDefault();
		if (gridGraph != null)
		{
			Vector2 size = gridGraph.size;
			base.gameObject.transform.localPosition = gridGraph.center;
			base.gameObject.transform.localScale = new Vector3(size.x, 1000f, size.y);
			base.gameObject.transform.rotation = Quaternion.Euler(gridGraph.rotation);
			Renderer[] renderers = Renderers;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].material.mainTextureScale = new Vector2(gridGraph.width, gridGraph.depth);
			}
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased && AstarPath.active != null)
		{
			Rebuild(AstarPath.active);
		}
		Renderer[] renderers = Renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = isTurnBased;
		}
	}
}
