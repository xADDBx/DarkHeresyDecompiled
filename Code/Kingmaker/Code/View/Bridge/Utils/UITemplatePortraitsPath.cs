using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.TextTools.Base;

namespace Kingmaker.Code.View.Bridge.Utils;

public class UITemplatePortraitsPath : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return UtilityChargen.GetPortraitsPath();
	}
}
