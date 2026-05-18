using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.UI.Common;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class TurnVirtualListController : MonoBehaviour
{
	private enum VirtualListDirection
	{
		None,
		Vertical,
		Horizontal
	}

	[SerializeField]
	private VirtualListDirection m_VirtualDirection;

	[Header("Content paddings")]
	[SerializeField]
	public RectOffset Padding;

	[SerializeField]
	public Vector2 Spacing;

	[Header("Scroll")]
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private float m_AutoScrollDelta = 1.25f;

	[SerializeField]
	private bool m_RecycleOnlyOutsideViewport;

	private ScrollBasePosition m_ForceScrollPosition;

	[Header("Item prefab")]
	public int ElementsInRow = 1;

	private IReadOnlyList<ITurnVirtualItemData> m_DataList;

	private readonly Dictionary<ViewModel, int> m_ViewModelToDataListItem = new Dictionary<ViewModel, int>();

	private ITurnVirtualItemView m_Prefab;

	private bool m_IsListInit;

	private readonly List<ITurnVirtualItemView> m_VisibleItems = new List<ITurnVirtualItemView>();

	private int m_FirstItemIndex = -1;

	private int m_LastItemIndex = -1;

	private readonly Queue<ITurnVirtualItemView> m_ItemsPool = new Queue<ITurnVirtualItemView>();

	private Tween m_ScrollTweener;

	[SerializeField]
	[UsedImplicitly]
	public float m_AnimationTime = 0.2f;

	public Action OnUpdatedCallback;

	private bool m_ExternalUpdateLock;

	private Sequence m_AnimationSequence;

	private ITurnVirtualItemView m_LastSelectedView;

	private ViewModel m_LastSelectedVM;

	private int m_LastSelectedIndex;

	public ScrollRectExtended ScrollRect => m_ScrollRect;

	public RectTransform Content => m_ScrollRect.content;

	private float ContentPositionY => Content.anchoredPosition.y;

	private float ScrollRectHeight => m_ScrollRect.viewport.rect.height;

	private float ContentPositionX => Content.anchoredPosition.x;

	private float ScrollRectWidth => m_ScrollRect.viewport.rect.width;

	private float m_CalculatedAnimationTime => m_AnimationTime / m_AnimationTimeDevider;

	private float m_AnimationTimeDevider => Game.Instance.Player.UISettings.TimeScaleAverage;

	public bool IsAnimating
	{
		get
		{
			if (m_AnimationSequence != null)
			{
				return m_AnimationSequence.IsPlaying();
			}
			return false;
		}
	}

	public void Initialize(ITurnVirtualItemView item)
	{
		if (!m_IsListInit)
		{
			m_Prefab = item;
			Content.pivot = new Vector2(0f, 1f);
			if (m_ScrollRect != null)
			{
				m_ScrollRect.viewport.pivot = new Vector2(0f, 1f);
			}
			m_ScrollRect.onValueChanged.AddListener(delegate(Vector2 data)
			{
				Scroll(data, fromMouse: true);
			});
			CleanList();
			m_IsListInit = true;
		}
	}

	public void CleanList()
	{
		m_FirstItemIndex = -1;
		m_LastItemIndex = -1;
		List<ITurnVirtualItemView> list = new List<ITurnVirtualItemView>();
		list.AddRange(m_VisibleItems);
		foreach (ITurnVirtualItemView item in list)
		{
			ReleaseItem(item);
		}
		m_VisibleItems.Clear();
		m_DataList = null;
	}

	public void UpdateData(IReadOnlyList<ITurnVirtualItemData> newDataList, Sequence containerTweener, ScrollBasePosition forceScroll = ScrollBasePosition.None, Action callback = null)
	{
		m_ExternalUpdateLock = true;
		OnUpdatedCallback = callback;
		ScrollBasePosition forceScrollPosition = ((forceScroll == ScrollBasePosition.None) ? m_ForceScrollPosition : forceScroll);
		m_ForceScrollPosition = forceScrollPosition;
		Vector2 newContentSize = GetContentSize(newDataList);
		if (m_AnimationSequence != null)
		{
			m_AnimationSequence.Kill();
			m_AnimationSequence = null;
		}
		m_AnimationSequence = GetAnimationSequence(newDataList, newContentSize, containerTweener);
		m_DataList = newDataList;
		m_ViewModelToDataListItem.Clear();
		for (int i = 0; i < newDataList.Count; i++)
		{
			m_ViewModelToDataListItem.Add(newDataList[i].ViewModel, i);
		}
		if (m_AnimationSequence == null)
		{
			UpdateView();
			return;
		}
		m_AnimationSequence.Play().SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			UpdateContentSize(newContentSize);
			UpdateView();
		});
	}

	private void UpdateView()
	{
		m_AnimationSequence.Kill();
		m_AnimationSequence = null;
		Vector2 contentSize = GetContentSize(m_DataList);
		UpdateContentSize(contentSize);
		UpdateScrollPosition();
		UpdateInternal();
		if (OnUpdatedCallback != null)
		{
			OnUpdatedCallback();
			OnUpdatedCallback = null;
		}
		m_ExternalUpdateLock = false;
	}

	public Vector2 GetContentSize(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		if (dataList == null || dataList.Count < 1)
		{
			return default(Vector2);
		}
		ITurnVirtualItemData turnVirtualItemData = dataList[dataList.Count - 1];
		float y = (turnVirtualItemData.VirtualPosition + turnVirtualItemData.VirtualSize).y + (float)Padding.top + (float)Padding.bottom + Spacing.x * (float)(dataList.Count - 1);
		return new Vector2((turnVirtualItemData.VirtualPosition + turnVirtualItemData.VirtualSize).x + (float)Padding.left + (float)Padding.right + Spacing.y * (float)(dataList.Count - 1), y);
	}

	protected void UpdateContentSize(Vector2 currentContentSize)
	{
		if (m_IsListInit)
		{
			Content.sizeDelta = currentContentSize;
		}
	}

	private void UpdateScrollPosition()
	{
		if ((bool)m_ScrollRect)
		{
			switch (m_ForceScrollPosition)
			{
			case ScrollBasePosition.None:
				break;
			case ScrollBasePosition.End:
				ScrollToEnd();
				break;
			case ScrollBasePosition.Start:
				ScrollToStart();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void ScrollToStart()
	{
		if (m_VirtualDirection == VirtualListDirection.Vertical)
		{
			m_ScrollRect.ScrollToBottom();
		}
		else
		{
			m_ScrollRect.ScrollToLeft();
		}
	}

	private void ScrollToEnd()
	{
		if (m_VirtualDirection == VirtualListDirection.Vertical)
		{
			m_ScrollRect.ScrollToTop();
		}
		else
		{
			m_ScrollRect.ScrollToRight();
		}
	}

	private Sequence GetContentSizeTweenSequence(Vector2 newContentSize)
	{
		Sequence sequence = null;
		if (!newContentSize.Equals(Content.sizeDelta))
		{
			sequence = DOTween.Sequence().Pause();
			Tweener t = Content.DOSizeDelta(newContentSize, m_CalculatedAnimationTime).Pause().OnUpdate(UpdateScrollPosition);
			sequence.Join(t);
		}
		return sequence;
	}

	private Sequence GetAnimationSequence(IReadOnlyList<ITurnVirtualItemData> newDataList, Vector2 newContentSize, Sequence containerTweener)
	{
		Sequence sequence = DOTween.Sequence().Pause();
		if (m_DataList == null)
		{
			return sequence.Join(containerTweener);
		}
		UpdateVisibleRange(newDataList);
		List<ITurnVirtualItemData> itemsToHide = GetItemsToHide(newDataList);
		Dictionary<ITurnVirtualItemData, Vector2> itemsToMove = GetItemsToMove(newDataList);
		List<ITurnVirtualItemData> list = GetItemsToShow(newDataList).ToList();
		if (!itemsToHide.Any() && !itemsToMove.Any() && !list.Any())
		{
			return sequence.Join(containerTweener);
		}
		if (itemsToHide.Any())
		{
			Sequence sequence2 = DOTween.Sequence();
			foreach (ITurnVirtualItemData item in itemsToHide)
			{
				Tween hideAnimation = item.BoundView.GetHideAnimation(delegate
				{
					ReleaseItem(item.BoundView);
				});
				sequence2.Join(hideAnimation);
			}
			sequence.Append(sequence2);
		}
		if (itemsToMove.Any())
		{
			Sequence sequence3 = DOTween.Sequence().Pause().OnStart(delegate
			{
			});
			foreach (KeyValuePair<ITurnVirtualItemData, Vector2> item2 in itemsToMove)
			{
				Vector2 targetPosition = new Vector2(item2.Value.x + (float)Padding.left, 0f - (item2.Value.y + (float)Padding.top));
				Tween moveAnimation = item2.Key.BoundView.GetMoveAnimation(null, targetPosition);
				sequence3.Join(moveAnimation);
			}
			sequence.Append(sequence3);
		}
		Sequence contentSizeTweenSequence = GetContentSizeTweenSequence(newContentSize);
		if (contentSizeTweenSequence != null)
		{
			sequence.Join(contentSizeTweenSequence);
		}
		sequence.Join(containerTweener);
		if (list.Any())
		{
			Sequence sequence4 = DOTween.Sequence().Pause().OnStart(delegate
			{
			});
			foreach (ITurnVirtualItemData item3 in list)
			{
				ITurnVirtualItemView turnVirtualItemView = ClaimItemView();
				SetupItemView(turnVirtualItemView, item3);
				turnVirtualItemView.Selectable?.SetFocus(value: false);
				turnVirtualItemView.CanvasGroup.alpha = 0f;
				Tween showAnimation = turnVirtualItemView.GetShowAnimation(null, new Vector2(item3.VirtualPosition.x + (float)Padding.left, 0f - (item3.VirtualPosition.y + (float)Padding.top)));
				sequence4.Join(showAnimation);
			}
			sequence.Append(sequence4);
		}
		return sequence;
	}

	private List<ITurnVirtualItemData> GetItemsToHide(IReadOnlyList<ITurnVirtualItemData> newDataList)
	{
		return m_DataList.Where((ITurnVirtualItemData itemData) => itemData.BoundView != null && !newDataList.Any((ITurnVirtualItemData newItemData) => newItemData.ViewModel == itemData.ViewModel)).ToList();
	}

	private Dictionary<ITurnVirtualItemData, Vector2> GetItemsToMove(IReadOnlyList<ITurnVirtualItemData> newDataList)
	{
		Dictionary<ITurnVirtualItemData, Vector2> dictionary = new Dictionary<ITurnVirtualItemData, Vector2>();
		foreach (ITurnVirtualItemData itemData in m_DataList)
		{
			if (itemData.BoundView != null)
			{
				ITurnVirtualItemData turnVirtualItemData = newDataList.FirstOrDefault((ITurnVirtualItemData item) => item.ViewModel == itemData.ViewModel);
				if (turnVirtualItemData != null && !itemData.VirtualPosition.Equals(turnVirtualItemData.VirtualPosition))
				{
					dictionary[itemData] = turnVirtualItemData.VirtualPosition;
				}
			}
		}
		return dictionary;
	}

	private List<ITurnVirtualItemData> GetItemsToShow(IReadOnlyList<ITurnVirtualItemData> newDataList)
	{
		return newDataList.Where((ITurnVirtualItemData itemData) => IsVisible(itemData, newDataList) && m_DataList.FirstOrDefault((ITurnVirtualItemData newItemData) => newItemData.ViewModel == itemData.ViewModel) == null).ToList();
	}

	public bool IsVisible(ITurnVirtualItemData item, IReadOnlyList<ITurnVirtualItemData> dataList = null)
	{
		int num = -100;
		IReadOnlyList<ITurnVirtualItemData> readOnlyList = dataList ?? m_DataList;
		if (readOnlyList != null)
		{
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				if (readOnlyList[i] == item)
				{
					num = i;
					break;
				}
			}
		}
		if (m_FirstItemIndex <= num)
		{
			return num <= m_LastItemIndex;
		}
		return false;
	}

	private void UpdateVisibleRange(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		m_FirstItemIndex = UpdateFirstItemIndex(dataList);
		m_LastItemIndex = UpdateLastItemIndex(dataList);
	}

	private void UpdateInternal()
	{
		if (m_DataList == null)
		{
			return;
		}
		UpdateVisibleRange(m_DataList);
		if (m_FirstItemIndex < 0 || m_LastItemIndex < 0)
		{
			return;
		}
		List<ITurnVirtualItemData> list = new List<ITurnVirtualItemData>();
		for (int i = m_FirstItemIndex; i <= m_LastItemIndex; i++)
		{
			list.Add(m_DataList[i]);
		}
		Queue<ITurnVirtualItemView> queue = new Queue<ITurnVirtualItemView>();
		foreach (ITurnVirtualItemView visibleItem in m_VisibleItems)
		{
			ITurnVirtualItemData turnVirtualItemData = list.FirstOrDefault((ITurnVirtualItemData data) => data.ViewModel == visibleItem.GetViewModel());
			if (turnVirtualItemData != null)
			{
				if (turnVirtualItemData.BoundView == null)
				{
					SetupItemView(visibleItem, turnVirtualItemData);
				}
				else if (m_AnimationSequence == null)
				{
					visibleItem.SetAnchoredPosition(new Vector2(turnVirtualItemData.VirtualPosition.x + (float)Padding.left, 0f - (turnVirtualItemData.VirtualPosition.y + (float)Padding.top)));
				}
				list.Remove(turnVirtualItemData);
			}
			else
			{
				queue.Enqueue(visibleItem);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (j < queue.Count)
			{
				ITurnVirtualItemView turnVirtualItemView = queue.Dequeue();
				turnVirtualItemView.WillBeReused = true;
				SetupItemView(turnVirtualItemView, list[j]);
				turnVirtualItemView.WillBeReused = false;
			}
			else
			{
				ITurnVirtualItemView view = ClaimItemView();
				SetupItemView(view, list[j]);
			}
		}
		foreach (ITurnVirtualItemView item in queue)
		{
			ReleaseItem(item);
		}
		UpdateSiblingIndexes();
	}

	private void UpdateSiblingIndexes()
	{
		for (int i = 0; i < m_DataList.Count; i++)
		{
			ITurnVirtualItemData turnVirtualItemData = m_DataList[i];
			if (turnVirtualItemData.BoundView != null)
			{
				turnVirtualItemData.BoundView.RectTransform.transform.SetAsLastSibling();
				turnVirtualItemData.BoundView.RectTransform.gameObject.name = $"{m_Prefab.RectTransform.name} {i}";
			}
		}
	}

	private int UpdateLastItemIndex(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		if (dataList == null || dataList.Count == 0)
		{
			return -1;
		}
		if (!m_ScrollRect)
		{
			return dataList.Count - 1;
		}
		return m_VirtualDirection switch
		{
			VirtualListDirection.Vertical => GetTopItemIndexInVertical(dataList), 
			VirtualListDirection.Horizontal => GetRightItemIndexInHorizontal(dataList), 
			_ => dataList.Count - 1, 
		};
	}

	private int GetTopItemIndexInVertical(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		float scrollRectHeight = ScrollRectHeight;
		for (int num = dataList.Count - 1; num >= 0; num--)
		{
			ITurnVirtualItemData item = dataList[num];
			float num2 = (m_RecycleOnlyOutsideViewport ? GetItemBottomBorder(item) : GetItemTopBorder(item));
			if (Mathf.Abs(num2 - scrollRectHeight) < float.Epsilon || num2 < scrollRectHeight)
			{
				return num;
			}
		}
		return dataList.Count - 1;
	}

	private int GetRightItemIndexInHorizontal(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		float scrollRectWidth = ScrollRectWidth;
		for (int num = dataList.Count - 1; num >= 0; num--)
		{
			ITurnVirtualItemData item = dataList[num];
			float num2 = (m_RecycleOnlyOutsideViewport ? GetItemLeftBorder(item) : GetItemRightBorder(item));
			if (Mathf.Abs(num2 - scrollRectWidth) < float.Epsilon || num2 < scrollRectWidth)
			{
				return num;
			}
		}
		return dataList.Count - 1;
	}

	private int UpdateFirstItemIndex(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		if (!m_ScrollRect || dataList == null || dataList.Count == 0)
		{
			return -1;
		}
		return m_VirtualDirection switch
		{
			VirtualListDirection.Vertical => GetBottomItemIndexVertical(dataList), 
			VirtualListDirection.Horizontal => GetLeftItemIndexHorizontal(dataList), 
			_ => -1, 
		};
	}

	private int GetBottomItemIndexVertical(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		float num = 0f;
		for (int i = 0; i < dataList.Count; i++)
		{
			ITurnVirtualItemData item = dataList[i];
			float num2 = (m_RecycleOnlyOutsideViewport ? GetItemTopBorder(item) : GetItemBottomBorder(item));
			if (Mathf.Abs(num2 - num) < float.Epsilon || num2 > num)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetLeftItemIndexHorizontal(IReadOnlyList<ITurnVirtualItemData> dataList)
	{
		float num = 0f;
		for (int i = 0; i < dataList.Count; i++)
		{
			ITurnVirtualItemData item = dataList[i];
			float num2 = (m_RecycleOnlyOutsideViewport ? GetItemRightBorder(item) : GetItemLeftBorder(item));
			if (Mathf.Abs(num2 - num) < float.Epsilon || num2 > num)
			{
				return i;
			}
		}
		return 0;
	}

	private float GetItemLeftBorder(ITurnVirtualItemData item)
	{
		return ContentPositionX + item.VirtualPosition.x + Spacing.x;
	}

	private float GetItemRightBorder(ITurnVirtualItemData item)
	{
		return ContentPositionX + item.VirtualPosition.x + item.VirtualSize.x;
	}

	private float GetItemTopBorder(ITurnVirtualItemData item)
	{
		return ContentPositionY + item.VirtualPosition.y + item.VirtualSize.y;
	}

	private float GetItemBottomBorder(ITurnVirtualItemData item)
	{
		return ContentPositionY + item.VirtualPosition.y + Spacing.y;
	}

	private ITurnVirtualItemView ClaimItemView()
	{
		ITurnVirtualItemView turnVirtualItemView = ((m_ItemsPool.Count > 0) ? m_ItemsPool.Dequeue() : CreateItemView());
		turnVirtualItemView.View.gameObject.SetActive(value: true);
		turnVirtualItemView.RectTransform.pivot = new Vector2(0f, 1f);
		turnVirtualItemView.CanvasGroup.alpha = 1f;
		m_VisibleItems.Add(turnVirtualItemView);
		return turnVirtualItemView;
	}

	private ITurnVirtualItemView CreateItemView()
	{
		return UnityEngine.Object.Instantiate(m_Prefab.View, Content, worldPositionStays: false) as ITurnVirtualItemView;
	}

	private void SetupItemView(ITurnVirtualItemView view, ITurnVirtualItemData data)
	{
		view.ViewBind(data);
		data.BoundView = view;
		view.SetAnchoredPosition(new Vector2(data.VirtualPosition.x + (float)Padding.left, 0f - (data.VirtualPosition.y + (float)Padding.top)));
		view.Selectable?.SetFocus(value: false);
		view.CanvasGroup.alpha = 1f;
	}

	private void ReleaseItem(ITurnVirtualItemView view)
	{
		if (m_ItemsPool.Contains(view))
		{
			PFLog.UI.Warning("Attempt to realese ITurnVirtualItemView twice");
			return;
		}
		view.DestroyViewItem();
		view.RectTransform.anchoredPosition = new Vector2(view.RectTransform.anchoredPosition.x - 100000f, 100000f);
		view.Selectable?.SetFocus(value: false);
		view.View.gameObject.SetActive(value: false);
		m_VisibleItems.Remove(view);
		m_ItemsPool.Enqueue(view);
	}

	public void ScrollTo(ITurnVirtualItemData virtualData, Action onComplete = null)
	{
		if (!m_ExternalUpdateLock && m_DataList.Contains(virtualData) && !(Content == null))
		{
			m_ScrollTweener?.Kill();
			m_ForceScrollPosition = ScrollBasePosition.None;
			m_ScrollTweener = m_VirtualDirection switch
			{
				VirtualListDirection.Vertical => GetScrollTweenVertical(virtualData.VirtualPosition.y, 0.2f), 
				VirtualListDirection.Horizontal => GetScrollTweenHorizontal(virtualData.VirtualPosition.x, 0.2f), 
				_ => null, 
			};
			m_ScrollTweener?.OnComplete(delegate
			{
				onComplete?.Invoke();
			});
		}
	}

	private Tween GetScrollTweenVertical(float itemPosition, float duration)
	{
		float endValue = Mathf.InverseLerp(0f, ScrollRectHeight, itemPosition);
		return DOTween.To(() => m_ScrollRect.verticalNormalizedPosition, delegate(float x)
		{
			m_ScrollRect.verticalNormalizedPosition = x;
		}, endValue, duration).SetUpdate(isIndependentUpdate: true).OnUpdate(UpdateView);
	}

	private Tween GetScrollTweenHorizontal(float itemPosition, float duration)
	{
		float endValue = Mathf.InverseLerp(0f, ScrollRectWidth, itemPosition);
		return DOTween.To(() => m_ScrollRect.horizontalNormalizedPosition, delegate(float x)
		{
			m_ScrollRect.horizontalNormalizedPosition = x;
		}, endValue, duration).SetUpdate(isIndependentUpdate: true).OnUpdate(UpdateView);
	}

	public void ScrollToSmoothly(ScrollBasePosition scrollPosition)
	{
		float verticalNormalizedPosition = m_ScrollRect.verticalNormalizedPosition;
		m_ScrollTweener = DOTween.To(endValue: scrollPosition switch
		{
			ScrollBasePosition.End => 1f, 
			ScrollBasePosition.Start => 0f, 
			_ => throw new ArgumentOutOfRangeException("scrollPosition", scrollPosition, null), 
		}, getter: () => m_ScrollRect.verticalNormalizedPosition, setter: delegate(float x)
		{
			m_ScrollRect.verticalNormalizedPosition = x;
		}, duration: 0.2f).SetUpdate(isIndependentUpdate: true).OnUpdate(delegate
		{
			UpdateView();
		})
			.OnComplete(delegate
			{
				UpdateView();
			});
	}

	public void ScrollTo(ScrollBasePosition scrollPosition)
	{
		if (!m_ExternalUpdateLock)
		{
			m_ForceScrollPosition = scrollPosition;
			UpdateView();
		}
	}

	public void Scroll(Vector2 data, bool fromMouse = false)
	{
		if (m_ExternalUpdateLock)
		{
			return;
		}
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		if (m_ScrollRect == null)
		{
			return;
		}
		if (!fromMouse)
		{
			float scrollSensitivity = m_ScrollRect.scrollSensitivity;
			switch (m_VirtualDirection)
			{
			case VirtualListDirection.None:
				pointerEventData.scrollDelta = new Vector2(data.x * scrollSensitivity, data.y * scrollSensitivity);
				break;
			case VirtualListDirection.Vertical:
				pointerEventData.scrollDelta = new Vector2(0f, data.y * scrollSensitivity);
				break;
			case VirtualListDirection.Horizontal:
				pointerEventData.scrollDelta = new Vector2(data.x * scrollSensitivity, 0f);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			m_ScrollRect.OnScroll(pointerEventData);
		}
		m_ForceScrollPosition = ScrollBasePosition.None;
		UpdateView();
	}

	public void EntitySelected(IConsoleNavigationEntity entity)
	{
		if (entity is ITurnVirtualItemView turnVirtualItemView && (bool)m_ScrollRect && m_LastSelectedView != entity && m_ViewModelToDataListItem.TryGetValue(turnVirtualItemView.GetViewModel(), out var value))
		{
			m_LastSelectedIndex = value;
			if (m_LastSelectedIndex >= 0)
			{
				m_LastSelectedView = turnVirtualItemView;
				m_LastSelectedVM = turnVirtualItemView.GetViewModel();
			}
			else
			{
				m_LastSelectedVM = null;
			}
			if (m_ScrollRect.EnsureVisibleVertical(turnVirtualItemView.RectTransform, turnVirtualItemView.RectTransform.rect.height * m_AutoScrollDelta))
			{
				m_ForceScrollPosition = ScrollBasePosition.None;
				UpdateView();
			}
		}
	}
}
