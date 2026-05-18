using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoConvictionsTabVM : CharInfoComponentVM
{
	private ReactiveProperty<CharInfoAlignmentHistoryVM> m_ChoicesMadeVM = new ReactiveProperty<CharInfoAlignmentHistoryVM>();

	private ReactiveProperty<CharInfoAlignmentVM> m_ConvictionsVM = new ReactiveProperty<CharInfoAlignmentVM>();

	public ReadOnlyReactiveProperty<CharInfoAlignmentVM> ConvictionsVM => m_ConvictionsVM;

	public ReadOnlyReactiveProperty<CharInfoAlignmentHistoryVM> ChoicesMadeVM => m_ChoicesMadeVM;

	public CharInfoConvictionsTabVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		m_ConvictionsVM.Value = new CharInfoAlignmentVM(unit);
		m_ChoicesMadeVM.Value = new CharInfoAlignmentHistoryVM(unit);
	}
}
