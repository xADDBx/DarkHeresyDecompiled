using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class MessageBoxConsoleView : MessageBoxBaseView
{
	[Header("Input Field")]
	[SerializeField]
	protected ConsoleInputField m_InputField;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	public static readonly string InputLayerName = "MessageBoxInputContext";

	protected readonly ReactiveProperty<bool> ConfirmBindActive = new ReactiveProperty<bool>();

	private ConsoleHintDescription m_ConfirmHint;

	private ConsoleHintDescription m_DeclineHint;

	protected readonly ReactiveProperty<bool> CanEditNameByYourself = new ReactiveProperty<bool>(value: true);

	protected override void OnBind()
	{
		CreateInput();
		base.OnBind();
		GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged).AddTo(this);
	}

	protected override void SetAcceptInteractable(bool interactable)
	{
		ConfirmBindActive.Value = interactable;
	}

	protected override void SetAcceptText(string acceptText)
	{
		m_ConfirmHint.SetLabel(acceptText);
	}

	protected override void SetDeclineText(string declineText)
	{
		m_DeclineHint.SetLabel(declineText);
	}

	protected override void BindTextField()
	{
		m_InputField.gameObject.SetActive(base.ViewModel.BoxType == DialogMessageBoxType.TextField);
		if (base.ViewModel.BoxType == DialogMessageBoxType.TextField)
		{
			m_InputField.SetPlaceholderText(base.ViewModel.InputPlaceholder);
			base.ViewModel.InputText.Subscribe(delegate(string value)
			{
				m_InputField.Text = value;
			}).AddTo(this);
			m_InputField.InputField.onValueChanged.AddListener(OnTextInputChanged);
			m_InputField.Select();
		}
	}

	protected override void DestroyTextField()
	{
		m_InputField.InputField.onValueChanged.RemoveListener(OnTextInputChanged);
		m_InputField.Abort();
	}

	protected override void BindProgressBar()
	{
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = InputLayerName
		};
		CreateInputImpl(m_InputLayer, m_HintsWidget);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		m_DeclineHint = hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, base.ViewModel.ShowDecline)).AddTo(this) as ConsoleHintDescription;
		m_ConfirmHint = hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, ConfirmBindActive.And(base.ViewModel.IsProgressBar.Not()).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this) as ConsoleHintDescription;
		inputLayer.AddButton(delegate
		{
			if (!base.ViewModel.ShowDecline.CurrentValue && !base.ViewModel.IsProgressBar.CurrentValue)
			{
				OnDeclineClick();
			}
		}, 9).AddTo(this);
		inputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
	}

	protected virtual void OnConfirmClick()
	{
		base.ViewModel.OnAcceptPressed();
	}

	protected virtual void OnDeclineClick()
	{
		base.ViewModel.OnDeclinePressed();
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	private void OnCurrentInputLayerChanged()
	{
		if (GamePad.Instance.CurrentInputLayer != m_InputLayer && !(GamePad.Instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName))
		{
			GamePad.Instance.PopLayer(m_InputLayer);
			GamePad.Instance.PushLayer(m_InputLayer);
		}
	}
}
