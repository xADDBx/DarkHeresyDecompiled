using Kingmaker.Items;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ItemSlotView<TViewModel> : VirtualListElementViewBase<TViewModel>, IItemSlotView where TViewModel : ItemSlotVM
{
	[SerializeField]
	protected UsableSourceType UsableSource;

	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	private GameObject m_CountBackground;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	[SerializeField]
	private OwlcatMultiSelectable m_ItemStatus;

	[SerializeField]
	private OwlcatMultiSelectable m_ItemGrade;

	[SerializeField]
	private OwlcatMultiSelectable m_ItemTagType;

	[SerializeField]
	private GameObject m_UsableBgr;

	[Header("Raycast Zone")]
	[SerializeField]
	private RectTransform m_RaycastZone;

	[SerializeField]
	private Vector2 m_ExpandRaycastSize;

	[SerializeField]
	protected CanvasGroup m_BlinkMark;

	[Header("Tooltip tune")]
	[SerializeField]
	private bool m_DoNotUseComparativeTooltip;

	public ItemEntity Item => base.ViewModel?.Item?.CurrentValue;

	public ItemSlotVM SlotVM => base.ViewModel;

	protected override void BindViewImplementation()
	{
		SetupItem();
		SetupDropZoneSize();
		AddDisposable(ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateView, delegate
		{
			SetupItem();
		}));
		if (m_DoNotUseComparativeTooltip)
		{
			base.ViewModel.CompareTooltipEnabled = false;
			base.ViewModel.UpdateTooltips(force: true);
		}
	}

	public virtual void RefreshItem()
	{
		SetupContextMenu();
		SetupGrade(base.ViewModel.ItemGrade.CurrentValue);
		SetupStatus(base.ViewModel.ItemStatus.CurrentValue);
		SetupUsable(base.ViewModel.IsUsable.CurrentValue);
		SetupItemTagType(base.ViewModel.ItemTagType.CurrentValue);
	}

	protected virtual void SetupContextMenu()
	{
	}

	protected virtual void SetupIcon(Sprite value)
	{
		m_Icon.gameObject.SetActive(value);
		m_Icon.sprite = value;
	}

	private void SetupItem()
	{
		SetupIcon(base.ViewModel.Icon.CurrentValue);
		SetupCount(base.ViewModel.Count.CurrentValue);
		SetupItemTagType(base.ViewModel.ItemTagType.CurrentValue);
		RefreshItem();
	}

	private void SetupCount(int value)
	{
		if (!(m_Count == null))
		{
			if (m_CountBackground != null)
			{
				m_CountBackground.SetActive(value > 1);
			}
			m_Count.gameObject.SetActive(value > 1);
			m_Count.text = ((value > 1) ? base.ViewModel.Count.ToString() : string.Empty);
		}
	}

	private void SetupGrade(ItemGrade itemGrade)
	{
		if (!(m_ItemGrade == null))
		{
			m_ItemGrade.gameObject.SetActive(base.ViewModel.HasItem);
			m_ItemGrade.SetActiveLayer(itemGrade.ToString());
		}
	}

	private void SetupStatus(ItemStatus itemStatus)
	{
		if (!(m_ItemStatus == null))
		{
			bool hasItem = base.ViewModel.HasItem;
			m_ItemStatus.gameObject.SetActive(hasItem && itemStatus != ItemStatus.None);
			if (hasItem)
			{
				m_ItemStatus.SetActiveLayer(itemStatus.ToString());
			}
		}
	}

	private void SetupUsable(bool value)
	{
		if (!(m_UsableBgr == null))
		{
			m_UsableBgr.gameObject.SetActive(value);
		}
	}

	private void SetupItemTagType(ItemTagType itemTagType)
	{
		if (!(m_ItemTagType == null))
		{
			m_ItemTagType.SetActiveLayer(base.ViewModel.HasItem ? itemTagType.ToString() : "None");
		}
	}

	protected virtual void ClearView()
	{
		m_Icon.gameObject.SetActive(value: false);
		m_Icon.sprite = null;
		if (!(m_Count == null))
		{
			if (m_CountBackground != null)
			{
				m_CountBackground.SetActive(value: false);
			}
			m_Count.gameObject.SetActive(value: false);
			m_Count.text = string.Empty;
		}
	}

	private void SetupDropZoneSize()
	{
		if ((bool)m_RaycastZone)
		{
			m_RaycastZone.sizeDelta = m_ExpandRaycastSize;
		}
	}

	public RectTransform GetParentContainer()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		float width = rectTransform.rect.width;
		float height = rectTransform.rect.height;
		while (true)
		{
			if (!rectTransform)
			{
				return null;
			}
			if (rectTransform.rect.width > width && rectTransform.rect.height > height)
			{
				break;
			}
			rectTransform = (RectTransform)rectTransform.parent;
		}
		return rectTransform;
	}

	protected override void DestroyViewImplementation()
	{
		ClearView();
	}
}
