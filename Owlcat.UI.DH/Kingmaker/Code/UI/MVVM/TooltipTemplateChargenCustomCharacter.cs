using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateChargenCustomCharacter : TooltipBaseTemplate
{
	private readonly CharGenMode m_Mode;

	private readonly CharGenCompanionType m_CompanionType;

	public TooltipTemplateChargenCustomCharacter(CharGenMode mode, CharGenCompanionType companionType)
	{
		m_Mode = mode;
		m_CompanionType = companionType;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Mode == CharGenMode.NewCompanion)
		{
			LocalizedString localizedString = ((m_CompanionType == CharGenCompanionType.Navigator) ? UIStrings.Instance.CharGen.CreateNewNavigator : UIStrings.Instance.CharGen.CreateNewCompanion);
			list.Add(new TooltipBrickTitle(localizedString));
		}
		else
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.NewGameWin.CreateNewCharacter));
		}
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Mode == CharGenMode.NewCompanion)
		{
			LocalizedString localizedString = ((m_CompanionType == CharGenCompanionType.Navigator) ? UIStrings.Instance.CharGen.CreateNewNavigatorDescription : UIStrings.Instance.CharGen.CreateNewCompanionDescription);
			list.Add(new TooltipBrickText(localizedString));
		}
		else
		{
			list.Add(new TooltipBrickText(UIStrings.Instance.NewGameWin.CreateNewCharacterDescription));
		}
		return list;
	}
}
