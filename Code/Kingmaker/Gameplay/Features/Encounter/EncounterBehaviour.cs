using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Spawners;
using Owlcat.AI;
using Owlcat.BehaviourTrees;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Encounter;

public sealed class EncounterBehaviour : MonoBehaviour
{
	public delegate void CallbackType(EncounterBehaviour encounter);

	[Serializable]
	public sealed class Group
	{
		[TextArea]
		public string Comment;

		public Color GizmoColor = Color.yellow;

		public int CombatGroup;

		public bool IsSquad;

		[ShowIf("IsSquad")]
		public UnitSpawner SquadLeader = new UnitSpawner();

		public UnitSpawner[] Spawners = new UnitSpawner[0];

		public bool TryValidateSquadBrains(out BlueprintUnit problematicUnit)
		{
			if (IsSquad)
			{
				UnitSpawner[] spawners = Spawners;
				foreach (UnitSpawner unitSpawner in spawners)
				{
					if (!(unitSpawner == null))
					{
						BlueprintUnit blueprint = unitSpawner.Blueprint;
						if (blueprint != null && blueprint.GetComponent<AIAgentComponent>()?.BehaviourTree.Nodes.All((BehaviourTreeNodeElement node) => node is SquadControlNodeElement) == false)
						{
							problematicUnit = unitSpawner.Blueprint;
							return false;
						}
					}
				}
			}
			problematicUnit = null;
			return true;
		}
	}

	[CanBeNull]
	public static CallbackType OnResetCallback;

	[CanBeNull]
	public static CallbackType OnValidateCallback;

	[SerializeField]
	[InspectorReadOnly]
	private BpRef<BlueprintEncounter> _blueprint;

	public Color MainGroupGizmosColor = Color.red;

	[TextArea]
	public string MainGroupComment;

	public Group[] AdditionalGroups = new Group[0];

	public BlueprintEncounter Blueprint
	{
		get
		{
			return _blueprint;
		}
		set
		{
			_blueprint = value.Reference();
		}
	}

	private void OnValidate()
	{
		OnValidateCallback?.Invoke(this);
		Group[] additionalGroups = AdditionalGroups;
		foreach (Group group in additionalGroups)
		{
			if (group.GizmoColor == default(Color))
			{
				group.GizmoColor = Color.yellow;
			}
		}
	}

	private void Reset()
	{
		OnResetCallback?.Invoke(this);
	}
}
