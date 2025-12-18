using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BugReportConsoleView : BugReportBaseView
{
	[Header("Hints")]
	[SerializeField]
	private CanvasGroup m_OpenBugReportGroup;

	[SerializeField]
	private TextMeshProUGUI m_OpenBugReportText;

	[SerializeField]
	private ConsoleHint m_FirstOpenBugReportHint;

	[SerializeField]
	private ConsoleHint m_SecondOpenBugReportHint;

	[SerializeField]
	private ConsoleHint m_SendHint;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	protected override void OnBind()
	{
		base.OnBind();
		m_OpenBugReportText.text = UIStrings.Instance.UIBugReport.OpenBugReportText;
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		base.BuildNavigationImpl(navigationBehaviour);
		navigationBehaviour.FocusOnEntityManual(m_ContextDropdown);
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		base.CreateInputImpl(inputLayer);
		m_FirstOpenBugReportHint.BindCustomAction(18, inputLayer).AddTo(this);
		m_SecondOpenBugReportHint.BindCustomAction(19, inputLayer).AddTo(this);
		m_SendHint.SetLabel(UIStrings.Instance.UIBugReport.SendButton);
		m_SendHint.Bind(m_InputLayer.AddButton(delegate
		{
			OnSend();
		}, 10, InputActionEventType.ButtonJustLongPressed)).AddTo(this);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnClose();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnShowDrawing();
		}, 11, InputActionEventType.ButtonJustReleased), UIStrings.Instance.UIBugReport.EditScreenShotTitleText).AddTo(this);
		m_PrivacyToggle.IsOn.Subscribe(OnPrivacyToggle).AddTo(this);
		m_InputLayer.LayerBinded.Subscribe(OnLayerBinded).AddTo(this);
	}

	private void OnLayerBinded(bool value)
	{
		m_OpenBugReportGroup.alpha = (value ? 1 : 0);
	}

	private void OnPrivacyToggle(bool isOn)
	{
		TrySetSendAlpha(isOn ? 1f : 0.2f);
	}

	private void TrySetSendAlpha(float value)
	{
		if (!(m_SendHint == null))
		{
			MonoBehaviour sendHint = m_SendHint;
			if ((object)sendHint != null && sendHint.TryGetComponent<CanvasGroup>(out var component))
			{
				component.alpha = value;
			}
		}
	}
}
