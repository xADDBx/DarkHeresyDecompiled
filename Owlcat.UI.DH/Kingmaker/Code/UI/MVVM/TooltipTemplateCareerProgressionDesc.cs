using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateCareerProgressionDesc : TooltipBaseTemplate
{
	private readonly CareerPathVM m_CareerPath;

	private UITextCharSheet Strings => UIStrings.Instance.CharacterSheet;

	public TooltipTemplateCareerProgressionDesc(CareerPathVM careerPath)
	{
		m_CareerPath = careerPath;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		LocalizedString careerUpgradeHeader = Strings.CareerUpgradeHeader;
		yield return new BrickTitleVM(careerUpgradeHeader.Text);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_CareerPath.CanCommit.CurrentValue)
		{
			yield return new BrickTextVM(UIStrings.Instance.CharacterSheet.CareerUpgradedDescription, TooltipTextType.Simple, TooltipTextAlignment.Midl, m_CareerPath.Unit);
			yield break;
		}
		LocalizedString careerUpgradeDescription = Strings.CareerUpgradeDescription;
		yield return new BrickTextVM(careerUpgradeDescription, TooltipTextType.Simple, TooltipTextAlignment.Midl, m_CareerPath.Unit);
	}
}
