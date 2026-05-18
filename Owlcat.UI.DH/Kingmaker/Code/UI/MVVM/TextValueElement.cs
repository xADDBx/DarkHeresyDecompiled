namespace Kingmaker.Code.UI.MVVM;

public class TextValueElement
{
	public TextEntity Text;

	public TextEntity Value;

	public TextValueElement(TextEntity text, TextEntity value = null)
	{
		Text = text;
		Value = value;
	}

	public TextValueElement(string text, string value = null)
	{
		Text = new TextEntity(text);
		Value = new TextEntity(value);
	}
}
