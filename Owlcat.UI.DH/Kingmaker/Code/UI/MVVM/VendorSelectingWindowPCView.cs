using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorSelectingWindowPCView : VendorSelectingWindowBaseView
{
	[SerializeField]
	private OwlcatButton CloseButton;

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(CloseButton.OnLeftClickAsObservable(), delegate
		{
			OnCloseClick();
		}).AddTo(this);
	}
}
