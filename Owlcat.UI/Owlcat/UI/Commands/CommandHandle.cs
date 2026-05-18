using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Owlcat.UI.Commands;

public class CommandHandle : IDisposable
{
	private readonly Command m_Command;

	private readonly IBindable m_Target;

	private readonly Observable<bool> m_Enabled;

	private ICollection<Command> m_TargetCommands;

	private CompositeDisposable m_Disposables;

	public Command Command => m_Command;

	public CommandHandle(Command command, IBindable target, Observable<bool> enabled = null)
	{
		m_Command = command;
		m_Target = target;
		m_Enabled = enabled;
		Initialize();
	}

	private void Initialize()
	{
		m_Disposables = new CompositeDisposable();
		IBindable target = m_Target;
		if (!(target is ICommandProvider commandProvider))
		{
			if (!(target is Component component))
			{
				throw new InvalidOperationException($"Object '{m_Target}' neither ICommandProvider nor Component");
			}
			if (!component.TryGetComponent<FocusLayer>(out var component2))
			{
				throw new InvalidOperationException(string.Format("Object '{0}' must have {1} component", component, "FocusLayer"));
			}
			component2.Add(m_Command).AddTo(m_Disposables);
		}
		else
		{
			if (!(commandProvider.Commands is ICollection<Command> collection))
			{
				throw new InvalidOperationException(string.Format("{0} '{1}' must have muttable commands collection", "ICommandProvider", commandProvider));
			}
			collection.Add(m_Command);
			m_TargetCommands = collection;
		}
		m_Command.Source = m_Target;
		m_Enabled?.Subscribe(delegate(bool value)
		{
			m_Command.Enabled = value;
		})?.AddTo(m_Disposables);
	}

	public void Add(IDisposable disposable)
	{
		m_Disposables.Add(disposable);
	}

	public void Dispose()
	{
		m_TargetCommands?.Remove(m_Command);
		m_Disposables?.Dispose();
	}

	public static implicit operator Command(CommandHandle handle)
	{
		return handle?.m_Command;
	}
}
