using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CounterWindowConsoleView : CounterWindowPCView
{
	[Header("ConsoleInput")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_AcceptHint;

	[SerializeField]
	private ConsoleHint m_CloseHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CounterWindow"
		});
		m_InputLayer.AddAxis(delegate(InputActionEventData _, float value)
		{
			OnLeftStickX(value);
		}, 0, repeat: true).AddTo(this);
		m_InputLayer.AddButton(delegate(InputActionEventData handler)
		{
			ChangeValue(Mathf.Max(1, Mathf.FloorToInt((float)handler.GetButtonTimePressed())));
		}, 5, InputActionEventType.ButtonRepeating).AddTo(this);
		m_InputLayer.AddButton(delegate(InputActionEventData handler)
		{
			ChangeValue(-1 * Mathf.Max(1, Mathf.FloorToInt((float)handler.GetButtonTimePressed())));
		}, 4, InputActionEventType.ButtonRepeating).AddTo(this);
		int mediumShift = GetMediumShiftAmount();
		if (mediumShift > 1)
		{
			m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				ChangeValue(mediumShift);
			}, 15, InputActionEventType.ButtonRepeating), $"+{mediumShift}").AddTo(this);
			m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				ChangeValue(-mediumShift);
			}, 14, InputActionEventType.ButtonRepeating), $"-{mediumShift}").AddTo(this);
		}
		m_CloseHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9)).AddTo(this);
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		m_AcceptHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Accept();
		}, 8)).AddTo(this);
		m_AcceptHint.SetLabel(GetOperationButtonText());
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_CloseHint.transform.SetAsLastSibling();
			m_AcceptHint.transform.SetAsFirstSibling();
		}).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void OnLeftStickX(float x)
	{
		float num = ((Mathf.Abs(x) > 0.1f) ? Mathf.Sign(x) : 0f);
		ChangeValue((int)num);
	}

	private void ChangeValue(int value)
	{
		m_CountSlider.value += value;
	}

	private int GetMediumShiftAmount()
	{
		return Mathf.Min((base.ViewModel.MaxValue - 1) / 2, 10);
	}
}
