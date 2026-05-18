using System.Collections;
using Kingmaker.Cheats;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.View;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker;

public class RagdollRecieverMain : MonoBehaviour
{
	private AkSwitchReference testAkRef;

	private AkSwitchReference testAkRef02;

	public float TimerWait = 0.5f;

	public float Impulse01 = 5f;

	public float Impulse02 = 10f;

	private float previousImpulse;

	public UnitEntityView _UnitEntityView;

	private Coroutine m_CurrentDelayedStop;

	private bool firstLaunch = true;

	private string SoundString = "BodyfallsRagDoll_Play";

	private void Awake()
	{
		FindRagdollSender();
	}

	public void FindRagdollSender()
	{
		RagdollSender[] componentsInChildren = GetComponentsInChildren<RagdollSender>();
		RagdollSender[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Receiver = this;
		}
		_UnitEntityView = GetComponentInParent<UnitEntityView>();
		if (CheatsSoundRagdoll.SoundRagdollDebug >= 1)
		{
			PFLog.SoundRagdoll.Log(string.Format("[Init] senders:{0} unit:{1}", componentsInChildren.Length, _UnitEntityView?.name ?? "NULL"));
		}
		if (componentsInChildren.Length == 0 && _UnitEntityView != null)
		{
			PFLog.TechArt.Log("No Ragdoll sender scripts! Rebake prefab:" + _UnitEntityView.name);
		}
	}

	public void Send(string _name, float _value, SurfaceType? _surface)
	{
		bool flag = CheatsSoundRagdoll.SoundRagdollDebug >= 1;
		if (firstLaunch)
		{
			firstLaunch = false;
			m_CurrentDelayedStop = StartCoroutine(DelayedStop(TimerWait));
			if (flag)
			{
				PFLog.SoundRagdoll.Log($"[Receiver] first launch — cooldown {TimerWait}s started");
			}
		}
		if (_value < Impulse01)
		{
			if (flag)
			{
				PFLog.SoundRagdoll.Log($"[Receiver] {_name} impulse:{_value:F1} < threshold:{Impulse01} — skip");
			}
			return;
		}
		if (m_CurrentDelayedStop != null)
		{
			if (flag)
			{
				PFLog.SoundRagdoll.Log($"[Receiver] {_name} impulse:{_value:F1} — blocked by cooldown");
			}
			return;
		}
		if (_UnitEntityView == null || _UnitEntityView.EntityData == null)
		{
			if (flag)
			{
				PFLog.SoundRagdoll.Log("[Receiver] " + _name + " — UnitEntityView or EntityData is null!");
			}
			return;
		}
		if (_UnitEntityView.EntityData.Health?.LastHandledDamage == null)
		{
			if (flag)
			{
				PFLog.SoundRagdoll.Log("[Receiver] " + _name + " — LastHandledDamage is null, blocked!");
			}
			return;
		}
		previousImpulse = _value;
		testAkRef = _UnitEntityView.Blueprint.VisualSettings.BodySizeSoundSwitch;
		testAkRef02 = _UnitEntityView.Blueprint.VisualSettings.BodyTypeSoundSwitch;
		testAkRef.Set(base.gameObject);
		testAkRef02.Set(base.gameObject);
		SurfaceType? surfaceType = _surface ?? SoundSurfaceMap.GetSurfaceSoundTypeSwitch(base.transform.position);
		if (surfaceType.HasValue)
		{
			AkUnitySoundEngine.SetSwitch("Terrain", surfaceType.ToString(), base.gameObject);
		}
		SoundEventsManager.PostEvent(SoundString, base.gameObject);
		if (flag)
		{
			PFLog.SoundRagdoll.Log($"[Receiver] PLAY bone:{_name} impulse:{_value:F1} surface:{surfaceType} obj:{base.gameObject.name}");
		}
		m_CurrentDelayedStop = StartCoroutine(DelayedStop(TimerWait));
	}

	private IEnumerator DelayedStop(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_CurrentDelayedStop = null;
	}
}
