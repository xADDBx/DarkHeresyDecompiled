using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenNameBaseView : CharInfoComponentWithLevelUpView<CharGenNameVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_DisclaimerText;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		m_Title.text = UIStrings.Instance.CharGen.SelectNameTitle.Text;
		m_DisclaimerText.text = UIStrings.Instance.CharGen.SelectNameDisclaimer.Text;
		base.ViewModel.CurrentDisplayName.Subscribe(delegate(string value)
		{
			m_StateSelectable.SetActiveLayer(string.IsNullOrEmpty(value) ? "Choose" : "Normal");
		}).AddTo(this);
	}
}
