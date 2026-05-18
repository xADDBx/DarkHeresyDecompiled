using System;
using DG.Tweening;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CombatUnitOrderView : View<InitiativeTrackerMechanicEntityVM>, ITurnVirtualItemView
{
	[Serializable]
	private struct ViewVariant : IBindable<InitiativeTrackerMechanicEntityVM>, IBindable
	{
		public GameObject Root;

		public RectTransform RootTransform;

		public CombatUnitEntityView EntityView;

		public CombatUnitDelayedActionView DelayedActionView;

		public void Bind(InitiativeTrackerMechanicEntityVM vm)
		{
			EntityView.Bind(vm);
			DelayedActionView.Bind(vm);
			Root.SetActive(value: true);
		}

		public void Unbind()
		{
			EntityView.Unbind();
			DelayedActionView.Unbind();
			Root.SetActive(value: false);
		}

		public RectTransform GetUnitNameAnchor(bool isInitiativeHolder)
		{
			if (!isInitiativeHolder)
			{
				return EntityView.UnitNameAnchor;
			}
			return DelayedActionView.UnitNameAnchor;
		}
	}

	[SerializeField]
	private RectTransform m_RootTransform;

	[SerializeField]
	private CanvasGroup m_RootCanvasGroup;

	[SerializeField]
	private ViewVariant m_PartyUnitVariant;

	[SerializeField]
	private ViewVariant m_DefaultVariant;

	[SerializeField]
	private InitiativeTrackerSquadView m_SquadView;

	[SerializeField]
	private InitiativeTrackerRoundWidget m_RoundWidget;

	[Header("Animation settings")]
	[SerializeField]
	protected float m_AnimationTime = 0.3f;

	private IBindable<InitiativeTrackerMechanicEntityVM> m_CurrentView;

	public MonoBehaviour View => this;

	public OwlcatSelectable Selectable => null;

	public RectTransform RectTransform => m_RootTransform;

	public CanvasGroup CanvasGroup => m_RootCanvasGroup;

	public bool WillBeReused { get; set; }

	public RectTransform GetUnitNameAnchor(MechanicEntity entity)
	{
		IBindable<InitiativeTrackerMechanicEntityVM> currentView = m_CurrentView;
		if (!(currentView is ViewVariant viewVariant))
		{
			if (currentView is InitiativeTrackerSquadView initiativeTrackerSquadView)
			{
				return initiativeTrackerSquadView.GetUnitNameAnchor(entity);
			}
			return null;
		}
		return viewVariant.GetUnitNameAnchor(base.ViewModel.IsInitiativeHolder);
	}

	Tween ITurnVirtualItemView.GetHideAnimation(Action completeAction)
	{
		return GetHideAnimationInternal(completeAction);
	}

	Tween ITurnVirtualItemView.GetMoveAnimation(Action completeAction, Vector2 targetPosition)
	{
		return GetMoveAnimationInternal(completeAction, targetPosition);
	}

	Tween ITurnVirtualItemView.GetShowAnimation(Action completeAction, Vector2 targetPosition)
	{
		return GetShowAnimationInternal(completeAction, targetPosition);
	}

	void ITurnVirtualItemView.SetAnchoredPosition(Vector2 position)
	{
		RectTransform.anchoredPosition = position;
	}

	ViewModel ITurnVirtualItemView.GetViewModel()
	{
		return base.ViewModel;
	}

	void ITurnVirtualItemView.ViewBind(ITurnVirtualItemData data)
	{
		SetAlphaAndScale();
		Bind((InitiativeTrackerMechanicEntityVM)(data?.ViewModel));
	}

	void ITurnVirtualItemView.DestroyViewItem()
	{
		OnUnbind();
	}

	public Vector2 GetSizeUnit(bool isPartyUnit)
	{
		ViewVariant obj = (isPartyUnit ? m_PartyUnitVariant : m_DefaultVariant);
		return obj.RootTransform.sizeDelta;
	}

	public Vector2 GetSizeSquad(int squadCount)
	{
		return m_SquadView.GetSize(squadCount);
	}

	public Vector2 GetSizeRound()
	{
		return m_RoundWidget.GetSize();
	}

	protected override void OnBind()
	{
		m_PartyUnitVariant.Root.SetActive(value: false);
		m_DefaultVariant.Root.SetActive(value: false);
		m_RoundWidget.SetActive(isActive: false);
		if (base.ViewModel.HasUnit)
		{
			if (base.ViewModel.Squad != null)
			{
				SetupSquad();
			}
			else
			{
				SetupUnit();
			}
		}
		else
		{
			SetupRoundNumber();
		}
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_CurrentView?.Unbind();
		m_CurrentView = null;
		m_RoundWidget.SetActive(isActive: false);
		EventBus.Unsubscribe(this);
	}

	protected abstract Tween GetHideAnimationInternal(Action completeAction);

	protected abstract Tween GetMoveAnimationInternal(Action completeAction, Vector2 targetPosition);

	protected abstract Tween GetShowAnimationInternal(Action completeAction, Vector2 targetPosition);

	private void SetupUnit()
	{
		ViewVariant viewVariant = (base.ViewModel.IsPlayer.CurrentValue ? m_PartyUnitVariant : m_DefaultVariant);
		viewVariant.Bind(base.ViewModel);
		m_CurrentView = viewVariant;
		SetupOwnSize(viewVariant.RootTransform.sizeDelta);
	}

	private void SetupSquad()
	{
		m_SquadView.Bind(base.ViewModel);
		m_CurrentView = m_SquadView;
	}

	private void SetupRoundNumber()
	{
		base.ViewModel.Round.Subscribe(m_RoundWidget.SetRoundText).AddTo(this);
		m_RoundWidget.SetActive(isActive: true);
		SetupOwnSize(m_RoundWidget.GetSize());
		base.transform.SetAsFirstSibling();
	}

	private void SetAlphaAndScale()
	{
		CanvasGroup.alpha = 0f;
		RectTransform.localScale = new Vector3(1f, 1f, 1f);
	}

	private void SetupOwnSize(Vector2 size)
	{
		RectTransform.sizeDelta = size;
	}
}
