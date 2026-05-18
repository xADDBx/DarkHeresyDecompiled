using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public sealed class Ability : MechanicEntityFact, IHashable, IOwlPackable<Ability>
{
	[JsonProperty]
	[OwlPackInclude]
	private AbilityData m_Data;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private bool m_HiddenByCommand;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private int m_IndexInItemSettings;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private BlueprintAbilityModifier[] m_Modifiers;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private List<EntityFactRef> m_HiddenSources = new List<EntityFactRef>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Ability",
		OldNames = null,
		Fields = new FieldInfo[15]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("m_ParentContext", typeof(MechanicsContext)),
			new FieldInfo("m_Context", typeof(MechanicsContext)),
			new FieldInfo("m_Data", typeof(AbilityData)),
			new FieldInfo("m_HiddenByCommand", typeof(bool)),
			new FieldInfo("m_IndexInItemSettings", typeof(int)),
			new FieldInfo("m_Modifiers", typeof(BlueprintAbilityModifier[])),
			new FieldInfo("m_HiddenSources", typeof(List<EntityFactRef>))
		}
	};

	public new BlueprintAbility Blueprint => (BlueprintAbility)base.Blueprint;

	[NotNull]
	public AbilityData Data => m_Data ?? throw new NullReferenceException();

	[CanBeNull]
	public AbilityData MaybeData => m_Data;

	public override bool Hidden
	{
		get
		{
			if (!base.Hidden && !m_HiddenByCommand && m_HiddenSources.Count <= 0 && !Blueprint.Hidden && !(base.SourceItem is ItemEntityUsable) && !(m_Data == null))
			{
				return !m_Data.IsVisible();
			}
			return true;
		}
	}

	protected override IEnumerable<BlueprintScriptableObject> AdditionalComponentsProviders => m_Modifiers.EmptyIfNull();

	public IEnumerable<BlueprintAbilityModifier> Modifiers => m_Modifiers.EmptyIfNull();

	[CanBeNull]
	public BlueprintAbilityModifier PlayerAssignedModifier => base.Owner?.GetOptional<PartAbilityModifiers>()?.GetManuallyAddedModifier(this);

	public Ability(BlueprintAbility blueprint, int indexInItemSettings = 0)
		: base(blueprint, null)
	{
		m_IndexInItemSettings = indexInItemSettings;
	}

	[UsedImplicitly]
	private Ability(JsonConstructorMark _)
	{
	}

	protected Ability()
	{
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		m_Data = CreateData();
		UpdateModifiers();
	}

	protected override void OnDetach()
	{
		m_Data = null;
		base.OnDetach();
	}

	private AbilityData CreateData()
	{
		return new AbilityData(this, base.Owner, m_IndexInItemSettings, m_Modifiers);
	}

	public void UpdateModifiers()
	{
		BlueprintAbilityModifier[] array = GetAllModifiers().ToArray();
		if ((m_Modifiers != null || array.Length != 0) && (m_Modifiers == null || !m_Modifiers.SequenceEqual(array)))
		{
			m_Modifiers = array;
			m_Data = CreateData();
			Setup(Blueprint);
		}
	}

	protected override void OnSourceAdded(EntityFactSource source)
	{
		base.OnSourceAdded(source);
		if (source.Entity is ItemEntityWeapon)
		{
			Setup(Blueprint);
		}
	}

	public void Hide(EntityFactRef source)
	{
		m_HiddenSources.Add(source);
	}

	public void HideByCommand()
	{
		m_HiddenByCommand = true;
	}

	public void Unhide(EntityFactRef source)
	{
		m_HiddenSources.Remove(source);
	}

	private IEnumerable<BlueprintAbilityModifier> GetAllModifiers()
	{
		return Blueprint.Modifiers.EmptyIfNull().Dereference().Concat(GetWeaponModifiers())
			.Concat(GetOwnerModifiers());
	}

	public IEnumerable<BlueprintAbilityModifier> GetWeaponModifiers()
	{
		ItemEntityWeapon itemEntityWeapon = base.SourceItem as ItemEntityWeapon;
		WeaponAbility settings = itemEntityWeapon?.Blueprint.WeaponAbilities[m_IndexInItemSettings];
		if (settings == null || settings.ModifiersOverrideType != ModifiersOverrideType.Override)
		{
			foreach (BlueprintAbilityModifier item in (itemEntityWeapon?.Blueprint.AbilityModifiers.Dereference()).EmptyIfNull())
			{
				yield return item;
			}
		}
		foreach (BlueprintAbilityModifier item2 in (settings?.Modifiers?.Dereference()).EmptyIfNull())
		{
			yield return item2;
		}
	}

	private IEnumerable<BlueprintAbilityModifier> GetOwnerModifiers()
	{
		return base.Owner?.GetOptional<PartAbilityModifiers>()?.GetAddedModifiers(this) ?? Enumerable.Empty<BlueprintAbilityModifier>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(m_Data);
		result.Append(ref val2);
		result.Append(ref m_HiddenByCommand);
		result.Append(ref m_IndexInItemSettings);
		BlueprintAbilityModifier[] modifiers = m_Modifiers;
		if (modifiers != null)
		{
			for (int i = 0; i < modifiers.Length; i++)
			{
				Hash128 val3 = SimpleBlueprintHasher.GetHash128(modifiers[i]);
				result.Append(ref val3);
			}
		}
		List<EntityFactRef> hiddenSources = m_HiddenSources;
		if (hiddenSources != null)
		{
			for (int j = 0; j < hiddenSources.Count; j++)
			{
				EntityFactRef obj = hiddenSources[j];
				Hash128 val4 = StructHasher<EntityFactRef>.GetHash128(ref obj);
				result.Append(ref val4);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Ability source = new Ability();
		result = Unsafe.As<Ability, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Ability>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ComponentsData", ref m_ComponentsData, state);
		List<EntityFactComponent> value = base.m_Components;
		formatter.Field(1, "m_Components", ref value, state);
		formatter.Field(2, "m_Sources", ref m_Sources, state);
		formatter.Field(3, "m_ChildrenFacts", ref m_ChildrenFacts, state);
		string value2 = base.UniqueId;
		formatter.StringField(4, "UniqueId", ref value2, state);
		formatter.Field(5, "m_Blueprint", ref m_Blueprint, state);
		bool value3 = base.IsActive;
		formatter.UnmanagedField(6, "IsActive", ref value3, state);
		EntityFactRef value4 = base.ChildOf;
		formatter.Field(7, "ChildOf", ref value4, state);
		formatter.Field(8, "m_ParentContext", ref m_ParentContext, state);
		formatter.Field(9, "m_Context", ref m_Context, state);
		formatter.Field(10, "m_Data", ref m_Data, state);
		formatter.UnmanagedField(11, "m_HiddenByCommand", ref m_HiddenByCommand, state);
		formatter.UnmanagedField(12, "m_IndexInItemSettings", ref m_IndexInItemSettings, state);
		formatter.Field(13, "m_Modifiers", ref m_Modifiers, state);
		formatter.Field(14, "m_HiddenSources", ref m_HiddenSources, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Ability>();
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
				m_ComponentsData = formatter.ReadPackable<Dictionary<string, List<IEntityFactComponentSavableData>>>(state);
				break;
			case 1:
				base.m_Components = formatter.ReadPackable<List<EntityFactComponent>>(state);
				break;
			case 2:
				m_Sources = formatter.ReadPackable<List<EntityFactSource>>(state);
				break;
			case 3:
				m_ChildrenFacts = formatter.ReadPackable<List<EntityFactRef>>(state);
				break;
			case 4:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 5:
				m_Blueprint = formatter.ReadPackable<BlueprintFact>(state);
				break;
			case 6:
				base.IsActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				base.ChildOf = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 8:
				m_ParentContext = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 9:
				m_Context = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 10:
				m_Data = formatter.ReadPackable<AbilityData>(state);
				break;
			case 11:
				m_HiddenByCommand = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				m_IndexInItemSettings = formatter.ReadUnmanaged<int>(state);
				break;
			case 13:
				m_Modifiers = formatter.ReadPackable<BlueprintAbilityModifier[]>(state);
				break;
			case 14:
				m_HiddenSources = formatter.ReadPackable<List<EntityFactRef>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
