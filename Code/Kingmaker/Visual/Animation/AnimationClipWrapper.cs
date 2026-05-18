using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Enums.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Decorators;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[Serializable]
[CreateAssetMenu(fileName = "AnimationClipWithEvents", menuName = "Animation Manager/Animation Clip with Events")]
public class AnimationClipWrapper : AnimationClipWrapperSwitcher
{
	private enum RecognizedEventNames
	{
		PostEvent = 0,
		PostEventMapped = 1,
		PostMainWeaponEquipEvent = 4,
		PostOffWeaponEquipEvent = 5,
		PostMainWeaponUnequipEvent = 6,
		PostOffWeaponUnequipEvent = 7,
		PostArmorFoleyEvent = 8,
		PostEventWithSurface = 9,
		PostCommandActEvent = 11,
		PostEventWithPrefix = 12,
		PostDecoratorObject = 13,
		PlayFootstep = 14,
		PlayBodyfall = 15,
		FxAnimatorToggleAction = 16
	}

	[SerializeField]
	private AnimationClip m_AnimationClip;

	[SerializeField]
	private List<AnimationClipEventTrack> m_EventTracks = new List<AnimationClipEventTrack>();

	[NonSerialized]
	private AnimationClipEvent[] _AnimationEventsSorted;

	[NonSerialized]
	private AnimationClipEvent[] _MechanicEventsSorted;

	[HideInInspector]
	[SerializeField]
	private string m_CreationDate;

	[HideInInspector]
	[SerializeField]
	private List<string> m_LabelsGenerated = new List<string>();

	[HideInInspector]
	[SerializeField]
	private List<string> m_LabelsCustom = new List<string>();

	public AnimationClip AnimationClip
	{
		get
		{
			return m_AnimationClip;
		}
		set
		{
			m_AnimationClip = value;
		}
	}

	public List<AnimationClipEventTrack> EventTracks
	{
		get
		{
			return m_EventTracks;
		}
		set
		{
			m_EventTracks = value;
		}
	}

	public float Length
	{
		get
		{
			if (!(AnimationClip != null))
			{
				return 0f;
			}
			return AnimationClip.length;
		}
	}

	public bool IsLooping
	{
		get
		{
			if (!(AnimationClip != null))
			{
				return false;
			}
			return AnimationClip.isLooping;
		}
	}

	public IEnumerable<AnimationClipEvent> Events => from _eventTrack in EventTracks.SelectMany(delegate(AnimationClipEventTrack _eventTrack)
		{
			if (_eventTrack == null)
			{
				PFLog.Default.Error("Event track is null!");
			}
			return (_eventTrack?.Events).EmptyIfNull();
		})
		where _eventTrack != null
		select _eventTrack;

	public AnimationClipEvent[] AnimationEventsSorted
	{
		get
		{
			if (_AnimationEventsSorted == null)
			{
				_AnimationEventsSorted = Events.Where(AnimationEventFilter).ToArray();
				if (_AnimationEventsSorted.Any())
				{
					Array.Sort(_AnimationEventsSorted, (AnimationClipEvent _event1, AnimationClipEvent _event2) => (int)((_event1.Time - _event2.Time) * 1000f));
				}
			}
			return _AnimationEventsSorted;
		}
	}

	public AnimationClipEvent[] MechanicEventsSorted
	{
		get
		{
			if (_MechanicEventsSorted == null)
			{
				_MechanicEventsSorted = Events.Where(MechanicEventFilter).ToArray();
				if (_MechanicEventsSorted.Any())
				{
					Array.Sort(_MechanicEventsSorted, (AnimationClipEvent _event1, AnimationClipEvent _event2) => (int)((_event1.Time - _event2.Time) * 1000f));
				}
			}
			return _MechanicEventsSorted;
		}
	}

	public string CreationDate
	{
		get
		{
			return m_CreationDate;
		}
		set
		{
			m_CreationDate = value;
		}
	}

	public List<string> LabelsGenerated
	{
		get
		{
			return m_LabelsGenerated;
		}
		set
		{
			m_LabelsGenerated = value;
		}
	}

	public List<string> LabelsCustom
	{
		get
		{
			return m_LabelsCustom;
		}
		set
		{
			m_LabelsCustom = value;
		}
	}

	private static bool MechanicEventFilter(AnimationClipEvent e)
	{
		return e is AnimationClipEventAct;
	}

	private static bool AnimationEventFilter(AnimationClipEvent e)
	{
		return !MechanicEventFilter(e);
	}

	public AnimationClipWrapper(AnimationClip animationClip, IEnumerable<AnimationClipEventTrack> animationSoundTracks = null)
	{
		m_AnimationClip = animationClip;
		m_EventTracks = ((animationSoundTracks != null) ? new List<AnimationClipEventTrack>(animationSoundTracks) : new List<AnimationClipEventTrack>());
	}

	public override AnimationClipWrapper GetWrapper(IAnimationManager animationManager)
	{
		return this;
	}

	public override IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		yield return this;
	}

	public override string ToString()
	{
		return string.Format("{0} {1}", m_AnimationClip, string.Join(", ", m_EventTracks.Select((AnimationClipEventTrack _track) => (!(_track != null)) ? "" : _track.ToString())));
	}

	private static AnimationClipEvent GetAnimationClipEvent(AnimationEvent animationEvent)
	{
		if (animationEvent == null)
		{
			throw new ArgumentNullException("animationEvent");
		}
		if (!Enum.TryParse<RecognizedEventNames>(animationEvent.functionName, ignoreCase: false, out var result))
		{
			throw new NotSupportedException("Animation event of type " + animationEvent.functionName + " is not supported.");
		}
		return result switch
		{
			RecognizedEventNames.PostEvent => new AnimationClipEventSound(animationEvent.time, isLooped: false, animationEvent.stringParameter, animationEvent.stringParameter, 1f), 
			RecognizedEventNames.PostEventWithPrefix => new AnimationClipEventSoundWithPrefix(animationEvent.time, isLooped: false, animationEvent.stringParameter, 1f), 
			RecognizedEventNames.PostEventMapped => new AnimationClipEventSoundMapped(animationEvent.time, (MappedAnimationEventType)animationEvent.intParameter), 
			RecognizedEventNames.PostMainWeaponEquipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.MainWeaponEquip), 
			RecognizedEventNames.PostOffWeaponEquipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.OffWeaponEquip), 
			RecognizedEventNames.PostMainWeaponUnequipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.MainWeaponUnequip), 
			RecognizedEventNames.PostOffWeaponUnequipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.OffWeaponUnequip), 
			RecognizedEventNames.PostArmorFoleyEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.ArmorFoley), 
			RecognizedEventNames.PostEventWithSurface => new AnimationClipEventSoundSurface(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.PlayBodyfall => new AnimationClipEventBodyFall(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.PlayFootstep => new AnimationClipEventFootStep(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.FxAnimatorToggleAction => new AnimationClipEventToggleFxAnimator(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.PostCommandActEvent => new AnimationClipEventAct(animationEvent.time), 
			RecognizedEventNames.PostDecoratorObject => new AnimationClipEventDecoratorObject(animationEvent.time, animationEvent.floatParameter, animationEvent.objectReferenceParameter as UnitAnimationDecoratorObject), 
			_ => throw new NotSupportedException($"Animation event of type {result} is not supported."), 
		};
	}
}
