using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public interface IItemSlotView
{
	ItemSlotVM SlotVM { get; }

	RectTransform GetParentContainer();
}
