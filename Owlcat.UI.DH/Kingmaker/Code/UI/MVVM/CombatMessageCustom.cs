namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageCustom : CombatMessageBase
{
	public string Text;

	public override string GetText()
	{
		return Text;
	}
}
