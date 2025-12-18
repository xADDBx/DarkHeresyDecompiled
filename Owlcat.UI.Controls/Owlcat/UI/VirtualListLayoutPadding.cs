using System;

namespace Owlcat.UI;

[Serializable]
public struct VirtualListLayoutPadding
{
	public float Top;

	public float Bottom;

	public float Left;

	public float Right;

	public static VirtualListLayoutPadding Zero { get; }
}
