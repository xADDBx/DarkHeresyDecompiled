namespace Owlcat.Runtime.Visual.XPBD.Layouts;

public static class ServiceFlagsExtensions
{
	public static bool HasFlag(this uint flags, ServiceFlags flag)
	{
		return (flags & (uint)flag) != 0;
	}
}
