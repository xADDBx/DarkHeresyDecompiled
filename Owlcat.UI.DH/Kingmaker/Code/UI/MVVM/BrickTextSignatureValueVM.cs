namespace Kingmaker.Code.UI.MVVM;

public class BrickTextSignatureValueVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly string SignatureText;

	public readonly string Value;

	public BrickTextSignatureValueVM(string text, string signatureText, string value)
	{
		Text = text;
		SignatureText = signatureText;
		Value = value;
	}
}
