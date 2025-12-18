using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharInfoIgnoreNavigationConsoleView
{
	List<GridConsoleNavigationBehaviour> GetIgnoreNavigation();
}
