using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows;

public class OpenedServiceWindowHUDView : View<OpenedServiceWindowHUDVM>
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.CloseAll();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Object.Destroy(base.gameObject);
	}
}
