using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;

namespace Code.View.UI.MVVM.Dialog;

public class EpilogTrueAnswerVM : ViewModel
{
	public readonly BlueprintCase BlueprintCase;

	public readonly ReactiveProperty<TrueAnswerEntityVM> TrueAnswer = new ReactiveProperty<TrueAnswerEntityVM>();

	public EpilogTrueAnswerVM(DetectiveCasePage casePage)
	{
		BlueprintCase = casePage.BlueprintCase;
		TrueAnswer.Value = new TrueAnswerEntityVM(casePage);
	}
}
