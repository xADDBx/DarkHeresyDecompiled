using System;

namespace Kingmaker.Code.UI.MVVM;

public class BrickMultipleTextVM : TooltipBrickVM
{
	public readonly MultipleTextData[] Texts;

	public BrickMultipleTextVM(params MultipleTextData[] texts)
	{
		Texts = texts ?? Array.Empty<MultipleTextData>();
	}
}
