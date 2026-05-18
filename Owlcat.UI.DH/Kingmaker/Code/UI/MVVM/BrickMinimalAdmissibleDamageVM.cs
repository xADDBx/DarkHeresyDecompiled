namespace Kingmaker.Code.UI.MVVM;

public class BrickMinimalAdmissibleDamageVM : TooltipBrickVM
{
	public readonly int MinimalAdmissibleDamage;

	public readonly string ReasonValue;

	public BrickMinimalAdmissibleDamageVM(int minimalAdmissibleDamage, string reasonValue)
	{
		MinimalAdmissibleDamage = minimalAdmissibleDamage;
		ReasonValue = reasonValue;
	}
}
