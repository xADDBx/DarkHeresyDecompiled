using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateFactionReputationLevel : TooltipBaseTemplate
{
	public FactionType FactionType;

	[CanBeNull]
	public string Status;

	public TooltipTemplateFactionReputationLevel(FactionType factionType, string status)
	{
		FactionType = factionType;
		Status = status;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIStrings.Instance.CommonTexts.Information, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickFactionStatus(null, UIStrings.Instance.CharacterSheet.GetFactionLabel(FactionType), Status)
		};
	}
}
