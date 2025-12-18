using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class HitChanceEntityVM : ViewModel
{
	public readonly float Chance;

	public readonly int Index;

	public readonly bool IsLast;

	public HitChanceEntityVM(int index, float chance, bool isLast)
	{
		Index = index;
		Chance = chance;
		IsLast = isLast;
	}
}
