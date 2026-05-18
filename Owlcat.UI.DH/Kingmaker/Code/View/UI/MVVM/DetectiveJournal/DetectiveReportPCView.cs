using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveReportPCView : DetectiveReportBaseView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatMultiButton m_BackButton;

	[SerializeField]
	private GameObject m_SendReportButtonContainer;

	[SerializeField]
	private OwlcatMultiButton m_SendReportButton;

	[SerializeField]
	private OwlcatMultiButton m_StampButton;

	[SerializeField]
	private TMP_Text m_SendReportLabel;

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_BackButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		m_SendReportButton.Interactable = false;
		m_StampButton.Interactable = false;
		ObservableSubscribeExtensions.Subscribe(m_SendReportButton.OnLeftClickAsObservable(), delegate
		{
			OnButtonClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_StampButton.OnLeftClickAsObservable(), delegate
		{
			OnButtonClick();
		}).AddTo(this);
		m_SendReportButtonContainer.Or(null)?.gameObject.SetActive(!base.ViewModel.BlueprintCase.IsClosed());
		base.ViewModel.QuestionIsAnswered.Skip(1).Subscribe(delegate
		{
			UpdateSendReportState();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(0.6000000238418579), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			UpdateSendReportState();
		}).AddTo(this);
		m_IsStamping.Subscribe(delegate(bool value)
		{
			m_SendReportLabel.text = (value ? UIStrings.Instance.DetectiveJournal.ConfirmSendReportLabel : UIStrings.Instance.DetectiveJournal.SendReportLabel);
			m_StampingAnimator.SetBool(DetectiveReportBaseView.IsReady, value);
		}).AddTo(this);
		base.OnBind();
	}

	private void UpdateSendReportState()
	{
		m_SendReportButton.gameObject.SetActive(!base.ViewModel.BlueprintCase.IsClosed());
		if (base.ViewModel.ReportContext.SelectedAnswer.CurrentValue == null)
		{
			m_SendReportButton.Interactable = false;
			m_StampButton.Interactable = false;
			m_SendReportButton.SetHint(UIStrings.Instance.DetectiveJournal.CannotSendReportHint).AddTo(this);
		}
		else
		{
			bool interactable = base.ViewModel.IsInteractable && base.ViewModel.QuestionIsAnswered.CurrentValue;
			m_SendReportButton.Interactable = interactable;
			m_StampButton.Interactable = interactable;
			LocalizedString localizedString = (base.ViewModel.QuestionIsAnswered.CurrentValue ? UIStrings.Instance.DetectiveJournal.SendReportHint : UIStrings.Instance.DetectiveJournal.CompleteReportHint);
			m_SendReportButton.SetHint(localizedString).AddTo(this);
		}
	}

	private void OnButtonClick()
	{
		if (base.ViewModel.BlueprintCase.IsClosed())
		{
			return;
		}
		if (m_IsStamping.Value)
		{
			m_StampingAnimator.SetBool(DetectiveReportBaseView.Stamping, value: true);
			ObservableSubscribeExtensions.Subscribe(base.StampAnimationFinishedAsObservable.Take(1), delegate
			{
				base.ViewModel.SendReport();
			}).AddTo(this);
		}
		else
		{
			m_IsStamping.Value = true;
		}
	}
}
