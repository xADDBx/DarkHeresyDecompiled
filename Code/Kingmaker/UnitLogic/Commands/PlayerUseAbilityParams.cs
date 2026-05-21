using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Commands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public sealed class PlayerUseAbilityParams : UnitUseAbilityParams, IMemoryPackable<PlayerUseAbilityParams>, IMemoryPackFormatterRegister, IOwlPackable<PlayerUseAbilityParams>
{
	[Preserve]
	private sealed class PlayerUseAbilityParamsFormatter : MemoryPackFormatter<PlayerUseAbilityParams>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PlayerUseAbilityParams value)
		{
			PlayerUseAbilityParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref PlayerUseAbilityParams value)
		{
			PlayerUseAbilityParams.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PlayerUseAbilityParams value)
		{
			PlayerUseAbilityParams.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref PlayerUseAbilityParams value)
		{
			PlayerUseAbilityParams.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_AbilityId;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private string m_VariantId;

	public new static readonly TypeInfo OwlPackTypeInfo;

	protected override bool DefaultIsOneFrameCommand
	{
		get
		{
			bool flag = base.Ability?.AbilityGroups != null && base.Ability.AbilityGroups.Any((BlueprintAbilityGroup g) => g?.IsWeaponAttackGroup ?? false);
			if (Game.Instance.Player.UISettings.FastPartyCast && Game.Instance.Player.IsInCombat)
			{
				return !flag;
			}
			return false;
		}
	}

	[MemoryPackConstructor]
	private PlayerUseAbilityParams()
	{
	}

	[JsonConstructor]
	public PlayerUseAbilityParams(JsonConstructorMark _)
		: base(_)
	{
	}

	public PlayerUseAbilityParams([NotNull] AbilityData ability, [NotNull] TargetWrapper target)
		: base(ability, target)
	{
		if (ability.ConvertedFrom != null)
		{
			m_AbilityId = ability.ConvertedFrom.UniqueId;
			m_VariantId = ability.UniqueId;
		}
		else
		{
			m_AbilityId = ability.UniqueId;
			m_VariantId = null;
		}
	}

	public void Prepare()
	{
		AbilityData abilityData = LinksHelper.GetPartyAbility(m_AbilityId);
		if (abilityData == null)
		{
			throw new Exception("Can't find ability by id " + m_AbilityId);
		}
		if (m_VariantId != null)
		{
			abilityData = abilityData.GetConversions().FirstOrDefault((AbilityData a) => a.UniqueId.Equals(m_VariantId, StringComparison.Ordinal));
			if (abilityData == null)
			{
				throw new Exception("Can't find ability variants by AbilityId=" + m_AbilityId + " VariantId=" + m_VariantId);
			}
		}
		base.Ability = abilityData;
		Metrics.Ability.Id(abilityData.Blueprint.AssetGuid).Caster(abilityData.Caster?.Blueprint.AssetGuid).Modifier(abilityData.Fact?.PlayerAssignedModifier?.AssetGuid)
			.Toggles((from t in (abilityData.Caster as BaseUnitEntity)?.ToggleAbilities.Visible
				where t.Enabled
				select t.Blueprint.AssetGuid))
			.Send();
	}

	protected override AbstractUnitCommand CreateCommandInternal()
	{
		return new PlayerUseAbility(this);
	}

	static PlayerUseAbilityParams()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "PlayerUseAbilityParams",
			OldNames = null,
			Fields = new FieldInfo[22]
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
				new FieldInfo("m_IgnoreCooldown", typeof(bool?)),
				new FieldInfo("DisableLog", typeof(bool)),
				new FieldInfo("HitPolicy", typeof(AttackHitPolicyType)),
				new FieldInfo("DamagePolicy", typeof(DamagePolicyType)),
				new FieldInfo("KillTarget", typeof(bool)),
				new FieldInfo("m_AbilityId", typeof(string)),
				new FieldInfo("m_VariantId", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerUseAbilityParams>())
		{
			MemoryPackFormatterProvider.Register(new PlayerUseAbilityParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerUseAbilityParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PlayerUseAbilityParams>());
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
		if (!MemoryPackFormatterProvider.IsRegistered<AttackHitPolicyType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<AttackHitPolicyType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DamagePolicyType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<DamagePolicyType>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref PlayerUseAbilityParams? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(22, in value.Type);
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
		ref bool? ignoreCooldown = ref value.m_IgnoreCooldown;
		value3 = value.DisableLog;
		AttackHitPolicyType value7 = value.HitPolicy;
		DamagePolicyType value8 = value.DamagePolicy;
		value4 = value.KillTarget;
		writer.DangerousWriteUnmanaged(in movementType, in isOneFrameCommand, in slowMotionRequired, in ignoreCooldown, in value3, in value7, in value8, in value4);
		writer.WriteString(value.m_AbilityId);
		writer.WriteString(value.m_VariantId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PlayerUseAbilityParams? value)
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
		bool? value17;
		bool value18;
		AttackHitPolicyType value19;
		DamagePolicyType value20;
		bool value21;
		string abilityId;
		string variantId;
		if (memberCount == 22)
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
				value17 = value.m_IgnoreCooldown;
				value18 = value.DisableLog;
				value19 = value.HitPolicy;
				value20 = value.DamagePolicy;
				value21 = value.KillTarget;
				abilityId = value.m_AbilityId;
				variantId = value.m_VariantId;
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
				reader.DangerousReadUnmanaged<bool?>(out value17);
				reader.ReadUnmanaged<bool>(out value18);
				reader.ReadUnmanaged<AttackHitPolicyType>(out value19);
				reader.ReadUnmanaged<DamagePolicyType>(out value20);
				reader.ReadUnmanaged<bool>(out value21);
				abilityId = reader.ReadString();
				variantId = reader.ReadString();
				goto IL_0490;
			}
			reader.ReadUnmanaged<CommandType>(out value2);
			value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			reader.ReadUnmanaged<CommandPreprocessingFlags>(out value4);
			value5 = reader.ReadPackable<TargetWrapper>();
			reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value6, out value7, out value8, out value9, out value10, out value11, out value12);
			value13 = reader.ReadPackable<ForcedPath>();
			reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?, bool?, bool, AttackHitPolicyType, DamagePolicyType, bool>(out value14, out value15, out value16, out value17, out value18, out value19, out value20, out value21);
			abilityId = reader.ReadString();
			variantId = reader.ReadString();
		}
		else
		{
			if (memberCount > 22)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PlayerUseAbilityParams), 22, memberCount);
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
				value17 = null;
				value18 = false;
				value19 = AttackHitPolicyType.Default;
				value20 = DamagePolicyType.Default;
				value21 = false;
				abilityId = null;
				variantId = null;
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
				value17 = value.m_IgnoreCooldown;
				value18 = value.DisableLog;
				value19 = value.HitPolicy;
				value20 = value.DamagePolicy;
				value21 = value.KillTarget;
				abilityId = value.m_AbilityId;
				variantId = value.m_VariantId;
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
																			reader.DangerousReadUnmanaged<bool?>(out value17);
																			if (memberCount != 16)
																			{
																				reader.ReadUnmanaged<bool>(out value18);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanaged<AttackHitPolicyType>(out value19);
																					if (memberCount != 18)
																					{
																						reader.ReadUnmanaged<DamagePolicyType>(out value20);
																						if (memberCount != 19)
																						{
																							reader.ReadUnmanaged<bool>(out value21);
																							if (memberCount != 20)
																							{
																								abilityId = reader.ReadString();
																								if (memberCount != 21)
																								{
																									variantId = reader.ReadString();
																									_ = 22;
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
					}
				}
			}
			if (value != null)
			{
				goto IL_0490;
			}
		}
		value = new PlayerUseAbilityParams
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
			m_IgnoreCooldown = value17,
			DisableLog = value18,
			HitPolicy = value19,
			DamagePolicy = value20,
			KillTarget = value21,
			m_AbilityId = abilityId,
			m_VariantId = variantId
		};
		return;
		IL_0490:
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
		value.m_IgnoreCooldown = value17;
		value.DisableLog = value18;
		value.HitPolicy = value19;
		value.DamagePolicy = value20;
		value.KillTarget = value21;
		value.m_AbilityId = abilityId;
		value.m_VariantId = variantId;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref PlayerUseAbilityParams? value)
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
		writer.WriteProperty("m_IgnoreCooldown");
		writer.DangerousWriteUnmanaged(value.m_IgnoreCooldown);
		writer.WriteProperty("DisableLog");
		writer.WriteUnmanaged(value.DisableLog);
		writer.WriteProperty("HitPolicy");
		writer.WriteUnmanaged(value.HitPolicy);
		writer.WriteProperty("DamagePolicy");
		writer.WriteUnmanaged(value.DamagePolicy);
		writer.WriteProperty("KillTarget");
		writer.WriteUnmanaged(value.KillTarget);
		writer.WriteProperty("m_AbilityId");
		writer.WriteString(value.m_AbilityId);
		writer.WriteProperty("m_VariantId");
		writer.WriteString(value.m_VariantId);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref PlayerUseAbilityParams? value)
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
		bool? v13;
		bool v14;
		AttackHitPolicyType v15;
		DamagePolicyType v16;
		bool v17;
		string abilityId;
		string variantId;
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
			v13 = null;
			v14 = false;
			v15 = AttackHitPolicyType.Default;
			v16 = DamagePolicyType.Default;
			v17 = false;
			abilityId = null;
			variantId = null;
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
			v13 = value.m_IgnoreCooldown;
			v14 = value.DisableLog;
			v15 = value.HitPolicy;
			v16 = value.DamagePolicy;
			v17 = value.KillTarget;
			abilityId = value.m_AbilityId;
			variantId = value.m_VariantId;
		}
		bool[] array = new bool[22];
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
				case "m_IgnoreCooldown":
					reader.DangerousReadUnmanaged<bool?>(out v13);
					array[15] = true;
					break;
				case "DisableLog":
					reader.ReadUnmanaged<bool>(out v14);
					array[16] = true;
					break;
				case "HitPolicy":
					reader.ReadUnmanaged<AttackHitPolicyType>(out v15);
					array[17] = true;
					break;
				case "DamagePolicy":
					reader.ReadUnmanaged<DamagePolicyType>(out v16);
					array[18] = true;
					break;
				case "KillTarget":
					reader.ReadUnmanaged<bool>(out v17);
					array[19] = true;
					break;
				case "m_AbilityId":
					abilityId = reader.ReadString();
					array[20] = true;
					break;
				case "m_VariantId":
					variantId = reader.ReadString();
					array[21] = true;
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
				case "m_IgnoreCooldown":
					reader.DangerousReadUnmanaged<bool?>(out v13);
					break;
				case "DisableLog":
					reader.ReadUnmanaged<bool>(out v14);
					break;
				case "HitPolicy":
					reader.ReadUnmanaged<AttackHitPolicyType>(out v15);
					break;
				case "DamagePolicy":
					reader.ReadUnmanaged<DamagePolicyType>(out v16);
					break;
				case "KillTarget":
					reader.ReadUnmanaged<bool>(out v17);
					break;
				case "m_AbilityId":
					abilityId = reader.ReadString();
					break;
				case "m_VariantId":
					variantId = reader.ReadString();
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
			value.m_IgnoreCooldown = v13;
			value.DisableLog = v14;
			value.HitPolicy = v15;
			value.DamagePolicy = v16;
			value.KillTarget = v17;
			value.m_AbilityId = abilityId;
			value.m_VariantId = variantId;
		}
		else
		{
			value = new PlayerUseAbilityParams
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
				m_IgnoreCooldown = v13,
				DisableLog = v14,
				HitPolicy = v15,
				DamagePolicy = v16,
				KillTarget = v17,
				m_AbilityId = abilityId,
				m_VariantId = variantId
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PlayerUseAbilityParams source = new PlayerUseAbilityParams();
		result = Unsafe.As<PlayerUseAbilityParams, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PlayerUseAbilityParams>(OwlPackTypeInfo);
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
		formatter.UnmanagedNullableField(15, "m_IgnoreCooldown", ref m_IgnoreCooldown, state);
		bool value6 = base.DisableLog;
		formatter.UnmanagedField(16, "DisableLog", ref value6, state);
		AttackHitPolicyType value7 = base.HitPolicy;
		formatter.EnumField(17, "HitPolicy", ref value7, state);
		DamagePolicyType value8 = base.DamagePolicy;
		formatter.EnumField(18, "DamagePolicy", ref value8, state);
		bool value9 = base.KillTarget;
		formatter.UnmanagedField(19, "KillTarget", ref value9, state);
		formatter.StringField(20, "m_AbilityId", ref m_AbilityId, state);
		formatter.StringField(21, "m_VariantId", ref m_VariantId, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PlayerUseAbilityParams>();
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
				m_IgnoreCooldown = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 16:
				base.DisableLog = formatter.ReadUnmanaged<bool>(state);
				break;
			case 17:
				base.HitPolicy = formatter.ReadEnum<AttackHitPolicyType>(state);
				break;
			case 18:
				base.DamagePolicy = formatter.ReadEnum<DamagePolicyType>(state);
				break;
			case 19:
				base.KillTarget = formatter.ReadUnmanaged<bool>(state);
				break;
			case 20:
				m_AbilityId = formatter.ReadString(state);
				break;
			case 21:
				m_VariantId = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
