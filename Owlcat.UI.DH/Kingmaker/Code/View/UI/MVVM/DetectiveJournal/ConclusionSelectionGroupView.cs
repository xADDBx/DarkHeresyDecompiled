using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionSelectionGroupView : View<ConclusionSelectionGroupVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_RemoveSelectedConclusion;

	[SerializeField]
	private TMP_Text m_RemoveSelectedConclusionLabel;

	[SerializeField]
	private WidgetList m_ConclusionsContainer;

	[Header("Views")]
	[SerializeField]
	private ConclusionSelectionEntityView m_ConclusionSelectionEntityPrefab;

	public List<ConclusionSelectionEntityView> Conclusions => m_ConclusionsContainer.Entries.Cast<ConclusionSelectionEntityView>().ToList();

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(m_RemoveSelectedConclusion.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.RemoveSelectedConclusions();
			ServiceWindowsSounds.Instance.DetectiveJournal.RemoveConclusion.Play();
		}).AddTo(this);
		m_RemoveSelectedConclusionLabel.text = UIStrings.Instance.DetectiveJournal.RemoveConclusionLabel.Text;
		m_ConclusionsContainer.DrawEntries(base.ViewModel.EntitiesCollection, m_ConclusionSelectionEntityPrefab).AddTo(this);
		m_RemoveSelectedConclusion.SetClickSound(ButtonSoundsEnum.NoSound);
	}
}
