using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Networking;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ActionBarBaseSlotView : View<ActionBarSlotVM>
{
	[Header("Button")]
	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[Header("Icon Block")]
	[SerializeField]
	protected Image m_Icon;

	private VisibilityController m_IconVisibility;

	[SerializeField]
	private Material m_IconDefaultMaterial;

	[SerializeField]
	private Material m_IconDisableMaterial;

	[Header("Text")]
	[SerializeField]
	private Image m_SelectedMark;

	[SerializeField]
	private FadeAnimator m_TargetPingEntity;

	[Header("Attack Ability Group Cooldown Alert Mark")]
	[SerializeField]
	private Image m_AttackAbilityGroupCooldownAlertMark;

	[SerializeField]
	private Color m_MinAlertColor;

	[SerializeField]
	private Color m_MaxAlertColor;

	[SerializeField]
	private float m_AlertAnimationBlinkTime = 0.4f;

	[Header("Action Point Block")]
	[SerializeField]
	private GameObject m_ActionPointBlock;

	[SerializeField]
	private Slider m_ActionPoints;

	[SerializeField]
	private int m_ActionPointsMaxValue = 6;

	[Header("Splash Block")]
	[SerializeField]
	private RectTransform m_SplashAnim;

	private VisibilityController m_SelectedMarkVisibility;

	private VisibilityController m_ActionPointVisibility;

	private VisibilityController m_SplashBlockVisibility;

	private IDisposable m_PingDelay;

	private bool m_IsPingInProcess;

	private readonly float m_SplashBasePosX = -80f;

	private readonly float m_SplashFinalPosX = 34f;

	private Tweener m_AttackAbilityGroupCooldownAlertTweener;

	private VisibilityController m_SlotVisibility;

	public MechanicActionBarSlot MechanicActionBarSlot => base.ViewModel.MechanicActionBarSlot;

	public bool IsEmpty => base.ViewModel.IsEmpty.CurrentValue;

	protected virtual void Awake()
	{
		m_SlotVisibility = VisibilityController.Control(base.transform);
		m_IconVisibility = VisibilityController.Control(m_Icon);
		m_SelectedMarkVisibility = VisibilityController.Control(m_SelectedMark);
		m_ActionPointVisibility = VisibilityController.Control(m_ActionPointBlock);
		m_SplashBlockVisibility = VisibilityController.ControlParent(m_SplashAnim);
	}

	public virtual void Initialize()
	{
		SetupButton();
		if (!(m_SplashAnim == null))
		{
			Vector2 anchoredPosition = m_SplashAnim.anchoredPosition;
			anchoredPosition.x = m_SplashBasePosX;
			m_SplashAnim.anchoredPosition = anchoredPosition;
			m_SplashBlockVisibility.SetVisible(visible: false);
		}
	}

	protected override void OnBind()
	{
		m_SlotVisibility.SetVisible(visible: true);
		if (m_TargetPingEntity != null)
		{
			if (m_TargetPingEntity.CanvasGroup != null)
			{
				m_TargetPingEntity.CanvasGroup.alpha = 0f;
			}
			m_TargetPingEntity.DisappearAnimation();
		}
		base.ViewModel.Icon.Subscribe(delegate
		{
			SetIcon();
		}).AddTo(this);
		base.ViewModel.ActionPointCost.Subscribe(delegate(int value)
		{
			if (!(m_ActionPointBlock == null))
			{
				m_ActionPointVisibility.SetVisible(value >= 0);
				m_ActionPoints.maxValue = ((value > m_ActionPointsMaxValue) ? value : m_ActionPointsMaxValue);
				m_ActionPoints.value = value;
			}
		}).AddTo(this);
		base.ViewModel.OnClickCommand.Subscribe(delegate
		{
			SplashAnimation();
		}).AddTo(this);
		base.ViewModel.IsCasting.CombineLatest(base.ViewModel.IsEmpty, (bool casting, bool empty) => new { casting, empty }).Subscribe(_ =>
		{
			SetupButton();
		}).AddTo(this);
		base.ViewModel.CanActivateSlot.Subscribe(delegate(bool canActivate)
		{
			SetCanUse(canActivate);
		}).AddTo(this);
		base.ViewModel.IsSelected.Subscribe(m_SelectedMarkVisibility.SetVisible).AddTo(this);
		base.ViewModel.IsAlerted.Subscribe(delegate(bool value)
		{
			if (!(m_AttackAbilityGroupCooldownAlertMark == null))
			{
				if (value)
				{
					PlayAttackAbilityGroupCooldownAlertAnimation();
				}
				else
				{
					StopAttackAbilityGroupCooldownAlertAnimation();
				}
			}
		}).AddTo(this);
		base.ViewModel.CoopPingActionBarSlot.Subscribe(delegate((NetPlayer player, bool show) value)
		{
			PingActionBarAbility(value.player, value.show);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_AttackAbilityGroupCooldownAlertTweener?.Kill();
		m_AttackAbilityGroupCooldownAlertTweener = null;
		m_PingDelay?.Dispose();
		m_PingDelay = null;
		m_SlotVisibility.SetVisible(visible: false);
		m_ActionPointVisibility?.SetVisible(visible: false);
	}

	public void SetVisible(bool visible)
	{
		m_SlotVisibility.SetVisible(visible);
	}

	public void PingActionBarAbility(NetPlayer player, bool show)
	{
		if (m_TargetPingEntity == null)
		{
			return;
		}
		if (!show)
		{
			if (m_IsPingInProcess)
			{
				m_TargetPingEntity.DisappearAnimation(delegate
				{
					m_IsPingInProcess = false;
				});
			}
			return;
		}
		m_PingDelay?.Dispose();
		int index = player.Index - 1;
		Image component = m_TargetPingEntity.GetComponent<Image>();
		if (component != null)
		{
			component.color = ConfigRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
		}
		m_TargetPingEntity.AppearAnimation();
		m_IsPingInProcess = true;
		m_PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingEntity.DisappearAnimation(delegate
			{
				m_IsPingInProcess = false;
			});
		}, 7.5f);
	}

	public virtual void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
	}

	private void SetCanUse(bool canUse)
	{
		m_Icon.material = (canUse ? m_IconDefaultMaterial : m_IconDisableMaterial);
		SetInteractable(canUse);
	}

	public void SetInteractable(bool value)
	{
		m_MainButton.Interactable = value;
	}

	protected void SetupButton()
	{
		ActionBarSlotVM viewModel = base.ViewModel;
		if (viewModel == null || viewModel.IsEmpty.CurrentValue)
		{
			m_MainButton.SetActiveLayer(ActionBarButtonStates.Empty.ToString());
		}
		else if (base.ViewModel.IsSelectionBusy.CurrentValue)
		{
			m_MainButton.SetActiveLayer(ActionBarButtonStates.Selected.ToString());
		}
		else if (base.ViewModel.IsCasting.CurrentValue)
		{
			m_MainButton.SetActiveLayer(ActionBarButtonStates.IsCasting.ToString());
		}
		else
		{
			m_MainButton.SetActiveLayer(ActionBarButtonStates.Common.ToString());
		}
	}

	private void SetIcon()
	{
		Sprite currentValue = base.ViewModel.Icon.CurrentValue;
		bool visible = currentValue != null;
		m_IconVisibility.SetVisible(visible);
		m_Icon.sprite = currentValue;
	}

	private void SplashAnimation()
	{
		if (!(m_SplashAnim == null))
		{
			m_SplashAnim.DOKill();
			m_SplashBlockVisibility.SetVisible(visible: true);
			Vector2 anchoredPosition = m_SplashAnim.anchoredPosition;
			anchoredPosition.x = m_SplashBasePosX;
			m_SplashAnim.anchoredPosition = anchoredPosition;
			m_SplashAnim.DOAnchorPosX(m_SplashFinalPosX, 0.4f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				m_SplashBlockVisibility.SetVisible(visible: false);
			});
		}
	}

	private void StopAttackAbilityGroupCooldownAlertAnimation()
	{
		Color color = m_AttackAbilityGroupCooldownAlertMark.color;
		color.a = 0f;
		m_AttackAbilityGroupCooldownAlertMark.color = color;
		m_AttackAbilityGroupCooldownAlertTweener.Kill();
		m_AttackAbilityGroupCooldownAlertTweener = null;
	}

	private void PlayAttackAbilityGroupCooldownAlertAnimation()
	{
		m_AttackAbilityGroupCooldownAlertTweener?.Kill();
		m_AttackAbilityGroupCooldownAlertMark.color = m_MinAlertColor;
		m_AttackAbilityGroupCooldownAlertTweener = m_AttackAbilityGroupCooldownAlertMark.DOColor(m_MaxAlertColor, m_AlertAnimationBlinkTime).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
		m_AttackAbilityGroupCooldownAlertTweener.Play();
	}
}
