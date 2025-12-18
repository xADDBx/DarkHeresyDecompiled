using System;
using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class StringValueTemplate : TextTemplate
{
	private readonly Func<string> m_GetValue;

	public StringValueTemplate(Func<string> getValue)
	{
		m_GetValue = getValue;
	}

	public override string Generate(bool capitalized, List<string> parameters)
	{
		return m_GetValue();
	}
}
