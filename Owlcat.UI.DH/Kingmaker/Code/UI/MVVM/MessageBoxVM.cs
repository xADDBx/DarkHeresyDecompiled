using System;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public class MessageBoxVM : ViewModel, INetLobbyRequest, ISubscriber
{
	public readonly DialogMessageBoxType BoxType;

	public readonly string MessageText;

	public readonly string AcceptText;

	public readonly string DeclineText;

	public readonly string InputPlaceholder;

	private readonly ReactiveProperty<bool> m_ShowDecline = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<string> m_InputText = new ReactiveProperty<string>();

	private readonly ReactiveProperty<int> m_WaitTime = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<bool> m_IsProgressBar = new ReactiveProperty<bool>();

	private readonly Action<DialogMessageBoxButton> m_OnClose;

	private readonly Action<string> m_TextClose;

	private readonly Action<TMP_LinkInfo> m_LinkInvoke;

	private readonly Action m_DisposeAction;

	public readonly ReadOnlyReactiveProperty<float> LoadingProgress;

	public readonly Observable<Unit> LoadingProgressCloseTrigger;

	public ReadOnlyReactiveProperty<bool> ShowDecline => m_ShowDecline;

	public ReadOnlyReactiveProperty<string> InputText => m_InputText;

	public ReadOnlyReactiveProperty<int> WaitTime => m_WaitTime;

	public ReadOnlyReactiveProperty<bool> IsProgressBar => m_IsProgressBar;

	public MessageBoxVM(string messageText, DialogMessageBoxType boxType, Action<DialogMessageBoxButton> onClose, Action<TMP_LinkInfo> onLinkInvoke, string yesLabel, string noLabel, Action<string> onTextClose, string inputText, string inputPlaceholder, int waitTime, Action disposeAction, ReadOnlyReactiveProperty<float> loadingProgress, Observable<Unit> loadingProgressCloseTrigger)
	{
		EventBus.Subscribe(this).AddTo(this);
		BoxType = boxType;
		UICommonTexts commonTexts = UIStrings.Instance.CommonTexts;
		MessageText = messageText;
		AcceptText = (string.IsNullOrEmpty(yesLabel) ? ((string)commonTexts.Accept) : yesLabel);
		DeclineText = (string.IsNullOrEmpty(noLabel) ? ((string)commonTexts.Cancel) : noLabel);
		m_IsProgressBar.Value = boxType == DialogMessageBoxType.ProgressBar;
		m_ShowDecline.Value = boxType != 0 && !IsProgressBar.CurrentValue;
		m_OnClose = onClose;
		m_TextClose = onTextClose;
		m_LinkInvoke = onLinkInvoke;
		m_InputText.Value = (string.IsNullOrEmpty(inputText) ? string.Empty : inputText);
		InputPlaceholder = (string.IsNullOrEmpty(inputPlaceholder) ? string.Empty : inputPlaceholder);
		m_WaitTime.Value = waitTime;
		m_DisposeAction = disposeAction;
		DOTween.To(() => WaitTime.CurrentValue, delegate(int value)
		{
			m_WaitTime.Value = value;
		}, 0, waitTime).SetUpdate(isIndependentUpdate: true);
		LoadingProgress = loadingProgress;
		LoadingProgressCloseTrigger = loadingProgressCloseTrigger;
	}

	public void OnAcceptPressed()
	{
		m_OnClose?.Invoke(DialogMessageBoxButton.Yes);
		m_TextClose?.Invoke(InputText.CurrentValue);
		m_DisposeAction?.Invoke();
	}

	public void OnDeclinePressed()
	{
		m_OnClose?.Invoke(DialogMessageBoxButton.No);
		m_TextClose?.Invoke(string.Empty);
		m_DisposeAction?.Invoke();
	}

	public void OnLinkInvoke(TMP_LinkInfo linkInfo)
	{
		m_LinkInvoke?.Invoke(linkInfo);
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		if (!IsProgressBar.CurrentValue)
		{
			OnDeclinePressed();
		}
	}

	public void SetInputText(string text)
	{
		m_InputText.Value = text;
	}

	public void HandleNetLobbyClose()
	{
	}
}
