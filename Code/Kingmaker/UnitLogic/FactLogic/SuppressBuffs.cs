using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Buffs and Facts/SuppressBuffs")]
[TypeId("54b8118d35ef44847b10a125ed9d64f7")]
public class SuppressBuffs : UnitFactComponentDelegate
{
	[SerializeField]
	[FormerlySerializedAs("Buffs")]
	private BlueprintBuffReference[] m_Buffs = new BlueprintBuffReference[0];

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups = new BlueprintAbilityGroupReference[0];

	public ReferenceArrayProxy<BlueprintBuff> Buffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffs = m_Buffs;
			return buffs;
		}
	}

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		UnitPartBuffSuppress orCreate = base.Owner.GetOrCreate<UnitPartBuffSuppress>();
		foreach (BlueprintBuff item in Buffs.ToList())
		{
			orCreate.Suppress(item);
		}
		foreach (BlueprintAbilityGroup item2 in Groups.ToList())
		{
			orCreate.Suppress(item2);
		}
	}

	protected override void OnDeactivate()
	{
		UnitPartBuffSuppress optional = base.Owner.GetOptional<UnitPartBuffSuppress>();
		if (optional == null)
		{
			PFLog.Default.Error("UnitPartSuppressBuff is missing");
			return;
		}
		foreach (BlueprintBuff item in Buffs.ToList())
		{
			optional.Release(item);
		}
		foreach (BlueprintAbilityGroup item2 in Groups.ToList())
		{
			optional.Release(item2);
		}
	}
}
