using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Enums.Stats;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Encounter.Components;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.AI;
using Owlcat.BehaviourTrees;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("40bcbced7cd544b798c360aa423f1fd8")]
public sealed class BlueprintEncounter : BlueprintMechanicEntityFact
{
	[Serializable]
	public sealed class SpawnerEntry
	{
		public EntityReference Ref = new EntityReference();

		[InspectorReadOnly]
		public BpRef<BlueprintUnit> Unit = new BpRef<BlueprintUnit>();

		[InspectorReadOnly]
		public bool Active;
	}

	[Serializable]
	public sealed class Group
	{
		[InspectorReadOnly]
		[TextArea]
		public string Comment;

		[InspectorReadOnly]
		public int CombatGroup;

		[InspectorReadOnly]
		public bool IsSquad;

		[InspectorReadOnly]
		[ShowIf("IsSquad")]
		public SpawnerEntry SquadLeader = new SpawnerEntry();

		public SpawnerEntry[] Spawners = new SpawnerEntry[0];

		public bool TryValidateSquadBrains(out BpRef<BlueprintUnit> problematicUnit)
		{
			if (IsSquad)
			{
				SpawnerEntry[] spawners = Spawners;
				foreach (SpawnerEntry spawnerEntry in spawners)
				{
					if (spawnerEntry != null)
					{
						BlueprintUnit blueprint = spawnerEntry.Unit.Blueprint;
						if (blueprint != null && blueprint.GetComponent<AIAgentComponent>()?.BehaviourTree.Nodes.All((BehaviourTreeNodeElement node) => node is SquadControlNodeElement) == false)
						{
							problematicUnit = spawnerEntry.Unit;
							return false;
						}
					}
				}
			}
			problematicUnit = null;
			return true;
		}
	}

	[InspectorReadOnly]
	public BpRef<BlueprintArea> Area = new BpRef<BlueprintArea>();

	[InspectorReadOnly]
	public SceneReference Scene = new SceneReference();

	[SerializeField]
	[ExcludeFieldFromBuild]
	[JsonIgnore]
	private EncounterStatsPlaceholder _statsPlaceholder;

	[SerializeField]
	private bool _overrideCombatCR;

	[SerializeField]
	[ShowIf("_overrideCombatCR")]
	private int _combatCR;

	[Header("Experience")]
	public UnitDifficultyType Difficulty = UnitDifficultyType.Common;

	[SerializeField]
	private bool _overrideExperience;

	[SerializeField]
	[ShowIf("_overrideExperience")]
	private int _overrideExperienceValue;

	[Header("Victory")]
	public bool AllowVictoryWhenAllEnemiesDead = true;

	public bool AllowVictoryByMorale = true;

	public bool AllowVictoryByCustomCondition;

	[ShowIf("AllowVictoryByCustomCondition")]
	public ConditionsChecker CustomVictoryCondition = new ConditionsChecker();

	public ActionList OnVictory = new ActionList();

	public ActionList OnVictoryWhenAllEnemiesDead = new ActionList();

	public ActionList OnVictoryByMorale = new ActionList();

	[ShowIf("AllowVictoryByCustomCondition")]
	public ActionList OnVictoryByCustomCondition = new ActionList();

	[Header("Psychic Phenomena Override")]
	public PhenomenaListOverride PhenomenaOverride = new PhenomenaListOverride();

	public PhenomenaListOverride PerilsOverride = new PhenomenaListOverride();

	[Header("Groups")]
	public Group[] Groups = new Group[0];

	[Header("Additional combat objective")]
	[SerializeField]
	private BlueprintEtudeReference[] _additionalCombatObjectiveMarkers;

	[InfoBox("Этот блок настроек - meta информация для статистики")]
	public Chapter Chapter;

	public Cluster Cluster;

	public int CR => CROverride ?? Area.MaybeBlueprint?.GetCR() ?? 0;

	public int? CROverride
	{
		get
		{
			int? result = EncounterCROverrideHelper.Get(AssetGuid);
			if (!result.HasValue)
			{
				if (!_overrideCombatCR)
				{
					return null;
				}
				return _combatCR;
			}
			return result;
		}
	}

	public bool IsDefault => ConfigRoot.Instance.EncounterRoot.DefaultEncounter == this;

	public int? OverrideExperience
	{
		get
		{
			if (!_overrideExperience)
			{
				return null;
			}
			return _overrideExperienceValue;
		}
	}

	public BlueprintEtudeReference[] Editor_AdditionalCombatObjectiveMarkers => _additionalCombatObjectiveMarkers;
}
