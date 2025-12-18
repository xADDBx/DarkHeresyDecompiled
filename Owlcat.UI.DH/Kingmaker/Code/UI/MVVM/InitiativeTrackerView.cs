using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InitiativeTrackerView : View<InitiativeTrackerVM>
{
	protected class TurnVirtualUnitData : ITurnVirtualItemData
	{
		public Vector2 VirtualSize { get; private set; }

		public Vector2 VirtualPosition { get; private set; }

		public ViewModel ViewModel { get; set; }

		public ITurnVirtualItemView BoundView { get; set; }

		public void SetBoundView(ITurnVirtualItemView view)
		{
			BoundView = view;
		}

		public void SetViewParameters(Vector2 virtualPosition, Vector2 size)
		{
			VirtualPosition = virtualPosition;
			VirtualSize = size;
		}
	}

	[SerializeField]
	protected TurnVirtualListController VirtualList;

	[SerializeField]
	protected CombatUnitOrderView CombatUnitPrefab;

	private bool m_IsInit;

	private bool m_WantUpdateView;

	private readonly Dictionary<ViewModel, int> m_ViewModelToVirtualItemIndex = new Dictionary<ViewModel, int>();

	private readonly List<ITurnVirtualItemData> m_VirtualEntries = new List<ITurnVirtualItemData>();

	protected IReadOnlyList<ITurnVirtualItemData> VirtualEntries => m_VirtualEntries;

	public void RoundChanged(int round)
	{
		if (round > 1)
		{
			UISounds.Instance.Sounds.Combat.NewRound.Play();
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning($"{UIStrings.Instance.TurnBasedTexts.Round.Text} {round}", addToLog: false);
			});
		}
	}

	protected abstract void OnInitialize();

	protected abstract void PrepareInitiativeTracker();

	protected abstract void UpdateUnits();

	protected abstract void Show();

	protected abstract void Hide();

	protected virtual void OnUnitHovered()
	{
		if (base.gameObject.activeInHierarchy)
		{
			ScrollToHoveredUnit(null);
		}
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		PrepareInitiativeTracker();
		EventBus.Subscribe(this).AddTo(this);
		base.ViewModel.RoundCounter.Subscribe(RoundChanged).AddTo(this);
		base.ViewModel.HoveredEntity.Subscribe(delegate
		{
			OnUnitHovered();
		}).AddTo(this);
		base.ViewModel.CurrentUnit.Subscribe(OnCurrentUnitChanged).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.EntitiesUpdated.DebounceFrame(1, UnityFrameProvider.PreLateUpdate), delegate
		{
			m_WantUpdateView = true;
		}).AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			if (m_WantUpdateView && !VirtualList.IsAnimating)
			{
				m_WantUpdateView = false;
				UpdateUnits();
			}
		}).AddTo(this);
		m_WantUpdateView = true;
		base.ViewModel.CanBeVisible.CombineLatest(base.ViewModel.HasUnits, (bool _, bool _) => new { }).Subscribe(_ =>
		{
			OnVisibilityChanged();
		}).AddTo(this);
		VirtualList.ScrollTo(ScrollBasePosition.Start);
	}

	protected override void OnUnbind()
	{
		VirtualList.CleanList();
		m_WantUpdateView = false;
		Hide();
	}

	protected void AddTurnVirtualItem(ITurnVirtualItemData itemData)
	{
		if (!m_ViewModelToVirtualItemIndex.ContainsKey(itemData.ViewModel))
		{
			m_ViewModelToVirtualItemIndex.Add(itemData.ViewModel, m_VirtualEntries.Count);
			m_VirtualEntries.Add(itemData);
		}
	}

	protected void ClearTurnVirtualItems()
	{
		m_ViewModelToVirtualItemIndex.Clear();
		m_VirtualEntries.Clear();
	}

	protected bool IsUnitVisible(ViewModel unitModel)
	{
		if (unitModel == null || !TryGetTurnVirtualItem(unitModel, out var itemData))
		{
			return false;
		}
		return VirtualList.IsVisible(itemData);
	}

	protected void ScrollToHoveredUnit(Action onComplete)
	{
		InitiativeTrackerMechanicEntityVM currentValue = base.ViewModel.HoveredEntity.CurrentValue;
		if (currentValue == null || !currentValue.MechanicEntity.IsPlayerFaction)
		{
			onComplete?.Invoke();
		}
		else if (!TryScrollTo(base.ViewModel.HoveredEntity.CurrentValue, onComplete) && !TryScrollTo(base.ViewModel.SquadLeaderUnit.CurrentValue, onComplete))
		{
			onComplete?.Invoke();
		}
	}

	private bool TryScrollTo(ViewModel unitModel, Action onComplete)
	{
		if (unitModel == null || !TryGetTurnVirtualItem(unitModel, out var itemData) || VirtualList.IsVisible(itemData))
		{
			onComplete?.Invoke();
			return false;
		}
		VirtualList.ScrollTo(itemData, onComplete);
		return true;
	}

	private bool TryGetTurnVirtualItem(ViewModel viewModel, out ITurnVirtualItemData itemData)
	{
		if (!m_ViewModelToVirtualItemIndex.TryGetValue(viewModel, out var value))
		{
			itemData = null;
			return false;
		}
		if (value < 0 || value >= m_VirtualEntries.Count)
		{
			itemData = null;
			return false;
		}
		itemData = m_VirtualEntries[value];
		return itemData != null;
	}

	private void OnCurrentUnitChanged(InitiativeTrackerMechanicEntityVM entity)
	{
		BaseUnitEntity baseUnitEntity = entity?.UnitAsBaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.IsInPlayerParty && UtilityNet.InLobbyAndPlaying && baseUnitEntity.IsMyNetRole())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.TurnBasedTexts.YouTurn, addToLog: false);
			});
		}
	}

	private void OnVisibilityChanged()
	{
		if (base.ViewModel.CanBeVisible.CurrentValue && base.ViewModel.HasUnits.CurrentValue)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Awake()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			VirtualList.Initialize(CombatUnitPrefab);
			OnInitialize();
			m_IsInit = true;
		}
	}
}
