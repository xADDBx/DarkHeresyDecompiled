using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class NarratorStartTemplate : TextTemplate
{
	public override int Balance => 1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		return "<i><color=#" + DialogCueColors.NarratorColorStringID + ">";
	}
}
