using System;

namespace Owlcat.UI.Navigation;

[Flags]
public enum NavigationFlags
{
	None = 0,
	DontPointer = 1,
	DontOrthogonal = 2,
	DontImplicitNavigation = 3,
	DontHierarhyNavigation = 4,
	DontFloatNavigation = 5
}
