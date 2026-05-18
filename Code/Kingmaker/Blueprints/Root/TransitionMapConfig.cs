using System;
using System.Linq;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class TransitionMapConfig
{
	[SerializeField]
	private BpRef<BlueprintMultiEntrance>[] m_MultiEntrances = Array.Empty<BpRef<BlueprintMultiEntrance>>();

	public BlueprintMultiEntrance GetMultiEntrance(BlueprintMultiEntranceMap zone)
	{
		return m_MultiEntrances.Select((BpRef<BlueprintMultiEntrance> r) => r.MaybeBlueprint).FirstOrDefault((BlueprintMultiEntrance me) => me != null && me.Map == zone);
	}

	public bool HasVisibleEntries(BlueprintMultiEntranceMap zone)
	{
		BlueprintMultiEntrance multiEntrance = GetMultiEntrance(zone);
		if (multiEntrance != null)
		{
			return multiEntrance.GetVisibleEntries().Length != 0;
		}
		return false;
	}
}
