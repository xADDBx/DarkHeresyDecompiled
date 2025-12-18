using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/CheatRoot")]
[TypeId("b0076bcae99e7b54281f148c70f9177f")]
public class CheatRoot : BlueprintScriptableObject
{
	[SerializeField]
	private BpRef<BlueprintBuff> m_Iddqd;

	[SerializeField]
	private BpRef<BlueprintUnit> m_Enemy;

	[SerializeField]
	private BpRef<BlueprintBuff>[] m_FullBuffList;

	public DamageSettings TestDamage = new DamageSettings
	{
		Bonus = 10
	};

	[Tooltip("Press key O for execute")]
	public ActionList TestAction;

	public static CheatRoot Instance => ConfigRoot.Instance.Cheats;

	public BlueprintBuff Iddqd => m_Iddqd;

	public BlueprintUnit Enemy => m_Enemy;

	public BpRefArray<BlueprintBuff> FullBuffList => m_FullBuffList;
}
