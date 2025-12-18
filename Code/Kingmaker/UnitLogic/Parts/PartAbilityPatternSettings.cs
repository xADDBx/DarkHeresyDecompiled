using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartAbilityPatternSettings : UnitPart, IHashable, IOwlPackable<PartAbilityPatternSettings>
{
	private readonly List<(EntityFactComponent Runtime, OverrideAbilityPatternSettings Component)> m_PatternEntries = new List<(EntityFactComponent, OverrideAbilityPatternSettings)>();

	private readonly List<(EntityFactComponent Runtime, OverrideAbilityPatternSize Component)> m_OverrideRadius = new List<(EntityFactComponent, OverrideAbilityPatternSize)>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilityPatternSettings",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public static IAbilityAoEPatternProvider GetAbilityPatternSettings(AbilityData ability, [CanBeNull] IAbilityAoEPatternProvider currentPattern = null)
	{
		IAbilityAoEPatternProvider abilityAoEPatternProvider = null;
		PartAbilityPatternSettings abilityPatternSettingsOptional = ability.Caster.GetAbilityPatternSettingsOptional();
		if (abilityPatternSettingsOptional != null)
		{
			foreach (var patternEntry in abilityPatternSettingsOptional.m_PatternEntries)
			{
				using (patternEntry.Runtime.SetScope())
				{
					AbilityAoEPatternSettings patternSettings = patternEntry.Component.GetPatternSettings(ability);
					if (patternSettings != null)
					{
						abilityAoEPatternProvider = patternSettings;
						break;
					}
				}
			}
		}
		if (abilityAoEPatternProvider == null)
		{
			abilityAoEPatternProvider = ability.Blueprint.PatternSettings ?? currentPattern;
		}
		abilityAoEPatternProvider?.OverrideHaloSize(null);
		if (abilityPatternSettingsOptional != null)
		{
			foreach (var item in abilityPatternSettingsOptional.m_OverrideRadius)
			{
				using (item.Runtime.SetScope())
				{
					item.Component.OverrideSize(ability, abilityAoEPatternProvider);
				}
			}
		}
		return abilityAoEPatternProvider;
	}

	public void Add(OverrideAbilityPatternSettings component)
	{
		m_PatternEntries.Add((component.Runtime, component));
	}

	public void Remove(OverrideAbilityPatternSettings component)
	{
		m_PatternEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	public void Add(OverrideAbilityPatternSize component)
	{
		m_OverrideRadius.Add((component.Runtime, component));
	}

	public void Remove(OverrideAbilityPatternSize component)
	{
		m_OverrideRadius.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_PatternEntries.Empty() && m_OverrideRadius.Empty())
		{
			RemoveSelf();
		}
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
		PartAbilityPatternSettings source = new PartAbilityPatternSettings();
		result = Unsafe.As<PartAbilityPatternSettings, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityPatternSettings>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityPatternSettings>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
