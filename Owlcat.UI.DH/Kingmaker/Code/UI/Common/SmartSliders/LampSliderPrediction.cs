using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class LampSliderPrediction : MonoBehaviour, IDisposable
{
	[SerializeField]
	private LampStrip m_Base;

	[SerializeField]
	private LampStrip m_Current;

	[SerializeField]
	private LampStrip m_Predict;

	private readonly List<IDisposable> m_Disposes = new List<IDisposable>();

	public IDisposable Bind(ReadOnlyReactiveProperty<int> maxValue, ReadOnlyReactiveProperty<int> currentValue, ReadOnlyReactiveProperty<int> predictionValue)
	{
		AddDisposable(maxValue.Subscribe(delegate(int v)
		{
			int num2 = Mathf.RoundToInt(v);
			m_Current.SetLampCount(num2);
			m_Predict.SetLampCount(num2);
			m_Base.SetLampCount(num2);
			m_Base.SetVisibleRange(0, num2);
		}));
		AddDisposable(currentValue.CombineLatest(maxValue, predictionValue, (int cur, int max, int pred) => new { cur, max, pred }).Subscribe(v =>
		{
			bool num = v.pred <= v.cur;
			m_Current.SetVisibleRange(0, v.pred);
			m_Predict.SetVisibleRange(v.pred, v.cur);
			if (num)
			{
				m_Predict.Blink();
			}
			else
			{
				m_Predict.StopBlink();
			}
		}));
		return this;
	}

	protected void AddDisposable(IDisposable d)
	{
		m_Disposes.Add(d);
	}

	public void Dispose()
	{
		foreach (IDisposable dispose in m_Disposes)
		{
			dispose.Dispose();
		}
		m_Disposes.Clear();
		m_Current.HideAll();
		m_Predict.HideAll();
		m_Base.HideAll();
	}
}
