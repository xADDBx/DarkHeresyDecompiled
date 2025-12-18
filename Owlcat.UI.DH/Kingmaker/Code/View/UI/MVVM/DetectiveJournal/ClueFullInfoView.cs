using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueFullInfoView : View<ClueFullInfoVM>
{
	[Header("Views")]
	[SerializeField]
	private ClueInformationScreenView m_ScreenView;

	[SerializeField]
	private TypeToPrefabSelector<ClueInformationBaseView> m_ClueInfoPrefabs;

	[SerializeField]
	private NewStudyView m_NewStudyView;

	[SerializeField]
	private ScreenAddendumView m_AddendumPrefab;

	[SerializeField]
	private AnalysisButtonView m_FirstAnalysisButtonPrefab;

	[Header("No Addendums Elements")]
	[SerializeField]
	private GameObject m_NoAddendumsContainer;

	[SerializeField]
	private TMP_Text m_NoAddendumsLabel;

	[FormerlySerializedAs("m_NoAddendumsCallToActionParent")]
	[SerializeField]
	private GameObject m_NoAddendumsHideIfExplore;

	[SerializeField]
	private TMP_Text m_NoAddendumsCallToAction;

	[SerializeField]
	private Animator m_CodeAnimator;

	[Header("Elements")]
	[SerializeField]
	private RectTransform m_AnalysisButtonParent;

	[SerializeField]
	private RectTransform m_ClueParent;

	[SerializeField]
	private RectTransform m_CluePaperParent;

	[SerializeField]
	private RectTransform m_AddendumsListLeft;

	[SerializeField]
	private RectTransform m_AddendumsListRight;

	[SerializeField]
	private List<OwlcatMultiButton> m_CloseButton;

	private ClueInformationBaseView m_CurrentClueView;

	private AnalysisButtonView m_FirstAnalysisButton;

	private readonly List<ScreenAddendumView> m_AddendumViews = new List<ScreenAddendumView>();

	protected override void OnBind()
	{
		m_NoAddendumsCallToAction.text = UIStrings.Instance.DetectiveJournal.NoAddendumsCallToAction.Text;
		m_NoAddendumsLabel.text = UIStrings.Instance.DetectiveJournal.NoAddendumsLabel.Text;
		if (base.ViewModel.AddendumVMs.Count == 0)
		{
			m_CodeAnimator.StartPlayback();
		}
		m_ScreenView.Bind(base.ViewModel.BlueprintClue);
		ClueInformationBaseView entity = m_ClueInfoPrefabs.GetEntity(base.ViewModel.UIData.UIType);
		m_CurrentClueView = WidgetFactory.GetWidget(entity, activate: true, strictMatching: true);
		RectTransform parent = (m_CurrentClueView.IsPaperInfo ? m_CluePaperParent : m_ClueParent);
		m_CurrentClueView.transform.SetParent(parent, worldPositionStays: false);
		m_CurrentClueView.Bind(base.ViewModel.BlueprintClue);
		base.ViewModel.StudyVM.Subscribe(delegate(DetectiveStudyVM value)
		{
			m_FirstAnalysisButton?.Unbind();
			WidgetFactory.DisposeWidget(m_FirstAnalysisButton);
			m_FirstAnalysisButton = null;
			m_NoAddendumsHideIfExplore.Or(null)?.SetActive(value == null);
			m_AnalysisButtonParent.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_FirstAnalysisButton = WidgetFactory.GetWidget(m_FirstAnalysisButtonPrefab, activate: true, strictMatching: true);
				m_FirstAnalysisButton.transform.SetParent(m_AnalysisButtonParent, worldPositionStays: false);
				m_FirstAnalysisButton.Bind(value);
			}
		}).AddTo(this);
		base.ViewModel.AddendumVMs.ObserveCountChanged().Subscribe(delegate
		{
			DrawAddendums();
		}).AddTo(this);
		DrawAddendums();
		m_CloseButton.ForEach(delegate(OwlcatMultiButton b)
		{
			ObservableSubscribeExtensions.Subscribe(b.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.Close();
			}).AddTo(this);
		});
		base.ViewModel.StudyToMake.Subscribe(delegate(NewStudyVM value)
		{
			if (value != null)
			{
				m_FirstAnalysisButton?.Unbind();
				WidgetFactory.DisposeWidget(m_FirstAnalysisButton);
				m_FirstAnalysisButton = null;
			}
			m_NewStudyView.Bind(value);
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		WidgetFactory.DisposeWidget(m_CurrentClueView);
		WidgetFactory.DisposeWidget(m_FirstAnalysisButton);
		m_FirstAnalysisButton = null;
		m_AddendumViews.ForEach(WidgetFactory.DisposeWidget);
		m_AddendumViews.Clear();
		base.gameObject.SetActive(value: false);
	}

	private void DrawAddendums()
	{
		m_NoAddendumsContainer.SetActive(base.ViewModel.AddendumVMs.Count == 0);
		m_AddendumViews.ForEach(WidgetFactory.DisposeWidget);
		m_AddendumViews.Clear();
		for (int i = 0; i < base.ViewModel.AddendumVMs.Count; i++)
		{
			AddendumInfoVM addendum = base.ViewModel.AddendumVMs[i];
			ScreenAddendumView widget = WidgetFactory.GetWidget(m_AddendumPrefab);
			widget.Bind(addendum);
			widget.SetAddendumId(i + 1);
			widget.Show((base.ViewModel.StudyToMake.CurrentValue?.CurrentGroup.CurrentValue.NewAddendums.Any((AddendumInfoVM a) => a.Info.Equals(addendum.Info))).GetValueOrDefault());
			m_AddendumViews.Add(widget);
		}
		int num = 0;
		int num2 = 0;
		foreach (ScreenAddendumView addendumView in m_AddendumViews)
		{
			bool flag = num <= num2;
			addendumView.transform.SetParent(flag ? m_AddendumsListLeft : m_AddendumsListRight, worldPositionStays: false);
			if (flag)
			{
				num += addendumView.BlocksCount;
			}
			else
			{
				num2 += addendumView.BlocksCount;
			}
		}
	}
}
