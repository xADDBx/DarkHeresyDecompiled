using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Highlighting;

public class MultiHighlighter : MonoBehaviour, IInvasiveLinkedListNode<MultiHighlighter>
{
	public enum ModeType
	{
		SingleColor,
		Flashing,
		Gradient
	}

	[Serializable]
	public class HighlightSource : IComparable<HighlightSource>
	{
		public float Lifetime;

		public int Priority;

		public ModeType Mode;

		[Header("Single")]
		public Color Color;

		public float OnsetTime;

		public float RemoveTime;

		[Header("Flashing")]
		public Color AlternateColor;

		public float FlashingFrequency;

		[Header("Gradient")]
		public Gradient ColorGradient;

		public float GradientLifetime;

		public bool UnscaledTime;

		[HideInInspector]
		public float AddTime;

		public int CompareTo(HighlightSource other)
		{
			return other.Priority.CompareTo(Priority);
		}
	}

	internal static InvasiveLinkedList<MultiHighlighter> Instances;

	private static float m_LastUpdateScaledTime;

	private static float m_LastUpdateUnscaledTime;

	private readonly List<HighlightSource> m_HighlightSources = new List<HighlightSource>();

	private HighlightSource m_CurrentlyPlaying;

	private Highlighter m_Highlighter;

	MultiHighlighter IInvasiveLinkedListNode<MultiHighlighter>.Prev { get; set; }

	MultiHighlighter IInvasiveLinkedListNode<MultiHighlighter>.Next { get; set; }

	public Highlighter Highlighter => m_Highlighter;

	protected HighlightSource CurrentlyPlaying => m_CurrentlyPlaying;

	[UsedImplicitly]
	private void OnEnable()
	{
		Instances.AddFirst(this);
		m_Highlighter = GetComponent<Highlighter>();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void Play(HighlightSource source)
	{
		float addTime = (source.UnscaledTime ? Time.unscaledTime : Time.time);
		if (!m_HighlightSources.Contains(source))
		{
			m_HighlightSources.Add(source);
			m_HighlightSources.Sort();
		}
		source.AddTime = addTime;
	}

	public void Stop(HighlightSource source)
	{
		m_HighlightSources.Remove(source);
	}

	internal static void UpdateInstances()
	{
		float time = Time.time;
		float unscaledTime = Time.unscaledTime;
		if (!Mathf.Approximately(time, m_LastUpdateScaledTime) || !Mathf.Approximately(unscaledTime, m_LastUpdateUnscaledTime))
		{
			m_LastUpdateScaledTime = time;
			m_LastUpdateUnscaledTime = unscaledTime;
			InvasiveLinkedList<MultiHighlighter>.Enumerator enumerator = Instances.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.UpdateState(time, unscaledTime);
			}
		}
	}

	private void UpdateState(float scaledTime, float unscaledTime)
	{
		if (m_Highlighter == null)
		{
			return;
		}
		for (int num = m_HighlightSources.Count - 1; num >= 0; num--)
		{
			HighlightSource highlightSource = m_HighlightSources[num];
			float num2 = (highlightSource.UnscaledTime ? unscaledTime : scaledTime);
			if (highlightSource.AddTime <= num2 - highlightSource.Lifetime && highlightSource.Lifetime > 0f)
			{
				m_HighlightSources.RemoveAt(num);
			}
		}
		HighlightSource highlightSource2 = ((m_HighlightSources.Count > 0) ? m_HighlightSources[0] : null);
		if (highlightSource2 != m_CurrentlyPlaying)
		{
			if (m_CurrentlyPlaying != null)
			{
				m_Highlighter.ConstantOff(m_CurrentlyPlaying.RemoveTime);
				m_Highlighter.FlashingOff();
			}
			if (highlightSource2 != null)
			{
				switch (highlightSource2.Mode)
				{
				case ModeType.SingleColor:
					m_Highlighter.ConstantOn(highlightSource2.Color, highlightSource2.OnsetTime);
					break;
				case ModeType.Flashing:
					m_Highlighter.FlashingOn(highlightSource2.Color, highlightSource2.AlternateColor, 1f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case ModeType.Gradient:
					break;
				}
			}
			m_CurrentlyPlaying = highlightSource2;
		}
		if (m_CurrentlyPlaying != null && m_CurrentlyPlaying.Mode == ModeType.Gradient && m_CurrentlyPlaying.GradientLifetime > 0f)
		{
			float num3 = Mathf.Repeat((m_CurrentlyPlaying.UnscaledTime ? unscaledTime : scaledTime) - m_CurrentlyPlaying.Lifetime, m_CurrentlyPlaying.GradientLifetime);
			m_Highlighter.ConstantOnImmediate(m_CurrentlyPlaying.ColorGradient.Evaluate(num3 / m_CurrentlyPlaying.GradientLifetime));
		}
	}

	protected void ReapplyColorInCurrentHighlight(HighlightSource baseAnim)
	{
		if (!(m_Highlighter == null) && baseAnim == m_CurrentlyPlaying)
		{
			m_Highlighter.ConstantOn(baseAnim.Color, baseAnim.OnsetTime);
		}
	}
}
