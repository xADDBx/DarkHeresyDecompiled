using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ReportAnswerVM : SelectionGroupEntityVM
{
	public readonly BlueprintCase BlueprintCase;

	public readonly BlueprintCaseAnswer Answer;

	private readonly ReactiveProperty<bool> m_IsNew = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsNew => m_IsNew;

	public ReportAnswerVM(BlueprintCase blueprintCase, BlueprintCaseAnswer answer)
		: base(allowSwitchOff: true)
	{
		BlueprintCase = blueprintCase;
		Answer = answer;
		m_IsNew.Value = UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.IsEntityNew(answer);
	}

	protected override void DoSelectMe()
	{
		UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.SetSelectedAnswer(BlueprintCase, Answer);
		m_IsNew.Value = false;
	}
}
