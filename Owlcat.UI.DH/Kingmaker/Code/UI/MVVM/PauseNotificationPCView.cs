using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PauseNotificationPCView : PauseNotificationBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatMultiButton m_UnpauseButton;

	[SerializeField]
	private TextMeshProUGUI m_UnpauseText;

	protected override void OnBind()
	{
		base.OnBind();
		m_UnpauseText.text = UIStrings.Instance.CommonTexts.Unpause;
		ObservableSubscribeExtensions.Subscribe(m_UnpauseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Unpause();
		}).AddTo(this);
	}
}
