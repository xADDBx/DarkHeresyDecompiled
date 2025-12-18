using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTextSignatureValueVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly string SignatureText;

	public readonly string Value;

	public TooltipBrickTextSignatureValueVM(string text, string signatureText, string value)
	{
		Text = text;
		SignatureText = signatureText;
		Value = value;
	}
}
