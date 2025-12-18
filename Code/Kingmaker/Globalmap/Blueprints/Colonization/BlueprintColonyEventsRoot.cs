using System;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Obsolete]
[TypeId("d5360515209f38448bb1eed62787cfca")]
public class BlueprintColonyEventsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class ColonyEventToTimer
	{
		[SerializeField]
		[JsonProperty]
		private BlueprintColonyEventReference m_ColonyEvent;

		[SerializeField]
		[JsonProperty]
		public int Segments;

		public BlueprintColonyEvent ColonyEvent => m_ColonyEvent?.Get();
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintColonyEventsRoot>
	{
	}

	public ColonyEventToTimer[] Events;

	public int EventCountInColony = 2;
}
