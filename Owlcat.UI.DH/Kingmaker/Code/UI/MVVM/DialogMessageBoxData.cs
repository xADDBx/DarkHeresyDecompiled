using System;
using Kingmaker.Code.View.Bridge.Enums;
using R3;
using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public class DialogMessageBoxData
{
	public string MessageText { get; }

	public DialogMessageBoxType BoxType { get; }

	public Action<DialogMessageBoxButton> OnClose { get; }

	public Action<TMP_LinkInfo> OnLinkInvoke { get; }

	public string YesLabel { get; }

	public string NoLabel { get; }

	public Action<string> OnTextResult { get; }

	public string InputText { get; }

	public string InputPlaceholder { get; }

	public int WaitTime { get; }

	public uint MaxInputTextLength { get; }

	public ReadOnlyReactiveProperty<float> LoadingProgress { get; }

	public Observable<Unit> LoadingProgressCloseTrigger { get; }

	public DialogMessageBoxData(string messageText, DialogMessageBoxType boxType = DialogMessageBoxType.Message, Action<DialogMessageBoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue, ReadOnlyReactiveProperty<float> loadingProgress = null, Observable<Unit> loadingProgressCloseTrigger = null)
	{
		MessageText = messageText;
		BoxType = boxType;
		OnClose = onClose;
		OnLinkInvoke = onLinkInvoke;
		YesLabel = yesLabel;
		NoLabel = noLabel;
		OnTextResult = onTextResult;
		InputText = inputText;
		InputPlaceholder = inputPlaceholder;
		WaitTime = waitTime;
		MaxInputTextLength = maxInputTextLength;
		LoadingProgress = loadingProgress;
		LoadingProgressCloseTrigger = loadingProgressCloseTrigger;
	}
}
