using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateDetectiveAnswer : TooltipBaseTemplate
{
	public readonly BlueprintCaseAnswer Answer;

	public TooltipTemplateDetectiveAnswer(BlueprintCaseAnswer answer)
	{
		Answer = answer;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(UIStrings.Instance.DetectiveJournal.HypothesisLabel, TooltipTitleType.H2);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (!Game.Instance.DetectiveSystem.TryGetAnswerDegree(Answer, out var degree) || degree < 0)
		{
			yield break;
		}
		IEnumerable<BlueprintCaseItem> items = from item in Answer.DegreeProgression.ElementAt(degree).Items.Dereference()
			where !(item is BlueprintClue) && Game.Instance.DetectiveSystem.HasItemExcludingHidden(item)
			select item;
		if (!items.Any())
		{
			yield return new BrickTextVM(UIStrings.Instance.DetectiveJournal.NoStrongEvidence.Text);
			yield break;
		}
		yield return new BrickTextVM(UIStrings.Instance.DetectiveJournal.HasSomeEvidence.Text);
		foreach (BlueprintCaseItem item in items)
		{
			if (!(item is BlueprintClue))
			{
				yield return new BrickCaseItemVM(item, forceHideContradiction: false);
			}
		}
	}
}
