using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Owlcat.UI.Commands;

public class Command : INotifyPropertyChanged
{
	private object m_Source;

	private object m_Label;

	private CommandPhase m_Phase;

	private CommandDisplayMode m_DisplayMode;

	private bool m_TriggerOnConsumedEvent;

	private bool m_TriggerWillConsumeEvent = true;

	private bool m_Enabled = true;

	private bool m_Active = true;

	private float m_Progress;

	private readonly CommandDelegate m_Delegate;

	public string Binding { get; }

	public object Source
	{
		get
		{
			return m_Source;
		}
		set
		{
			SetProperty(ref m_Source, value, "Source");
		}
	}

	public object Label
	{
		get
		{
			return m_Label;
		}
		set
		{
			SetProperty(ref m_Label, value, "Label");
		}
	}

	public CommandDisplayMode DisplayMode
	{
		get
		{
			return m_DisplayMode;
		}
		set
		{
			SetProperty(ref m_DisplayMode, value, "DisplayMode");
		}
	}

	public CommandPhase Phase
	{
		get
		{
			return m_Phase;
		}
		set
		{
			SetProperty(ref m_Phase, value, "Phase");
		}
	}

	public bool TriggerOnConsumedEvent
	{
		get
		{
			return m_TriggerOnConsumedEvent;
		}
		set
		{
			SetProperty(ref m_TriggerOnConsumedEvent, value, "TriggerOnConsumedEvent");
		}
	}

	public bool TriggerWillConsumeEvent
	{
		get
		{
			return m_TriggerWillConsumeEvent;
		}
		set
		{
			SetProperty(ref m_TriggerWillConsumeEvent, value, "TriggerWillConsumeEvent");
		}
	}

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			SetProperty(ref m_Enabled, value, "Enabled");
		}
	}

	public bool Active
	{
		get
		{
			return m_Active;
		}
		internal set
		{
			SetProperty(ref m_Active, value, "Active");
		}
	}

	public float Progress
	{
		get
		{
			return m_Progress;
		}
		internal set
		{
			SetProperty(ref m_Progress, value, "Progress");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public event Action Triggering;

	public event Action Triggered;

	public Command(string binding)
	{
		Binding = binding;
	}

	public Command(string binding, Action callback)
		: this(binding)
	{
		m_Delegate = new CommandDelegate(callback, null, null);
	}

	public Command(string binding, Action<float> callback)
		: this(binding)
	{
		m_Delegate = new CommandDelegate(null, callback, null);
	}

	public Command(string binding, Action<Vector2> callback)
		: this(binding)
	{
		m_Delegate = new CommandDelegate(null, null, callback);
	}

	public bool CanTrigger(InputEvent e)
	{
		if (!Active || !Enabled)
		{
			return false;
		}
		if (e.Consumed && !TriggerOnConsumedEvent)
		{
			return false;
		}
		if (Phase == CommandPhase.Bubble != e.Bubbling)
		{
			return false;
		}
		if (!e.IsTrigger(Binding))
		{
			return false;
		}
		Progress = e.Progress;
		if (e.Progress != 1f)
		{
			return false;
		}
		return true;
	}

	public bool TryTrigger(InputEvent e)
	{
		if (!CanTrigger(e))
		{
			return false;
		}
		try
		{
			this.Triggering?.Invoke();
			m_Delegate.Invoke(e);
			return true;
		}
		catch (Exception ex)
		{
			UIKitLogger.Exception(ex);
			throw ex;
		}
		finally
		{
			this.Triggered?.Invoke();
		}
	}

	private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public override string ToString()
	{
		return "[" + Binding + "]";
	}
}
