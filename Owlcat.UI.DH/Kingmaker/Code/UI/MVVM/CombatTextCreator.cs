using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public abstract class CombatTextCreator<TCombatTextView, TCombatMessage> where TCombatTextView : CombatTextEntityBaseView<TCombatMessage> where TCombatMessage : CombatMessageBase
{
	[SerializeField]
	public RectTransform ContainerRect;

	[SerializeField]
	public TCombatTextView m_PrefabView;

	private Action m_CollectionUpdated;

	public readonly List<CombatTextEntityBaseView<TCombatMessage>> ActiveViews = new List<CombatTextEntityBaseView<TCombatMessage>>();

	public void SetCallback(Action collectionUpdated)
	{
		m_CollectionUpdated = collectionUpdated;
	}

	public virtual TCombatTextView Create(TCombatMessage message)
	{
		TCombatTextView combatTextView = WidgetPool.Retain(m_PrefabView, ContainerRect);
		combatTextView.SetData(message, delegate
		{
			DisposeCombatText(combatTextView);
		});
		ActiveViews.Add(combatTextView);
		return combatTextView;
	}

	public void Clear()
	{
		ActiveViews.ToList().ForEach(DisposeCombatText);
		ActiveViews.Clear();
		m_CollectionUpdated?.Invoke();
	}

	protected virtual void OnTextViewDisposed(CombatTextEntityBaseView<TCombatMessage> combatText)
	{
	}

	private void DisposeCombatText(CombatTextEntityBaseView<TCombatMessage> combatText)
	{
		WidgetPool.Release(combatText);
		ActiveViews.Remove(combatText);
		m_CollectionUpdated?.Invoke();
		OnTextViewDisposed(combatText);
	}
}
