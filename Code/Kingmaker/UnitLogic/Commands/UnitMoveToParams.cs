using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class UnitMoveToParams : UnitCommandParams<UnitMoveTo>, IMemoryPackable<UnitMoveToParams>, IMemoryPackFormatterRegister, IOwlPackable<UnitMoveToParams>
{
	[Preserve]
	private sealed class UnitMoveToParamsFormatter : MemoryPackFormatter<UnitMoveToParams>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnitMoveToParams value)
		{
			UnitMoveToParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref UnitMoveToParams value)
		{
			UnitMoveToParams.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitMoveToParams value)
		{
			UnitMoveToParams.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref UnitMoveToParams value)
		{
			UnitMoveToParams.DeserializeJson(ref reader, ref value);
		}
	}

	public const float DefaultApproachRadiusForAgent = 0.3f;

	[JsonProperty(PropertyName = "ara", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	[MemoryPackInclude]
	public float ApproachRadiusForAgentASP;

	public static readonly TypeInfo OwlPackTypeInfo;

	[JsonProperty(PropertyName = "ori")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public float? Orientation { get; set; }

	[JsonProperty(PropertyName = "raw")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public bool RunAway { get; set; }

	[JsonProperty(PropertyName = "roa")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public bool Roaming { get; set; }

	[MemoryPackIgnore]
	public float MaxApproachForAgentASP => Mathf.Max(Mathf.Sqrt(2.Cells().Meters), ApproachRadiusForAgentASP);

	public override int DefaultApproachRadius => 10000;

	public override bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		if (base.FromCutscene)
		{
			return false;
		}
		if (!(currentCommand is UnitMoveTo unitMoveTo))
		{
			return false;
		}
		if (currentCommand.Executor.MovementAgent.IsTraverseInProgress)
		{
			return false;
		}
		unitMoveTo.Params.ForcedPath = base.ForcedPath;
		base.ForcedPath = null;
		unitMoveTo.Params.Target = base.Target;
		unitMoveTo.Params.ApproachRadiusForAgentASP = ApproachRadiusForAgentASP;
		unitMoveTo.Params.Orientation = Orientation;
		unitMoveTo.Params.RunAway = RunAway;
		unitMoveTo.Params.Roaming = Roaming;
		unitMoveTo.Params.OverrideSpeed = base.OverrideSpeed;
		unitMoveTo.Params.MovementType = base.MovementType;
		unitMoveTo.Executor.MoveTo(unitMoveTo.Params.ForcedPath, base.Target.Point, ApproachRadiusForAgentASP);
		return true;
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	private UnitMoveToParams()
		: base(default(JsonConstructorMark))
	{
	}

	public UnitMoveToParams([NotNull] ForcedPath path, [CanBeNull] TargetWrapper target, float approachRadiusForAgent = 0.3f)
		: base(target)
	{
		base.ForcedPath = path;
		ApproachRadiusForAgentASP = approachRadiusForAgent;
	}

	static UnitMoveToParams()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "UnitMoveToParams",
			OldNames = null,
			Fields = new FieldInfo[19]
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
				new FieldInfo("ApproachRadiusForAgentASP", typeof(float)),
				new FieldInfo("Orientation", typeof(float?)),
				new FieldInfo("RunAway", typeof(bool)),
				new FieldInfo("Roaming", typeof(bool))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveToParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitMoveToParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveToParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitMoveToParams>());
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
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref UnitMoveToParams? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(19, in value.Type);
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
		ref WalkSpeedType? movementType = ref value.m_MovementType;
		ref bool? isOneFrameCommand = ref value.m_IsOneFrameCommand;
		ref bool? slowMotionRequired = ref value.m_SlowMotionRequired;
		ref float approachRadiusForAgentASP = ref value.ApproachRadiusForAgentASP;
		value5 = value.Orientation;
		value3 = value.RunAway;
		value4 = value.Roaming;
		writer.DangerousWriteUnmanaged(in movementType, in isOneFrameCommand, in slowMotionRequired, in approachRadiusForAgentASP, in value5, in value3, in value4);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitMoveToParams? value)
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
		float value17;
		float? value18;
		bool value19;
		bool value20;
		if (memberCount == 19)
		{
			if (value != null)
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
				value17 = value.ApproachRadiusForAgentASP;
				value18 = value.Orientation;
				value19 = value.RunAway;
				value20 = value.Roaming;
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
				reader.ReadUnmanaged<float>(out value17);
				reader.DangerousReadUnmanaged<float?>(out value18);
				reader.ReadUnmanaged<bool>(out value19);
				reader.ReadUnmanaged<bool>(out value20);
				goto IL_03fb;
			}
			reader.ReadUnmanaged<CommandType>(out value2);
			value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			reader.ReadUnmanaged<CommandPreprocessingFlags>(out value4);
			value5 = reader.ReadPackable<TargetWrapper>();
			reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value6, out value7, out value8, out value9, out value10, out value11, out value12);
			value13 = reader.ReadPackable<ForcedPath>();
			reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?, float, float?, bool, bool>(out value14, out value15, out value16, out value17, out value18, out value19, out value20);
		}
		else
		{
			if (memberCount > 19)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitMoveToParams), 19, memberCount);
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
				value17 = 0f;
				value18 = null;
				value19 = false;
				value20 = false;
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
				value17 = value.ApproachRadiusForAgentASP;
				value18 = value.Orientation;
				value19 = value.RunAway;
				value20 = value.Roaming;
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
																			reader.ReadUnmanaged<float>(out value17);
																			if (memberCount != 16)
																			{
																				reader.DangerousReadUnmanaged<float?>(out value18);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanaged<bool>(out value19);
																					if (memberCount != 18)
																					{
																						reader.ReadUnmanaged<bool>(out value20);
																						_ = 19;
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
					}
				}
			}
			if (value != null)
			{
				goto IL_03fb;
			}
		}
		value = new UnitMoveToParams
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
			m_SlowMotionRequired = value16,
			ApproachRadiusForAgentASP = value17,
			Orientation = value18,
			RunAway = value19,
			Roaming = value20
		};
		return;
		IL_03fb:
		value.Type = value2;
		value.OwnerRef = value3;
		value.PreprocessingFlags = value4;
		value.Target = value5;
		value.FromCutscene = value6;
		value.InterruptAsSoonAsPossible = value7;
		value.OverrideSpeed = value8;
		value.DoNotInterruptAfterFight = value9;
		value.m_FreeAction = value10;
		value.m_NeedLoS = value11;
		value.m_ApproachRadius = value12;
		value.m_ForcedPath = value13;
		value.m_MovementType = value14;
		value.m_IsOneFrameCommand = value15;
		value.m_SlowMotionRequired = value16;
		value.ApproachRadiusForAgentASP = value17;
		value.Orientation = value18;
		value.RunAway = value19;
		value.Roaming = value20;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref UnitMoveToParams? value)
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
		writer.WriteProperty("ApproachRadiusForAgentASP");
		writer.WriteUnmanaged(value.ApproachRadiusForAgentASP);
		writer.WriteProperty("Orientation");
		writer.DangerousWriteUnmanaged(value.Orientation);
		writer.WriteProperty("RunAway");
		writer.WriteUnmanaged(value.RunAway);
		writer.WriteProperty("Roaming");
		writer.WriteUnmanaged(value.Roaming);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref UnitMoveToParams? value)
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
		float v13;
		float? v14;
		bool v15;
		bool v16;
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
			v13 = 0f;
			v14 = null;
			v15 = false;
			v16 = false;
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
			v13 = value.ApproachRadiusForAgentASP;
			v14 = value.Orientation;
			v15 = value.RunAway;
			v16 = value.Roaming;
		}
		bool[] array = new bool[19];
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
				case "ApproachRadiusForAgentASP":
					reader.ReadUnmanaged<float>(out v13);
					array[15] = true;
					break;
				case "Orientation":
					reader.DangerousReadUnmanaged<float?>(out v14);
					array[16] = true;
					break;
				case "RunAway":
					reader.ReadUnmanaged<bool>(out v15);
					array[17] = true;
					break;
				case "Roaming":
					reader.ReadUnmanaged<bool>(out v16);
					array[18] = true;
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
				case "ApproachRadiusForAgentASP":
					reader.ReadUnmanaged<float>(out v13);
					break;
				case "Orientation":
					reader.DangerousReadUnmanaged<float?>(out v14);
					break;
				case "RunAway":
					reader.ReadUnmanaged<bool>(out v15);
					break;
				case "Roaming":
					reader.ReadUnmanaged<bool>(out v16);
					break;
				}
			}
		}
		if (value != null)
		{
			value.Type = v;
			value.OwnerRef = val;
			value.PreprocessingFlags = v2;
			value.Target = val2;
			value.FromCutscene = v3;
			value.InterruptAsSoonAsPossible = v4;
			value.OverrideSpeed = v5;
			value.DoNotInterruptAfterFight = v6;
			value.m_FreeAction = v7;
			value.m_NeedLoS = v8;
			value.m_ApproachRadius = v9;
			value.m_ForcedPath = val3;
			value.m_MovementType = v10;
			value.m_IsOneFrameCommand = v11;
			value.m_SlowMotionRequired = v12;
			value.ApproachRadiusForAgentASP = v13;
			value.Orientation = v14;
			value.RunAway = v15;
			value.Roaming = v16;
		}
		else
		{
			value = new UnitMoveToParams
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
				m_SlowMotionRequired = v12,
				ApproachRadiusForAgentASP = v13,
				Orientation = v14,
				RunAway = v15,
				Roaming = v16
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitMoveToParams source = new UnitMoveToParams();
		result = Unsafe.As<UnitMoveToParams, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitMoveToParams>(OwlPackTypeInfo);
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
		formatter.UnmanagedField(15, "ApproachRadiusForAgentASP", ref ApproachRadiusForAgentASP, state);
		float? value6 = Orientation;
		formatter.UnmanagedNullableField(16, "Orientation", ref value6, state);
		bool value7 = RunAway;
		formatter.UnmanagedField(17, "RunAway", ref value7, state);
		bool value8 = Roaming;
		formatter.UnmanagedField(18, "Roaming", ref value8, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitMoveToParams>();
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
				ApproachRadiusForAgentASP = formatter.ReadUnmanaged<float>(state);
				break;
			case 16:
				Orientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 17:
				RunAway = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				Roaming = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
