using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class InteractionStairsPart : InteractionPart<InteractionStairsSettings>, IHashable, IOwlPackable<InteractionStairsPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionStairsPart",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool))
		}
	};

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		if (user.IsDirectlyControllable)
		{
			GraphNode node = ObstacleAnalyzer.GetNearestNode(user.Position).node;
			GraphNode startNode = base.Settings.NodeLink.StartNode;
			GraphNode endNode = base.Settings.NodeLink.EndNode;
			Vector3 posToMove = ((node == endNode) ? startNode.Vector3Position() : ((node != startNode) ? ((Math.Abs(node.Vector3Position().y - endNode.Vector3Position().y) < Math.Abs(node.Vector3Position().y - startNode.Vector3Position().y)) ? ObstacleAnalyzer.FindClosestPointToStandOn(startNode.Vector3Position(), user.MovementAgent.Corpulence, (GridNodeBase)startNode) : ObstacleAnalyzer.FindClosestPointToStandOn(endNode.Vector3Position(), user.MovementAgent.Corpulence, (GridNodeBase)endNode)) : endNode.Vector3Position()));
			MoveOnStairs(user, posToMove);
		}
	}

	private void MoveOnStairs(BaseUnitEntity selectedUnit, Vector3 posToMove)
	{
		Vector3 defaultDirection = ClickGroundHandler.GetDefaultDirection(posToMove);
		List<BaseUnitEntity> list = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.Where((BaseUnitEntity u) => u.IsDirectlyControllable && !u.MovementAgent.IsTraverseInProgress).ToList();
		List<BaseUnitEntity> list2 = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable).ToList();
		int unitIndex = list2.IndexOf(selectedUnit);
		Vector3 worldPosition = PartyFormationHelper.FindFormationCenterFromOneUnit(FormationAnchor.Front, defaultDirection, unitIndex, posToMove, list2, list.Select((BaseUnitEntity u) => u.FromBaseUnitEntity()).ToArray());
		UnitCommandsRunner.MoveSelectedUnitsToPointRT(selectedUnit, worldPosition, defaultDirection, isControllerGamepad: true, preview: false, 1f, list, null, list2);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InteractionStairsPart source = new InteractionStairsPart();
		result = Unsafe.As<InteractionStairsPart, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<InteractionStairsPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionStairsPart>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				base.AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				AlreadyVisited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LastCombatRoundInteractionAttempt = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
