using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorGenericSlotPCView : VendorGenericSlotView<ItemSlotPCView>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotView.IsDraggable = false;
		m_ItemSlotView.Bind(base.ViewModel);
		AddDisposable(m_ItemSlotView.OnLeftClickAsObservable.Subscribe(base.OnClick));
		AddDisposable(m_ItemSlotView.OnDoubleClickAsObservable.Subscribe(base.OnDoubleClick));
	}
}
