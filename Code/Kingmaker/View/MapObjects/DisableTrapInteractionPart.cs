using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Interaction;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.MapObjects.Traps;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class DisableTrapInteractionPart : InteractionPart<InteractionDisableTrapSettings>, IHashable, IOwlPackable<DisableTrapInteractionPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DisableTrapInteractionPart",
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

	public new TrapObjectData Owner => (TrapObjectData)base.Owner;

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	public override BaseUnitEntity SelectUnit(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, IInteractionVariantActor variantActor = null)
	{
		return Owner.SelectUnit(units, muteEvents);
	}

	public override bool CanInteract()
	{
		if (base.CanInteract() && Owner.TrapActive)
		{
			if (!Owner.Config.IsNotScriptZoneTrigger)
			{
				return Owner.IsAwarenessCheckPassed;
			}
			return true;
		}
		return false;
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		if (Owner.TrapActive)
		{
			Owner.Interact(user);
		}
	}

	public override bool IsEnoughCloseForInteraction(BaseUnitEntity unit, Vector3? position = null)
	{
		if (unit.IsInCombat && Owner.Config.ScriptZone.Entity != null)
		{
			IEnumerable<GridNodeBase> nodesSpiralAround = GridAreaHelper.GetNodesSpiralAround((GridNodeBase)unit.CurrentNode.node, unit.SizeRect, 1);
			foreach (IScriptZoneShape shape in Owner.Config.ScriptZone.Entity.Config.Shapes)
			{
				foreach (GridNodeBase item in nodesSpiralAround)
				{
					if (shape.Contains(item))
					{
						return true;
					}
				}
			}
		}
		return base.IsEnoughCloseForInteraction(unit, position);
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
		DisableTrapInteractionPart source = new DisableTrapInteractionPart();
		result = Unsafe.As<DisableTrapInteractionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DisableTrapInteractionPart>(OwlPackTypeInfo);
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
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DisableTrapInteractionPart>();
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
