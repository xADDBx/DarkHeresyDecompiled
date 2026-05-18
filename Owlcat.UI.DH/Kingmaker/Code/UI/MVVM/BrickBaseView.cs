using Code.View.UI.Helpers;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BrickBaseView<T> : View<T>, IBrickView where T : TooltipBrickVM
{
	protected AccessibilityTextHelper m_TextHelper;

	Transform IBrickView.Transform => base.transform;

	void IBrickView.BindVM(TooltipBrickVM vm)
	{
		Bind((T)vm);
	}
}
