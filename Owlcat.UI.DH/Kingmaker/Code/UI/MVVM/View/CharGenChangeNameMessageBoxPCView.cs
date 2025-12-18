using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenChangeNameMessageBoxPCView : MessageBoxPCView
{
	[Header("CharGenChangeNameMessageBoxPCView")]
	[SerializeField]
	private OwlcatButton m_RandomNameButton;

	[SerializeField]
	private TextMeshProUGUI m_RandomNameLabel;

	private CharGenChangeNameMessageBoxVM ChangeNameViewModel => base.ViewModel as CharGenChangeNameMessageBoxVM;

	public override void Awake()
	{
		m_RandomNameLabel.text = UIStrings.Instance.CharGen.SetRandomNameButton;
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_RandomNameButton.gameObject.SetActive(!base.ViewModel.IsProgressBar.CurrentValue);
		m_RandomNameLabel.gameObject.SetActive(!base.ViewModel.IsProgressBar.CurrentValue);
		ObservableSubscribeExtensions.Subscribe(m_RandomNameButton.OnLeftClickAsObservable(), delegate
		{
			ChangeNameViewModel.SetRandomName();
		}).AddTo(this);
		m_RandomNameButton.SetHint(UIStrings.Instance.CharGen.SetRandomName).AddTo(this);
	}

	protected override void OnTextInputChanged(string value)
	{
		string text = "";
		if (value.EndsWith(" "))
		{
			text = " ";
		}
		value = value.Trim();
		value += text;
		m_InputField.text = value;
		base.ViewModel.SetInputText(value);
	}
}
