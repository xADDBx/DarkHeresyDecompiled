using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BrickGroupView : BrickBaseView<BricksGroupBaseVM>
{
	public abstract void AddChild(RectTransform childTransform);
}
public abstract class BrickGroupView<T> : BrickGroupView where T : BricksGroupBaseVM
{
	protected new T ViewModel => (T)base.ViewModel;
}
