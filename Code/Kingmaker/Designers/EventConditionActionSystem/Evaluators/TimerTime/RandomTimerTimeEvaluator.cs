using System;
using Kingmaker.Controllers.Timer;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.TimerTime;

[Serializable]
[TypeId("f6a402fd8a504634ebb386423dc102cd")]
public class RandomTimerTimeEvaluator : TimerTimeEvaluator
{
	[SerializeField]
	private float m_MinTime;

	[SerializeField]
	private float m_MaxTime;

	public override string GetCaption()
	{
		return $"Timer after ({m_MinTime}, {m_MaxTime}) seconds";
	}

	protected override Kingmaker.Controllers.Timer.TimerTime GetValueInternal()
	{
		return new Kingmaker.Controllers.Timer.TimerTime(TimeSpan.FromSeconds(PFStatefulRandom.Timer.Range(m_MinTime, m_MaxTime)));
	}
}
