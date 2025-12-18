using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTransitionWindowConsoleView : VendorTransitionWindowView
{
	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	protected override void OnBind()
	{
		base.OnBind();
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Transition Window Console View"
		});
		CreateInput();
	}

	private void CreateInput()
	{
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Deal();
		}, 8, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.CommonTexts.Accept).AddTo(this);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ChangeSliderValue(1);
		}, 5), UIStrings.Instance.CommonTexts.Increase).AddTo(this);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ChangeSliderValue(-1);
		}, 4), UIStrings.Instance.CommonTexts.Decrease).AddTo(this);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Close();
	}

	private void ChangeSliderValue(int value)
	{
		m_Slider.value += value;
	}
}
