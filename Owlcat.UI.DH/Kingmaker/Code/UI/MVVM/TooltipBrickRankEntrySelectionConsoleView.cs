using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickRankEntrySelectionConsoleView : TooltipBrickRankEntrySelectionView, IConsoleTooltipBrick
{
	private SimpleConsoleNavigationEntity m_MainButtonEntity;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		base.OnBind();
		RankEntrySelectionFeatureVM currentValue = base.ViewModel.RankEntrySelectionVM.SelectedFeature.CurrentValue;
		TooltipBaseTemplate tooltip = ((currentValue != null) ? currentValue.Tooltip.CurrentValue : base.ViewModel.RankEntrySelectionVM.Tooltip);
		m_MainButtonEntity = new SimpleConsoleNavigationEntity(m_MainButton, tooltip);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_MainButtonEntity;
	}
}
