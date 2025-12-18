using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyTutorialPartPCView : NetLobbyTutorialPartBaseView
{
	[SerializeField]
	private OwlcatButton m_ContinueButton;

	[SerializeField]
	private TextMeshProUGUI m_ContinueButtonLabel;

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_ContinueButton.OnLeftClickAsObservable(), delegate
		{
			ShowBlock();
		}).AddTo(this);
		m_ContinueButtonLabel.text = UIStrings.Instance.CommonTexts.Accept;
	}
}
