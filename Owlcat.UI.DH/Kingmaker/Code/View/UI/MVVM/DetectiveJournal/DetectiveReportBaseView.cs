using Kingmaker.Code.UI.MVVM;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveReportBaseView : View<DetectiveReportVM>
{
	[Header("Views")]
	[SerializeField]
	private CaseCardBaseView m_CardView;

	[SerializeField]
	private DetectivePaperReportView m_PaperReportView;

	[SerializeField]
	private AnswerSelectionBaseView QuestionSelectionBase;

	[Header("Visual")]
	[SerializeField]
	private GameObject m_PaperReportParent;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected Animator m_StampingAnimator;

	protected readonly ReactiveProperty<bool> m_IsStamping = new ReactiveProperty<bool>();

	protected static readonly int IsReady = Animator.StringToHash("IsReady");

	protected static readonly int Stamping = Animator.StringToHash("Stamping");

	protected static readonly int Closed = Animator.StringToHash("Closed");

	public Observable<Unit> StampAnimationFinishedAsObservable => m_PaperReportView.StampAnimationFinishedAsObservable;

	protected override void OnBind()
	{
		m_CardView.Bind(base.ViewModel.CaseCardVM);
		m_PaperReportView.Bind(base.ViewModel.PaperReportVM);
		QuestionSelectionBase.Bind(base.ViewModel.AnswerSelectionVM);
		m_PaperReportParent.gameObject.SetActive(value: true);
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		base.gameObject.SetActive(value: true);
		SetVisible(state: true);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.AnimateHideCommand, delegate
		{
			base.ViewModel.Close(reportWasSent: true);
		}).AddTo(this);
		m_StampingAnimator.SetBool(Closed, base.ViewModel.BlueprintCase.IsClosed());
		base.ViewModel.QuestionIsAnswered.Subscribe(delegate(bool value)
		{
			if (!value)
			{
				m_IsStamping.Value = false;
			}
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.OnDisable();
		base.gameObject.SetActive(value: false);
		m_PaperReportParent.gameObject.SetActive(value: false);
		SetVisible(state: false);
		m_IsStamping.Value = false;
	}

	private void SetVisible(bool state)
	{
		if (state)
		{
			m_FadeAnimator.AppearAnimation();
		}
		else
		{
			m_FadeAnimator.DisappearAnimation();
		}
	}
}
