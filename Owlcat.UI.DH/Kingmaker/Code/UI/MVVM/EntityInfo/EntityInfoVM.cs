using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.GameModes;
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
	private readonly IReadOnlyList<IEntityInfoProvider<GameObjectInfo>> m_ObjectInfoProviders;

	private readonly IReadOnlyList<IEntityInfoProvider<NodeInfo>> m_NodeInfoProviders;

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

	public EntityInfoVM(Func<bool> isTurnBasedMode, Func<Vector3> getPointerPosition, UIStrings uiStrings)
	{
		m_ObjectInfoProviders = new List<IEntityInfoProvider<GameObjectInfo>>
		{
			new UnitPlaceholderInfoProvider(),
			new DestructibleCoverInfoProvider(),
			new CoverInfoProvider(uiStrings)
		};
		m_NodeInfoProviders = new List<IEntityInfoProvider<NodeInfo>>
		{
			new AreaEffectInfoProvider(uiStrings)
		};
		m_CheckIsTurnBasedMode = isTurnBasedMode;
		m_IsTurnBasedMode = m_CheckIsTurnBasedMode();
		m_GetPointerPosition = getPointerPosition;
		m_ObjectInfo = new ReactiveProperty<IEntityInfo>().AddTo(this);
		m_NodeInfo = new ReactiveProperty<IEntityInfo>().AddTo(this);
		EntityInfo = m_ObjectInfo.CombineLatest(m_NodeInfo, (IEntityInfo objectInfo, IEntityInfo nodeInfo) => objectInfo ?? nodeInfo).ToReadOnlyReactiveProperty().AddTo(this);
		GameUIState.Instance.GameMode.Subscribe(HandleGameModeChanged).AddTo(this);
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

	private void HandleGameModeChanged(GameModeType gameMode)
	{
		if (!CanShowInfo(gameMode))
		{
			m_LastHover = null;
			m_ObjectInfo.Value = null;
			ClearNodeInfo();
		}
	}

	private bool CanShowInfo(GameModeType gameMode)
	{
		if (gameMode != GameModeType.Cutscene)
		{
			return gameMode != GameModeType.Dialog;
		}
		return false;
	}

	private void UpdateObjectInfo(GameObject hoveredObject)
	{
		if (!CanShowInfo(GameUIState.Instance.GameMode.CurrentValue))
		{
			m_LastHover = null;
			m_ObjectInfo.Value = null;
			return;
		}
		m_LastHover = hoveredObject;
		if (!hoveredObject)
		{
			return;
		}
		GameObjectInfo gameObjectInfo = default(GameObjectInfo);
		gameObjectInfo.GameObject = hoveredObject;
		gameObjectInfo.IsHighlighted = m_IsHighlighted;
		gameObjectInfo.IsTurnBasedMode = m_IsTurnBasedMode;
		GameObjectInfo value = gameObjectInfo;
		foreach (IEntityInfoProvider<GameObjectInfo> objectInfoProvider in m_ObjectInfoProviders)
		{
			if (objectInfoProvider.TryGetEntityInfo(value, out var entityInfo))
			{
				ClearNodeInfo();
				m_ObjectInfo.Value = entityInfo;
				return;
			}
		}
		m_ObjectInfo.Value = null;
	}

	private void ClearNodeInfo()
	{
		m_PreviousPointerPosition = null;
		m_PreviousNode = null;
		m_NodeInfo.Value = null;
	}

	private void UpdateNodeInfo()
	{
		if (m_IsDisposed || m_ObjectInfo.Value != null || !CanShowInfo(GameUIState.Instance.GameMode.CurrentValue))
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
			NodeInfo nodeInfo = default(NodeInfo);
			nodeInfo.Node = nearestNodeXZ;
			nodeInfo.IsHighlighted = m_IsHighlighted;
			nodeInfo.IsTurnBasedMode = m_IsTurnBasedMode;
			NodeInfo value = nodeInfo;
			foreach (IEntityInfoProvider<NodeInfo> nodeInfoProvider in m_NodeInfoProviders)
			{
				if (nodeInfoProvider.TryGetEntityInfo(value, out var entityInfo))
				{
					m_NodeInfo.Value = entityInfo;
					return;
				}
			}
			m_NodeInfo.Value = null;
		}
	}
}
