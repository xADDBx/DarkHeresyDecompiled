using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewStudyView : View<NewStudyVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_ClosePanelButton;

	[SerializeField]
	private TMP_Text m_ClosePanelLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_ActorSelectable;

	[SerializeField]
	private Image m_CompanionPortrait;

	[SerializeField]
	private ScrambledTMP m_BarkDescription;

	[SerializeField]
	private WidgetList m_NewAddendumsTitles;

	[Header("Views")]
	[SerializeField]
	private AnalysisButtonView m_AnalysisButton;

	[SerializeField]
	private AddendumTitleView m_AddendumTitlePrefab;

	protected override void OnBind()
	{
		m_ClosePanelButton.gameObject.SetActive(value: false);
		m_AnalysisButton.gameObject.SetActive(value: false);
		m_ClosePanelButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close).AddTo(this);
		base.ViewModel.CurrentGroup.Subscribe(UpdateStudiesGroup).AddTo(this);
		base.ViewModel.NextStudy.Subscribe(m_AnalysisButton.Bind).AddTo(this);
		base.ViewModel.NewAddendums.ObserveCountChanged().Subscribe(delegate
		{
			DrawTitles();
		}).AddTo(this);
		DrawTitles();
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_BarkDescription.SetTextWithoutAnimation(string.Empty);
	}

	private void UpdateStudiesGroup(StudyGroup group)
	{
		if (group != null)
		{
			m_NewAddendumsTitles.gameObject.SetActive(value: false);
			m_ClosePanelLabel.text = (base.ViewModel.HasNextGroup ? UIStrings.Instance.DetectiveJournal.NextStudy : UIStrings.Instance.DetectiveJournal.FinishStudies);
			m_BarkDescription.SetText("###############", group.BarkText);
			m_ActorSelectable.SetActiveLayer((group.Companion != null) ? 1 : 0);
			m_CompanionPortrait.sprite = group.Companion?.PortraitSafe.HalfLengthPortrait;
			m_AnalysisButton.gameObject.SetActive(base.ViewModel.HasNextGroup);
			m_ClosePanelButton.gameObject.SetActive(!base.ViewModel.HasNextGroup);
			ObservableSubscribeExtensions.Subscribe(Observable.Timer(1f.Seconds(), UnityTimeProvider.TimeUpdateIgnoreTimeScale), delegate
			{
				m_NewAddendumsTitles.gameObject.SetActive(value: true);
			}).AddTo(this);
		}
	}

	private void DrawTitles()
	{
		m_NewAddendumsTitles.Clear();
		m_NewAddendumsTitles.DrawEntries(base.ViewModel.NewAddendums, m_AddendumTitlePrefab).AddTo(this);
	}
}
