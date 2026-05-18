using Kingmaker.Blueprints.Root.Strings;
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
	private HintView m_FirstOpenBugReportHint;

	[SerializeField]
	private HintView m_SecondOpenBugReportHint;

	[SerializeField]
	private HintView m_SendHint;

	protected override void OnBind()
	{
		base.OnBind();
		m_OpenBugReportText.text = UIStrings.Instance.UIBugReport.OpenBugReportText;
	}

	protected void CreateInputImpl()
	{
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
