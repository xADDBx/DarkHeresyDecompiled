using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.TextTools;

public class TooltipStartTemplate : TextTemplate
{
	private readonly TooltipType m_Type;

	public override int MinParameters => 1;

	public override int MaxParameters => 1;

	public override int Balance => 1;

	private static GlossaryColors Colors => ConfigRoot.Instance.UIConfig.PaperGlossaryColors;

	public TooltipStartTemplate(TooltipType type)
	{
		m_Type = type;
	}

	public override string Generate(bool capitalized, List<string> parameters)
	{
		string text = ((parameters.Count > 0) ? parameters[0] : string.Empty);
		bool emptyLink = false;
		EntityLink.Type type;
		if (!UtilityLink.CheckLinkKeyHasContent(text))
		{
			type = EntityLink.Type.Empty;
			text = type.ToString();
			emptyLink = true;
		}
		else
		{
			type = EntityLink.GetEntityType(UtilityLink.GetKeysFromLink(text)[0]);
		}
		string color = GetColor(emptyLink, type);
		return "<link=\"" + text + "\"><color=" + color + "><b>";
	}

	private string GetColor(bool emptyLink, EntityLink.Type type = EntityLink.Type.Empty)
	{
		if (emptyLink)
		{
			return "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryEmpty);
		}
		if (type == EntityLink.Type.UnitFact)
		{
			return "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryMechanics);
		}
		return m_Type switch
		{
			TooltipType.Glosary => "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryGlossary), 
			TooltipType.Decisions => "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryDecisions), 
			TooltipType.Mechanics => "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryMechanics), 
			_ => "#" + ColorUtility.ToHtmlStringRGB((Color)Colors.GlossaryDefault), 
		};
	}
}
