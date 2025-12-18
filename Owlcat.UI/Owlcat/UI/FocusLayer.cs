using System;
using System.Collections.Generic;
using Owlcat.UI.Commands;
using UnityEngine;

namespace Owlcat.UI;

[AddComponentMenu("UI/Focus Layer")]
public class FocusLayer : MonoBehaviour
{
	private class Disposable : IDisposable
	{
		private readonly CommandLayer m_Layer;

		private readonly ICommandProvider m_Provider;

		private readonly IReadOnlyCollection<Command> m_Commands;

		private readonly Command m_Command;

		private bool m_Disposed;

		public Disposable(CommandLayer layer)
		{
			m_Layer = layer;
		}

		public Disposable(CommandLayer layer, ICommandProvider provider)
			: this(layer)
		{
			m_Layer.Add(m_Provider = provider);
		}

		public Disposable(CommandLayer layer, IReadOnlyCollection<Command> commands)
			: this(layer)
		{
			m_Layer.Add(m_Commands = commands);
		}

		public Disposable(CommandLayer layer, Command command)
			: this(layer)
		{
			m_Layer.Add(m_Command = command);
		}

		public void Dispose()
		{
			m_Disposed = true;
			if (m_Provider != null)
			{
				m_Layer.Remove(m_Provider);
			}
			if (m_Commands != null)
			{
				m_Layer.Remove(m_Commands);
			}
			if (m_Command != null)
			{
				m_Layer.Remove(m_Command);
			}
		}
	}

	private readonly CommandLayer m_Layer = new CommandLayer(".\\Library\\PackageCache\\com.owlcat.uikit@31ccf1b446d8\\Runtime\\Owlcat.UI\\Input@2\\DisplayOrder\\FocusLayer.cs", 12);

	[SerializeField]
	private CommandLayerMode m_Mode;

	public CommandLayerMode Mode
	{
		get
		{
			return m_Layer.Mode;
		}
		set
		{
			m_Layer.Mode = (m_Mode = value);
		}
	}

	public IReadOnlyCollection<Command> Commands => m_Layer.Commands;

	public IDisposable Add(Command command)
	{
		return new Disposable(m_Layer, command);
	}

	public IDisposable Add(IReadOnlyCollection<Command> commands)
	{
		return new Disposable(m_Layer, commands);
	}

	public IDisposable Add(ICommandProvider provider)
	{
		return new Disposable(m_Layer, provider);
	}

	private void OnValidate()
	{
		m_Layer.Mode = m_Mode;
	}

	private void Start()
	{
		m_Layer.Mode = m_Mode;
		m_Layer.Name = base.name;
	}

	private void OnEnable()
	{
		CommandLayerStack.Current.Add(m_Layer, base.transform);
	}

	private void OnDisable()
	{
		CommandLayerStack.Current.Remove(m_Layer);
	}
}
