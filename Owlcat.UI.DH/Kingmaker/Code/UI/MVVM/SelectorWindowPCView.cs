using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SelectorWindowPCView<TEntityView, TEntityVM> : SelectorWindowBaseView<TEntityView, TEntityVM> where TEntityView : VirtualListElementViewBase<TEntityVM>, IHasTooltipTemplate where TEntityVM : SelectionGroupEntityVM
{
}
