using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public static class TextFieldParamsExtension
{
	public static void ApplyTextFieldParams(this TMP_Text textField, TextFieldParams textFieldParams)
	{
		if (!(textField == null))
		{
			textField.fontStyle = textFieldParams.FontStyle;
		}
	}

	public static TextFieldParams GetTextFieldParams(this TMP_Text textField)
	{
		if (!(textField == null))
		{
			return new TextFieldParams(textField.fontStyle);
		}
		return TextFieldParams.Default;
	}
}
