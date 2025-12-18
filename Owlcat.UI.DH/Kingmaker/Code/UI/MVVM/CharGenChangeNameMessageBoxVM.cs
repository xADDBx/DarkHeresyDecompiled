using System;
using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenChangeNameMessageBoxVM : MessageBoxVM
{
	private readonly Func<string> m_GetRandomName;

	public CharGenChangeNameMessageBoxVM(string messageText, string yesLabel, Action<string> onTextClose, string inputText, Func<string> getRandomName, Action disposeAction)
		: base(messageText, DialogMessageBoxType.TextField, null, null, yesLabel, null, onTextClose, inputText, null, 0, disposeAction, null, null)
	{
		m_GetRandomName = getRandomName;
	}

	public void SetRandomName()
	{
		m_InputText.Value = m_GetRandomName();
	}
}
