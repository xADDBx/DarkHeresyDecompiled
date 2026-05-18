using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public interface ITurnVirtualItemData
{
	ViewModel ViewModel { get; }

	ITurnVirtualItemView BoundView { get; set; }

	Vector2 VirtualSize { get; }

	Vector2 VirtualPosition { get; }
}
