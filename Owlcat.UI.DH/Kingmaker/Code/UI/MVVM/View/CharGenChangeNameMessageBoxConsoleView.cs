namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenChangeNameMessageBoxConsoleView : MessageBoxConsoleView
{
	private bool m_IsAcceptInteractable;

	private CharGenChangeNameMessageBoxVM ChangeNameViewModel => base.ViewModel as CharGenChangeNameMessageBoxVM;

	protected new void CreateInputImpl()
	{
	}

	protected override void SetAcceptInteractable(bool interactable)
	{
		m_IsAcceptInteractable = interactable;
	}

	protected override void OnConfirmClick()
	{
		if (m_IsAcceptInteractable)
		{
			base.OnConfirmClick();
		}
		else if (CanEditNameByYourself.Value)
		{
			m_InputField.Select();
		}
	}

	protected override void OnTextInputChanged(string value)
	{
		string text = string.Empty;
		if (value.EndsWith(" "))
		{
			text = " ";
		}
		value = value.Trim();
		value += text;
		m_InputField.InputField.text = value;
		base.ViewModel.SetInputText(value);
		m_InputField.InputField.textComponent.ForceMeshUpdate(ignoreActiveState: true);
	}
}
