using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FeaturesFilterVM : ViewModel
{
	private readonly ReactiveProperty<FeatureFilterType> m_CurrentFilter = new ReactiveProperty<FeatureFilterType>();

	public static FeatureFilterType ThisSessionFilter;

	public ReadOnlyReactiveProperty<FeatureFilterType> CurrentFilter => m_CurrentFilter;

	public FeaturesFilterVM()
	{
		m_CurrentFilter.Value = ThisSessionFilter;
	}

	public void SetCurrentFilter(FeatureFilterType filterData)
	{
		m_CurrentFilter.Value = filterData;
		ThisSessionFilter = filterData;
	}
}
