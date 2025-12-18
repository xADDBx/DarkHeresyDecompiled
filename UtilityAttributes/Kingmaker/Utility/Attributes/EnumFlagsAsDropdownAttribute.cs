using UnityEngine;

namespace Kingmaker.Utility.Attributes;

public class EnumFlagsAsDropdownAttribute : PropertyAttribute
{
	public string NoneName;

	public EnumFlagsAsDropdownAttribute()
	{
	}

	public EnumFlagsAsDropdownAttribute(string noneName)
	{
		NoneName = noneName;
	}
}
