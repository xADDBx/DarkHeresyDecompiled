using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class ClickPointerManager : MonoBehaviour, IGameModeHandler, ISubscriber, IViewDetachedHandler, ISubscriber<IEntity>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IPartyCombatHandler
{
	public ClickPointerPrefab PointerPrefab;

	public ClickPointerPrefab PreviewPointerPrefab;

	public GameObject PreviewArrow;

	private readonly Dictionary<BaseUnitEntity, ClickPointerPrefab> m_PreviewUnitMarks = new Dictionary<BaseUnitEntity, ClickPointerPrefab>();

	private readonly Dictionary<BaseUnitEntity, ClickPointerPrefab> m_UnitMarks = new Dictionary<BaseUnitEntity, ClickPointerPrefab>();

	private readonly List<(BaseUnitEntity, ClickPointerPrefab)> m_UnitMarksList = new List<(BaseUnitEntity, ClickPointerPrefab)>();

	private GameObject m_PlayerShipMark;

	public static ClickPointerManager Instance { get; private set; }

	public Dictionary<BaseUnitEntity, Vector3> UnitMarksLocalMap { get; } = new Dictionary<BaseUnitEntity, Vector3>();


	private bool SignalIsActive => Game.Instance.Controllers.DetectiveRadarController.SignalState == DetectiveRadarState.Activated;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
		Instance = this;
	}

	private void OnDisable()
	{
		Clear();
		EventBus.Unsubscribe(this);
		Instance = null;
	}

	public void AddPointer(Vector3 clickEventWorldPosition, BaseUnitEntity unit)
	{
		if (!IsCorrectMode(Game.Instance.CurrentModeType) || !unit.IsDirectlyControllable)
		{
			return;
		}
		UnitMarksLocalMap[unit] = clickEventWorldPosition;
		if (!Game.Instance.IsControllerGamepad)
		{
			if (!m_UnitMarks.TryGetValue(unit, out var value))
			{
				value = Object.Instantiate(PointerPrefab, base.transform);
				m_UnitMarks.Add(unit, value);
				m_UnitMarksList.Add((unit, value));
			}
			value.SetVisible(visible: true);
			value.transform.localPosition = clickEventWorldPosition;
			if (unit.IsMainCharacter)
			{
				value.SetSignalState(SignalIsActive);
			}
		}
	}

	public void AddPreviewPointer(Vector3 markerPos, BaseUnitEntity unit)
	{
		if (IsCorrectMode(Game.Instance.CurrentModeType))
		{
			if (!m_PreviewUnitMarks.TryGetValue(unit, out var value))
			{
				value = Object.Instantiate(PreviewPointerPrefab, base.transform);
				m_PreviewUnitMarks.Add(unit, value);
			}
			value.SetVisible(visible: true);
			value.transform.localPosition = markerPos;
		}
	}

	[UsedImplicitly]
	private void Update()
	{
		if (Game.Instance.IsControllerGamepad)
		{
			return;
		}
		foreach (var unitMarks in m_UnitMarksList)
		{
			BaseUnitEntity item = unitMarks.Item1;
			ClickPointerPrefab item2 = unitMarks.Item2;
			UnitMoveTo currentOrQueued = item.Commands.GetCurrentOrQueued<UnitMoveTo>();
			UnitMoveToProper currentOrQueued2 = item.Commands.GetCurrentOrQueued<UnitMoveToProper>();
			UnitAreaTransition current2 = item.Commands.GetCurrent<UnitAreaTransition>();
			bool flag = (currentOrQueued != null && !currentOrQueued.IsFinished) || (currentOrQueued2 != null && !currentOrQueued2.IsFinished) || (current2 != null && !current2.IsFinished);
			bool flag2 = TurnController.IsInTurnBasedCombat();
			if (item.GetSaddledUnit() == null && flag && !Game.Instance.IsControllerGamepad && IsCorrectMode(Game.Instance.CurrentModeType) && !flag2)
			{
				if (Game.Instance.Controllers.SelectionCharacter.SelectedUnits.Contains(item))
				{
					item2.SetVisible(visible: true);
				}
				else
				{
					item2.SetVisible(visible: true, 0.5f);
				}
			}
			else
			{
				item2.SetVisible(visible: false);
			}
		}
	}

	public void CancelPreview()
	{
		UnitMarksLocalMap.Clear();
		foreach (KeyValuePair<BaseUnitEntity, ClickPointerPrefab> previewUnitMark in m_PreviewUnitMarks)
		{
			previewUnitMark.Deconstruct(out var _, out var value);
			value.SetVisible(visible: false);
		}
		if ((bool)m_PlayerShipMark)
		{
			m_PlayerShipMark.SetActive(value: false);
		}
		PreviewArrow.SetActive(value: false);
	}

	public void ShowPreviewArrow(Vector3 worldPosition, Vector3 direction)
	{
		PreviewArrow.SetActive(value: true);
		float y = Mathf.Atan2(direction.x, direction.z) * 57.29578f;
		PreviewArrow.transform.eulerAngles = new Vector3(0f, y, 0f);
		Quaternion quaternion = Quaternion.LookRotation(direction);
		PreviewArrow.transform.localPosition = worldPosition + quaternion * Vector3.forward;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!IsCorrectMode(gameMode))
		{
			CancelPreview();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private bool IsCorrectMode(GameModeType gameMode)
	{
		if (gameMode != GameModeType.Dialog && gameMode != GameModeType.Cutscene)
		{
			return gameMode != GameModeType.GameOver;
		}
		return false;
	}

	public void OnViewDetached(IEntityViewBase view)
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity baseUnitEntity)
		{
			if (UnitMarksLocalMap.TryGetValue(baseUnitEntity, out var _))
			{
				UnitMarksLocalMap.Remove(baseUnitEntity);
			}
			if (m_UnitMarks.TryGetValue(baseUnitEntity, out var value2))
			{
				Object.Destroy(value2);
				m_UnitMarks.Remove(baseUnitEntity);
				m_UnitMarksList.Remove((baseUnitEntity, value2));
			}
			if (m_PreviewUnitMarks.TryGetValue(baseUnitEntity, out var value3))
			{
				Object.Destroy(value3);
				m_PreviewUnitMarks.Remove(baseUnitEntity);
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			CancelPreview();
		}
	}

	private void Clear()
	{
		BaseUnitEntity key;
		ClickPointerPrefab value;
		foreach (KeyValuePair<BaseUnitEntity, ClickPointerPrefab> unitMark in m_UnitMarks)
		{
			unitMark.Deconstruct(out key, out value);
			Object.Destroy(value);
		}
		m_UnitMarks.Clear();
		m_UnitMarksList.Clear();
		foreach (KeyValuePair<BaseUnitEntity, ClickPointerPrefab> previewUnitMark in m_PreviewUnitMarks)
		{
			previewUnitMark.Deconstruct(out key, out value);
			Object.Destroy(value);
		}
		m_PreviewUnitMarks.Clear();
		if (m_PlayerShipMark != null)
		{
			m_PlayerShipMark.SetActive(value: false);
			Object.Destroy(m_PlayerShipMark);
		}
		UnitMarksLocalMap.Clear();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		CancelPreview();
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		CancelPreview();
	}
}
