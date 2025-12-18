using System;

namespace Owlcat.Runtime.Core.Utility.EditorAttributes;

public class OwlcatToolAttribute : Attribute
{
	public string Name;

	public OwlcatToolAttribute(string name)
	{
		Name = name;
	}
}
