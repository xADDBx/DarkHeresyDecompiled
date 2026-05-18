namespace Kingmaker.Code.UI.MVVM;

public class TextValueAddElement
{
	public TextEntity Text;

	public TextEntity Value;

	public TextEntity AddValue;

	public TextValueAddElement(TextEntity text, TextEntity value = null, TextEntity addValue = null)
	{
		Text = text;
		Value = value;
		AddValue = addValue;
	}

	public TextValueAddElement(string text, string value = null, string addValue = null)
	{
		Text = new TextEntity(text);
		Value = new TextEntity(value);
		AddValue = new TextEntity(addValue);
	}
}
