using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCustomPortraitCreatorPCView : CharGenCustomPortraitCreatorView
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private TextMeshProUGUI m_CloseButtonLabel;

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		m_CloseButtonLabel.text = UIStrings.Instance.CommonTexts.Cancel;
	}
}
