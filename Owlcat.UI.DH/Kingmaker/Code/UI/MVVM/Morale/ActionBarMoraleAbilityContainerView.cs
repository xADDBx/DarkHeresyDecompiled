using DG.Tweening;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.Morale;

public class ActionBarMoraleAbilityContainerView : View<ActionBarMoraleVM>
{
	private bool ShowBroken;

	[Header("Slots")]
	[SerializeField]
	private ActionBarSlotAbilityPCView m_SlotView;

	[SerializeField]
	private WidgetList m_AbilityWidgetList;

	[SerializeField]
	private VerticalLayoutGroup m_AbilityLayout;

	[SerializeField]
	private CanvasGroup m_AbilityCanvasGroup;

	[SerializeField]
	private float m_DefaultAbilityContainerHeight;

	[Header("Buttons")]
	[SerializeField]
	private CanvasGroup m_ButtonsContainer;

	[SerializeField]
	private OwlcatMultiButton m_ExpandAbilityContainerButton;

	[SerializeField]
	private OwlcatMultiButton m_CollapseAbilityContainerButton;

	private RectTransform m_MainRectTransform;

	private RectTransform m_AbilityContainerRectTransform;

	private CompositeDisposable m_SlotsDisposable = new CompositeDisposable();

	private void Awake()
	{
		m_MainRectTransform = base.transform as RectTransform;
		m_AbilityContainerRectTransform = m_AbilityLayout.transform as RectTransform;
	}

	public void Initialize(bool broken)
	{
		ShowBroken = broken;
	}

	protected override void OnBind()
	{
		m_ExpandAbilityContainerButton.OnLeftClickAsObservable().Subscribe(ExpandAbilityContainer).AddTo(this);
		m_CollapseAbilityContainerButton.OnLeftClickAsObservable().Subscribe(CollapseAbilityContainer).AddTo(this);
		base.ViewModel.UnitChanged.Subscribe(DrawAbilities).AddTo(this);
		base.ViewModel.AbilitiesListUpdated.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(DrawAbilities).AddTo(this);
		DrawAbilities();
	}

	protected override void OnUnbind()
	{
		m_SlotsDisposable?.Clear();
	}

	private void DrawAbilities()
	{
		m_SlotsDisposable?.Clear();
		m_SlotsDisposable?.Add(m_AbilityWidgetList.DrawEntries(ShowBroken ? base.ViewModel.BrokenSlots : base.ViewModel.HeroicSlots, m_SlotView));
		SetAbilityContainerVisual();
	}

	private void SetAbilityContainerVisual()
	{
		bool flag = m_AbilityWidgetList.Entries.Count > 1;
		m_ButtonsContainer.alpha = (flag ? 1f : 0f);
		m_ButtonsContainer.blocksRaycasts = flag;
		m_AbilityCanvasGroup.alpha = (flag ? 0f : 1f);
		m_AbilityCanvasGroup.blocksRaycasts = !flag;
		SwitchButtons(isCollapsed: true);
		UpdateAbilityContainerHeight(m_DefaultAbilityContainerHeight, immediate: true);
	}

	private void SwitchButtons(bool isCollapsed)
	{
		m_ExpandAbilityContainerButton.gameObject.SetActive(isCollapsed);
		m_CollapseAbilityContainerButton.gameObject.SetActive(!isCollapsed);
	}

	private void ExpandAbilityContainer()
	{
		UpdateAbilityContainerHeight(GetAbilityContainerHeight(), immediate: false);
		SwitchButtons(isCollapsed: false);
		m_AbilityCanvasGroup.alpha = 1f;
		m_AbilityCanvasGroup.blocksRaycasts = true;
	}

	private void CollapseAbilityContainer()
	{
		UpdateAbilityContainerHeight(m_DefaultAbilityContainerHeight, immediate: false);
		SwitchButtons(isCollapsed: true);
		m_AbilityCanvasGroup.alpha = 0f;
		m_AbilityCanvasGroup.blocksRaycasts = false;
	}

	private void UpdateAbilityContainerHeight(float targetHeight, bool immediate)
	{
		if (immediate)
		{
			m_MainRectTransform.sizeDelta = new Vector2(m_MainRectTransform.sizeDelta.x, targetHeight);
		}
		else
		{
			m_MainRectTransform.DOSizeDelta(new Vector2(m_MainRectTransform.sizeDelta.x, targetHeight), 0.2f).SetEase(Ease.OutCubic).SetUpdate(isIndependentUpdate: true)
				.SetAutoKill(autoKillOnCompletion: true);
		}
	}

	private float GetAbilityContainerHeight()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_AbilityContainerRectTransform);
		float num = 0f;
		int num2 = 0;
		foreach (RectTransform item in m_AbilityContainerRectTransform)
		{
			if (item.gameObject.activeSelf)
			{
				num += LayoutUtility.GetPreferredHeight(item);
				num2++;
			}
		}
		if (num2 > 1)
		{
			num += (float)(num2 - 1) * m_AbilityLayout.spacing;
		}
		return num + (float)(m_AbilityLayout.padding.top + m_AbilityLayout.padding.bottom);
	}
}
