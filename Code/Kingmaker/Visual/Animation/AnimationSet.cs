using System;
using System.Collections.Generic;
using System.Linq;
using Animancer.TransitionLibraries;
using JetBrains.Annotations;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Decorators;
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
	private List<AnimationActionEntry> m_ActionEntries;

	[CanBeNull]
	private IReadOnlyDictionary<UnitAnimationType, AnimationActionEntry> m_ActionsByType;

	[HideInInspector]
	public TransitionLibraryAsset TransitionLibrary;

	public UnitAnimationDecoratorObject[] Decorators;

	public List<AnimationActionEntry> ActionEntries => m_ActionEntries;

	public IEnumerable<AnimationActionBase> Actions => m_ActionEntries.Select((AnimationActionEntry x) => x.Action);

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
}
