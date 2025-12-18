using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionSelectionGroupVM : SelectionGroupRadioVM<ConclusionSelectionEntityVM>
{
	public ConclusionSelectionGroupVM(List<ConclusionSelectionEntityVM> visibleCollection, ReactiveProperty<ConclusionSelectionEntityVM> entity = null, bool cyclical = false)
		: base(visibleCollection, entity, cyclical)
	{
		foreach (BlueprintConclusion item in visibleCollection.Select((ConclusionSelectionEntityVM x) => x.Conclusion))
		{
			UIUtilityDetective.ExaminedDetectiveData.SelectedConclusions.AddExaminedEntityIfNeeded(item);
		}
	}

	public void RemoveSelectedConclusions()
	{
		SelectedEntity.Value = null;
	}
}
