using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuVM : ViewModel
{
	public readonly List<ContextMenuEntityVM> Entities = new List<ContextMenuEntityVM>();

	public RectTransform Owner;

	public ContextMenuVM(IContextMenuCollection collection)
	{
		foreach (IContextMenuCollectionEntity item2 in collection)
		{
			if (item2.IsValid())
			{
				ContextMenuEntityVM item = new ContextMenuEntityVM(item2).AddTo(this);
				Entities.Add(item);
			}
		}
		Owner = collection.GetOwner();
	}
}
