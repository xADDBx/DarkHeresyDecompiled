namespace Owlcat.UI;

public class SelectionGroupRadioView<TEntityVM, TEntityView> : SelectionGroupView<SelectionGroupRadioVM<TEntityVM>, TEntityVM, TEntityView> where TEntityVM : SelectionGroupEntityVM where TEntityView : SelectionGroupEntityView<TEntityVM>
{
	public void SelectPrevValidEntity()
	{
		base.ViewModel.SelectPrevValidEntity();
	}

	public void SelectNextValidEntity()
	{
		base.ViewModel.SelectNextValidEntity();
	}
}
