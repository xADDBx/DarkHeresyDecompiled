using System.Collections;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuCollection : IContextMenuCollection, IEnumerable<IContextMenuCollectionEntity>, IEnumerable
{
	private readonly List<ContextMenuCollectionEntity> m_Entities;

	public RectTransform Owner { get; }

	public bool IsValid => m_Entities?.Any((ContextMenuCollectionEntity e) => e.IsValid) ?? false;

	public ContextMenuCollection(List<ContextMenuCollectionEntity> entities, RectTransform owner)
	{
		m_Entities = entities;
		Owner = owner;
	}

	public IEnumerator<IContextMenuCollectionEntity> GetEnumerator()
	{
		return m_Entities.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public RectTransform GetOwner()
	{
		return Owner;
	}
}
