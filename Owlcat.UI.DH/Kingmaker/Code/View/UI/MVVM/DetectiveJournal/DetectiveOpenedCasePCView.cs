using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveOpenedCasePCView : DetectiveOpenedCaseBaseView
{
	[Header("PC")]
	[SerializeField]
	private TMP_Text m_PrepareReportLabel;

	[SerializeField]
	private OwlcatMultiButton m_PrepareReportButton;

	private IDisposable m_ButtonHint;

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_PrepareReportButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.PrepareReport();
		}).AddTo(this);
		TMP_Text prepareReportLabel = m_PrepareReportLabel;
		BlueprintCase blueprintCase = base.ViewModel.BlueprintCase;
		prepareReportLabel.text = ((blueprintCase != null && blueprintCase.IsClosed()) ? UIStrings.Instance.DetectiveJournal.WatchReportLabel.Text : UIStrings.Instance.DetectiveJournal.PrepareReportLabel.Text);
		m_PrepareReportButton.gameObject.SetActive(base.ViewModel.BlueprintCase != null && !base.ViewModel.BlueprintCase.IsFailed());
		base.ViewModel.CanOpenReport.Subscribe(delegate(bool value)
		{
			m_PrepareReportButton.Interactable = value;
			m_ButtonHint?.Dispose();
			if (!value)
			{
				m_ButtonHint = m_PrepareReportButton.SetHint(UIStrings.Instance.DetectiveJournal.CannotPrepareReport).AddTo(this);
			}
		}).AddTo(this);
		base.OnBind();
	}
}
