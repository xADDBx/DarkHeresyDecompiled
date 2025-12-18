namespace Kingmaker.Enums;

public static class ContextPropertyNameExtension
{
	public static bool IsModifier(this ContextPropertyName name)
	{
		return name >= ContextPropertyName.ModValue1;
	}
}
