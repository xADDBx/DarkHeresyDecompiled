using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EtudeCounterVM : ViewModel, IEtudeCounterHandler, ISubscriber
{
	private class EtudeCounterConfig
	{
		public string Label;

		public Func<int> ValueGetter;

		public Func<int> TargetValueGetter;
	}

	private readonly ReactiveProperty<string> m_CounterText = new ReactiveProperty<string>();

	private Dictionary<string, EtudeCounterConfig> m_Configs = new Dictionary<string, EtudeCounterConfig>();

	private StringBuilder m_StringBuilder = new StringBuilder();

	private IDisposable m_UpdateSubscription;

	public ReadOnlyReactiveProperty<string> CounterText => m_CounterText;

	public EtudeCounterVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_CounterText.Value = null;
		m_UpdateSubscription?.Dispose();
		m_UpdateSubscription = null;
	}

	void IEtudeCounterHandler.ShowEtudeCounter(string id, string label, Func<int> valueGetter, Func<int> targetValueGetter)
	{
		m_Configs.Add(id, new EtudeCounterConfig
		{
			Label = label,
			ValueGetter = valueGetter,
			TargetValueGetter = targetValueGetter
		});
		if (m_UpdateSubscription == null && !m_Configs.Empty())
		{
			m_UpdateSubscription = Observable.EveryUpdate().Subscribe(UpdateValues);
		}
	}

	void IEtudeCounterHandler.HideEtudeCounter(string id)
	{
		m_Configs.Remove(id);
		if (m_Configs.Empty() && m_UpdateSubscription != null)
		{
			m_UpdateSubscription.Dispose();
			m_UpdateSubscription = null;
			m_CounterText.Value = null;
		}
	}

	private void UpdateValues()
	{
		m_StringBuilder.Clear();
		foreach (EtudeCounterConfig value in m_Configs.Values)
		{
			if (m_StringBuilder.Length > 0)
			{
				m_StringBuilder.Append('\n');
			}
			m_StringBuilder.Append(value.Label);
			m_StringBuilder.Append(' ');
			m_StringBuilder.Append(value.ValueGetter());
		}
		m_CounterText.Value = m_StringBuilder.ToString();
	}
}
