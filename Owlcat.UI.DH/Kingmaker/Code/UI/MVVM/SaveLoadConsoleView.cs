using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadConsoleView : SaveLoadBaseView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	private SaveSlotCollectionVirtualConsoleView SlotCollectionView => m_SlotCollectionView as SaveSlotCollectionVirtualConsoleView;

	protected override void OnBind()
	{
		base.OnBind();
		GamePad.Instance.BaseLayer?.Unbind();
		GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		GamePad.Instance.BaseLayer?.Bind();
		base.OnUnbind();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		m_CommonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9, base.ViewModel.SaveListUpdating.Not().ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		m_PrevHint.Bind(inputLayer.AddButton(delegate
		{
			SelectPrev();
		}, 14, base.ViewModel.SaveLoadMenuVM.HasFewEntities)).AddTo(this);
		m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			SelectNext();
		}, 15, base.ViewModel.SaveLoadMenuVM.HasFewEntities)).AddTo(this);
		inputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
		SlotCollectionView.AddInput(inputLayer, m_CommonHintsWidget, base.ViewModel.SaveListUpdating, base.ViewModel.IsCurrentIronManSave);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, value * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != m_InputLayer && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == "SaveFullScreenshotConsoleView") && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == NetLobbyConsoleView.InputLayerName))
		{
			instance.PopLayer(m_InputLayer);
			instance.PushLayer(m_InputLayer);
		}
	}
}
