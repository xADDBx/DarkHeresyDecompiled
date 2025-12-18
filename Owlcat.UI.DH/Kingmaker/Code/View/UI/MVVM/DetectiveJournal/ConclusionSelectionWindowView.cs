using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionSelectionWindowView : View<ConclusionSelectionWindowVM>
{
	[Header("Views")]
	[SerializeField]
	private ConclusionEntityFromView m_EntityFromView;

	[SerializeField]
	private ConclusionSelectionGroupView m_SelectionGroupView;

	[SerializeField]
	private ConclusionEntitiesToView m_EntitiesToView;

	[FormerlySerializedAs("m_NewWindowsLines")]
	[SerializeField]
	private ConclusionWindowsLines WindowsLines;

	[SerializeField]
	private ConstructConclusionScreenView m_ScreenView;

	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private TMP_Text m_CloseButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_AcceptButton;

	[SerializeField]
	private TMP_Text m_AcceptLabel;

	protected override void OnBind()
	{
		m_ScreenView.Bind(base.ViewModel.CaseItemFrom);
		m_EntityFromView.Bind(base.ViewModel.CaseItemFrom);
		m_SelectionGroupView.Bind(base.ViewModel.SelectionVM);
		m_EntitiesToView.Bind(base.ViewModel.Sources);
		m_CloseButtonLabel.text = UIStrings.Instance.CommonTexts.Back.Text;
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		m_AcceptLabel.text = UIStrings.Instance.CommonTexts.Accept.Text;
		ObservableSubscribeExtensions.Subscribe(m_AcceptButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close(applySelection: true);
		}).AddTo(this);
		base.gameObject.SetActive(value: true);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			WindowsLines.DrawLinesFrom(m_EntityFromView.LineFrom, m_SelectionGroupView.Conclusions);
			WindowsLines.DrawLinesTo(m_SelectionGroupView.Conclusions, m_EntitiesToView.LineDirections);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		WindowsLines.Clear();
	}
}
