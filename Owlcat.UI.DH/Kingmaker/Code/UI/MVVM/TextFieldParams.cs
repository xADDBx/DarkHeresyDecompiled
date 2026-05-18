using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public readonly struct TextFieldParams
{
	public readonly FontStyles FontStyle;

	public readonly TextAlignmentOptions? TextAlignment;

	public static TextFieldParams Default => new TextFieldParams(FontStyles.Normal, null);

	public static TextFieldParams Bold => new TextFieldParams(FontStyles.Bold);

	public static TextFieldParams Italic => new TextFieldParams(FontStyles.Italic);

	public static TextFieldParams Strikethrough => new TextFieldParams(FontStyles.Strikethrough);

	public static TextFieldParams Center => new TextFieldParams(FontStyles.Normal, TextAlignmentOptions.Center);

	public static TextFieldParams Left => new TextFieldParams(FontStyles.Normal, TextAlignmentOptions.Left);

	public TextFieldParams(FontStyles fontStyle = FontStyles.Normal, TextAlignmentOptions? textAlignment = null)
	{
		TextAlignment = textAlignment;
		FontStyle = fontStyle;
	}
}
