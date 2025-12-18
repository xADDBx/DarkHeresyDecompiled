using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
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
		m_RemoveSelectedConclusion.OnLeftClickAsObservable().Subscribe(base.ViewModel.RemoveSelectedConclusions).AddTo(this);
		m_RemoveSelectedConclusionLabel.text = UIStrings.Instance.DetectiveJournal.RemoveConclusionLabel.Text;
		m_ConclusionsContainer.DrawEntries(base.ViewModel.EntitiesCollection, m_ConclusionSelectionEntityPrefab).AddTo(this);
	}
}
