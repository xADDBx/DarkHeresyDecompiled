using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("686b4249a3c54e3b9539d2efeef970d4")]
public class BuffStacksForOverheal : UnitFactComponentDelegate
{
	public class ComponentData : IEntityFactComponentTransientData
	{
		public int OldWounds { get; set; }
	}

	public bool ReplaceLowStacks;

	public bool BuffStacksTemporaryWounds;

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();
}
