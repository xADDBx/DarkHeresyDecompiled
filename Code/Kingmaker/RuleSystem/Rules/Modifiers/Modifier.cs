using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public readonly struct Modifier : IEquatable<Modifier>
{
	private sealed class ModifierValueComparer : IComparer<Modifier>
	{
		int IComparer<Modifier>.Compare(Modifier x, Modifier y)
		{
			int value = x.Value;
			return value.CompareTo(y.Value);
		}
	}

	private sealed class ModifierDescriptorComparer : IComparer<Modifier>
	{
		int IComparer<Modifier>.Compare(Modifier x, Modifier y)
		{
			return Kingmaker.Enums.ModifierDescriptorComparer.Instance.Compare(x.Descriptor, y.Descriptor);
		}
	}

	public static readonly IComparer<Modifier> ValueComparer = new ModifierValueComparer();

	public static readonly IComparer<Modifier> DescriptorComparer = new ModifierDescriptorComparer();

	public readonly ModifierType Type;

	public readonly int Value;

	[CanBeNull]
	public readonly EntityFact Fact;

	[CanBeNull]
	public readonly BlueprintComponent Component;

	[CanBeNull]
	public readonly ItemEntity Item;

	public readonly BonusType Bonus;

	public readonly StatType Stat;

	public readonly ModifierDescriptor Descriptor;

	public bool Stackable => Descriptor.IsStackable();

	public bool Positive => IsPositive(this);

	public bool Zero => IsZero(this);

	public bool Permanent => Descriptor.IsPermanentModifier();

	public bool IsBaseValue
	{
		get
		{
			if (Type == ModifierType.ValAdd)
			{
				return Descriptor == ModifierDescriptor.BaseValue;
			}
			return false;
		}
	}

	public bool IsPercent
	{
		get
		{
			ModifierType type = Type;
			return type == ModifierType.PctAdd || type == ModifierType.PctMul || type == ModifierType.PctMul_Extra;
		}
	}

	public Modifier(ModifierType type, int value, [CanBeNull] EntityFact fact = null, [CanBeNull] BlueprintComponent component = null, [CanBeNull] ItemEntity item = null, BonusType bonusType = BonusType.None, StatType stat = StatType.Unknown, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		Type = type;
		Value = value;
		Fact = fact;
		Component = component;
		Item = item;
		Bonus = bonusType;
		Stat = stat;
		Descriptor = descriptor;
	}

	public bool StacksWith(Modifier other)
	{
		if (!Descriptor.IsStackable())
		{
			return !SameStack(other);
		}
		return true;
	}

	public bool SameStack(Modifier other)
	{
		if (other.Descriptor == Descriptor && other.Type == Type)
		{
			return other.Positive == Positive;
		}
		return false;
	}

	public static bool IsPositive(Modifier m)
	{
		return m.Type switch
		{
			ModifierType.ValAdd => m.Value > 0, 
			ModifierType.PctAdd => m.Value > 0, 
			ModifierType.PctMul => m.Value >= 100, 
			ModifierType.ValAdd_Extra => m.Value > 0, 
			ModifierType.PctMul_Extra => m.Value >= 100, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static bool IsZero(Modifier m)
	{
		return m.Type switch
		{
			ModifierType.ValAdd => m.Value == 0, 
			ModifierType.PctAdd => m.Value == 0, 
			ModifierType.PctMul => m.Value == 100, 
			ModifierType.ValAdd_Extra => m.Value == 0, 
			ModifierType.PctMul_Extra => m.Value == 100, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public override string ToString()
	{
		return $"{Type}[{Value}]#{GetSourceText(this)}{GetDescriptorText(this)}";
		static string GetDescriptorText(Modifier m)
		{
			if (m.Descriptor == ModifierDescriptor.None)
			{
				return string.Empty;
			}
			return $":{m.Descriptor}";
		}
		static string GetSourceText(Modifier m)
		{
			if (m.Fact == null)
			{
				if (m.Stat == StatType.Unknown)
				{
					if (m.Bonus == BonusType.None)
					{
						return "unknown-source";
					}
					return m.Bonus.ToString();
				}
				return m.Stat.ToString();
			}
			return m.Fact.Blueprint.name;
		}
	}

	public bool Equals(Modifier other)
	{
		if (Type == other.Type && Descriptor == other.Descriptor && Value == other.Value && Stat == other.Stat && Bonus == other.Bonus && Fact == other.Fact && Component == other.Component)
		{
			return Item == other.Item;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Modifier other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine((int)Type, Value, Fact, Component, Item, (int)Bonus, (int)Stat, (int)Descriptor);
	}

	public static bool operator ==(Modifier left, Modifier right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Modifier left, Modifier right)
	{
		return !(left == right);
	}

	public Modifier WithType(ModifierType type)
	{
		return new Modifier(type, Value, Fact, Component, Item, Bonus, Stat, Descriptor);
	}
}
