namespace Kingmaker.Code.UI.MVVM;

public abstract class NestedSelectionGroupRadioPCView<TEntityVM, TEntityView> : NestedSelectionGroupPCView<NestedSelectionGroupRadioVM<TEntityVM>, TEntityVM, TEntityView> where TEntityVM : NestedSelectionGroupEntityVM where TEntityView : NestedSelectionGroupEntityPCView<TEntityVM>
{
}
