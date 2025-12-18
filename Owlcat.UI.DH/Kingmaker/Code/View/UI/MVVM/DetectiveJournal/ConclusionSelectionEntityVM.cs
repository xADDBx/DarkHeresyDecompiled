using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionSelectionEntityVM : SelectionGroupEntityVM
{
	public readonly BlueprintConclusion Conclusion;

	public readonly BlueprintConclusion.Source Source;

	private readonly ReactiveProperty<bool> m_IsViewed = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsViewed => m_IsViewed;

	public ConclusionSelectionEntityVM(BlueprintConclusion conclusion, BlueprintConclusion.Source source)
		: base(allowSwitchOff: false)
	{
		Conclusion = conclusion;
		Source = source;
		m_IsViewed.Value = !UIUtilityDetective.ExaminedDetectiveData.ExaminedConclusions.IsEntityNew(Conclusion);
		UIUtilityDetective.ExaminedDetectiveData.ExaminedConclusions.AddExaminedEntityIfNeeded(Conclusion);
	}

	protected override void DoSelectMe()
	{
		m_IsViewed.Value = true;
	}
}
