using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class StatModifiersBreakdown
{
	[NotNull]
	private static List<StatBonusEntry> s_Data = new List<StatBonusEntry>();

	[CanBeNull]
	private static string s_BonusColor = null;

	[CanBeNull]
	private static string s_PenaltyColor = null;

	private static string BonusColor
	{
		get
		{
			if (s_BonusColor == null)
			{
				s_BonusColor = ColorUtility.ToHtmlStringRGB(ConfigRoot.Instance.UIConfig.TooltipColors.Bonus);
			}
			return s_BonusColor;
		}
	}

	private static string PenaltyColor
	{
		get
		{
			if (s_PenaltyColor == null)
			{
				s_PenaltyColor = ColorUtility.ToHtmlStringRGB(ConfigRoot.Instance.UIConfig.TooltipColors.Penaty);
			}
			return s_PenaltyColor;
		}
	}

	private static bool ShouldShowIfZero(Modifier bonus)
	{
		return bonus.Stat != StatType.Unknown;
	}

	public static string GetBonusSourceText(Modifier bonus)
	{
		if (bonus.Fact != null)
		{
			return GetBonusSourceText(bonus.Fact);
		}
		if (bonus.Stat != 0)
		{
			return LocalizedTexts.Instance.Stats.GetText(bonus.Stat);
		}
		return LocalizedTexts.Instance.Descriptors.GetText(bonus.Descriptor);
	}

	private static string GetBonusSourceText(IUIDataProvider source)
	{
		if (source == null)
		{
			return string.Empty;
		}
		string text = source.Name;
		if (string.IsNullOrEmpty(text))
		{
			if (source is EntityFact entityFact)
			{
				text = entityFact.ToString();
			}
			else if (source is Object @object)
			{
				text = @object.name;
			}
		}
		return text;
	}

	private static void AddBonus(Modifier bonus)
	{
		if (bonus.Value != 0 || ShouldShowIfZero(bonus))
		{
			AddBonus(bonus.Value, GetBonusSourceText(bonus), bonus.Descriptor, ShouldShowIfZero(bonus));
		}
	}

	private static void AddBonus(int value, [CanBeNull] IUIDataProvider bonusSource, ModifierDescriptor descriptor)
	{
		if (value != 0)
		{
			AddBonus(value, GetBonusSourceText(bonusSource), descriptor);
		}
	}

	public static void AddBonus(int bonusValue, [CanBeNull] string bonusSource, ModifierDescriptor descriptor = ModifierDescriptor.None, bool addZero = false)
	{
		if (bonusValue != 0 || addZero)
		{
			s_Data.Add(new StatBonusEntry
			{
				Bonus = bonusValue,
				Source = bonusSource,
				Descriptor = descriptor
			});
		}
	}

	public static void AddModifiersManager(AbstractModifiersManager modifiersManager)
	{
		AddModifiersManagerInternal(modifiersManager.List);
	}

	public static void AddModifiersManager(IEnumerable<Modifier> modifiers)
	{
		AddModifiersManagerInternal(modifiers);
	}

	public static void AddCompositeModifiersManager(CompositeModifiersManager modifiersManager)
	{
		AddModifiersManagerInternal(modifiersManager.SortedModifiersList);
	}

	private static void AddModifiersManagerInternal(IEnumerable<Modifier> modifiers)
	{
		foreach (Modifier modifier in modifiers)
		{
			if (modifier.Value != 0)
			{
				ModifierDescriptor descriptor = ((modifier.Descriptor != 0) ? modifier.Descriptor : ModifierDescriptor.UntypedUnstackable);
				if (modifier.Stat != 0)
				{
					string text = LocalizedTexts.Instance.Stats.GetText(modifier.Stat);
					AddBonus(modifier.Value, text, descriptor);
				}
				else
				{
					IUIDataProvider bonusSource = TryGetSourceFromFact(modifier.Fact) ?? modifier.Item;
					AddBonus(modifier.Value, bonusSource, descriptor);
				}
			}
		}
	}

	public static void AddStatModifiers([NotNull] ModifiableValue stat)
	{
		foreach (Modifier displayModifier in stat.GetDisplayModifiers())
		{
			if (displayModifier.Value == 0 || ModifierDisabled(stat, displayModifier))
			{
				continue;
			}
			ModifierDescriptor modifierDescriptor = displayModifier.Descriptor;
			if (modifierDescriptor == ModifierDescriptor.CareerAdvancement)
			{
				AddCareerBonuses(displayModifier);
				continue;
			}
			if (displayModifier.Stat != 0)
			{
				string text = LocalizedTexts.Instance.Stats.GetText(displayModifier.Stat);
				AddBonus(displayModifier.Value, text, modifierDescriptor);
				continue;
			}
			IUIDataProvider iUIDataProvider = TryGetSourceFromFact(displayModifier.Fact) ?? displayModifier.Item;
			if ((iUIDataProvider == null || string.IsNullOrEmpty(iUIDataProvider.Name)) && modifierDescriptor == ModifierDescriptor.None)
			{
				modifierDescriptor = (displayModifier.Stackable ? ModifierDescriptor.UntypedStackable : ModifierDescriptor.UntypedUnstackable);
			}
			AddBonus(displayModifier.Value, iUIDataProvider, modifierDescriptor);
		}
	}

	private static IUIDataProvider TryGetSourceFromFact(EntityFact fact)
	{
		if (fact != null)
		{
			return ((IUIDataProvider)fact.SourceItem) ?? fact;
		}
		return null;
	}

	private static void AddCareerBonuses(Modifier mod)
	{
		EntityFact fact = mod.Fact;
		if (fact == null)
		{
			return;
		}
		int num = mod.Value;
		if (fact.Sources.Any())
		{
			Dictionary<BlueprintPath, int> dictionary = new Dictionary<BlueprintPath, int>();
			int num2 = (fact.Blueprint as BlueprintStatAdvancement)?.ValuePerRank ?? 0;
			BlueprintPath key;
			int value;
			foreach (EntityFactSource source in fact.Sources)
			{
				if (source?.Path != null)
				{
					dictionary.TryAdd(source.Path, 0);
					key = source.Path;
					value = dictionary[key]++;
				}
			}
			foreach (KeyValuePair<BlueprintPath, int> item in dictionary)
			{
				item.Deconstruct(out key, out value);
				BlueprintPath bonusSource = key;
				int num3 = value;
				AddBonus(num2 * num3, bonusSource, ModifierDescriptor.CareerAdvancement);
			}
			num -= num2 * dictionary.Sum((KeyValuePair<BlueprintPath, int> x) => x.Value);
		}
		if (num > 0)
		{
			string empty = string.Empty;
			AddBonus(num, empty, ModifierDescriptor.CareerAdvancement);
		}
	}

	public static bool ModifierDisabled([NotNull] ModifiableValue stat, Modifier mod)
	{
		return false;
	}

	public static void AddBonusSources([NotNull] AbstractModifiersManager modifiers)
	{
		foreach (Modifier item in modifiers.List)
		{
			AddBonus(item);
		}
	}

	public static void AddStoredData([NotNull] StatModifiersBreakdownData storedData)
	{
		s_Data.AddRange(storedData.Bonuses);
	}

	public static void AppendModifiersBreakdown(this StringBuilder sb, string caption = "")
	{
		if (s_Data.Count <= 0)
		{
			return;
		}
		s_Data.Sort(StatBonusEntry.Compare);
		sb.Append(caption);
		foreach (StatBonusEntry s_Datum in s_Data)
		{
			sb.AppendBonus(s_Datum.Bonus, s_Datum.Source, s_Datum.Descriptor);
		}
		s_Data.Clear();
	}

	public static StatModifiersBreakdownData Build()
	{
		StatModifiersBreakdownData result = new StatModifiersBreakdownData(s_Data);
		s_Data = new List<StatBonusEntry>();
		return result;
	}

	public static void Clear()
	{
		s_Data.Clear();
	}

	private static void AppendBonus([NotNull] this StringBuilder sb, int bonusValue, [CanBeNull] string bonusSource, ModifierDescriptor descriptor)
	{
		if (descriptor != 0)
		{
			string text = ConfigRoot.Instance.LocalizedTexts.Descriptors.GetText(descriptor);
			sb.Append(text).Append(": ");
		}
		else if (!string.IsNullOrWhiteSpace(bonusSource))
		{
			sb.Append(bonusSource).Append(": ");
		}
		string value = ((bonusValue < 0) ? PenaltyColor : BonusColor);
		sb.Append("<color=#").Append(value).Append('>');
		sb.Append(UtilityText.AddSign(bonusValue));
		if (descriptor != 0 && !string.IsNullOrWhiteSpace(bonusSource))
		{
			sb.Append(" [").Append(bonusSource).Append(']');
		}
		sb.Append("</color>");
		sb.AppendLine();
	}
}
