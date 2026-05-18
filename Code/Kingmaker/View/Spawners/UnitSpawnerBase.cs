using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Customization;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("6011d470489d44f18bb1b158e71ade47")]
public abstract class UnitSpawnerBase : EntityViewBase, IAbstractUnitSpawnerConfig, IEntityConfig
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Blueprint;

	[SerializeField]
	private bool m_SpawnOnSceneInit = true;

	[SerializeField]
	private bool m_RespawnIfDead;

	[SerializeField]
	[ShowCreator]
	private ConditionsReference m_spawnConditions;

	[HideInInspector]
	[SerializeField]
	private UnitCustomizationVariation m_SelectedCustomizationVariation;

	[CanBeNull]
	private UnitGroupView m_Group;

	public bool HasSpawned
	{
		get
		{
			return Data.HasSpawned;
		}
		protected set
		{
			Data.HasSpawned = value;
		}
	}

	public BlueprintUnit Blueprint
	{
		get
		{
			return m_Blueprint?.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintUnitReference>();
		}
	}

	public AbstractUnitEntity SpawnedUnit => Data?.SpawnedUnit;

	public bool SpawnOnSceneInit => m_SpawnOnSceneInit;

	public bool RespawnIfDead => m_RespawnIfDead;

	public ConditionsHolder SpawnConditions => m_spawnConditions;

	public bool SpawnedUnitHasDied => Data.SpawnedUnitHasDied;

	public new AbstractUnitSpawnerEntity Data => (AbstractUnitSpawnerEntity)base.Data;

	public override bool CreatesDataOnLoad => true;

	public bool HasCustomizationPreset => Blueprint?.CustomizationPreset != null;

	[CanBeNull]
	public string GroupId => m_Group?.UniqueId;

	public UnitCustomizationVariation SelectedCustomizationVariation => m_SelectedCustomizationVariation;

	public IUnitSpawnRestriction[] Restrictions => GetComponents<IUnitSpawnRestriction>();

	public string sceneName => base.gameObject.scene.name;

	protected override void Awake()
	{
		base.Awake();
		m_Group = GetComponentInParent<UnitGroupView>();
	}

	public sealed override Entity CreateEntityData(bool load)
	{
		return CreateSpawnerEntity(load);
	}

	protected abstract AbstractUnitSpawnerEntity CreateSpawnerEntity(bool load);

	string IAbstractUnitSpawnerConfig.get_name()
	{
		return base.name;
	}
}
