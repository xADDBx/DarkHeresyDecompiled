using System;

namespace Owlcat.Runtime.Visual.Waaagh;

[AttributeUsage(AttributeTargets.Field)]
internal class WaaaghProfileCategoryAttribute : Attribute
{
	public WaaaghProfileCategory Category { get; set; }

	public WaaaghProfileCategoryAttribute(WaaaghProfileCategory category)
	{
		Category = category;
	}
}
