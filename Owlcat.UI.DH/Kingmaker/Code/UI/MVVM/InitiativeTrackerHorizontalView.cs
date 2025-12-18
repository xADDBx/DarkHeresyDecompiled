using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InitiativeTrackerHorizontalView : InitiativeTrackerView, IHideUIWhileActionCameraHandler, ISubscriber
{
	[SerializeField]
	private RectTransform m_VirtualListViewport;

	[SerializeField]
	private Vector2 m_MaxViewportSize;

	[SerializeField]
	protected TextMeshProUGUI m_StateValue;

	[SerializeField]
	protected CombatUnitEntityView m_CurrentUnit;

	[SerializeField]
	private MoraleBalanceView m_MoraleBalance;

	[Header("Animator")]
	[SerializeField]
	private MoveAnimator m_OwnAnimation;

	[Header("Name")]
	[SerializeField]
	protected TextMeshProUGUI m_NameNormal;

	[SerializeField]
	private FadeAnimator m_NameZoneAnimator;

	[SerializeField]
	private RectTransform m_NameZoneRect;

	private int m_HasScrollPaddingForVirtualList;

	private float m_FixedNormalizedPosition;

	private Vector2 m_TargetVieportSize;

	private Vector2 m_TargetContentSize;

	protected override void OnInitialize()
	{
		m_NameNormal.text = string.Empty;
		m_NameZoneAnimator.Initialize();
		m_NameZoneAnimator.DisappearAnimation();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_MoraleBalance.Bind(base.ViewModel.MoraleBalanceVM);
	}

	protected override void OnUnbind()
	{
		m_MoraleBalance.Unbind();
	}

	protected override void OnUnitHovered()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		InitiativeTrackerMechanicEntityVM currentValue = base.ViewModel.HoveredEntity.CurrentValue;
		if (currentValue == null)
		{
			HideUnitName();
			return;
		}
		CombatMechanicEntityVM viewModel = m_CurrentUnit.ViewModel;
		if (viewModel != null && viewModel == currentValue)
		{
			UpdateName();
		}
		else if (viewModel == null || !viewModel.IsPlayer.CurrentValue)
		{
			if (TryGetNamePosition(currentValue, out var position))
			{
				UpdateName(currentValue, position);
			}
		}
		else
		{
			ScrollToHoveredUnit(UpdateName);
		}
	}

	protected override void PrepareInitiativeTracker()
	{
		ClearTurnVirtualItems();
		m_VirtualListViewport.sizeDelta = new Vector2(m_MaxViewportSize.x, m_VirtualListViewport.sizeDelta.y);
		VirtualList.Content.sizeDelta = new Vector2(m_MaxViewportSize.x, VirtualList.Content.sizeDelta.y);
		VirtualList.Content.anchoredPosition = Vector2.zero;
	}

	protected override void UpdateUnits()
	{
		ClearTurnVirtualItems();
		if (!base.ViewModel.HasUnits.CurrentValue)
		{
			return;
		}
		Vector2 nextPosition = Vector2.zero;
		for (int i = 0; i < base.ViewModel.TrackerEntities.Count; i++)
		{
			if (i == base.ViewModel.RoundIndex + 1)
			{
				nextPosition = AddRoundToVirtualDataView(nextPosition);
			}
			InitiativeTrackerMechanicEntityVM initiativeTrackerMechanicEntityVM = base.ViewModel.TrackerEntities[i];
			if (initiativeTrackerMechanicEntityVM.IsCurrent.CurrentValue)
			{
				m_CurrentUnit.Bind(base.ViewModel.TrackerEntities[i]);
				continue;
			}
			bool currentValue = initiativeTrackerMechanicEntityVM.IsPlayer.CurrentValue;
			if (currentValue || !initiativeTrackerMechanicEntityVM.IsInSquad.CurrentValue || initiativeTrackerMechanicEntityVM.IsSquadLeader.CurrentValue || initiativeTrackerMechanicEntityVM.NeedToShow.CurrentValue)
			{
				nextPosition = AddUnitToVirtualDataView(initiativeTrackerMechanicEntityVM, nextPosition, currentValue);
			}
		}
		m_FixedNormalizedPosition = VirtualList.ScrollRect.horizontalNormalizedPosition;
		m_TargetContentSize = VirtualList.GetContentSize(base.VirtualEntries);
		m_TargetVieportSize = new Vector2(Mathf.Min(m_MaxViewportSize.x, m_TargetContentSize.x), m_VirtualListViewport.sizeDelta.y);
		Sequence sequence = DOTween.Sequence().Pause().SetUpdate(isIndependentUpdate: true);
		Tweener t = m_VirtualListViewport.DOSizeDelta(m_TargetVieportSize, (!base.ViewModel.SkipScroll) ? VirtualList.m_AnimationTime : (VirtualList.m_AnimationTime - 0.1f)).Pause().OnComplete(delegate
		{
			m_VirtualListViewport.sizeDelta = m_TargetVieportSize;
		});
		sequence.Append(t);
		if (!base.ViewModel.SkipScroll)
		{
			VirtualList.UpdateData(base.VirtualEntries, sequence, ScrollBasePosition.Start);
		}
		else
		{
			VirtualList.UpdateData(base.VirtualEntries, sequence, ScrollBasePosition.None, delegate
			{
				VirtualList.ScrollRect.horizontalNormalizedPosition = m_FixedNormalizedPosition;
			});
		}
		base.ViewModel.SkipScroll = false;
		List<InitiativeTrackerMechanicEntityVM> list = new List<InitiativeTrackerMechanicEntityVM>();
		foreach (InitiativeTrackerMechanicEntityVM unit in base.ViewModel.TrackerEntities)
		{
			if (unit.IsEnemy.CurrentValue && !list.Contains((InitiativeTrackerMechanicEntityVM uniqUnit) => uniqUnit.MechanicEntity == unit.MechanicEntity))
			{
				list.Add(unit);
			}
		}
		m_StateValue.text = list.Count().ToString();
	}

	private Vector2 AddUnitToVirtualDataView(InitiativeTrackerMechanicEntityVM mechanicEntityVm, Vector2 nextPosition, bool isPartyUnit)
	{
		TurnVirtualUnitData turnVirtualUnitData = new TurnVirtualUnitData
		{
			ViewModel = mechanicEntityVm
		};
		Vector2 sizeWithPortrait = CombatUnitPrefab.GetSizeWithPortrait(isPartyUnit);
		turnVirtualUnitData.SetViewParameters(nextPosition, sizeWithPortrait);
		AddTurnVirtualItem(turnVirtualUnitData);
		nextPosition.x += sizeWithPortrait.x + VirtualList.Spacing.x;
		return nextPosition;
	}

	private Vector2 AddRoundToVirtualDataView(Vector2 nextPosition)
	{
		Vector2 sizeRound = CombatUnitPrefab.GetSizeRound();
		TurnVirtualUnitData turnVirtualUnitData = new TurnVirtualUnitData
		{
			ViewModel = base.ViewModel.RoundVM
		};
		turnVirtualUnitData.SetViewParameters(nextPosition, sizeRound);
		AddTurnVirtualItem(turnVirtualUnitData);
		nextPosition.x += sizeRound.x + VirtualList.Spacing.x;
		return nextPosition;
	}

	protected override void Show()
	{
		if (base.ViewModel != null)
		{
			base.gameObject.SetActive(value: true);
			m_OwnAnimation.AppearAnimation();
			UISounds.Instance.Sounds.InitiativeTracker.InitiativeTrackerShow.Play();
		}
	}

	protected override void Hide()
	{
		VirtualList.Content.sizeDelta = new Vector2(m_MaxViewportSize.x, VirtualList.Content.sizeDelta.y);
		VirtualList.Content.anchoredPosition = Vector2.zero;
		UISounds.Instance.Sounds.InitiativeTracker.InitiativeTrackerHide.Play();
		m_OwnAnimation.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	public void HandleHideUI()
	{
		Hide();
	}

	public void HandleShowUI()
	{
		DelayedInvoker.InvokeInTime(delegate
		{
			base.gameObject.SetActive(value: true);
		}, 2.5f);
	}

	private void UpdateName()
	{
		InitiativeTrackerMechanicEntityVM currentValue = base.ViewModel.HoveredEntity.CurrentValue;
		Vector3 position;
		if (currentValue == null || !IsUnitVisible(currentValue))
		{
			HideUnitName();
		}
		else if (TryGetNamePosition(currentValue, out position))
		{
			ShowUnitName(position, currentValue.DisplayName);
		}
	}

	private void UpdateName(InitiativeTrackerMechanicEntityVM entity, Vector3 position)
	{
		if (entity == null || !IsUnitVisible(entity))
		{
			m_NameZoneAnimator.Or(null)?.DisappearAnimation();
		}
		else
		{
			ShowUnitName(position, entity.DisplayName);
		}
	}

	private void ShowUnitName(Vector3 namePosition, string displayName)
	{
		m_NameNormal.text = displayName;
		m_NameZoneAnimator.Or(null)?.AppearAnimation();
		m_NameZoneRect.position = namePosition;
	}

	private void HideUnitName()
	{
		m_NameZoneAnimator.Or(null)?.DisappearAnimation();
	}

	private bool TryGetNamePosition(InitiativeTrackerMechanicEntityVM entity, out Vector3 position)
	{
		MechanicEntity mechanicEntity = m_CurrentUnit.ViewModel?.MechanicEntity;
		if (mechanicEntity != null && entity.MechanicEntity == mechanicEntity)
		{
			position = m_CurrentUnit.UnitNameAnchor.position;
			return true;
		}
		CombatUnitOrderView combatUnitOrderView = base.VirtualEntries.FirstOrDefault((ITurnVirtualItemData data) => data.ViewModel == entity)?.BoundView as CombatUnitOrderView;
		if (!combatUnitOrderView || !combatUnitOrderView.UnitNameAnchor || !combatUnitOrderView.gameObject.activeSelf)
		{
			position = Vector3.zero;
			return false;
		}
		position = combatUnitOrderView.UnitNameAnchor.position;
		return true;
	}
}
