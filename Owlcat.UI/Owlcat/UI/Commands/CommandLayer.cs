using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using ObservableCollections;
using UnityEngine.Pool;

namespace Owlcat.UI.Commands;

public sealed class CommandLayer
{
	private class Subscription : IDisposable
	{
		private readonly CommandLayer m_Layer;

		private readonly ICommandProvider m_Provider;

		private readonly IObservableCollection<Command> m_Collection;

		public Subscription(CommandLayer layer, ICommandProvider provider)
		{
			m_Layer = layer;
			m_Provider = provider;
			m_Collection = provider.Commands as IObservableCollection<Command>;
			foreach (Command command in m_Provider.Commands)
			{
				m_Layer.Add(command, m_Provider);
			}
			if (m_Collection != null)
			{
				m_Collection.CollectionChanged += OnCommandsChanged;
			}
		}

		private void OnCommandsChanged(in NotifyCollectionChangedEventArgs<Command> args)
		{
			if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				m_Layer.RemoveAll(m_Provider);
				return;
			}
			if (args.IsSingleItem)
			{
				if (args.OldItem != null)
				{
					m_Layer.Remove(args.OldItem, m_Provider);
				}
				if (args.NewItem != null)
				{
					m_Layer.Add(args.NewItem, m_Provider);
				}
				return;
			}
			ReadOnlySpan<Command> oldItems = args.OldItems;
			for (int i = 0; i < oldItems.Length; i++)
			{
				Command command = oldItems[i];
				m_Layer.Remove(command, m_Provider);
			}
			oldItems = args.NewItems;
			for (int i = 0; i < oldItems.Length; i++)
			{
				Command command2 = oldItems[i];
				m_Layer.Add(command2, m_Provider);
			}
		}

		public void Dispose()
		{
			if (m_Collection != null)
			{
				m_Collection.CollectionChanged -= OnCommandsChanged;
			}
			m_Layer.RemoveAll(m_Provider);
		}
	}

	private CommandLayerMode m_Mode = CommandLayerMode.Additive;

	private readonly ObservableList<Command> m_Commands = new ObservableList<Command>();

	private readonly Dictionary<Command, object> m_CommandSource = new Dictionary<Command, object>();

	private readonly Dictionary<ICommandProvider, Subscription> m_CommandProviders = new Dictionary<ICommandProvider, Subscription>();

	public IReadOnlyList<Command> Commands => m_Commands;

	public CommandLayerMode Mode
	{
		get
		{
			return m_Mode;
		}
		set
		{
			SetProperty(ref m_Mode, value, "Mode");
		}
	}

	public string Name { get; set; }

	public event Action Changed;

	public CommandLayer([CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
	{
		Name = $"{Path.GetFileName(callerFilePath)}:{callerLineNumber}";
	}

	public void Consume(InputEvent e)
	{
		if (e.Bubbling)
		{
			for (int num = m_Commands.Count - 1; num >= 0; num--)
			{
				e.Consumed |= TryTrigger(num, e);
			}
		}
		else
		{
			for (int i = 0; i < m_Commands.Count; i++)
			{
				e.Consumed |= TryTrigger(i, e);
			}
		}
	}

	private bool TryTrigger(int commandIndex, InputEvent e)
	{
		Command command = m_Commands[commandIndex];
		bool triggerWillConsumeEvent = command.TriggerWillConsumeEvent;
		return command.TryTrigger(e) && triggerWillConsumeEvent;
	}

	public override string ToString()
	{
		return $"[{Name}: {Mode}]";
	}

	private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			this.Changed?.Invoke();
		}
	}

	public void Add(ICommandProvider provider)
	{
		m_CommandProviders.Add(provider, new Subscription(this, provider));
	}

	public void Remove(ICommandProvider provider)
	{
		if (m_CommandProviders.Remove(provider, out var value))
		{
			value.Dispose();
		}
	}

	public bool Contains(ICommandProvider provider)
	{
		return m_CommandProviders.ContainsKey(provider);
	}

	public void Add(Command command)
	{
		Add(command, command);
	}

	public void Remove(Command command)
	{
		Remove(command, command);
	}

	public bool Contains(Command command)
	{
		return m_CommandSource.ContainsKey(command);
	}

	private void Add(Command command, object source)
	{
		m_Commands.Add(command);
		m_CommandSource.Add(command, source);
		command.PropertyChanged += OnCommandPropertyChanged;
		this.Changed?.Invoke();
	}

	private void Remove(Command command, object source)
	{
		command.PropertyChanged -= OnCommandPropertyChanged;
		m_CommandSource.Remove(command);
		m_Commands.Remove(command);
		this.Changed?.Invoke();
	}

	private void RemoveAll(object source)
	{
		List<Command> value;
		using (CollectionPool<List<Command>, Command>.Get(out value))
		{
			foreach (KeyValuePair<Command, object> item in m_CommandSource)
			{
				if (item.Value == source)
				{
					value.Add(item.Key);
				}
			}
			foreach (Command item2 in value)
			{
				Remove(item2, source);
			}
		}
	}

	private void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != "Active")
		{
			this.Changed?.Invoke();
		}
	}
}
