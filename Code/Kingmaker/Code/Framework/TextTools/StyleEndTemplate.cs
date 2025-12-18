using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.Code.Framework.TextTools;

public class StyleEndTemplate : TextTemplate
{
	public override int Balance => -1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		return "</style>";
	}
}
