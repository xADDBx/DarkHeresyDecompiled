using System.Collections.Generic;
using System.Linq;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Visual.Effects.GlobalEffects;
using Owlcat.Runtime.Visual.Effects.GlobalEffects.Controllers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.GlobalEffects;

public class SoundController : OverridableControllerBase<SoundComponent, SoundOverride>
{
	private const float kDiffThreshold = 0.5f;

	private const string kRtpcGlobalEffectGain = "rtpc_GlobalEffect_Gain";

	private const string kRtpcGlobalEffect = "rtpc_GlobalEffect";

	private AudioObject m_AudioObject;

	private uint m_PlayingId;

	private float m_LastGainRTPCValue;

	private float m_LastGlobalEffectRTPCValue;

	public SoundController(SoundComponent component)
		: base(component)
	{
		m_PlayingId = 0u;
	}

	public override void Initialize(GlobalEffectContext context)
	{
		base.Initialize(context);
		GameObject gameObject = new GameObject("AudioObject_" + base.Component.SoundEventName);
		gameObject.transform.parent = context.GlobalEffect.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		m_AudioObject = gameObject.AddComponent<AudioObject>();
		if (context.GlobalEffect.TryGetComponent<Volume>(out var component) && !component.isGlobal && context.GlobalEffect.TryGetComponent<Collider>(out var component2))
		{
			Bounds bounds = component2.bounds;
			Vector3 extents = bounds.extents;
			SoundGridDrawer soundGridDrawer = gameObject.AddComponent<SoundGridDrawer>();
			soundGridDrawer.gridPoints = new List<Vector3>
			{
				bounds.center + new Vector3(0f - extents.x, 0f, 0f - extents.z),
				bounds.center + new Vector3(0f - extents.x, 0f, extents.z),
				bounds.center + new Vector3(extents.x, 0f, extents.z),
				bounds.center + new Vector3(extents.x, 0f, 0f - extents.z)
			};
			soundGridDrawer.gridYPosition = bounds.center.y;
			SoundGridMover soundGridMover = gameObject.AddComponent<SoundGridMover>();
			soundGridMover.gridDrawer = soundGridDrawer;
			if (!Game.Instance.Controllers.CustomUpdateController.Contains(soundGridMover))
			{
				soundGridMover.OnEnable();
			}
		}
	}

	public override void Update(GlobalEffectContext context)
	{
		if (context.Camera != Camera.main)
		{
			return;
		}
		if (m_PlayingId == 0)
		{
			m_PlayingId = SoundEventsManager.PostEvent(base.Component.SoundEventName, m_AudioObject.gameObject);
		}
		else
		{
			if (!(base.VolumeOverride != null))
			{
				return;
			}
			SoundParameter soundParameter = base.VolumeOverride.CompositeParameterList.Value.FirstOrDefault((SoundParameter se) => se.AkSoundEvent == base.Component.SoundEventName);
			if (soundParameter != null)
			{
				if (Mathf.Abs(m_LastGainRTPCValue - soundParameter.GlobalEffectGainRTPC.value) > 0.5f)
				{
					m_LastGainRTPCValue = soundParameter.GlobalEffectGainRTPC.value;
					AkUnitySoundEngine.SetRTPCValue("rtpc_GlobalEffect_Gain", soundParameter.GlobalEffectGainRTPC.value, m_AudioObject.gameObject);
					UnityEngine.Debug.Log(string.Format("{0}: {1}", "rtpc_GlobalEffect_Gain", m_LastGainRTPCValue));
				}
				if (Mathf.Abs(m_LastGlobalEffectRTPCValue - soundParameter.GlobalEffectRTPC.value) > 0.5f)
				{
					m_LastGlobalEffectRTPCValue = soundParameter.GlobalEffectRTPC.value;
					AkUnitySoundEngine.SetRTPCValue("rtpc_GlobalEffect", soundParameter.GlobalEffectRTPC.value, m_AudioObject.gameObject);
					UnityEngine.Debug.Log($"rtpc_GlobalEffect: {m_LastGlobalEffectRTPCValue}");
				}
			}
		}
	}

	public override void CleanUp()
	{
		if (m_PlayingId != 0)
		{
			SoundEventsManager.StopPlayingById(m_PlayingId);
			m_PlayingId = 0u;
		}
	}
}
