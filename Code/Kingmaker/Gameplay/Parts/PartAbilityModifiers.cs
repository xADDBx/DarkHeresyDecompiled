using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
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
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAbilityModifiers : MechanicEntityPart, IHashable, IOwlPackable<PartAbilityModifiers>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public struct AvailableEntry : IOwlPackable, IOwlPackable<AvailableEntry>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "AvailableEntry",
			Fields = new FieldInfo[3]
			{
				new FieldInfo("Modifier", typeof(BlueprintAbilityModifier)),
				new FieldInfo("SourceFact", typeof(EntityFactRef)),
				new FieldInfo("SourceComponent", typeof(BlueprintComponentReference))
			}
		};

		[NotNull]
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintAbilityModifier Modifier { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public EntityFactRef SourceFact { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintComponentReference SourceComponent { get; private set; }

		public AvailableEntry([NotNull] BlueprintAbilityModifier modifier, [NotNull] EntityFactComponent source)
		{
			Modifier = modifier;
			SourceFact = source.Fact;
			SourceComponent = source.SourceBlueprintComponent;
		}

		public bool IsFrom(EntityFactComponent source)
		{
			if (SourceFact == source.Fact)
			{
				return SourceComponent == source.SourceBlueprintComponent;
			}
			return false;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			AvailableEntry source = default(AvailableEntry);
			result = Unsafe.As<AvailableEntry, TPossiblyBase>(ref source);
		}

		public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<AvailableEntry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			BlueprintAbilityModifier value = Modifier;
			formatter.Field(0, "Modifier", ref value, state);
			EntityFactRef value2 = SourceFact;
			formatter.Field(1, "SourceFact", ref value2, state);
			BlueprintComponentReference value3 = SourceComponent;
			formatter.Field(2, "SourceComponent", ref value3, state);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AvailableEntry>();
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
					Modifier = formatter.ReadPackable<BlueprintAbilityModifier>(state);
					break;
				case 1:
					SourceFact = formatter.ReadPackable<EntityFactRef>(state);
					break;
				case 2:
					SourceComponent = formatter.ReadPackable<BlueprintComponentReference>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public readonly struct AdditionalModifierTagEntry
	{
		public readonly BlueprintAbilityTag SourceTag;

		public readonly BlueprintAbilityTag AdditionalTag;

		public readonly EntityFactRef SourceFact;

		public readonly BlueprintComponentReference SourceComponent;

		public AdditionalModifierTagEntry([NotNull] BlueprintAbilityTag sourceTag, [NotNull] BlueprintAbilityTag additionalTag, [NotNull] EntityFactComponent source)
			: this(sourceTag, additionalTag, source.Fact, source.SourceBlueprintComponent)
		{
		}

		public AdditionalModifierTagEntry([NotNull] BlueprintAbilityTag sourceTag, [NotNull] BlueprintAbilityTag additionalTag, [NotNull] EntityFactRef sourceFact, [NotNull] BlueprintComponentReference sourceComponent)
		{
			this = default(AdditionalModifierTagEntry);
			SourceTag = sourceTag;
			AdditionalTag = additionalTag;
			SourceFact = sourceFact;
			SourceComponent = sourceComponent;
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	[OwlPackOldName("Kingmaker.Gameplay.Parts.PartAbilityModifiers+AppliedEntry")]
	public sealed class AddedEntry : IHashable, IOwlPackable, IOwlPackable<AddedEntry>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "AddedEntry",
			OldNames = new string[1] { "Kingmaker.Gameplay.Parts.PartAbilityModifiers+AppliedEntry" },
			Fields = new FieldInfo[5]
			{
				new FieldInfo("Modifier", typeof(BlueprintAbilityModifier)),
				new FieldInfo("Ability", typeof(BlueprintAbility)),
				new FieldInfo("AbilityTag", typeof(BlueprintAbilityTag)),
				new FieldInfo("SourceFact", typeof(EntityFactRef)),
				new FieldInfo("SourceComponent", typeof(BlueprintComponentReference))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintAbilityModifier Modifier { get; private set; }

		[CanBeNull]
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintAbility Ability { get; private set; }

		[CanBeNull]
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintAbilityTag AbilityTag { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public EntityFactRef SourceFact { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintComponentReference SourceComponent { get; private set; }

		public bool IsAddedManually
		{
			get
			{
				if (SourceFact == null)
				{
					return SourceComponent == null;
				}
				return false;
			}
		}

		private AddedEntry(OwlPackConstructorParameter _)
		{
		}

		public AddedEntry([NotNull] BlueprintAbilityModifier modifier, [CanBeNull] BlueprintAbility ability, [CanBeNull] BlueprintAbilityTag abilityTag, [CanBeNull] EntityFact sourceFact, [CanBeNull] BlueprintComponent sourceComponent)
		{
			Modifier = modifier;
			Ability = ability;
			AbilityTag = abilityTag;
			SourceFact = sourceFact;
			SourceComponent = sourceComponent;
		}

		public bool IsSuitableFor([NotNull] Ability ability)
		{
			bool flag = Ability == ability.Blueprint || (AbilityTag != null && ability.Blueprint.Tags.Contains(AbilityTag)) || (Ability == null && AbilityTag == null);
			if (!flag && Ability != null)
			{
				flag = ability.Owner?.GetOptional<PartAbilityReplacements>()?.GetTargetFor(ability) == Ability;
			}
			if (!flag)
			{
				return false;
			}
			MechanicEntityFact mechanicEntityFact = SourceFact.Fact as MechanicEntityFact;
			RestrictionCalculator restrictionCalculator = (SourceComponent.Get() as IRestrictionProvider)?.GetRestriction() ?? (mechanicEntityFact as ToggleAbility)?.Blueprint.AbilityModifierRestriction;
			if (mechanicEntityFact != null && restrictionCalculator != null)
			{
				return restrictionCalculator.IsPassed(mechanicEntityFact.MaybeContext, mechanicEntityFact.Owner, null, null, ability.Data);
			}
			return true;
		}

		public bool IsFrom(EntityFactComponent source)
		{
			if (SourceFact == source.Fact)
			{
				return SourceComponent == source.SourceBlueprintComponent;
			}
			return false;
		}

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = SimpleBlueprintHasher.GetHash128(Modifier);
			result.Append(ref val);
			Hash128 val2 = SimpleBlueprintHasher.GetHash128(Ability);
			result.Append(ref val2);
			Hash128 val3 = SimpleBlueprintHasher.GetHash128(AbilityTag);
			result.Append(ref val3);
			EntityFactRef obj = SourceFact;
			Hash128 val4 = StructHasher<EntityFactRef>.GetHash128(ref obj);
			result.Append(ref val4);
			BlueprintComponentReference obj2 = SourceComponent;
			Hash128 val5 = StructHasher<BlueprintComponentReference>.GetHash128(ref obj2);
			result.Append(ref val5);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			AddedEntry source = new AddedEntry(default(OwlPackConstructorParameter));
			result = Unsafe.As<AddedEntry, TPossiblyBase>(ref source);
		}

		public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<AddedEntry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			BlueprintAbilityModifier value = Modifier;
			formatter.Field(0, "Modifier", ref value, state);
			BlueprintAbility value2 = Ability;
			formatter.Field(1, "Ability", ref value2, state);
			BlueprintAbilityTag value3 = AbilityTag;
			formatter.Field(2, "AbilityTag", ref value3, state);
			EntityFactRef value4 = SourceFact;
			formatter.Field(3, "SourceFact", ref value4, state);
			BlueprintComponentReference value5 = SourceComponent;
			formatter.Field(4, "SourceComponent", ref value5, state);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AddedEntry>();
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
					Modifier = formatter.ReadPackable<BlueprintAbilityModifier>(state);
					break;
				case 1:
					Ability = formatter.ReadPackable<BlueprintAbility>(state);
					break;
				case 2:
					AbilityTag = formatter.ReadPackable<BlueprintAbilityTag>(state);
					break;
				case 3:
					SourceFact = formatter.ReadPackable<EntityFactRef>(state);
					break;
				case 4:
					SourceComponent = formatter.ReadPackable<BlueprintComponentReference>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public sealed class ToggleAbilityEntry : IHashable, IOwlPackable, IOwlPackable<ToggleAbilityEntry>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ToggleAbilityEntry",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Modifier", typeof(BlueprintAbilityModifier)),
				new FieldInfo("Target", typeof(BlueprintToggleAbility))
			}
		};

		[NotNull]
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintAbilityModifier Modifier { get; private set; }

		[NotNull]
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintToggleAbility Target { get; private set; }

		private ToggleAbilityEntry(OwlPackConstructorParameter _)
		{
		}

		public ToggleAbilityEntry([NotNull] BlueprintAbilityModifier modifier, [NotNull] BlueprintToggleAbility target)
		{
			Modifier = modifier;
			Target = target;
		}

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = SimpleBlueprintHasher.GetHash128(Modifier);
			result.Append(ref val);
			Hash128 val2 = SimpleBlueprintHasher.GetHash128(Target);
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ToggleAbilityEntry source = new ToggleAbilityEntry(default(OwlPackConstructorParameter));
			result = Unsafe.As<ToggleAbilityEntry, TPossiblyBase>(ref source);
		}

		public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<ToggleAbilityEntry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			BlueprintAbilityModifier value = Modifier;
			formatter.Field(0, "Modifier", ref value, state);
			BlueprintToggleAbility value2 = Target;
			formatter.Field(1, "Target", ref value2, state);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ToggleAbilityEntry>();
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
					Modifier = formatter.ReadPackable<BlueprintAbilityModifier>(state);
					break;
				case 1:
					Target = formatter.ReadPackable<BlueprintToggleAbility>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[OwlPackOldName("_appliedModifiers")]
	private List<AddedEntry> _addedModifiers = new List<AddedEntry>();

	[JsonProperty]
	[OwlPackInclude]
	private List<ToggleAbilityEntry> _toggleAbilityEntries = new List<ToggleAbilityEntry>();

	private readonly List<AvailableEntry> _availableModifiers = new List<AvailableEntry>();

	private readonly List<AdditionalModifierTagEntry> _additionalModifierTags = new List<AdditionalModifierTagEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilityModifiers",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("_addedModifiers", typeof(List<AddedEntry>), new string[1] { "_appliedModifiers" }),
			new FieldInfo("_toggleAbilityEntries", typeof(List<ToggleAbilityEntry>))
		}
	};

	public IEnumerable<BlueprintAbilityModifier> KnownModifiers => _availableModifiers.Select((AvailableEntry e) => e.Modifier);

	public ReadonlyList<AddedEntry> AddedModifiers => _addedModifiers;

	public ReadonlyList<ToggleAbilityEntry> BoundToToggleAbilityModifiers => _toggleAbilityEntries;

	public IEnumerable<BlueprintAbilityModifier> GetAddedModifiers(Ability ability)
	{
		return (from e in _addedModifiers
			where e.IsSuitableFor(ability) && (!e.IsAddedManually || IsSuitableModifier(e.Modifier, ability))
			select e.Modifier).Distinct();
	}

	[CanBeNull]
	public BlueprintAbilityModifier GetManuallyAddedModifier(Ability ability)
	{
		return _addedModifiers.FirstOrDefault((AddedEntry e) => e.IsAddedManually && e.IsSuitableFor(ability) && IsSuitableModifier(e.Modifier, ability))?.Modifier;
	}

	public bool IsAddedManually(BlueprintAbility ability, BlueprintAbilityModifier modifier)
	{
		return FindEntry(ability, modifier)?.IsAddedManually ?? false;
	}

	public bool IsAddedManually(BlueprintAbilityTag tag, BlueprintAbilityModifier modifier)
	{
		return FindEntry(tag, modifier)?.IsAddedManually ?? false;
	}

	public IEnumerable<BlueprintAbilityModifier> GetBoundModifiers(BlueprintToggleAbility ability)
	{
		return from i in _toggleAbilityEntries
			where i.Target == ability
			select i.Modifier;
	}

	public bool IsSuitableModifier(BlueprintAbilityModifier modifier, Ability ability)
	{
		return MatchTags(modifier, ability.Data.Blueprint.Tags);
	}

	public bool IsSuitableModifier(BlueprintAbilityModifier modifier, ToggleAbility ability)
	{
		if (!ability.Blueprint.AbilityModifierTags.Empty())
		{
			return MatchTags(modifier, ability.Blueprint.AbilityModifierTags);
		}
		return true;
	}

	public bool IsSuitableModifier(BlueprintAbilityModifier modifier, BlueprintAbilityTag tag)
	{
		return MatchTag(modifier, tag);
	}

	private bool MatchTags(BlueprintAbilityModifier modifier, BpRefArray<BlueprintAbilityTag> tags)
	{
		if (modifier.Match(tags))
		{
			return true;
		}
		foreach (BlueprintAbilityTag tag in modifier.Tags)
		{
			foreach (AdditionalModifierTagEntry additionalModifierTag in _additionalModifierTags)
			{
				if (additionalModifierTag.SourceTag == tag && tags.Contains(additionalModifierTag.AdditionalTag))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool MatchTag(BlueprintAbilityModifier modifier, BlueprintAbilityTag tag)
	{
		if (modifier.Match(tag))
		{
			return true;
		}
		foreach (BlueprintAbilityTag tag2 in modifier.Tags)
		{
			foreach (AdditionalModifierTagEntry additionalModifierTag in _additionalModifierTags)
			{
				if (additionalModifierTag.SourceTag == tag2 && additionalModifierTag.AdditionalTag == tag)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void AddModifier([NotNull] BlueprintAbilityModifier modifier, [CanBeNull] BlueprintAbility targetAbility, [CanBeNull] BlueprintAbilityTag targetTag, [CanBeNull] EntityFact sourceFact = null, [CanBeNull] BlueprintComponent sourceComponent = null)
	{
		AddedEntry addedEntry = new AddedEntry(modifier, targetAbility, targetTag, sourceFact, sourceComponent);
		_addedModifiers.Add(addedEntry);
		UpdateModifiers(addedEntry);
	}

	public void AddModifier([NotNull] BlueprintAbilityModifier modifier, [NotNull] BlueprintAbility targetAbility, [CanBeNull] EntityFact sourceFact = null, [CanBeNull] BlueprintComponent sourceComponent = null)
	{
		AddModifier(modifier, targetAbility, null, sourceFact, sourceComponent);
	}

	public void AddModifier([NotNull] BlueprintAbilityModifier modifier, [NotNull] BlueprintAbilityTag targetTag, [CanBeNull] EntityFact sourceFact = null, [CanBeNull] BlueprintComponent sourceComponent = null)
	{
		AddModifier(modifier, null, targetTag, sourceFact, sourceComponent);
	}

	public void AddModifier<T>([NotNull] BlueprintAbilityModifier modifier, [NotNull] EntityFact sourceFact, [NotNull] T sourceComponent) where T : BlueprintComponent, IRestrictionProvider
	{
		AddModifier(modifier, null, null, sourceFact, sourceComponent);
	}

	public void AddModifier(BlueprintAbilityModifier modifier, ToggleAbility sourceFact)
	{
		AddModifier(modifier, null, null, sourceFact);
	}

	public void RemoveModifier(EntityFactComponent source)
	{
		List<AddedEntry> value;
		using (CollectionPool<List<AddedEntry>, AddedEntry>.Get(out value))
		{
			foreach (AddedEntry addedModifier in _addedModifiers)
			{
				if (addedModifier.IsFrom(source))
				{
					value.Add(addedModifier);
				}
			}
			foreach (AddedEntry item in value)
			{
				RemoveModifier(item);
			}
		}
	}

	public void RemoveModifier([NotNull] AddedEntry addedEntry)
	{
		if (_addedModifiers.Remove(addedEntry))
		{
			UpdateModifiers(addedEntry);
		}
	}

	public void RemoveModifier([NotNull] BlueprintAbility ability, [NotNull] BlueprintAbilityModifier modifier)
	{
		AddedEntry addedEntry = FindEntry(ability, modifier);
		if (addedEntry != null)
		{
			RemoveModifier(addedEntry);
		}
	}

	public void RemoveModifier([NotNull] BlueprintAbilityTag tag, [NotNull] BlueprintAbilityModifier modifier)
	{
		AddedEntry addedEntry = FindEntry(tag, modifier);
		if (addedEntry != null)
		{
			RemoveModifier(addedEntry);
		}
	}

	public void RemoveAllManuallyAddedModifiers(BlueprintAbility ability)
	{
		List<AddedEntry> value;
		using (CollectionPool<List<AddedEntry>, AddedEntry>.Get(out value))
		{
			foreach (AddedEntry addedModifier in _addedModifiers)
			{
				if (addedModifier.Ability == ability && addedModifier.IsAddedManually)
				{
					value.Add(addedModifier);
				}
			}
			foreach (AddedEntry item in value)
			{
				RemoveModifier(item);
			}
		}
	}

	public void RemoveAllBoundModifiers(BlueprintToggleAbility ability)
	{
		_toggleAbilityEntries.RemoveAll((ToggleAbilityEntry i) => i.Target == ability);
	}

	public void AddModifiers([NotNull] ToggleAbility ability)
	{
		foreach (BlueprintAbilityModifier boundModifier in GetBoundModifiers(ability.Blueprint))
		{
			AddModifier(boundModifier, ability);
		}
	}

	public void RemoveModifiers([NotNull] ToggleAbility ability)
	{
		List<AddedEntry> value;
		using (CollectionPool<List<AddedEntry>, AddedEntry>.Get(out value))
		{
			foreach (AddedEntry addedModifier in _addedModifiers)
			{
				if (addedModifier.SourceFact == ability)
				{
					value.Add(addedModifier);
				}
			}
			foreach (AddedEntry item in value)
			{
				RemoveModifier(item);
			}
		}
	}

	public void AddAvailableModifier([NotNull] BlueprintAbilityModifier modifier, [NotNull] EntityFactComponent source)
	{
		_availableModifiers.Add(new AvailableEntry(modifier, source));
	}

	public void RemoveAvailableModifier([NotNull] EntityFactComponent source)
	{
		_availableModifiers.RemoveAll((AvailableEntry e) => e.IsFrom(source));
	}

	public void BindModifier([NotNull] BlueprintAbilityModifier modifier, [NotNull] BlueprintToggleAbility target)
	{
		if (!_toggleAbilityEntries.Contains((ToggleAbilityEntry i) => i.Modifier == modifier && i.Target == target))
		{
			ToggleAbilityEntry item = new ToggleAbilityEntry(modifier, target);
			_toggleAbilityEntries.Add(item);
		}
	}

	private void AddToggleAbility(ToggleAbilityEntry entry)
	{
		ToggleAbilityEntry item = new ToggleAbilityEntry(entry.Modifier, entry.Target);
		_toggleAbilityEntries.Add(item);
	}

	public void UnbindModifier([NotNull] BlueprintAbilityModifier modifier, [NotNull] BlueprintToggleAbility target)
	{
		_toggleAbilityEntries.RemoveAll((ToggleAbilityEntry i) => i.Modifier == modifier && i.Target == target);
	}

	public void AddAdditionalModifierTag([NotNull] BlueprintAbilityTag sourceTag, [NotNull] BlueprintAbilityTag additionalTag, [NotNull] EntityFactComponent source)
	{
		_additionalModifierTags.Add(new AdditionalModifierTagEntry(sourceTag, additionalTag, source));
	}

	private void AddAdditionalModifierTag(AdditionalModifierTagEntry entry)
	{
		_additionalModifierTags.Add(new AdditionalModifierTagEntry(entry.SourceTag, entry.AdditionalTag, entry.SourceFact, entry.SourceComponent));
	}

	public void RemoveAdditionalModifierTag([NotNull] EntityFactComponent source)
	{
		_additionalModifierTags.RemoveAll((AdditionalModifierTagEntry i) => i.SourceFact == source.Fact && i.SourceComponent == source.SourceBlueprintComponent);
	}

	public void CopyModifiersTo(PartAbilityModifiers otherPart)
	{
		foreach (AdditionalModifierTagEntry additionalModifierTag in _additionalModifierTags)
		{
			otherPart.AddAdditionalModifierTag(additionalModifierTag);
		}
		foreach (ToggleAbilityEntry toggleAbilityEntry in _toggleAbilityEntries)
		{
			otherPart.AddToggleAbility(toggleAbilityEntry);
		}
	}

	[CanBeNull]
	private AddedEntry FindEntry([NotNull] BlueprintAbility ability, [NotNull] BlueprintAbilityModifier modifier)
	{
		return _addedModifiers.FirstItem((AddedEntry e) => e.Ability == ability && e.Modifier == modifier);
	}

	[CanBeNull]
	private AddedEntry FindEntry([NotNull] BlueprintAbilityTag tag, [NotNull] BlueprintAbilityModifier modifier)
	{
		return _addedModifiers.FirstItem((AddedEntry e) => e.AbilityTag == tag && e.Modifier == modifier);
	}

	private void UpdateModifiers([NotNull] AddedEntry addedEntry)
	{
		foreach (Ability item in base.Owner.Facts.GetAll<Ability>())
		{
			if (addedEntry.IsSuitableFor(item))
			{
				item.UpdateModifiers();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<AddedEntry> addedModifiers = _addedModifiers;
		if (addedModifiers != null)
		{
			for (int i = 0; i < addedModifiers.Count; i++)
			{
				Hash128 val2 = ClassHasher<AddedEntry>.GetHash128(addedModifiers[i]);
				result.Append(ref val2);
			}
		}
		List<ToggleAbilityEntry> toggleAbilityEntries = _toggleAbilityEntries;
		if (toggleAbilityEntries != null)
		{
			for (int j = 0; j < toggleAbilityEntries.Count; j++)
			{
				Hash128 val3 = ClassHasher<ToggleAbilityEntry>.GetHash128(toggleAbilityEntries[j]);
				result.Append(ref val3);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartAbilityModifiers source = new PartAbilityModifiers();
		result = Unsafe.As<PartAbilityModifiers, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityModifiers>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_addedModifiers", ref _addedModifiers, state);
		formatter.Field(1, "_toggleAbilityEntries", ref _toggleAbilityEntries, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityModifiers>();
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
				_addedModifiers = formatter.ReadPackable<List<AddedEntry>>(state);
				break;
			case 1:
				_toggleAbilityEntries = formatter.ReadPackable<List<ToggleAbilityEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
