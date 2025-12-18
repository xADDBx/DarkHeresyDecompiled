using System;
using System.Collections.Generic;
using ObservableCollections;
using Owlcat.UI.Commands;
using UnityEngine;
using UnityEngine.Pool;

namespace Owlcat.UI;

public class CommandHintList : View<IReadOnlyCollection<Command>>
{
	[SerializeField]
	private CommandHint m_Prefab;

	private readonly Dictionary<Command, CommandHint> m_Children = new Dictionary<Command, CommandHint>();

	protected override void OnBind()
	{
		foreach (Command item in base.ViewModel)
		{
			Add(item);
		}
		if (base.ViewModel is IObservableCollection<Command> observableCollection)
		{
			observableCollection.CollectionChanged += OnCollectionChanged;
		}
	}

	protected override void OnUnbind()
	{
		if (base.ViewModel is IObservableCollection<Command> observableCollection)
		{
			observableCollection.CollectionChanged -= OnCollectionChanged;
		}
		List<Command> value;
		using (CollectionPool<List<Command>, Command>.Get(out value))
		{
			value.AddRange(m_Children.Keys);
			value.ForEach(Remove);
			m_Children.Clear();
		}
	}

	private void OnCollectionChanged(in NotifyCollectionChangedEventArgs<Command> e)
	{
		if (e.IsSingleItem)
		{
			if (e.OldItem != null)
			{
				Remove(e.OldItem);
			}
			if (e.NewItem != null)
			{
				Add(e.NewItem);
			}
			return;
		}
		ReadOnlySpan<Command> oldItems = e.OldItems;
		for (int i = 0; i < oldItems.Length; i++)
		{
			Command command = oldItems[i];
			Remove(command);
		}
		oldItems = e.NewItems;
		for (int i = 0; i < oldItems.Length; i++)
		{
			Command command2 = oldItems[i];
			Add(command2);
		}
	}

	private void Add(Command command)
	{
		CommandHint commandHint = WidgetPool.Retain(m_Prefab, base.transform);
		commandHint.Bind(command);
		m_Children.Add(command, commandHint);
	}

	private void Remove(Command command)
	{
		CommandHint commandHint = m_Children[command];
		commandHint.Unbind();
		WidgetPool.Release(commandHint);
	}
}
