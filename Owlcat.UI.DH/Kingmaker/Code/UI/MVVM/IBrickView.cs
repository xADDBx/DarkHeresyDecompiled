using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public interface IBrickView
{
	Transform Transform { get; }

	void BindVM(TooltipBrickVM vm);
}
