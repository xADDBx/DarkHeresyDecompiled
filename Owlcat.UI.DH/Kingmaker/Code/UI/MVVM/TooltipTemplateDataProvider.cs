using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UIDataProvider;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateDataProvider : TooltipBaseTemplate
{
	private readonly IUIDataProvider m_DataProvider;

	public TooltipTemplateDataProvider(IUIDataProvider dataProvider)
	{
		m_DataProvider = dataProvider;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		Sprite icon = ((m_DataProvider.Icon != null) ? m_DataProvider.Icon : UIUtilityText.GetIconByText(m_DataProvider.NameForAcronym));
		TextEntity title = new TextEntity(m_DataProvider.Name, TextFieldParams.Bold);
		yield return new BrickIconPatternVM(icon, null, title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new BrickTextVM(m_DataProvider.Description, TooltipTextType.Paragraph);
	}
}
