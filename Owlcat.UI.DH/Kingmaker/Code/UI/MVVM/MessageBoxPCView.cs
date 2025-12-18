using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MessageBoxPCView : MessageBoxBaseView
{
	[Header("Input Field")]
	[SerializeField]
	protected TMP_InputField m_InputField;

	[Header("Buttons Block")]
	[SerializeField]
	protected OwlcatMultiButton m_AcceptButton;

	[SerializeField]
	protected OwlcatMultiButton m_DeclineButton;

	[SerializeField]
	private TextMeshProUGUI m_AcceptText;

	[SerializeField]
	private TextMeshProUGUI m_DeclineText;

	protected override void OnBind()
	{
		base.OnBind();
		m_AcceptButton.gameObject.SetActive(!base.ViewModel.IsProgressBar.CurrentValue);
		m_DeclineButton.gameObject.SetActive(base.ViewModel.ShowDecline.CurrentValue);
		ObservableSubscribeExtensions.Subscribe(m_DeclineButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnDeclinePressed();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_AcceptButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnAcceptPressed();
		}).AddTo(this);
	}

	protected override void SetAcceptInteractable(bool interactable)
	{
		m_AcceptButton.Interactable = interactable;
	}

	protected override void SetAcceptText(string acceptText)
	{
		m_AcceptText.text = acceptText;
	}

	protected override void SetDeclineText(string declineText)
	{
		m_DeclineText.text = declineText;
	}

	protected override void BindTextField()
	{
		m_InputField.gameObject.SetActive(base.ViewModel.BoxType == DialogMessageBoxType.TextField);
		if (base.ViewModel.BoxType == DialogMessageBoxType.TextField)
		{
			if (m_InputField.placeholder is TextMeshProUGUI textMeshProUGUI)
			{
				textMeshProUGUI.text = base.ViewModel.InputPlaceholder;
			}
			base.ViewModel.InputText.Subscribe(delegate(string value)
			{
				m_InputField.text = value;
			}).AddTo(this);
			m_InputField.onValueChanged.AddListener(OnTextInputChanged);
		}
	}

	protected override void DestroyTextField()
	{
		m_InputField.onValueChanged.RemoveListener(OnTextInputChanged);
	}

	protected override void BindProgressBar()
	{
	}
}
