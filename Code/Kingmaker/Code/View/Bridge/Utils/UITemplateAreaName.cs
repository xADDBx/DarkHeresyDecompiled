using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.Code.View.Bridge.Utils;

public class UITemplateAreaName : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (Game.Instance.CurrentlyLoadedArea == null)
		{
			return string.Empty;
		}
		return Game.Instance.CurrentlyLoadedArea.AreaDisplayName;
	}
}
