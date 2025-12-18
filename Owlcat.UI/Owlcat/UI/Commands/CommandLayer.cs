using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using ObservableCollections;

namespace Owlcat.UI.Commands;

public sealed class CommandLayer
{
	private CommandLayerMode m_Mode = CommandLayerMode.Additive;

	internal readonly ObservableList<Command> m_Commands = new ObservableList<Command>();

	public IReadOnlyCollection<Command> Commands => m_Commands;

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
				e.Consumed |= m_Commands[num].TryTrigger(e) && m_Commands[num].TriggerWillConsumeEvent;
			}
		}
		else
		{
			for (int i = 0; i < m_Commands.Count; i++)
			{
				e.Consumed |= m_Commands[i].TryTrigger(e) && m_Commands[i].TriggerWillConsumeEvent;
			}
		}
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

	public void Add(ICommandProvider commandProvider)
	{
		Add(commandProvider.Commands);
	}

	public void Remove(ICommandProvider commandProvider)
	{
		Remove(commandProvider.Commands);
	}

	public void Add(IReadOnlyCollection<Command> commands)
	{
		foreach (Command command in commands)
		{
			Add(command);
		}
		if (commands is IObservableCollection<Command> observableCollection)
		{
			observableCollection.CollectionChanged += OnCommandsChanged;
		}
	}

	public void Remove(IReadOnlyCollection<Command> commands)
	{
		foreach (Command command in commands)
		{
			Remove(command);
		}
		if (commands is IObservableCollection<Command> observableCollection)
		{
			observableCollection.CollectionChanged -= OnCommandsChanged;
		}
	}

	public void Add(Command command)
	{
		command.PropertyChanged += OnCommandPropertyChanged;
		m_Commands.Add(command);
		this.Changed?.Invoke();
	}

	public void Remove(Command command)
	{
		command.PropertyChanged -= OnCommandPropertyChanged;
		m_Commands.Remove(command);
		this.Changed?.Invoke();
	}

	private void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != "Active")
		{
			this.Changed?.Invoke();
		}
	}

	private void OnCommandsChanged(in NotifyCollectionChangedEventArgs<Command> args)
	{
		if (args.IsSingleItem)
		{
			if (args.OldItem != null)
			{
				Remove(args.OldItem);
			}
			if (args.NewItem != null)
			{
				Add(args.NewItem);
			}
		}
		else
		{
			ReadOnlySpan<Command> oldItems = args.OldItems;
			for (int i = 0; i < oldItems.Length; i++)
			{
				Command command = oldItems[i];
				Remove(command);
			}
			oldItems = args.NewItems;
			for (int i = 0; i < oldItems.Length; i++)
			{
				Command command2 = oldItems[i];
				Add(command2);
			}
		}
		this.Changed?.Invoke();
	}
}
