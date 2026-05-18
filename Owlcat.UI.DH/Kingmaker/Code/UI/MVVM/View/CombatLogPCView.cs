using System.Collections;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatLogPCView : CombatLogBaseView
{
	[SerializeField]
	private OwlcatMultiButton m_SwitchPinButton;

	[SerializeField]
	private OwlcatButton m_ForceScrollToBottomButton;

	[Header("Resizable")]
	[SerializeField]
	private ResizePanel m_ResizePanel;

	[Header("Filters")]
	[SerializeField]
	private FadeAnimator m_FiltersAnimator;

	[SerializeField]
	private FadeAnimator[] m_FiltersAnimators;

	[SerializeField]
	private Image[] m_FilterArrows;

	private ReadOnlyReactiveProperty<bool> m_IsFiltersVisible;

	private readonly ReactiveProperty<bool> m_IsMoseHovered = new ReactiveProperty<bool>();

	private bool m_BottomEdgeVisible;

	private bool m_SaveInited;

	private int m_SaveFilterIndex;

	private bool BottomEdgeNeeded
	{
		get
		{
			if (Application.isPlaying)
			{
				if (base.ViewModel.Items.Count > 0)
				{
					ObservableList<CombatLogBaseVM> items = base.ViewModel.Items;
					return !items[items.Count - 1].HasView;
				}
				return false;
			}
			return true;
		}
	}

	public override void Awake()
	{
		base.Awake();
		m_FiltersAnimator.Initialize();
		m_FiltersAnimator.gameObject.SetActive(value: false);
		FadeAnimator[] filtersAnimators = m_FiltersAnimators;
		foreach (FadeAnimator obj in filtersAnimators)
		{
			obj.Initialize();
			obj.gameObject.SetActive(value: false);
		}
		m_ResizePanel.Initialize(this);
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_SaveFilterIndex = ((Game.Instance.Player.UISettings.LogSelectedFilterIndex >= 0 && Game.Instance.Player.UISettings.LogSelectedFilterIndex < m_Toggles.Count) ? Game.Instance.Player.UISettings.LogSelectedFilterIndex : 0);
		Vector2 combatLogMinSize = ConfigRoot.Instance.UIConfig.CombatLogMinSize;
		Vector2 logSize = Game.Instance.Player.UISettings.LogSize;
		logSize = new Vector2(Mathf.Max(logSize.x, combatLogMinSize.x), Mathf.Max(logSize.y, combatLogMinSize.y));
		SetSizeDelta(logSize);
		OwlcatR3UnitExtensions.Subscribe(m_ForceScrollToBottomButton.OnLeftClickAsObservable(), delegate
		{
			m_VirtualList.ScrollController.ForceScrollToBottom();
		}).AddTo(this);
		foreach (CombatLogToggleWithCustomHint toggle in m_Toggles)
		{
			toggle.SetHint(base.ViewModel.GetChannelName(m_Toggles.IndexOf(toggle))).AddTo(this);
		}
		m_ToggleGroup.ActiveToggle.Subscribe(delegate(OwlcatToggle toggle)
		{
			HandleActiveToggleChanged(toggle as CombatLogToggleWithCustomHint);
		}).AddTo(this);
		m_IsFiltersVisible = IsPinned.Or(m_IsMoseHovered).ToReadOnlyReactiveProperty(initialValue: false);
		m_IsFiltersVisible.Subscribe(SwitchFiltersVisibility).AddTo(this);
		UISounds.Instance.SetClickAndHoverSound(m_SwitchPinButton, ButtonSoundsEnum.PlastickSound);
		OwlcatR3UnitExtensions.Subscribe(m_SwitchPinButton.OnLeftClickAsObservable(), delegate
		{
			bool flag2 = (IsPinned.Value = !IsPinned.Value);
			bool flag3 = flag2;
			Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.CombatLog).State((!flag3) ? InterfaceMetricsEvent.InterfaceStates.Close : InterfaceMetricsEvent.InterfaceStates.Open).Send();
		}).AddTo(this);
		m_SwitchPinButton.SetHint(UIStrings.Instance.CombatTexts.CombatLogShowHide, "ShowHideCombatLog").AddTo(this);
		Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(OnLateUpdate).AddTo(this);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.ShowHideCombatLog.name, delegate
		{
			IsPinned.Value = !IsPinned.Value;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		StopAllCoroutines();
	}

	protected virtual void OnLateUpdate()
	{
		m_BottomEdgeVisible = VisibleEdge(m_BottomEdgeVisible, BottomEdgeNeeded, m_ForceScrollToBottomButton);
	}

	private bool VisibleEdge(bool visible, bool condition, OwlcatButton button)
	{
		if (button == null)
		{
			return visible;
		}
		if (visible == condition)
		{
			return visible;
		}
		button.gameObject.SetActive(condition);
		visible = condition;
		return visible;
	}

	public void ChangeOwnPosition(float pos)
	{
		((RectTransform)base.transform).DOAnchorPosY(pos, 0.2f);
	}

	public void SwitchFiltersVisibility(bool state)
	{
		StopAllCoroutines();
		if (state)
		{
			m_FiltersAnimator.AppearAnimation();
			StartCoroutine(PlayFiltersAppearAnimationCoroutine());
		}
		else
		{
			m_FiltersAnimator.DisappearAnimation();
			StartCoroutine(PlayFiltersDisappearAnimationCoroutine());
		}
	}

	private IEnumerator PlayFiltersAppearAnimationCoroutine()
	{
		CombatSounds.Instance.CombatLog.FiltersOpen.Play();
		for (int i = 0; i < m_FiltersAnimators.Length; i++)
		{
			yield return new WaitForSeconds(0.1f);
			if (i == m_FiltersAnimators.Length - 1)
			{
				m_FiltersAnimators[i].AppearAnimation(OnCompleteFiltersAnimation);
			}
			else
			{
				m_FiltersAnimators[i].AppearAnimation();
			}
		}
	}

	private IEnumerator PlayFiltersDisappearAnimationCoroutine()
	{
		CombatSounds.Instance.CombatLog.FiltersClose.Play();
		for (int i = m_FiltersAnimators.Length - 1; i >= 0; i--)
		{
			yield return new WaitForSeconds(0.1f);
			m_FiltersAnimators[i].DisappearAnimation();
		}
	}

	public override void SwitchPinnedState(bool pinned)
	{
		base.SwitchPinnedState(pinned);
		if (pinned)
		{
			m_SwitchPinButton.SetActiveLayer("Pinned");
			SetContainerState(state: true);
		}
		else
		{
			m_SwitchPinButton.SetActiveLayer("NotPinned");
		}
	}

	private void HandleActiveToggleChanged(CombatLogToggleWithCustomHint active)
	{
		if (active == null)
		{
			return;
		}
		int num = m_Toggles.IndexOf(active);
		base.ViewModel.SetCurrentChannelById(num);
		for (int i = 0; i < m_FilterArrows.Length; i++)
		{
			Image image = m_FilterArrows[i];
			if (i != num)
			{
				image.enabled = false;
				continue;
			}
			image.enabled = true;
			StartedTweeners.Add(image.rectTransform.DOShakeScale(0.1f, 1f, 10, 45f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
		}
		Game.Instance.Player.UISettings.LogSelectedFilterIndex = num;
	}

	private void OnCompleteFiltersAnimation()
	{
		if (!m_SaveInited)
		{
			m_Toggles[m_SaveFilterIndex].Set(value: true);
			m_SaveInited = false;
		}
	}
}
