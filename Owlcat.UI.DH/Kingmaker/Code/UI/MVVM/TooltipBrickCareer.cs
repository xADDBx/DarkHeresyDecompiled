using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickCareer : ITooltipBrick
{
	private readonly CareerPathVM m_CareerPathVM;

	public TooltipBrickCareer(CareerPathVM careerPath)
	{
		m_CareerPathVM = careerPath;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickFeatureVM(m_CareerPathVM.CareerPath, isHeader: false, m_CareerPathVM.CareerTooltip);
	}
}
