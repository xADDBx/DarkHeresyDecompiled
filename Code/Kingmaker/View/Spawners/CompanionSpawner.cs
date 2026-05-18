using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("dfea570039374dd99e2e6cf487f9add8")]
public class CompanionSpawner : UnitSpawnerBase, ICompanionSpawnerConfig, IAbstractUnitSpawnerConfig, IEntityConfig, IAddInspectorGUI
{
	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenRemote;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenInCapital;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenDetached;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private bool m_SpawnWhenEx = true;

	[SerializeField]
	[ShowCreator]
	[HideIf("m_SpawnNpcCopy")]
	private ConditionsReference ControlCondition;

	[SerializeField]
	[ShowCreator]
	[HideIf("m_SpawnNpcCopy")]
	private ConditionsReference ShowCondition;

	[SerializeField]
	[HideIf("m_SpawnNpcCopy")]
	private BlueprintFactionReference m_OverrideFaction;

	[SerializeField]
	[AddInspector]
	[UsedImplicitly]
	private bool m_Dummy;

	[SerializeField]
	[Tooltip("Spawn an NPC companion copy instead, that have exactly the same look")]
	private bool m_SpawnNpcCopy;

	public new CompanionSpawnerEntity Data => (CompanionSpawnerEntity)base.Data;

	BlueprintFaction ICompanionSpawnerConfig.OverrideFaction => m_OverrideFaction;

	bool ICompanionSpawnerConfig.SpawnNpcCopy => m_SpawnNpcCopy;

	bool ICompanionSpawnerConfig.SpawnWhenRemote => m_SpawnWhenRemote;

	bool ICompanionSpawnerConfig.SpawnWhenEx => m_SpawnWhenEx;

	bool ICompanionSpawnerConfig.SpawnWhenInCapital => m_SpawnWhenInCapital;

	bool ICompanionSpawnerConfig.SpawnWhenDetached => m_SpawnWhenDetached;

	ConditionsHolder ICompanionSpawnerConfig.ControlCondition => ControlCondition;

	ConditionsHolder ICompanionSpawnerConfig.ShowCondition => ShowCondition;

	protected override AbstractUnitSpawnerEntity CreateSpawnerEntity(bool load)
	{
		return Entity.Initialize(new CompanionSpawnerEntity(this));
	}

	string IAbstractUnitSpawnerConfig.get_name()
	{
		return base.name;
	}
}
