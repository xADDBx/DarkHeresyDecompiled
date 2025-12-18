using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class FormationSelectionItemVM : SelectionGroupEntityVM
{
	public readonly int FormationIndex;

	public FormationSelectionItemVM(int formationIndex)
		: base(allowSwitchOff: false)
	{
		FormationIndex = formationIndex;
	}

	protected override void DoSelectMe()
	{
	}
}
