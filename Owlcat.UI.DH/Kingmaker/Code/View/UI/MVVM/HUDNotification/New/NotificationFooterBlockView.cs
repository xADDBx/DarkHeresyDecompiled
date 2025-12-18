using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationFooterBlockView : View<NotificationFooterBlockVM>
{
	[SerializeField]
	private TMP_Text m_ButtonLabel;

	[Header("Elements")]
	[field: SerializeField]
	public OwlcatMultiButton Button { get; private set; }

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonClick?.Invoke();
		}).AddTo(this);
		m_ButtonLabel.text = base.ViewModel.ButtonLabel;
	}
}
