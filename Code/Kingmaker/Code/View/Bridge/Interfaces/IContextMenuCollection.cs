using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Interfaces;

public interface IContextMenuCollection : IEnumerable<IContextMenuCollectionEntity>, IEnumerable
{
	RectTransform GetOwner();
}
