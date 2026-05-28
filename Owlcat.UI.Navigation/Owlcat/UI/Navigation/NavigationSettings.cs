namespace Owlcat.UI.Navigation;

public struct NavigationSettings
{
	public NavigationFlags Flags;

	public IPointerProvider Pointer;

	public static IPointerProvider DefaultPointer { get; set; } = new DefaultPointerProvider();


	public static NavigationFlags DefaultFlags { get; set; }
}
