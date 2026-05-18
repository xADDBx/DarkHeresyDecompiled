using System;
using Kingmaker.Localization.Enums;
using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public class CrossPlatformConsoleVirtualKeyboard : IVirtualKeyboard
{
	private IVirtualKeyboard m_Keyboard;

	public static readonly string InputLayerContextName = "Virtual Keyboard";

	public CrossPlatformConsoleVirtualKeyboard(TMP_InputField inputField)
	{
	}

	public void OpenKeyboard(Action<string> successCallback, Action cancelCallback, string titleText, string inputText, string placeholderText, Locale language, uint maxTextLength)
	{
		if (m_Keyboard == null)
		{
			successCallback?.Invoke(inputText + " (keyboard not supported)");
			return;
		}
		if (maxTextLength == 0)
		{
			maxTextLength = 128u;
		}
		successCallback = (Action<string>)Delegate.Combine(successCallback, (Action<string>)delegate
		{
			Abort();
		});
		cancelCallback = (Action)Delegate.Combine(cancelCallback, (Action)delegate
		{
			Abort();
		});
		m_Keyboard.OpenKeyboard(successCallback, cancelCallback, titleText, inputText, placeholderText, language, maxTextLength);
	}

	public void Abort()
	{
		m_Keyboard?.Abort();
	}
}
