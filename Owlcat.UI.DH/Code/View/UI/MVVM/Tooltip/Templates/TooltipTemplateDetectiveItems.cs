using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateDetectiveItems : TooltipBaseTemplate
{
	public readonly DialogDetectiveCaseLink[] Items;

	private readonly Dictionary<BlueprintCase, List<DialogDetectiveCaseLink>> m_ItemsByCase;

	public TooltipTemplateDetectiveItems(IEnumerable<DialogDetectiveCaseLink> items)
	{
		Items = items.ToArray();
		m_ItemsByCase = new Dictionary<BlueprintCase, List<DialogDetectiveCaseLink>>();
		DialogDetectiveCaseLink[] items2 = Items;
		for (int i = 0; i < items2.Length; i++)
		{
			DialogDetectiveCaseLink item = items2[i];
			if (!m_ItemsByCase.ContainsKey(item.Case))
			{
				m_ItemsByCase.Add(item.Case, new List<DialogDetectiveCaseLink>());
			}
			if (item.Item != null)
			{
				m_ItemsByCase[item.Case].Add(item);
			}
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(UIStrings.Instance.Tooltips.RelatedDetectiveItemsTitle.Text, TooltipTitleType.H2);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		DetectiveJournalDecor decor = UIStrings.Instance.DetectiveDecor;
		yield return new BrickTextVM(UIStrings.Instance.Tooltips.RelatedDetectiveItemsDescription.Text);
		using (GameLogContext.Scope)
		{
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
			foreach (KeyValuePair<BlueprintCase, List<DialogDetectiveCaseLink>> caseWithItems in m_ItemsByCase)
			{
				GameLogContext.Case = caseWithItems.Key;
				yield return new BrickUnifiedStatusVM(UnifiedStatus.Detective, decor.CaseName);
				foreach (DialogDetectiveCaseLink item in caseWithItems.Value)
				{
					yield return new BrickCaseItemVM(item.Item);
				}
			}
		}
	}
}
