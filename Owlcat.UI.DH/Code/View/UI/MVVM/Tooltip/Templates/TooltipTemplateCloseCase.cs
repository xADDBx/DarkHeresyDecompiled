using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateCloseCase : TooltipBaseTemplate
{
	public readonly List<DialogDetectiveCloseCaseData> CasesToClose;

	public TooltipTemplateCloseCase(IEnumerable<DialogDetectiveCloseCaseData> casesToClose)
	{
		CasesToClose = casesToClose.ToList();
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(UIStrings.Instance.Tooltips.DetectiveCloseCaseDataTitle.Text, TooltipTitleType.H2);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return CasesToClose.SelectMany(GetCaseToCloseBricks);
	}

	private IEnumerable<ITooltipBrick> GetCaseToCloseBricks(DialogDetectiveCloseCaseData data)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
			GameLogContext.Case = data.Case;
			GameLogContext.CaseAnswer = data.Answer;
			yield return new BrickUnifiedStatusVM(UnifiedStatus.Detective, data.Case.Name);
			yield return new BrickTextVM(Game.Instance.DetectiveSystem.GetActualCaseQuestion(data.Case).Name);
			if (Game.Instance.DetectiveSystem.TryGetAnswerDegree(data.Answer, out var _))
			{
				yield return new BrickSeparatorVM(TooltipBrickElementType.Medium);
				yield return new BrickTextVM(UIStrings.Instance.DetectiveJournal.ClosedVerdict.Text);
			}
		}
	}
}
