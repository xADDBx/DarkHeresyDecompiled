using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ItemSlotBaseView : View<ItemSlotVM>, IItemSlotView
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	public ItemSlotVM SlotVM => base.ViewModel;

	protected override void OnBind()
	{
		base.ViewModel.Item.Subscribe(delegate
		{
			UpdateSlotLayer();
		}).AddTo(this);
		UpdateSlotLayer();
	}

	public void SetAvailable(bool available)
	{
		if (!(m_MainButton == null))
		{
			m_MainButton.Interactable = available;
		}
	}

	public void SetFocus(bool value)
	{
		if (!(m_MainButton == null))
		{
			m_MainButton.SetFocus(value);
		}
	}

	public bool IsValid()
	{
		if (m_MainButton != null)
		{
			return m_MainButton.IsValid();
		}
		return false;
	}

	public void SetLockState()
	{
		if (!(m_MainButton == null))
		{
			m_MainButton.SetActiveLayer((m_MainButton.MultiLayerNames.Length > 1) ? 2 : 0);
		}
	}

	public void SetLayer(int layerNumber)
	{
		if (!(m_MainButton == null))
		{
			m_MainButton.SetActiveLayer(layerNumber);
		}
	}

	protected virtual void UpdateSlotLayer()
	{
		if (!(m_MainButton == null))
		{
			if (m_MainButton.MultiLayerNames != null)
			{
				m_MainButton.SetActiveLayer((base.ViewModel.Count.CurrentValue > 0) ? 1 : 0);
			}
			else
			{
				m_MainButton.SetActiveLayer(0);
			}
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
}
