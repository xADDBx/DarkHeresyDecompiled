using JetBrains.Annotations;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Unit.Utility;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("310bf69dec7144cea850fa7ef7239508")]
public class BlueprintArmyType : BlueprintScriptableObject
{
	[SerializeField]
	private BpRef<BlueprintFeature>[] m_Features;

	public UnitDifficultyType DifficultyType;

	public UnitStatModifiers StatModifiers = new UnitStatModifiers();

	[SerializeField]
	private bool m_IsHuman;

	[SerializeField]
	private bool m_IsXenos;

	[SerializeField]
	private bool m_IsDaemon;

	[InfoBox("Должна стоять галка IsHumanoid, чтобы юнит мог пользоваться лестницей (CustomLinks)")]
	[SerializeField]
	private bool m_IsHumanoid;

	[Header("Morale Buffs Override")]
	[InfoBox("Дефолтные баффы задаются в MoraleRoot")]
	[SerializeField]
	private BpRef<BlueprintBuff> m_HeroicBuffOverride;

	[SerializeField]
	private BpRef<BlueprintBuff> m_BrokenBuffOverride;

	public BpRefArray<BlueprintFeature> Features => m_Features;

	public bool IsHuman => m_IsHuman;

	public bool IsXenos => m_IsXenos;

	public bool IsDaemon => m_IsDaemon;

	public bool IsHumanoid => m_IsHumanoid;

	[CanBeNull]
	public BlueprintBuff HeroicBuffOverride => m_HeroicBuffOverride;

	[CanBeNull]
	public BlueprintBuff BrokenBuffOverride => m_BrokenBuffOverride;
}
