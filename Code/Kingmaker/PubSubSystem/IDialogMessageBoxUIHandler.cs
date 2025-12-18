using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;
using R3;
using TMPro;

namespace Kingmaker.PubSubSystem;

public interface IDialogMessageBoxUIHandler : ISubscriber
{
	void HandleOpen(string messageText, DialogMessageBoxType boxType = DialogMessageBoxType.Message, Action<DialogMessageBoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue, ReadOnlyReactiveProperty<float> loadingProgress = null, Observable<Unit> loadingProgressCloseTrigger = null);

	void HandleClose();
}
