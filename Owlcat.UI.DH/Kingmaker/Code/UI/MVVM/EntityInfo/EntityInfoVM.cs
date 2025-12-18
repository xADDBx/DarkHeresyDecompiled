using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class EntityInfoVM : ViewModel, IMouseHoverHandler, ISubscriber, IInteractionHighlightUIHandler, ITurnBasedModeHandler, IAreaHandler
{
	private readonly IReadOnlyList<IEntityInfoProvider<GameObject>> m_ObjectInfoProviders;

	private readonly IReadOnlyList<IEntityInfoProvider<GridNodeBase>> m_NodeInfoProviders;

	private readonly ReactiveProperty<IEntityInfo> m_ObjectInfo;

	private readonly ReactiveProperty<IEntityInfo> m_NodeInfo;

	private bool m_IsDisposed;

	private bool m_IsHighlighted;

	private bool m_IsTurnBasedMode;

	private Func<bool> m_CheckIsTurnBasedMode;

	private Func<Vector3> m_GetPointerPosition;

	private Vector3? m_PreviousPointerPosition;

	private GridNodeBase m_PreviousNode;

	private GameObject m_LastHover;

	public ReadOnlyReactiveProperty<IEntityInfo> EntityInfo { get; }

	public EntityInfoVM(Func<bool> isTurnBasedMode, Func<Vector3> getPointerPosition, UIStrings uiStrings, UIIcons uiIcons)
	{
		m_ObjectInfoProviders = new List<IEntityInfoProvider<GameObject>>
		{
			new UnitPlaceholderInfoProvider(),
			new DestructibleCoverInfoProvider(),
			new CoverInfoProvider(uiStrings)
		};
		m_NodeInfoProviders = new List<IEntityInfoProvider<GridNodeBase>>
		{
			new AreaEffectInfoProvider(uiStrings, uiIcons)
		};
		m_CheckIsTurnBasedMode = isTurnBasedMode;
		m_IsTurnBasedMode = m_CheckIsTurnBasedMode();
		m_GetPointerPosition = getPointerPosition;
		m_ObjectInfo = new ReactiveProperty<IEntityInfo>().AddTo(this);
		m_NodeInfo = new ReactiveProperty<IEntityInfo>().AddTo(this);
		EntityInfo = m_ObjectInfo.CombineLatest(m_NodeInfo, (IEntityInfo objectInfo, IEntityInfo nodeInfo) => objectInfo ?? nodeInfo).ToReadOnlyReactiveProperty().AddTo(this);
		Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(UpdateNodeInfo).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	void IMouseHoverHandler.OnHoverObjectChanged(GameObject oldHover, GameObject newHover)
	{
		m_LastHover = newHover;
		UpdateObjectInfo(newHover);
	}

	void IInteractionHighlightUIHandler.HandleHighlightChange(bool isOn)
	{
		m_IsHighlighted = isOn;
		if (isOn)
		{
			UpdateObjectInfo(m_LastHover);
			return;
		}
		m_ObjectInfo.Value = null;
		ClearNodeInfo();
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_IsTurnBasedMode = isTurnBased;
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		m_IsTurnBasedMode = m_CheckIsTurnBasedMode();
	}

	protected override void OnDispose()
	{
		m_IsDisposed = true;
	}

	private void UpdateObjectInfo(GameObject hoveredObject)
	{
		m_LastHover = hoveredObject;
		if (!hoveredObject || !CanShowInfo())
		{
			m_ObjectInfo.Value = null;
			return;
		}
		foreach (IEntityInfoProvider<GameObject> objectInfoProvider in m_ObjectInfoProviders)
		{
			if (objectInfoProvider.TryGetEntityInfo(hoveredObject, out var entityInfo))
			{
				ClearNodeInfo();
				m_ObjectInfo.Value = entityInfo;
				return;
			}
		}
		m_ObjectInfo.Value = null;
	}

	private bool CanShowInfo()
	{
		if (m_IsHighlighted)
		{
			return m_IsTurnBasedMode;
		}
		return false;
	}

	private void ClearNodeInfo()
	{
		m_PreviousPointerPosition = null;
		m_PreviousNode = null;
		m_NodeInfo.Value = null;
	}

	private void UpdateNodeInfo()
	{
		if (!CanShowInfo() || m_IsDisposed || m_ObjectInfo.Value != null)
		{
			ClearNodeInfo();
			return;
		}
		Vector3 vector = m_GetPointerPosition();
		if (m_PreviousPointerPosition.HasValue && (vector - m_PreviousPointerPosition.Value).sqrMagnitude < float.Epsilon)
		{
			return;
		}
		m_PreviousPointerPosition = vector;
		GridNodeBase nearestNodeXZ = vector.GetNearestNodeXZ();
		if (nearestNodeXZ == null)
		{
			ClearNodeInfo();
		}
		else
		{
			if (nearestNodeXZ == m_PreviousNode)
			{
				return;
			}
			m_PreviousNode = nearestNodeXZ;
			foreach (IEntityInfoProvider<GridNodeBase> nodeInfoProvider in m_NodeInfoProviders)
			{
				if (nodeInfoProvider.TryGetEntityInfo(nearestNodeXZ, out var entityInfo))
				{
					m_NodeInfo.Value = entityInfo;
					return;
				}
			}
			m_NodeInfo.Value = null;
		}
	}
}
