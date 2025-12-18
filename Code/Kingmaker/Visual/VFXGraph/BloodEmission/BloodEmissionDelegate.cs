using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.Visual.VFXGraph.BloodEmission;

public class BloodEmissionDelegate : MonoSingleton<BloodEmissionDelegate>, IAreaActivationHandler, ISubscriber
{
	private Dictionary<int, BloodBinder> m_BindersDictionary = new Dictionary<int, BloodBinder>();

	private BlueprintArea m_LastLoadedArea;

	private void Awake()
	{
		m_LastLoadedArea = Game.Instance.CurrentlyLoadedArea;
		ClearAllParticles();
		EventBus.Subscribe(this);
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
	}

	public void Emit(Vector3 emitPos, Vector3 emitAngles, Vector3 emitSize, Color emitColor, GameObject vfxGraphPrefab)
	{
		GetBinder(vfxGraphPrefab).Emit(emitPos, emitAngles, emitSize, emitColor);
	}

	public void OnAreaActivated()
	{
		if (Game.Instance.CurrentlyLoadedArea == m_LastLoadedArea)
		{
			ClearParticlesAfterCurrentTime();
		}
		else
		{
			ClearAllParticles();
		}
	}

	private BloodBinder GetBinder(GameObject vfxGraphPrefab)
	{
		int instanceID = vfxGraphPrefab.GetInstanceID();
		if (!m_BindersDictionary.TryGetValue(instanceID, out var value))
		{
			GameObject gameObject = Object.Instantiate(vfxGraphPrefab, Vector3.zero, quaternion.identity, base.transform);
			value = gameObject.GetComponent<BloodBinder>();
			if (!value)
			{
				value = gameObject.AddComponent<BloodBinder>();
			}
			m_BindersDictionary[instanceID] = value;
		}
		return value;
	}

	private void ClearAllParticles()
	{
		foreach (BloodBinder value in m_BindersDictionary.Values)
		{
			value.KillAll();
		}
	}

	private void ClearParticlesAfterCurrentTime()
	{
		float time = (float)Game.Instance.Controllers.TimeController.RealTime.TotalSeconds;
		foreach (BloodBinder value in m_BindersDictionary.Values)
		{
			value.KillOlderThan(time);
		}
	}
}
