using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using R3;
using UnityEngine;

namespace Owlcat.UI.Commands;

public class CommandLayerStack
{
	internal readonly List<CommandLayer> m_Layers = new List<CommandLayer>();

	internal readonly Dictionary<CommandLayer, Transform> m_Transforms = new Dictionary<CommandLayer, Transform>();

	private readonly Comparison<CommandLayer> m_Comparison;

	private readonly IDisposable m_Subscription;

	private bool m_HasChanges;

	public static CommandLayerStack Current { get; private set; }

	public IReadOnlyList<CommandLayer> Layers => m_Layers;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Initialize()
	{
		Current = new CommandLayerStack();
	}

	public CommandLayerStack()
	{
		m_Comparison = (CommandLayer left, CommandLayer right) => -DisplayOrderComparer.Comparer.Compare(m_Transforms[left], m_Transforms[right]);
		m_Subscription = Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(ValidateIfHasChanges);
	}

	~CommandLayerStack()
	{
		m_Subscription.Dispose();
	}

	public void Add(CommandLayer layer, Transform transform)
	{
		DisplayOrderTracker.Attach(transform, OnCommandLayerChanged);
		m_Layers.Add(layer);
		m_Transforms.Add(layer, transform);
		m_HasChanges = true;
		layer.Changed += OnCommandLayerChanged;
	}

	public void Remove(CommandLayer layer)
	{
		DisplayOrderTracker.Detach(m_Transforms[layer], OnCommandLayerChanged);
		m_Layers.Remove(layer);
		m_Transforms.Remove(layer);
		m_HasChanges = true;
		layer.Changed -= OnCommandLayerChanged;
	}

	private void OnCommandLayerChanged()
	{
		m_HasChanges = true;
	}

	public bool Consume(InputEvent e)
	{
		ValidateIfHasChanges();
		e.Consumed = false;
		e.Bubbling = false;
		for (int i = 0; i < m_Layers.Count; i++)
		{
			m_Layers[i].Consume(e);
		}
		e.Bubbling = true;
		for (int num = m_Layers.Count - 1; num >= 0; num--)
		{
			m_Layers[num].Consume(e);
		}
		return e.Consumed;
	}

	private void ValidateIfHasChanges()
	{
		if (m_HasChanges)
		{
			m_HasChanges = false;
			Validate();
		}
	}

	private void Validate()
	{
		m_Layers.Sort(m_Comparison);
		int num = 0;
		foreach (CommandLayer layer in m_Layers)
		{
			num += layer.Commands.Count;
		}
		for (int i = 0; i < num; i++)
		{
			Validate(i, num);
		}
	}

	private void Validate(int i, int numCommands)
	{
		bool flag = false;
		CommandLayer layer;
		Command command = GetCommand(i, out layer);
		if (command.Phase == CommandPhase.Tunnel)
		{
			for (int j = 0; j < i; j++)
			{
				if (flag)
				{
					break;
				}
				CommandLayer layer2;
				Command command2 = GetCommand(j, out layer2);
				if (layer != layer2 && layer2.Mode == CommandLayerMode.Modal)
				{
					flag = true;
				}
				else if (command2.Phase == CommandPhase.Tunnel)
				{
					flag = IsConflict(command, command2);
				}
			}
		}
		else if (command.Phase == CommandPhase.Bubble)
		{
			int num = 0;
			for (num = 0; num < numCommands; num++)
			{
				if (flag)
				{
					break;
				}
				if (num == i)
				{
					continue;
				}
				CommandLayer layer3;
				Command command3 = GetCommand(num, out layer3);
				if (layer != layer3 && layer3.Mode == CommandLayerMode.Modal && num < i)
				{
					flag = true;
					continue;
				}
				if (layer != layer3 && layer.Mode == CommandLayerMode.Modal && num > i)
				{
					break;
				}
				if (command3.Phase == CommandPhase.Tunnel)
				{
					flag = IsConflict(command, command3);
				}
			}
			num--;
			while (num > i && !flag)
			{
				CommandLayer layer4;
				Command command4 = GetCommand(num, out layer4);
				if (command4.Phase == CommandPhase.Bubble)
				{
					flag = IsConflict(command, command4);
				}
				num--;
			}
		}
		command.Active = !flag;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Command GetCommand(int commandIndex, out CommandLayer layer)
	{
		for (int i = 0; i < m_Layers.Count; i++)
		{
			layer = m_Layers[i];
			if (commandIndex < layer.Commands.Count)
			{
				return layer.m_Commands[commandIndex];
			}
			commandIndex -= layer.Commands.Count;
		}
		throw new ArgumentOutOfRangeException("commandIndex");
	}

	private bool IsConflict(Command left, Command right)
	{
		if (left.Enabled && right.Enabled && !left.TriggerOnConsumedEvent && right.TriggerWillConsumeEvent)
		{
			return left.Binding == right.Binding;
		}
		return false;
	}
}
