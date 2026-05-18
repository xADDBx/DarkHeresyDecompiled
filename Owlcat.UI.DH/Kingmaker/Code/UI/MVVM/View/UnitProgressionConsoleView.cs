using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class UnitProgressionConsoleView : UnitProgressionCommonView, IConsoleNavigationOwner, IConsoleEntity, ICharInfoCanHookDecline, ICharInfoCanHookConfirm
{
	private readonly ReactiveProperty<bool> m_CanHookDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanFuncAdditional = new ReactiveProperty<bool>();

	private CareerPathsListsConsoleView CareerPathsListsConsoleView => m_CareerPathsListsCommonView as CareerPathsListsConsoleView;

	private CareerPathProgressionConsoleView CareerPathProgressionConsoleView => m_CareerPathProgressionCommonView as CareerPathProgressionConsoleView;

	public void AddInput()
	{
	}

	protected override void HandleState(UnitProgressionWindowState state)
	{
		base.HandleState(state);
		TooltipHelper.HideTooltip();
	}

	protected override void BindPathProgression(CareerPathVM careerPathVM)
	{
		base.BindPathProgression(careerPathVM);
		m_CanHookDecline.Value = careerPathVM != null;
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		m_CanFuncAdditional.Value = (entity as IFuncAdditionalClickHandler)?.CanFuncAdditionalClick() ?? false;
	}

	private void OnFuncAdditionalClick()
	{
	}

	public void EntityFocused(IConsoleEntity entity)
	{
	}

	public ReadOnlyReactiveProperty<bool> GetCanHookDeclineProperty()
	{
		return m_CanHookDecline;
	}

	public ReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty()
	{
		return CareerPathProgressionConsoleView.GetCanHookConfirmProperty();
	}
}
