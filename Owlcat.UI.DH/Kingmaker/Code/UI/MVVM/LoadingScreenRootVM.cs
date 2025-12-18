using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenRootVM : ViewModel, ILoadingScreen, IDialogMessageBoxUIHandler, ISubscriber
{
	private readonly ReactiveProperty<LoadingScreenVM> m_LoadingScreenVM = new ReactiveProperty<LoadingScreenVM>();

	private BlueprintArea m_Area;

	private readonly List<DialogMessageBoxData> m_DialogMessageBoxDatas = new List<DialogMessageBoxData>();

	public ReadOnlyReactiveProperty<LoadingScreenVM> LoadingScreenVM => m_LoadingScreenVM;

	public LoadingScreenRootVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeLoadingScreen();
	}

	public void ShowLoadingScreen()
	{
		if (LoadingProcess.Instance.IsLoadingInProcess && LoadingScreenVM.CurrentValue != null)
		{
			LoadingScreenVM.CurrentValue.SetLoadingArea(m_Area);
		}
		else
		{
			m_LoadingScreenVM.Value = new LoadingScreenVM(m_Area).AddTo(this);
		}
	}

	public void HideLoadingScreen()
	{
		DisposeLoadingScreen();
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return LoadingScreenVM.CurrentValue?.State ?? LoadingScreenState.Hidden;
	}

	private void DisposeLoadingScreen()
	{
		LoadingScreenVM.CurrentValue?.Dispose();
		m_LoadingScreenVM.Value = null;
		m_Area = null;
		foreach (DialogMessageBoxData data in m_DialogMessageBoxDatas)
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
			{
				h.HandleOpen(data.MessageText, data.BoxType, data.OnClose, data.OnLinkInvoke, data.YesLabel, data.NoLabel, data.OnTextResult, data.InputText, data.InputPlaceholder, data.WaitTime, data.MaxInputTextLength, data.LoadingProgress, data.LoadingProgressCloseTrigger);
			});
		}
		m_DialogMessageBoxDatas.Clear();
	}

	public void SetLoadingArea(BlueprintArea area)
	{
		m_Area = area;
		LoadingScreenVM.CurrentValue?.SetLoadingArea(area);
	}

	public void HandleOpen(string messageText, DialogMessageBoxType boxType = DialogMessageBoxType.Message, Action<DialogMessageBoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue, ReadOnlyReactiveProperty<float> loadingProgress = null, Observable<Unit> loadingProgressCloseTrigger = null)
	{
		if (LoadingScreenVM?.CurrentValue != null)
		{
			m_DialogMessageBoxDatas.Add(new DialogMessageBoxData(messageText, boxType, onClose, onLinkInvoke, yesLabel, noLabel, onTextResult, inputText, inputPlaceholder, waitTime, maxInputTextLength, loadingProgress, loadingProgressCloseTrigger));
		}
	}

	public void HandleClose()
	{
	}
}
