using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenAppearanceAnchorBarView : View<ObservableList<VirtualListElementVMBase>>
{
	[Header("Layout")]
	[SerializeField]
	private WidgetList m_ButtonsList;

	[SerializeField]
	private CharGenAppearanceAnchorButtonView m_ButtonPrefab;

	[Header("Scroll Target")]
	[SerializeField]
	private VirtualListVertical m_VirtualList;

	[SerializeField]
	private RectTransform m_VirtualListContent;

	[Header("Anchors")]
	[SerializeField]
	private List<CharGenAppearanceAnchorEntry> m_Anchors = new List<CharGenAppearanceAnchorEntry>();

	[Header("Scroll Behaviour")]
	[SerializeField]
	private bool m_SmoothScroll = true;

	[SerializeField]
	private float m_ScrollDuration = 0.3f;

	[SerializeField]
	private Ease m_ScrollEase = Ease.OutCubic;

	[SerializeField]
	[Tooltip("Units per second passed to ScrollController.ScrollTowards while the target VM is not yet laid out by virtualization.")]
	private float m_PendingNudgeSpeed = 5000f;

	private const float ActiveAnchorEpsilon = 0.5f;

	private readonly Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM> m_ComponentByType = new Dictionary<CharGenAppearancePageComponent, BaseCharGenAppearancePageComponentVM>();

	private readonly Dictionary<IVirtualListElementData, VirtualListElement> m_ElementByData = new Dictionary<IVirtualListElementData, VirtualListElement>();

	private ReactiveCommand<CharGenAppearancePageType> m_ActiveAnchorChanged;

	private Tweener m_ScrollTweener;

	private int m_ActiveIndex = -1;

	private bool m_HasPendingScroll;

	private CharGenAppearancePageType m_PendingScrollTo;

	private bool m_PendingScrollSmooth;

	private bool m_DetectionLocked;

	private float m_LastObservedScrollY;

	public Observable<CharGenAppearancePageType> ActiveAnchorChanged => m_ActiveAnchorChanged;

	protected override void OnBind()
	{
		m_ActiveAnchorChanged = new ReactiveCommand<CharGenAppearancePageType>().AddTo(this);
		DrawButtons();
		RebuildCaches();
		base.ViewModel.ObserveAdd().Subscribe(delegate
		{
			RebuildCaches();
		}).AddTo(this);
		base.ViewModel.ObserveRemove().Subscribe(delegate
		{
			RebuildCaches();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ObserveReset(), delegate
		{
			RebuildCaches();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_ScrollTweener?.Kill();
		m_ScrollTweener = null;
		m_ComponentByType.Clear();
		m_ElementByData.Clear();
		m_HasPendingScroll = false;
		m_ActiveIndex = -1;
		m_DetectionLocked = false;
	}

	public void SetActivePage(CharGenAppearancePageType pageType)
	{
		if (TryGetIndex(pageType, out var index))
		{
			ScrollToAnchor(index, m_SmoothScroll);
			SetActiveIndexInternal(index);
			LockDetectionUntilManualScroll();
		}
	}

	public bool IsActivePage(CharGenAppearancePageType pageType)
	{
		if (m_ActiveIndex >= 0 && m_ActiveIndex < m_Anchors.Count)
		{
			return m_Anchors[m_ActiveIndex].PageType == pageType;
		}
		return false;
	}

	public bool TryGetIndex(CharGenAppearancePageType pageType, out int index)
	{
		for (int i = 0; i < m_Anchors.Count; i++)
		{
			if (m_Anchors[i].PageType == pageType)
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}

	private void DrawButtons()
	{
		if (m_ButtonsList == null || m_ButtonPrefab == null)
		{
			return;
		}
		m_ButtonsList.DrawEntries(m_Anchors, m_ButtonPrefab).AddTo(this);
		for (int i = 0; i < m_ButtonsList.Entries.Count; i++)
		{
			if (m_ButtonsList.Entries[i] is CharGenAppearanceAnchorButtonView charGenAppearanceAnchorButtonView)
			{
				int captured = i;
				ObservableSubscribeExtensions.Subscribe(charGenAppearanceAnchorButtonView.Clicked, delegate
				{
					OnButtonClicked(captured);
				}).AddTo(this);
				charGenAppearanceAnchorButtonView.SetSelected(value: false);
			}
		}
	}

	private void RebuildCaches()
	{
		m_ComponentByType.Clear();
		m_ElementByData.Clear();
		if (base.ViewModel != null)
		{
			for (int i = 0; i < base.ViewModel.Count; i++)
			{
				if (base.ViewModel[i] is BaseCharGenAppearancePageComponentVM baseCharGenAppearancePageComponentVM)
				{
					m_ComponentByType[baseCharGenAppearancePageComponentVM.Type] = baseCharGenAppearancePageComponentVM;
				}
			}
		}
		if (m_VirtualList?.Elements == null)
		{
			return;
		}
		List<VirtualListElement> elements = m_VirtualList.Elements;
		for (int j = 0; j < elements.Count; j++)
		{
			VirtualListElement virtualListElement = elements[j];
			if (virtualListElement?.Data != null)
			{
				m_ElementByData[virtualListElement.Data] = virtualListElement;
			}
		}
	}

	private void OnButtonClicked(int index)
	{
		if (index >= 0 && index < m_Anchors.Count)
		{
			ScrollToAnchor(index, m_SmoothScroll);
			SetActiveIndexInternal(index);
			LockDetectionUntilManualScroll();
		}
	}

	private void LockDetectionUntilManualScroll()
	{
		m_DetectionLocked = true;
		if (m_VirtualListContent != null)
		{
			m_LastObservedScrollY = m_VirtualListContent.anchoredPosition.y;
		}
	}

	private void ScrollToAnchor(int index, bool smooth)
	{
		if (base.ViewModel == null || m_VirtualList == null || m_VirtualListContent == null || index < 0 || index >= m_Anchors.Count)
		{
			return;
		}
		CharGenAppearanceAnchorEntry charGenAppearanceAnchorEntry = m_Anchors[index];
		if (!TryResolveComponentVM(charGenAppearanceAnchorEntry.ScrollTo, out var vm))
		{
			m_HasPendingScroll = true;
			m_PendingScrollTo = charGenAppearanceAnchorEntry.PageType;
			m_PendingScrollSmooth = smooth;
			return;
		}
		VirtualListElement virtualListElement = FindElement(vm);
		if (virtualListElement == null || !virtualListElement.WasUpdatedAtLeastOnes)
		{
			m_HasPendingScroll = true;
			m_PendingScrollTo = charGenAppearanceAnchorEntry.PageType;
			m_PendingScrollSmooth = smooth;
			return;
		}
		m_HasPendingScroll = false;
		float num = ComputeAnchorTarget(index) - m_VirtualListContent.anchoredPosition.y;
		m_ScrollTweener?.Kill();
		m_ScrollTweener = null;
		if (Mathf.Abs(num) <= 0.5f)
		{
			return;
		}
		if (!smooth)
		{
			m_VirtualList.ScrollController.Scroll(num);
			return;
		}
		float scrolled = 0f;
		m_ScrollTweener = DOTween.To(() => scrolled, delegate(float progress)
		{
			m_VirtualList.ScrollController.Scroll(progress - scrolled);
			scrolled = progress;
		}, num, m_ScrollDuration).SetUpdate(isIndependentUpdate: true).SetEase(m_ScrollEase);
	}

	private void TryRunPendingScroll()
	{
		if (!m_HasPendingScroll || !TryGetIndex(m_PendingScrollTo, out var index))
		{
			return;
		}
		CharGenAppearanceAnchorEntry charGenAppearanceAnchorEntry = m_Anchors[index];
		if (TryResolveComponentVM(charGenAppearanceAnchorEntry.ScrollTo, out var vm))
		{
			VirtualListElement virtualListElement = FindElement(vm);
			if (virtualListElement != null && virtualListElement.WasUpdatedAtLeastOnes)
			{
				ScrollToAnchor(index, m_PendingScrollSmooth);
			}
			else if (m_VirtualList?.ScrollController != null)
			{
				m_VirtualList.ScrollController.ScrollTowards(vm, m_PendingNudgeSpeed);
			}
		}
	}

	private bool TryResolveComponentVM(CharGenAppearancePageComponent componentType, out BaseCharGenAppearancePageComponentVM vm)
	{
		if (m_ComponentByType.TryGetValue(componentType, out vm))
		{
			return vm != null;
		}
		return false;
	}

	private VirtualListElement FindElement(IVirtualListElementData data)
	{
		if (data == null)
		{
			return null;
		}
		if (m_ElementByData.TryGetValue(data, out var value) && value != null)
		{
			return value;
		}
		if (m_VirtualList?.Elements == null)
		{
			return null;
		}
		List<VirtualListElement> elements = m_VirtualList.Elements;
		for (int i = 0; i < elements.Count; i++)
		{
			if (elements[i].Data == data)
			{
				m_ElementByData[data] = elements[i];
				return elements[i];
			}
		}
		return null;
	}

	private void SetActiveIndexInternal(int newIndex)
	{
		if (newIndex != m_ActiveIndex)
		{
			SetButtonSelected(m_ActiveIndex, value: false);
			m_ActiveIndex = newIndex;
			SetButtonSelected(m_ActiveIndex, value: true);
			if (m_ActiveIndex >= 0 && m_ActiveIndex < m_Anchors.Count)
			{
				m_ActiveAnchorChanged.Execute(m_Anchors[m_ActiveIndex].PageType);
			}
		}
	}

	private void SetButtonSelected(int index, bool value)
	{
		if (!(m_ButtonsList == null) && index >= 0 && index < m_ButtonsList.Entries.Count && m_ButtonsList.Entries[index] is CharGenAppearanceAnchorButtonView charGenAppearanceAnchorButtonView)
		{
			charGenAppearanceAnchorButtonView.SetSelected(value);
		}
	}

	private void LateUpdate()
	{
		if (base.ViewModel == null || m_VirtualList == null || m_VirtualListContent == null)
		{
			return;
		}
		if (m_HasPendingScroll)
		{
			TryRunPendingScroll();
			m_LastObservedScrollY = m_VirtualListContent.anchoredPosition.y;
			if (m_HasPendingScroll)
			{
				return;
			}
		}
		TryUnlockDetectionOnManualScroll();
		if (!IsScrollTweening() && !m_DetectionLocked)
		{
			UpdateActiveFromScroll();
		}
	}

	private void TryUnlockDetectionOnManualScroll()
	{
		float y = m_VirtualListContent.anchoredPosition.y;
		if (m_DetectionLocked && !IsScrollTweening() && Mathf.Abs(y - m_LastObservedScrollY) > 0.5f)
		{
			m_DetectionLocked = false;
		}
		m_LastObservedScrollY = y;
	}

	private bool IsScrollTweening()
	{
		if (m_ScrollTweener != null && m_ScrollTweener.IsActive())
		{
			return m_ScrollTweener.IsPlaying();
		}
		return false;
	}

	private void UpdateActiveFromScroll()
	{
		float y = m_VirtualListContent.anchoredPosition.y;
		int num = -1;
		int num2 = -1;
		float start;
		for (int i = 0; i < m_Anchors.Count; i++)
		{
			if (TryResolveAnchorStart(i, out start))
			{
				if (num < 0)
				{
					num = i;
				}
				num2 = i;
			}
		}
		if (num < 0)
		{
			return;
		}
		int num3 = num;
		for (int j = num; j <= num2; j++)
		{
			if (TryResolveAnchorStart(j, out start))
			{
				if (!(ComputeAnchorTarget(j) <= y + 0.5f))
				{
					break;
				}
				num3 = j;
			}
		}
		if (num2 > num3 && IsLastVMVisibleFully() && ComputeAnchorTarget(num2) > y + 0.5f)
		{
			num3 = num2;
		}
		if (num3 != m_ActiveIndex)
		{
			SetActiveIndexInternal(num3);
		}
	}

	private float ComputeAnchorTarget(int index)
	{
		if (!TryResolveAnchorStart(index, out var start))
		{
			return 0f;
		}
		return Mathf.Max(0f, start - GetTopPaddingFromFirstAnchor());
	}

	private float GetTopPaddingFromFirstAnchor()
	{
		if (m_Anchors.Count > 0 && TryResolveAnchorStart(0, out var start))
		{
			return start;
		}
		return 0f;
	}

	private bool IsLastVMVisibleFully()
	{
		if (base.ViewModel == null || base.ViewModel.Count == 0)
		{
			return false;
		}
		if (m_VirtualList?.VisibleElements == null || m_VirtualListContent == null)
		{
			return false;
		}
		IVirtualListElementData virtualListElementData = base.ViewModel[base.ViewModel.Count - 1];
		if (virtualListElementData == null)
		{
			return false;
		}
		List<VirtualListElement> visibleElements = m_VirtualList.VisibleElements;
		for (int i = 0; i < visibleElements.Count; i++)
		{
			VirtualListElement virtualListElement = visibleElements[i];
			if (virtualListElement != null && virtualListElement.Data == virtualListElementData && virtualListElement.WasUpdatedAtLeastOnes)
			{
				float num = m_VirtualListContent.anchoredPosition.y + GetViewportHeight();
				return 0f - virtualListElement.OffsetMin.y <= num + 1f;
			}
		}
		return false;
	}

	private bool TryResolveAnchorStart(int index, out float start)
	{
		start = 0f;
		if (index < 0 || index >= m_Anchors.Count)
		{
			return false;
		}
		if (!TryResolveComponentVM(m_Anchors[index].ScrollTo, out var vm))
		{
			return false;
		}
		VirtualListElement virtualListElement = FindElement(vm);
		if (virtualListElement == null || !virtualListElement.WasUpdatedAtLeastOnes)
		{
			return false;
		}
		start = 0f - virtualListElement.OffsetMax.y;
		return true;
	}

	private float GetViewportHeight()
	{
		if (m_VirtualListContent != null && m_VirtualListContent.parent is RectTransform { rect: var rect })
		{
			return rect.height;
		}
		return 0f;
	}
}
