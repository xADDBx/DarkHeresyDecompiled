using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbySaveSlotCollectionConsoleView : NetLobbySaveSlotCollectionBaseView, ISavesUpdatedHandler, ISubscriber
{
	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private readonly ReactiveProperty<bool> m_ShowWaitingSaveAnim = new ReactiveProperty<bool>(value: false);

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		m_ShowWaitingSaveAnim.Value = true;
		CreateInputImpl();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ShowWaitingSaveAnim.Value = false;
	}

	public void CreateInputImpl()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_NavigationBehaviour.AddEntityVertical(base.NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "NetLobbySavesSlots"
		});
		base.AttachedFirstValidView.Subscribe(FocusOnFirstValidSaveSlot).AddTo(this);
		m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.OnBack();
		}, 9, m_ShowWaitingSaveAnim.Not().ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased), UIStrings.Instance.CharGen.Back).AddTo(this);
		m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 8, m_ShowWaitingSaveAnim.Not().ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.NetLobbyTexts.ChooseSaveHeader).AddTo(this);
		m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 11, m_ShowWaitingSaveAnim.Not().ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.SaveLoadTexts.ShowScreenshot).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void FocusOnFirstValidSaveSlot()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(1), delegate
		{
			foreach (IConsoleEntity entity in base.NavigationBehaviour.Entities)
			{
				if (entity is VirtualListElement { View: SaveSlotBaseView view } && view.IsValid())
				{
					base.NavigationBehaviour.FocusOnEntityManual(entity);
					m_NavigationBehaviour.FocusOnEntityManual(base.NavigationBehaviour);
					return;
				}
			}
			base.NavigationBehaviour.FocusOnFirstValidEntity();
			m_NavigationBehaviour.FocusOnEntityManual(base.NavigationBehaviour);
		}).AddTo(this);
	}

	public void OnSaveListUpdated()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			m_ShowWaitingSaveAnim.Value = false;
		}));
	}
}
