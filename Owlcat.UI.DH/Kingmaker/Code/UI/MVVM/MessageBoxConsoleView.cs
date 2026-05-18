using Kingmaker.Code.View.Bridge.Enums;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class MessageBoxConsoleView : MessageBoxBaseView
{
	[Header("Input Field")]
	[SerializeField]
	protected ConsoleInputField m_InputField;

	protected readonly ReactiveProperty<bool> ConfirmBindActive = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> CanEditNameByYourself = new ReactiveProperty<bool>(value: true);

	protected override void OnBind()
	{
		base.OnBind();
	}

	protected override void SetAcceptInteractable(bool interactable)
	{
		ConfirmBindActive.Value = interactable;
	}

	protected override void SetAcceptText(string acceptText)
	{
	}

	protected override void SetDeclineText(string declineText)
	{
	}

	protected override void BindTextField()
	{
		m_InputField.gameObject.SetActive(base.ViewModel.BoxType == DialogMessageBoxType.TextField);
		if (base.ViewModel.BoxType == DialogMessageBoxType.TextField)
		{
			m_InputField.SetPlaceholderText(base.ViewModel.InputPlaceholder);
			base.ViewModel.InputText.Subscribe(delegate(string value)
			{
				m_InputField.Text = value;
			}).AddTo(this);
			m_InputField.InputField.onValueChanged.AddListener(OnTextInputChanged);
			m_InputField.Select();
		}
	}

	protected override void DestroyTextField()
	{
		m_InputField.InputField.onValueChanged.RemoveListener(OnTextInputChanged);
		m_InputField.Abort();
	}

	protected override void BindProgressBar()
	{
	}

	protected void CreateInputImpl()
	{
	}

	protected virtual void OnConfirmClick()
	{
		base.ViewModel.OnAcceptPressed();
	}

	protected virtual void OnDeclineClick()
	{
		base.ViewModel.OnDeclinePressed();
	}

	public void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}
}
