using System;

namespace Owlcat.BehaviourTrees;

[AttributeUsage(AttributeTargets.Class)]
public class NodeMenuItemAttribute : Attribute
{
	public string MenuPath { get; }

	public string DefaultTitle { get; }

	public NodeMenuItemAttribute(string menuPath, string defaultTitle)
	{
		MenuPath = menuPath;
		DefaultTitle = defaultTitle;
	}
}
