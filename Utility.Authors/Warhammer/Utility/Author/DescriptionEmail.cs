using System;
using System.ComponentModel;

namespace Warhammer.Utility.Author;

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionEmail : DescriptionAttribute
{
	public DescriptionEmail(string email)
		: base(email)
	{
	}
}
