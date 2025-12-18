using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorOptionsPCView : View<VendorOptionsVM>
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private VendorOptionsItemPCView m_ItemView;

	[SerializeField]
	private RectTransform m_Container;

	private bool m_IsShow;

	private readonly List<VendorOptionsItemPCView> m_Items = new List<VendorOptionsItemPCView>();

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: false);
		m_IsShow = false;
		foreach (VendorOptionsItemVM itemVm in base.ViewModel.ItemVms)
		{
			VendorOptionsItemPCView widget = WidgetFactory.GetWidget(m_ItemView);
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Bind(itemVm);
			m_Items.Add(widget);
		}
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			Show();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Items.ForEach(WidgetFactory.DisposeWidget);
		m_Items.Clear();
	}

	public void Show()
	{
		m_IsShow = !m_IsShow;
		base.gameObject.SetActive(m_IsShow);
	}
}
