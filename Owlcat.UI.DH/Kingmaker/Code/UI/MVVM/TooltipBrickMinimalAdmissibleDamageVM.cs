using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickMinimalAdmissibleDamageVM : TooltipBaseBrickVM
{
	public readonly int MinimalAdmissibleDamage;

	public readonly string ReasonValue;

	public TooltipBrickMinimalAdmissibleDamageVM(int minimalAdmissibleDamage, string reasonValue)
	{
		MinimalAdmissibleDamage = minimalAdmissibleDamage;
		ReasonValue = reasonValue;
	}
}
