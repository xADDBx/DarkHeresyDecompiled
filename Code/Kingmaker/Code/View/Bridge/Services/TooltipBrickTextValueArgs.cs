using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.View.Bridge.Services;

public readonly struct TooltipBrickTextValueArgs
{
	public string Text { get; }

	public string Value { get; }

	public int NestedLevel { get; }

	public bool IsResultValue { get; }

	public bool NeedShowLine { get; }

	public TooltipTextType TextType { get; }

	public TooltipTextAlignment Alignment { get; }

	public TooltipBrickTextValueArgs(string text, string value, int nestedLevel = 0, bool isResultValue = false, bool needShowLine = true, TooltipTextType textType = TooltipTextType.Simple, TooltipTextAlignment alignment = TooltipTextAlignment.Midl)
	{
		Text = text;
		Value = value;
		TextType = textType;
		Alignment = alignment;
		NestedLevel = nestedLevel;
		IsResultValue = isResultValue;
		NeedShowLine = needShowLine;
	}
}
