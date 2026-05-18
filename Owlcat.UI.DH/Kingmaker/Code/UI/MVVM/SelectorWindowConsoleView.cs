using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SelectorWindowConsoleView<TEntityView, TEntityVM> : SelectorWindowBaseView<TEntityView, TEntityVM> where TEntityView : VirtualListElementViewBase<TEntityVM>, IHasTooltipTemplate where TEntityVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<IConsoleEntity> m_SelectedEntity = new ReactiveProperty<IConsoleEntity>();

	public ReadOnlyReactiveProperty<IConsoleEntity> SelectedEntity => m_SelectedEntity;

	protected virtual LocalizedString ConfirmText => UIStrings.Instance.CommonTexts.Accept;

	protected virtual bool ShouldCloseOnConfirm => true;

	protected override void OnBind()
	{
		base.OnBind();
		SetupInput();
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	private void SetupInput()
	{
	}
}
