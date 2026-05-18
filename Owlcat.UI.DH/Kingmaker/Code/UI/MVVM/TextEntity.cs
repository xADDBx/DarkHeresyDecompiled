namespace Kingmaker.Code.UI.MVVM;

public class TextEntity
{
	public readonly string Text;

	public readonly TextFieldParams TextParams;

	public TextEntity(string text, TextFieldParams textParams)
	{
		Text = text;
		TextParams = textParams;
	}

	public TextEntity(string text)
	{
		Text = text;
		TextParams = default(TextFieldParams);
	}
}
