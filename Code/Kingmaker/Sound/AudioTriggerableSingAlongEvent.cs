using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioTriggerableSingAlongEvent : AudioTriggerableEvent
{
	[Serializable]
	private class SingAlongEntry
	{
		public BlueprintUnitReference Unit;

		public List<AkEventReference> SingAlongEvents = new List<AkEventReference>();
	}

	private const string SING_ALONG_PREFIX = "Phrase_";

	[SerializeField]
	private List<SingAlongEntry> m_SingAlongEntries = new List<SingAlongEntry>();

	[SerializeField]
	private float m_SingAlongRadius = 5f;

	public override void OnTrigger()
	{
		if (m_ActionMode)
		{
			m_Event.ExecuteAction(base.gameObject, m_Action, (int)(1000f * m_TransitionDuration), m_CurveInterpolation);
		}
		else
		{
			AkUnitySoundEngine.PostEvent(m_Event.ValueHash, base.gameObject, 8192u, OnMusicSync, null);
		}
	}

	private void OnMusicSync(object inCookie, AkCallbackType inType, AkCallbackInfo inInfo)
	{
		if (!(inInfo is AkMusicSyncCallbackInfo akMusicSyncCallbackInfo))
		{
			return;
		}
		SingAlongEntry singAlongEntry = null;
		BaseUnitEntity baseUnitEntity = null;
		foreach (SingAlongEntry singAlongEntry2 in m_SingAlongEntries)
		{
			foreach (BaseUnitEntity item in Game.Instance.Player.Party)
			{
				if (item.DistanceTo(base.transform.position) <= m_SingAlongRadius && (singAlongEntry2.Unit.Is(item.Blueprint) || (item.Blueprint.PrototypeLink != null && singAlongEntry2.Unit.Is((BlueprintUnit)item.Blueprint.PrototypeLink))))
				{
					singAlongEntry = singAlongEntry2;
					baseUnitEntity = item;
					break;
				}
			}
			if (singAlongEntry != null)
			{
				break;
			}
		}
		if (!akMusicSyncCallbackInfo.userCueName.StartsWith("Phrase_") || !int.TryParse(akMusicSyncCallbackInfo.userCueName.Substring("Phrase_".Length), out var result))
		{
			PFLog.VO.Error("[VO] Can't Sing Along! " + base.gameObject.name + " musicInfo.userCueName in wrong format: " + akMusicSyncCallbackInfo.userCueName + ". Should start with Phrase_");
		}
		else if (singAlongEntry != null && baseUnitEntity != null && singAlongEntry.SingAlongEvents.Count > result)
		{
			PFLog.VO.Log("[VO] Sing Along! " + baseUnitEntity.CharacterName + " sings " + singAlongEntry.SingAlongEvents[result].Value);
			singAlongEntry.SingAlongEvents[result].Post(baseUnitEntity.View.gameObject);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, m_SingAlongRadius);
	}
}
