using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartJump : BaseUnitPart, IHashable, IOwlPackable<UnitPartJump>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Chunk : IHashable, IOwlPackable, IOwlPackable<Chunk>
	{
		[JsonProperty]
		[OwlPackInclude]
		public float MaxTime;

		[JsonProperty]
		[OwlPackInclude]
		public float PassedTime;

		[JsonProperty]
		[OwlPackInclude]
		public float InClipTime;

		[JsonProperty]
		[OwlPackInclude]
		public float Speed;

		[JsonProperty]
		[OwlPackInclude]
		public bool ProvokeAttackOfOpportunity;

		[JsonProperty]
		[OwlPackInclude]
		public bool IgnoreNavmesh;

		[JsonProperty]
		[OwlPackInclude]
		public bool PrepareForJump;

		[JsonProperty(IsReference = false)]
		[OwlPackInclude]
		public Vector3 TargetPosition;

		[JsonProperty]
		[OwlPackInclude]
		public MechanicEntity Pusher;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Chunk",
			OldNames = null,
			Fields = new FieldInfo[9]
			{
				new FieldInfo("MaxTime", typeof(float)),
				new FieldInfo("PassedTime", typeof(float)),
				new FieldInfo("InClipTime", typeof(float)),
				new FieldInfo("Speed", typeof(float)),
				new FieldInfo("ProvokeAttackOfOpportunity", typeof(bool)),
				new FieldInfo("IgnoreNavmesh", typeof(bool)),
				new FieldInfo("PrepareForJump", typeof(bool)),
				new FieldInfo("TargetPosition", typeof(Vector3)),
				new FieldInfo("Pusher", typeof(MechanicEntity))
			}
		};

		public bool IsFinished => PassedTime >= MaxTime;

		public float RemainingDistance => Mathf.Max(0f, MaxTime - PassedTime);

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref MaxTime);
			result.Append(ref PassedTime);
			result.Append(ref InClipTime);
			result.Append(ref Speed);
			result.Append(ref ProvokeAttackOfOpportunity);
			result.Append(ref IgnoreNavmesh);
			result.Append(ref PrepareForJump);
			result.Append(ref TargetPosition);
			Hash128 val = ClassHasher<MechanicEntity>.GetHash128(Pusher);
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Chunk source = new Chunk();
			result = Unsafe.As<Chunk, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<Chunk>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.UnmanagedField(0, "MaxTime", ref MaxTime, state);
			formatter.UnmanagedField(1, "PassedTime", ref PassedTime, state);
			formatter.UnmanagedField(2, "InClipTime", ref InClipTime, state);
			formatter.UnmanagedField(3, "Speed", ref Speed, state);
			formatter.UnmanagedField(4, "ProvokeAttackOfOpportunity", ref ProvokeAttackOfOpportunity, state);
			formatter.UnmanagedField(5, "IgnoreNavmesh", ref IgnoreNavmesh, state);
			formatter.UnmanagedField(6, "PrepareForJump", ref PrepareForJump, state);
			formatter.Field(7, "TargetPosition", ref TargetPosition, state);
			formatter.Field(8, "Pusher", ref Pusher, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Chunk>();
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
					MaxTime = formatter.ReadUnmanaged<float>(state);
					break;
				case 1:
					PassedTime = formatter.ReadUnmanaged<float>(state);
					break;
				case 2:
					InClipTime = formatter.ReadUnmanaged<float>(state);
					break;
				case 3:
					Speed = formatter.ReadUnmanaged<float>(state);
					break;
				case 4:
					ProvokeAttackOfOpportunity = formatter.ReadUnmanaged<bool>(state);
					break;
				case 5:
					IgnoreNavmesh = formatter.ReadUnmanaged<bool>(state);
					break;
				case 6:
					PrepareForJump = formatter.ReadUnmanaged<bool>(state);
					break;
				case 7:
					TargetPosition = formatter.ReadPackable<Vector3>(state);
					break;
				case 8:
					Pusher = formatter.ReadPackable<MechanicEntity>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartJump",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Chunks", typeof(Queue<Chunk>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private Queue<Chunk> m_Chunks { get; set; } = new Queue<Chunk>();


	public Chunk Active
	{
		get
		{
			Chunk result;
			while (m_Chunks.TryPeek(out result))
			{
				if (!result.IsFinished)
				{
					return result;
				}
				m_Chunks.Dequeue();
			}
			return null;
		}
	}

	public Chunk Jump(GraphNode targetNode, bool provokeAttackOfOpportunity, int cellsRemaining = 0, MechanicEntity pusher = null, bool useAttack = false)
	{
		if ((bool)base.Owner.Features.DisablePush || base.Owner.View.AnimationManager == null)
		{
			return null;
		}
		Vector3 vector = targetNode.Vector3Position() - base.Owner.Position;
		float num = vector.magnitude / 5f;
		if (num == 0f)
		{
			PFLog.Default.Error("Push time is zero");
			return null;
		}
		base.Owner.View.MovementAgent.Blocker.Unblock();
		base.Owner.View.MovementAgent.Blocker.Block(targetNode);
		Chunk chunk = new Chunk
		{
			MaxTime = num,
			ProvokeAttackOfOpportunity = provokeAttackOfOpportunity,
			TargetPosition = targetNode.Vector3Position(),
			Pusher = pusher,
			PrepareForJump = true,
			Speed = 5f
		};
		UnitAnimationActionHandle unitAnimationActionHandle = base.Owner?.View?.EntityData?.MaybeAnimationManager?.CreateHandle(UnitAnimationType.Jump, errorOnEmpty: false);
		if (base.Owner?.View?.AnimationManager?.GetAction(UnitAnimationType.Jump) is UnitAnimationActionJump unitAnimationActionJump)
		{
			chunk.InClipTime = unitAnimationActionJump.GetInClipLenght();
			chunk.MaxTime = unitAnimationActionJump.GetFlyClipLenght() + unitAnimationActionJump.GetInClipLenght();
			chunk.Speed = vector.magnitude / unitAnimationActionJump.GetFlyClipLenght();
		}
		if (unitAnimationActionHandle != null)
		{
			unitAnimationActionHandle.NeedAttackAfterJump = useAttack;
			base.Owner.View.EntityData.MaybeAnimationManager.Execute(unitAnimationActionHandle);
		}
		m_Chunks.Enqueue(chunk);
		return chunk;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Queue<Chunk> chunks = m_Chunks;
		if (chunks != null)
		{
			foreach (Chunk item in chunks)
			{
				Hash128 val2 = ClassHasher<Chunk>.GetHash128(item);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartJump source = new UnitPartJump();
		result = Unsafe.As<UnitPartJump, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartJump>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Queue<Chunk> value = m_Chunks;
		formatter.Field(0, "m_Chunks", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartJump>();
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
				m_Chunks = formatter.ReadPackable<Queue<Chunk>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
