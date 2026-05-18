using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartMovable : EntityPart, IHashable, IOwlPackable<PartMovable>
{
	public interface IOwner : IEntityPartOwner<PartMovable>, IEntityPartOwner
	{
		PartMovable Movable { get; }
	}

	public struct PreviousSimulationTickInfo
	{
		public bool HasRotation;

		public bool HasMotion;
	}

	private readonly PerFrameVar<float> m_MemoizedSpeed = new PerFrameVar<float>();

	public bool ForceHasMotion;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartMovable",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("PreviousPosition", typeof(Vector3)),
			new FieldInfo("LastMoveTime", typeof(TimeSpan))
		}
	};

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Vector3 PreviousPosition { get; set; }

	public float PreviousOrientation { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan LastMoveTime { get; set; }

	public bool HasMotionThisSimulationTick => (base.Owner.Position - PreviousPosition).sqrMagnitude > 1E-05f;

	public PreviousSimulationTickInfo PreviousSimulationTick { get; set; }

	public float CurrentSpeedMps
	{
		get
		{
			if (!m_MemoizedSpeed.UpToDate)
			{
				m_MemoizedSpeed.Value = CalculateCurrentSpeed();
			}
			return m_MemoizedSpeed.Value;
		}
	}

	public float DefaultSpeedMps => 3.65f;

	public float SlowMoSpeedMod { get; set; } = 1f;


	protected override void OnAttach()
	{
		Initialize();
	}

	protected override void OnPrePostLoad()
	{
		Initialize();
	}

	private void Initialize()
	{
	}

	private float CalculateCurrentSpeed()
	{
		if (Game.Instance.Player.ForcedWalk)
		{
			return ConfigRoot.Instance.DetectiveServoskull.ForcedWalkSpeed * SlowMoSpeedMod;
		}
		AbstractUnitCommand abstractUnitCommand = base.ConcreteOwner.GetOptional<PartUnitCommands>()?.Current;
		if (abstractUnitCommand != null && abstractUnitCommand.OverrideSpeed.HasValue)
		{
			return abstractUnitCommand.OverrideSpeed.Value * SlowMoSpeedMod;
		}
		float num = 0f;
		if (abstractUnitCommand != null)
		{
			UnitAnimationManager animationManager = abstractUnitCommand.Executor.AnimationManager;
			if (animationManager != null && animationManager.Speed >= 0f)
			{
				num = animationManager.Speed * SlowMoSpeedMod;
			}
			else if (animationManager != null)
			{
				num = DefaultSpeedMps;
				switch (abstractUnitCommand.MovementType)
				{
				case WalkSpeedType.Sprint:
					num *= 0.5f;
					break;
				case WalkSpeedType.Run:
					num *= 1.2f;
					break;
				case WalkSpeedType.Crouch:
					num *= 0.5f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case WalkSpeedType.Walk:
					break;
				}
			}
			else
			{
				PFLog.Default.ErrorWithReport("Unit has no animation manager");
			}
		}
		else
		{
			num = DefaultSpeedMps;
		}
		return Math.Max(0.01f, num);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Vector3 val2 = PreviousPosition;
		result.Append(ref val2);
		TimeSpan val3 = LastMoveTime;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartMovable source = new PartMovable();
		result = Unsafe.As<PartMovable, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartMovable>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Vector3 value = PreviousPosition;
		formatter.Field(0, "PreviousPosition", ref value, state);
		TimeSpan value2 = LastMoveTime;
		formatter.Field(1, "LastMoveTime", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartMovable>();
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
				PreviousPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 1:
				LastMoveTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
