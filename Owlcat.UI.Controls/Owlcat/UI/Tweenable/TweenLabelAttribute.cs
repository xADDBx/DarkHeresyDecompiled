using System;

namespace Owlcat.UI.Tweenable;

public class TweenLabelAttribute : Attribute
{
	public string Label;

	public TweenLabelAttribute(string label)
	{
		Label = label;
	}
}
