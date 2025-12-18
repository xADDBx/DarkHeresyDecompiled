using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public interface IHasNeighbours
{
	void SetNeighbours(List<IFloatConsoleNavigationEntity> entities);
}
