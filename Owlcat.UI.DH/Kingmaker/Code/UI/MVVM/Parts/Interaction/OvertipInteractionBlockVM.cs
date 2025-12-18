using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.Parts.Interaction;

public class OvertipInteractionBlockVM : ViewModel
{
	public readonly MechanicEntityUIState MechanicEntityUIState;

	public OvertipInteractionBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		MechanicEntityUIState = mechanicEntityUIState;
	}
}
