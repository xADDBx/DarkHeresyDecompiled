using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionSelectionWindowVM : ViewModel
{
	public readonly BlueprintCaseItem CaseItemFrom;

	public readonly ConclusionSelectionGroupVM SelectionVM;

	public readonly List<BlueprintConclusion.Source> Sources;

	public readonly ReactiveProperty<ConclusionSelectionEntityVM> SelectedConclusion = new ReactiveProperty<ConclusionSelectionEntityVM>();

	private readonly BlueprintConclusion m_ConclusionOnEnter;

	private readonly Action<bool> m_OnClose;

	public ConclusionSelectionWindowVM(BlueprintCaseItem caseItemFrom, Action<bool> onClose)
	{
		CaseItemFrom = caseItemFrom;
		m_OnClose = onClose;
		List<BlueprintConclusion> conclusionsFor = UIUtilityDetective.GetConclusionsFor(caseItemFrom);
		List<ConclusionSelectionEntityVM> list = conclusionsFor.Select(GetSelectionEntity).ToList();
		Sources = list.Select((ConclusionSelectionEntityVM c) => c.Source).Distinct().ToList();
		SelectionVM = new ConclusionSelectionGroupVM(list, SelectedConclusion).AddTo(this);
		SelectedConclusion.Value = SelectionVM.EntitiesCollection.FirstOrDefault((ConclusionSelectionEntityVM e) => UIUtilityDetective.Detective.HasConclusion(e.Conclusion));
		m_ConclusionOnEnter = SelectedConclusion.Value?.Conclusion;
		SelectedConclusion?.Skip(1).Subscribe(delegate
		{
			UISounds.Instance.Sounds.DetectiveSystem.ConclusionSelectionSelectConclusion.Play();
		}).AddTo(this);
		conclusionsFor.ForEach(delegate(BlueprintConclusion c)
		{
			UIUtilityDetective.ExaminedDetectiveData.ExaminedConclusions.AddExaminedEntityIfNeeded(c);
		});
	}

	public void Close(bool applySelection = false)
	{
		if (applySelection)
		{
			if (SelectedConclusion.Value?.Conclusion != null)
			{
				UIUtilityDetective.Detective.AddConclusion(SelectedConclusion.Value.Conclusion);
			}
			else if ((bool)m_ConclusionOnEnter)
			{
				UIUtilityDetective.Detective.RemoveConclusion(m_ConclusionOnEnter);
			}
			UISounds.Instance.Sounds.DetectiveSystem.ConclusionSelectionApplyConclusion.Play();
		}
		m_OnClose?.Invoke(applySelection);
		EventBus.RaiseEvent(delegate(IConclusionsUpdateHandler h)
		{
			h.UpdateConclusions();
		});
	}

	private ConclusionSelectionEntityVM GetSelectionEntity(BlueprintConclusion conclusion)
	{
		ExaminedDetectiveData.ExaminedData<ConclusionSourceWrapper> selected = UIUtilityDetective.ExaminedDetectiveData.SelectedConclusionSource;
		BlueprintConclusion.Source source = conclusion.Sources.FirstOrDefault((BlueprintConclusion.Source s) => selected.GetEntities().Any((ConclusionSourceWrapper e) => e.Is(s))) ?? UIUtilityDetective.GetSuitableConclusionSource(conclusion);
		if (source == null)
		{
			return null;
		}
		selected.AddExaminedEntityIfNeeded(new ConclusionSourceWrapper(source.Item1, source.Item2));
		return new ConclusionSelectionEntityVM(conclusion, source);
	}
}
