using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[Obsolete]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class UnitActivateAbilityParams : UnitCommandParams<UnitActivateAbility>, IMemoryPackable<UnitActivateAbilityParams>, IMemoryPackFormatterRegister, IOwlPackable<UnitActivateAbilityParams>
{
	[Preserve]
	private sealed class UnitActivateAbilityParamsFormatter : MemoryPackFormatter<UnitActivateAbilityParams>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnitActivateAbilityParams value)
		{
			UnitActivateAbilityParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref UnitActivateAbilityParams value)
		{
			UnitActivateAbilityParams.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitActivateAbilityParams value)
		{
			UnitActivateAbilityParams.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref UnitActivateAbilityParams value)
		{
			UnitActivateAbilityParams.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly EntityFactRef<ActivatableAbility> m_Ability;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackIgnore]
	public ActivatableAbility Ability => m_Ability;

	[JsonConstructor]
	private UnitActivateAbilityParams()
		: base(default(JsonConstructorMark))
	{
	}

	[MemoryPackConstructor]
	private UnitActivateAbilityParams(EntityFactRef<ActivatableAbility> m_ability)
		: base(default(JsonConstructorMark))
	{
		m_Ability = m_ability;
	}

	public UnitActivateAbilityParams(ActivatableAbility ability)
		: base((TargetWrapper)null)
	{
		m_Ability = ability;
	}

	public override bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		return (currentCommand as UnitActivateAbility)?.Ability == Ability;
	}

	static UnitActivateAbilityParams()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "UnitActivateAbilityParams",
			OldNames = null,
			Fields = new FieldInfo[16]
			{
				new FieldInfo("Type", typeof(CommandType)),
				new FieldInfo("OwnerRef", typeof(EntityRef<BaseUnitEntity>)),
				new FieldInfo("PreprocessingFlags", typeof(CommandPreprocessingFlags)),
				new FieldInfo("Target", typeof(TargetWrapper)),
				new FieldInfo("FromCutscene", typeof(bool)),
				new FieldInfo("InterruptAsSoonAsPossible", typeof(bool)),
				new FieldInfo("OverrideSpeed", typeof(float?)),
				new FieldInfo("DoNotInterruptAfterFight", typeof(bool)),
				new FieldInfo("m_FreeAction", typeof(bool?)),
				new FieldInfo("m_NeedLoS", typeof(bool?)),
				new FieldInfo("m_ApproachRadius", typeof(int?)),
				new FieldInfo("m_ForcedPath", typeof(ForcedPath)),
				new FieldInfo("m_MovementType", typeof(WalkSpeedType?)),
				new FieldInfo("m_IsOneFrameCommand", typeof(bool?)),
				new FieldInfo("m_SlowMotionRequired", typeof(bool?)),
				new FieldInfo("m_Ability", typeof(EntityFactRef<ActivatableAbility>))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitActivateAbilityParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitActivateAbilityParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitActivateAbilityParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitActivateAbilityParams>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CommandType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CommandType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CommandPreprocessingFlags>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CommandPreprocessingFlags>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<float?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<float>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<bool?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<bool>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<int?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WalkSpeedType?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<WalkSpeedType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WalkSpeedType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<WalkSpeedType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnitActivateAbilityParams? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(16, in value.Type);
		writer.WritePackable(in value.OwnerRef);
		writer.WriteUnmanaged(in value.PreprocessingFlags);
		TargetWrapper value2 = value.Target;
		writer.WritePackable(in value2);
		bool value3 = value.FromCutscene;
		bool value4 = value.InterruptAsSoonAsPossible;
		float? value5 = value.OverrideSpeed;
		bool value6 = value.DoNotInterruptAfterFight;
		writer.DangerousWriteUnmanaged(in value3, in value4, in value5, in value6, in value.m_FreeAction, in value.m_NeedLoS, in value.m_ApproachRadius);
		writer.WritePackable(in value.m_ForcedPath);
		writer.DangerousWriteUnmanaged(in value.m_MovementType, in value.m_IsOneFrameCommand, in value.m_SlowMotionRequired);
		writer.WritePackable(in value.m_Ability);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitActivateAbilityParams? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CommandType value2;
		EntityRef<BaseUnitEntity> value3;
		CommandPreprocessingFlags value4;
		TargetWrapper value5;
		bool value6;
		bool value7;
		float? value8;
		bool value9;
		bool? value10;
		bool? value11;
		int? value12;
		ForcedPath value13;
		WalkSpeedType? value14;
		bool? value15;
		bool? value16;
		EntityFactRef<ActivatableAbility> value17;
		if (memberCount == 16)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CommandType>(out value2);
				value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				reader.ReadUnmanaged<CommandPreprocessingFlags>(out value4);
				value5 = reader.ReadPackable<TargetWrapper>();
				reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value6, out value7, out value8, out value9, out value10, out value11, out value12);
				value13 = reader.ReadPackable<ForcedPath>();
				reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?>(out value14, out value15, out value16);
				value17 = reader.ReadPackable<EntityFactRef<ActivatableAbility>>();
			}
			else
			{
				value2 = value.Type;
				value3 = value.OwnerRef;
				value4 = value.PreprocessingFlags;
				value5 = value.Target;
				value6 = value.FromCutscene;
				value7 = value.InterruptAsSoonAsPossible;
				value8 = value.OverrideSpeed;
				value9 = value.DoNotInterruptAfterFight;
				value10 = value.m_FreeAction;
				value11 = value.m_NeedLoS;
				value12 = value.m_ApproachRadius;
				value13 = value.m_ForcedPath;
				value14 = value.m_MovementType;
				value15 = value.m_IsOneFrameCommand;
				value16 = value.m_SlowMotionRequired;
				value17 = value.m_Ability;
				reader.ReadUnmanaged<CommandType>(out value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<CommandPreprocessingFlags>(out value4);
				reader.ReadPackable(ref value5);
				reader.ReadUnmanaged<bool>(out value6);
				reader.ReadUnmanaged<bool>(out value7);
				reader.DangerousReadUnmanaged<float?>(out value8);
				reader.ReadUnmanaged<bool>(out value9);
				reader.DangerousReadUnmanaged<bool?>(out value10);
				reader.DangerousReadUnmanaged<bool?>(out value11);
				reader.DangerousReadUnmanaged<int?>(out value12);
				reader.ReadPackable(ref value13);
				reader.DangerousReadUnmanaged<WalkSpeedType?>(out value14);
				reader.DangerousReadUnmanaged<bool?>(out value15);
				reader.DangerousReadUnmanaged<bool?>(out value16);
				reader.ReadPackable(ref value17);
			}
		}
		else
		{
			if (memberCount > 16)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitActivateAbilityParams), 16, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = CommandType.None;
				value3 = default(EntityRef<BaseUnitEntity>);
				value4 = CommandPreprocessingFlags.None;
				value5 = null;
				value6 = false;
				value7 = false;
				value8 = null;
				value9 = false;
				value10 = null;
				value11 = null;
				value12 = null;
				value13 = null;
				value14 = null;
				value15 = null;
				value16 = null;
				value17 = default(EntityFactRef<ActivatableAbility>);
			}
			else
			{
				value2 = value.Type;
				value3 = value.OwnerRef;
				value4 = value.PreprocessingFlags;
				value5 = value.Target;
				value6 = value.FromCutscene;
				value7 = value.InterruptAsSoonAsPossible;
				value8 = value.OverrideSpeed;
				value9 = value.DoNotInterruptAfterFight;
				value10 = value.m_FreeAction;
				value11 = value.m_NeedLoS;
				value12 = value.m_ApproachRadius;
				value13 = value.m_ForcedPath;
				value14 = value.m_MovementType;
				value15 = value.m_IsOneFrameCommand;
				value16 = value.m_SlowMotionRequired;
				value17 = value.m_Ability;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CommandType>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<CommandPreprocessingFlags>(out value4);
						if (memberCount != 3)
						{
							reader.ReadPackable(ref value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<bool>(out value7);
									if (memberCount != 6)
									{
										reader.DangerousReadUnmanaged<float?>(out value8);
										if (memberCount != 7)
										{
											reader.ReadUnmanaged<bool>(out value9);
											if (memberCount != 8)
											{
												reader.DangerousReadUnmanaged<bool?>(out value10);
												if (memberCount != 9)
												{
													reader.DangerousReadUnmanaged<bool?>(out value11);
													if (memberCount != 10)
													{
														reader.DangerousReadUnmanaged<int?>(out value12);
														if (memberCount != 11)
														{
															reader.ReadPackable(ref value13);
															if (memberCount != 12)
															{
																reader.DangerousReadUnmanaged<WalkSpeedType?>(out value14);
																if (memberCount != 13)
																{
																	reader.DangerousReadUnmanaged<bool?>(out value15);
																	if (memberCount != 14)
																	{
																		reader.DangerousReadUnmanaged<bool?>(out value16);
																		if (memberCount != 15)
																		{
																			reader.ReadPackable(ref value17);
																			_ = 16;
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			_ = value;
		}
		value = new UnitActivateAbilityParams(value17)
		{
			Type = value2,
			OwnerRef = value3,
			PreprocessingFlags = value4,
			Target = value5,
			FromCutscene = value6,
			InterruptAsSoonAsPossible = value7,
			OverrideSpeed = value8,
			DoNotInterruptAfterFight = value9,
			m_FreeAction = value10,
			m_NeedLoS = value11,
			m_ApproachRadius = value12,
			m_ForcedPath = value13,
			m_MovementType = value14,
			m_IsOneFrameCommand = value15,
			m_SlowMotionRequired = value16
		};
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref UnitActivateAbilityParams? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("Type");
		writer.WriteUnmanaged(value.Type);
		writer.WriteProperty("OwnerRef");
		writer.WritePackable(value.OwnerRef);
		writer.WriteProperty("PreprocessingFlags");
		writer.WriteUnmanaged(value.PreprocessingFlags);
		writer.WriteProperty("Target");
		writer.WritePackable(value.Target);
		writer.WriteProperty("FromCutscene");
		writer.WriteUnmanaged(value.FromCutscene);
		writer.WriteProperty("InterruptAsSoonAsPossible");
		writer.WriteUnmanaged(value.InterruptAsSoonAsPossible);
		writer.WriteProperty("OverrideSpeed");
		writer.DangerousWriteUnmanaged(value.OverrideSpeed);
		writer.WriteProperty("DoNotInterruptAfterFight");
		writer.WriteUnmanaged(value.DoNotInterruptAfterFight);
		writer.WriteProperty("m_FreeAction");
		writer.DangerousWriteUnmanaged(value.m_FreeAction);
		writer.WriteProperty("m_NeedLoS");
		writer.DangerousWriteUnmanaged(value.m_NeedLoS);
		writer.WriteProperty("m_ApproachRadius");
		writer.DangerousWriteUnmanaged(value.m_ApproachRadius);
		writer.WriteProperty("m_ForcedPath");
		writer.WritePackable(value.m_ForcedPath);
		writer.WriteProperty("m_MovementType");
		writer.DangerousWriteUnmanaged(value.m_MovementType);
		writer.WriteProperty("m_IsOneFrameCommand");
		writer.DangerousWriteUnmanaged(value.m_IsOneFrameCommand);
		writer.WriteProperty("m_SlowMotionRequired");
		writer.DangerousWriteUnmanaged(value.m_SlowMotionRequired);
		writer.WriteProperty("m_Ability");
		writer.WritePackable(value.m_Ability);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref UnitActivateAbilityParams? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		CommandType v;
		EntityRef<BaseUnitEntity> val;
		CommandPreprocessingFlags v2;
		TargetWrapper val2;
		bool v3;
		bool v4;
		float? v5;
		bool v6;
		bool? v7;
		bool? v8;
		int? v9;
		ForcedPath val3;
		WalkSpeedType? v10;
		bool? v11;
		bool? v12;
		EntityFactRef<ActivatableAbility> val4;
		if (value == null)
		{
			v = CommandType.None;
			val = default(EntityRef<BaseUnitEntity>);
			v2 = CommandPreprocessingFlags.None;
			val2 = null;
			v3 = false;
			v4 = false;
			v5 = null;
			v6 = false;
			v7 = null;
			v8 = null;
			v9 = null;
			val3 = null;
			v10 = null;
			v11 = null;
			v12 = null;
			val4 = default(EntityFactRef<ActivatableAbility>);
		}
		else
		{
			v = value.Type;
			val = value.OwnerRef;
			v2 = value.PreprocessingFlags;
			val2 = value.Target;
			v3 = value.FromCutscene;
			v4 = value.InterruptAsSoonAsPossible;
			v5 = value.OverrideSpeed;
			v6 = value.DoNotInterruptAfterFight;
			v7 = value.m_FreeAction;
			v8 = value.m_NeedLoS;
			v9 = value.m_ApproachRadius;
			val3 = value.m_ForcedPath;
			v10 = value.m_MovementType;
			v11 = value.m_IsOneFrameCommand;
			v12 = value.m_SlowMotionRequired;
			val4 = value.m_Ability;
		}
		bool[] array = new bool[16];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "Type":
					reader.ReadUnmanaged<CommandType>(out v);
					array[0] = true;
					break;
				case "OwnerRef":
					val = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
					array[1] = true;
					break;
				case "PreprocessingFlags":
					reader.ReadUnmanaged<CommandPreprocessingFlags>(out v2);
					array[2] = true;
					break;
				case "Target":
					val2 = reader.ReadPackable<TargetWrapper>();
					array[3] = true;
					break;
				case "FromCutscene":
					reader.ReadUnmanaged<bool>(out v3);
					array[4] = true;
					break;
				case "InterruptAsSoonAsPossible":
					reader.ReadUnmanaged<bool>(out v4);
					array[5] = true;
					break;
				case "OverrideSpeed":
					reader.DangerousReadUnmanaged<float?>(out v5);
					array[6] = true;
					break;
				case "DoNotInterruptAfterFight":
					reader.ReadUnmanaged<bool>(out v6);
					array[7] = true;
					break;
				case "m_FreeAction":
					reader.DangerousReadUnmanaged<bool?>(out v7);
					array[8] = true;
					break;
				case "m_NeedLoS":
					reader.DangerousReadUnmanaged<bool?>(out v8);
					array[9] = true;
					break;
				case "m_ApproachRadius":
					reader.DangerousReadUnmanaged<int?>(out v9);
					array[10] = true;
					break;
				case "m_ForcedPath":
					val3 = reader.ReadPackable<ForcedPath>();
					array[11] = true;
					break;
				case "m_MovementType":
					reader.DangerousReadUnmanaged<WalkSpeedType?>(out v10);
					array[12] = true;
					break;
				case "m_IsOneFrameCommand":
					reader.DangerousReadUnmanaged<bool?>(out v11);
					array[13] = true;
					break;
				case "m_SlowMotionRequired":
					reader.DangerousReadUnmanaged<bool?>(out v12);
					array[14] = true;
					break;
				case "m_Ability":
					val4 = reader.ReadPackable<EntityFactRef<ActivatableAbility>>();
					array[15] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "Type":
					reader.ReadUnmanaged<CommandType>(out v);
					break;
				case "OwnerRef":
					reader.ReadPackable(ref val);
					break;
				case "PreprocessingFlags":
					reader.ReadUnmanaged<CommandPreprocessingFlags>(out v2);
					break;
				case "Target":
					reader.ReadPackable(ref val2);
					break;
				case "FromCutscene":
					reader.ReadUnmanaged<bool>(out v3);
					break;
				case "InterruptAsSoonAsPossible":
					reader.ReadUnmanaged<bool>(out v4);
					break;
				case "OverrideSpeed":
					reader.DangerousReadUnmanaged<float?>(out v5);
					break;
				case "DoNotInterruptAfterFight":
					reader.ReadUnmanaged<bool>(out v6);
					break;
				case "m_FreeAction":
					reader.DangerousReadUnmanaged<bool?>(out v7);
					break;
				case "m_NeedLoS":
					reader.DangerousReadUnmanaged<bool?>(out v8);
					break;
				case "m_ApproachRadius":
					reader.DangerousReadUnmanaged<int?>(out v9);
					break;
				case "m_ForcedPath":
					reader.ReadPackable(ref val3);
					break;
				case "m_MovementType":
					reader.DangerousReadUnmanaged<WalkSpeedType?>(out v10);
					break;
				case "m_IsOneFrameCommand":
					reader.DangerousReadUnmanaged<bool?>(out v11);
					break;
				case "m_SlowMotionRequired":
					reader.DangerousReadUnmanaged<bool?>(out v12);
					break;
				case "m_Ability":
					reader.ReadPackable(ref val4);
					break;
				}
			}
		}
		_ = value;
		value = new UnitActivateAbilityParams(val4)
		{
			Type = v,
			OwnerRef = val,
			PreprocessingFlags = v2,
			Target = val2,
			FromCutscene = v3,
			InterruptAsSoonAsPossible = v4,
			OverrideSpeed = v5,
			DoNotInterruptAfterFight = v6,
			m_FreeAction = v7,
			m_NeedLoS = v8,
			m_ApproachRadius = v9,
			m_ForcedPath = val3,
			m_MovementType = v10,
			m_IsOneFrameCommand = v11,
			m_SlowMotionRequired = v12
		};
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitActivateAbilityParams source = new UnitActivateAbilityParams();
		result = Unsafe.As<UnitActivateAbilityParams, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitActivateAbilityParams>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "Type", ref Type, state);
		formatter.Field(1, "OwnerRef", ref OwnerRef, state);
		formatter.EnumField(2, "PreprocessingFlags", ref PreprocessingFlags, state);
		TargetWrapper value = base.Target;
		formatter.Field(3, "Target", ref value, state);
		bool value2 = base.FromCutscene;
		formatter.UnmanagedField(4, "FromCutscene", ref value2, state);
		bool value3 = base.InterruptAsSoonAsPossible;
		formatter.UnmanagedField(5, "InterruptAsSoonAsPossible", ref value3, state);
		float? value4 = base.OverrideSpeed;
		formatter.UnmanagedNullableField(6, "OverrideSpeed", ref value4, state);
		bool value5 = base.DoNotInterruptAfterFight;
		formatter.UnmanagedField(7, "DoNotInterruptAfterFight", ref value5, state);
		formatter.UnmanagedNullableField(8, "m_FreeAction", ref m_FreeAction, state);
		formatter.UnmanagedNullableField(9, "m_NeedLoS", ref m_NeedLoS, state);
		formatter.UnmanagedNullableField(10, "m_ApproachRadius", ref m_ApproachRadius, state);
		formatter.Field(11, "m_ForcedPath", ref m_ForcedPath, state);
		formatter.EnumNullableField(12, "m_MovementType", ref m_MovementType, state);
		formatter.UnmanagedNullableField(13, "m_IsOneFrameCommand", ref m_IsOneFrameCommand, state);
		formatter.UnmanagedNullableField(14, "m_SlowMotionRequired", ref m_SlowMotionRequired, state);
		EntityFactRef<ActivatableAbility> value6 = m_Ability;
		formatter.Field(15, "m_Ability", ref value6, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitActivateAbilityParams>();
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
				Type = formatter.ReadEnum<CommandType>(state);
				break;
			case 1:
				OwnerRef = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 2:
				PreprocessingFlags = formatter.ReadEnum<CommandPreprocessingFlags>(state);
				break;
			case 3:
				base.Target = formatter.ReadPackable<TargetWrapper>(state);
				break;
			case 4:
				base.FromCutscene = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				base.InterruptAsSoonAsPossible = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				base.OverrideSpeed = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 7:
				base.DoNotInterruptAfterFight = formatter.ReadUnmanaged<bool>(state);
				break;
			case 8:
				m_FreeAction = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 9:
				m_NeedLoS = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 10:
				m_ApproachRadius = formatter.ReadNullableUnmanaged<int>(state);
				break;
			case 11:
				m_ForcedPath = formatter.ReadPackable<ForcedPath>(state);
				break;
			case 12:
				m_MovementType = formatter.ReadNullableEnum<WalkSpeedType>(state);
				break;
			case 13:
				m_IsOneFrameCommand = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 14:
				m_SlowMotionRequired = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 15:
				Unsafe.AsRef(in m_Ability) = formatter.ReadPackable<EntityFactRef<ActivatableAbility>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
