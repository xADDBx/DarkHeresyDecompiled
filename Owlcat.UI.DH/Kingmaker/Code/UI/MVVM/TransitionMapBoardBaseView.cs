using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TransitionMapBoardBaseView : View<TransitionMapBoardVM>
{
	[field: SerializeField]
	public RectTransform Bounds { get; private set; }
}
