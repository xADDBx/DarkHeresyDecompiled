using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadPortraitVM : ViewModel
{
	public readonly Sprite Portrait;

	public readonly string Rank;

	public SaveLoadPortraitVM(Sprite portrait, string rank)
	{
		Portrait = portrait;
		Rank = rank;
	}
}
