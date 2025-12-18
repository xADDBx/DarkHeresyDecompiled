using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LightweightOvertipNameBlockVM : ViewModel
{
	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>(string.Empty);

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public MechanicEntityUIState MechanicEntityUIState { get; }

	public LightweightOvertipNameBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
		m_Name.Value = mechanicEntityUIState.MechanicEntity.Name;
	}
}
