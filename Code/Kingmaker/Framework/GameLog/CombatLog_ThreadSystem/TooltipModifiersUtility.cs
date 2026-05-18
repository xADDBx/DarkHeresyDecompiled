using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.Framework.GameLog.CombatLog_ThreadSystem;

public static class TooltipModifiersUtility
{
	public readonly struct ModifiersListDescription : IEnumerable<ModifierDescription>, IEnumerable
	{
		private readonly IEnumerable<Modifier> _list;

		private readonly IReadonlyModifiersComposite? _baseValueDetails;

		private readonly IReadonlyModifiersComposite? _vitalDetails;

		public readonly ModifierType Type;

		public readonly int Sum;

		public readonly int Count;

		public string LocalizedTitle => Type switch
		{
			ModifierType.ValAdd => GameLogStrings.Instance.TooltipBrickStrings.ValAdd.Text, 
			ModifierType.PctAdd => GameLogStrings.Instance.TooltipBrickStrings.PctAdd.Text, 
			ModifierType.PctMul => GameLogStrings.Instance.TooltipBrickStrings.PctMul.Text, 
			ModifierType.ValAdd_Extra => GameLogStrings.Instance.TooltipBrickStrings.ValAddExtra.Text, 
			ModifierType.PctMul_Extra => GameLogStrings.Instance.TooltipBrickStrings.PctMulExtra.Text, 
			_ => throw new ArgumentOutOfRangeException(), 
		};

		public string TitleValue
		{
			get
			{
				switch (Type)
				{
				case ModifierType.ValAdd:
				case ModifierType.ValAdd_Extra:
					return UtilityText.AddSign(Sum).ToString(CultureInfo.InvariantCulture) ?? "";
				case ModifierType.PctAdd:
					return "×" + (1f + (float)Sum / 100f).ToString(CultureInfo.InvariantCulture) + " (" + Sum.ToStringWithSign() + "%)";
				case ModifierType.PctMul:
				case ModifierType.PctMul_Extra:
					return $"×{((float)Sum / 100f).ToString(CultureInfo.InvariantCulture)} ({Sum}%)";
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		public ModifiersListDescription(ModifierType type, IEnumerable<Modifier> list, IReadonlyModifiersComposite? baseValueDetails = null, IReadonlyModifiersComposite? vitalDetails = null)
		{
			Type = type;
			_list = list;
			_baseValueDetails = baseValueDetails;
			_vitalDetails = vitalDetails;
			int num = 0;
			double num2 = Type switch
			{
				ModifierType.ValAdd => 0, 
				ModifierType.PctAdd => 0, 
				ModifierType.PctMul => 1, 
				ModifierType.ValAdd_Extra => 0, 
				ModifierType.PctMul_Extra => 1, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			foreach (Modifier item in _list)
			{
				if (!item.Zero && !item.IsBaseValue)
				{
					num++;
					num2 = Aggregate(num2, item);
				}
			}
			Count = num;
			ModifierType type2 = Type;
			Sum = ((type2 == ModifierType.PctMul || type2 == ModifierType.PctMul_Extra) ? ((int)(num2 * 100.0)) : ((int)num2));
		}

		public IEnumerator<ModifierDescription> GetEnumerator()
		{
			foreach (Modifier item in _list)
			{
				if (!item.Zero)
				{
					yield return new ModifierDescription(item, GetDetails(item));
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IReadonlyModifiersComposite? GetDetails(Modifier modifier)
		{
			return modifier.Descriptor switch
			{
				ModifierDescriptor.BaseValue => _baseValueDetails, 
				ModifierDescriptor.Vital => _vitalDetails, 
				_ => null, 
			};
		}

		private static double Aggregate(double value, Modifier modifier)
		{
			switch (modifier.Type)
			{
			case ModifierType.ValAdd:
			case ModifierType.PctAdd:
			case ModifierType.ValAdd_Extra:
				return value + (double)modifier.Value;
			case ModifierType.PctMul:
			case ModifierType.PctMul_Extra:
				return value * ((double)modifier.Value / 100.0);
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public readonly struct ModifierDescription
	{
		private readonly Modifier _modifier;

		public readonly IReadonlyModifiersComposite? Details;

		public string LocalizedName => LogThreadBase.GetModifierName(_modifier);

		public string Value
		{
			get
			{
				switch (_modifier.Type)
				{
				case ModifierType.ValAdd:
				case ModifierType.ValAdd_Extra:
					return UtilityText.AddSign(_modifier.Value).ToString(CultureInfo.InvariantCulture) ?? "";
				case ModifierType.PctAdd:
					return UtilityText.AddSign(_modifier.Value).ToString(CultureInfo.InvariantCulture) + "%";
				case ModifierType.PctMul:
				case ModifierType.PctMul_Extra:
					return $"×{((float)_modifier.Value / 100f).ToString(CultureInfo.InvariantCulture)} ({_modifier.Value}%)";
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		public string PlainValue
		{
			get
			{
				int value = _modifier.Value;
				return value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public bool IsBaseValue => _modifier.IsBaseValue;

		public ModifierDescription(Modifier modifier, IReadonlyModifiersComposite? details = null)
		{
			_modifier = modifier;
			Details = details;
		}
	}

	public static IEnumerable<ModifiersListDescription> GetDescription(IReadonlyModifiersComposite modifiers, IReadonlyModifiersComposite? baseValueDetails = null, IReadonlyModifiersComposite? vitalDetails = null)
	{
		return Enumerable.Empty<ModifiersListDescription>().Append(new ModifiersListDescription(ModifierType.ValAdd, modifiers.ValueModifiersList, baseValueDetails, vitalDetails)).Append(new ModifiersListDescription(ModifierType.PctAdd, modifiers.PercentModifiersList, baseValueDetails, vitalDetails))
			.Append(new ModifiersListDescription(ModifierType.PctMul, modifiers.PercentMultipliersList, baseValueDetails, vitalDetails))
			.Append(new ModifiersListDescription(ModifierType.ValAdd_Extra, modifiers.ValueModifiersExtraList, baseValueDetails, vitalDetails))
			.Append(new ModifiersListDescription(ModifierType.PctMul_Extra, modifiers.PercentMultipliersExtraList, baseValueDetails, vitalDetails));
	}
}
