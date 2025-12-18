using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class AnimationSet : ScriptableObject
{
	[Serializable]
	public class AnimationActionEntry
	{
		[SerializeField]
		private AnimationActionBase m_Action;

		public UnitAnimationType Type;

		public bool IsUsing = true;

		public AnimationActionBase Action
		{
			get
			{
				return m_Action;
			}
			set
			{
				UnitAnimationAction unitAnimationAction = (UnitAnimationAction)value;
				if ((bool)unitAnimationAction && unitAnimationAction.Type != Type)
				{
					PFLog.Animations.Error($"Inappropriate action type: {unitAnimationAction.Type} (expected {Type})");
				}
				else
				{
					m_Action = value;
				}
			}
		}

		public AnimationActionEntry(UnitAnimationType type)
		{
			Type = type;
		}
	}

	[SerializeField]
	[HideInInspector]
	private AnimationActionBase m_StartupAction;

	[SerializeField]
	[HideInInspector]
	private List<AnimationActionEntry> m_ActionEntries;

	[SerializeField]
	[HideInInspector]
	private List<Transition> m_Transitions = new List<Transition>();

	[CanBeNull]
	private IReadOnlyDictionary<UnitAnimationType, AnimationActionEntry> m_ActionsByType;

	public List<AnimationActionEntry> ActionEntries => m_ActionEntries;

	public IEnumerable<AnimationActionBase> Actions => m_ActionEntries.Select((AnimationActionEntry x) => x.Action);

	public IEnumerable<Transition> Transitions => m_Transitions;

	public AnimationActionBase StartupAction
	{
		get
		{
			return m_StartupAction;
		}
		set
		{
			if (value == null)
			{
				m_StartupAction = null;
				return;
			}
			if (Actions.Contains(value))
			{
				m_StartupAction = value;
				return;
			}
			UnityEngine.Debug.LogErrorFormat("Action {0} dont included in this AnimationSet.", value);
		}
	}

	public UnitAnimationAction GetAction(UnitAnimationType type)
	{
		if (m_ActionsByType == null)
		{
			m_ActionsByType = (from v in ActionEntries
				group v by v.Type).ToDictionary((IGrouping<UnitAnimationType, AnimationActionEntry> v) => v.Key, (IGrouping<UnitAnimationType, AnimationActionEntry> v) => v.FirstOrDefault());
		}
		if (!m_ActionsByType.TryGetValue(type, out var value))
		{
			PFLog.Animations.Error($"{base.name}: Unexpected behaviour! No action of type {type}!");
			return null;
		}
		if (!value.IsUsing)
		{
			return null;
		}
		UnitAnimationAction obj = value.Action as UnitAnimationAction;
		if (!obj)
		{
			PFLog.Animations.Error(this, $"{base.name}: action of type {type} is not set");
		}
		return obj;
	}

	private void OnValidate()
	{
	}

	public void AddAction(AnimationActionBase action)
	{
		if (Actions.Contains(action))
		{
			PFLog.Animations.Error($"Action {action} already in AnimationSet");
			return;
		}
		UnitAnimationAction unitAction = action as UnitAnimationAction;
		if ((object)unitAction == null)
		{
			PFLog.Animations.Error($"Can't add action. {action} is not UnitAnimationAction");
			return;
		}
		int index = m_ActionEntries.FindIndex((AnimationActionEntry x) => x.Type == unitAction.Type);
		m_ActionEntries[index].Action = action;
	}

	public void RemoveAction(AnimationActionBase action)
	{
		int index = m_ActionEntries.FindIndex((AnimationActionEntry x) => x.Action == action);
		m_ActionEntries[index].Action = null;
		for (int i = 0; i < m_Transitions.Count; i++)
		{
			Transition transition = m_Transitions[i];
			if (transition.FromAction == action || transition.ToAction == action)
			{
				RemoveTransition(transition);
				i--;
			}
		}
		if (m_StartupAction == action)
		{
			m_StartupAction = null;
		}
	}

	public bool AddTransition(Transition transition)
	{
		if (m_Transitions.Any((Transition t) => t.FromClip == transition.FromClip && t.ToClip == transition.ToClip && t.FromAction == transition.FromAction && t.ToAction == transition.ToAction))
		{
			return false;
		}
		m_Transitions.Add(transition);
		return true;
	}

	public bool RemoveTransition(Transition transition)
	{
		return m_Transitions.Remove(transition);
	}
}
