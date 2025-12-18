using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetLobbyTutorialBlockVM : ViewModel
{
	public readonly Sprite BlockSprite;

	public readonly string BlockDescription;

	public NetLobbyTutorialBlockVM(Sprite blockSprite, string blockDescription)
	{
		BlockSprite = blockSprite;
		BlockDescription = blockDescription;
	}
}
