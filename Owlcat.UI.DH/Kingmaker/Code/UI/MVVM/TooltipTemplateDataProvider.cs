using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UIDataProvider;
using Owlcat.UI;
using TMPro;
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
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = m_DataProvider.Name,
			TextParams = new TextFieldParams
			{
				FontStyles = FontStyles.Bold
			}
		};
		yield return new TooltipBrickIconPattern(icon, null, titleValues);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(UIUtilityText.UpdateDescriptionWithUIProperties(m_DataProvider.Description, null), TooltipTextType.Paragraph);
	}
}
