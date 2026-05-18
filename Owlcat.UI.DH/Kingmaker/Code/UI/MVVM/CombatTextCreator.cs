using System;
using System.Collections.Generic;
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

	private Action<TCombatMessage> m_MessageDisposed;

	private Dictionary<TCombatTextView, TCombatMessage> m_ViewToMessageDictionary = new Dictionary<TCombatTextView, TCombatMessage>();

	private List<TCombatTextView> m_ActiveViews = new List<TCombatTextView>();

	public IReadOnlyList<TCombatTextView> ActiveViews => m_ActiveViews;

	public void SetCallbacks(Action<TCombatMessage> messageDisposed, Action collectionUpdated)
	{
		m_CollectionUpdated = collectionUpdated;
		m_MessageDisposed = messageDisposed;
	}

	public virtual TCombatTextView Create(TCombatMessage message)
	{
		TCombatTextView combatTextView = WidgetPool.Retain(m_PrefabView, ContainerRect);
		combatTextView.SetData(message, delegate
		{
			HandleTextComplete(combatTextView);
		});
		m_ViewToMessageDictionary.Add(combatTextView, message);
		m_ActiveViews.Add(combatTextView);
		return combatTextView;
	}

	public void Clear()
	{
		foreach (var (view, message) in m_ViewToMessageDictionary)
		{
			DisposeCombatText(view, message);
		}
		m_ViewToMessageDictionary.Clear();
		m_ActiveViews.Clear();
		m_CollectionUpdated?.Invoke();
	}

	protected virtual void OnTextViewDisposed(CombatTextEntityBaseView<TCombatMessage> combatText)
	{
	}

	private void HandleTextComplete(TCombatTextView combatText)
	{
		if (m_ViewToMessageDictionary.TryGetValue(combatText, out var value))
		{
			DisposeCombatText(combatText, value);
			m_ViewToMessageDictionary.Remove(combatText);
			m_ActiveViews.Remove(combatText);
			m_CollectionUpdated?.Invoke();
		}
	}

	private void DisposeCombatText(TCombatTextView view, TCombatMessage message)
	{
		WidgetPool.Release(view);
		m_MessageDisposed?.Invoke(message);
		OnTextViewDisposed(view);
	}
}
