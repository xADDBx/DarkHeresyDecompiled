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
		foreach (BlueprintBuff buff in Buffs)
		{
			orCreate.Suppress(buff);
		}
		foreach (BlueprintAbilityGroup group in Groups)
		{
			orCreate.Suppress(group);
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
		foreach (BlueprintBuff buff in Buffs)
		{
			optional.Release(buff);
		}
		foreach (BlueprintAbilityGroup group in Groups)
		{
			optional.Release(group);
		}
	}
}
