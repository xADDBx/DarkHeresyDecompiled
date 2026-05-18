using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Framework.Utility;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.GameLog.CombatLog_ThreadSystem;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.Framework.GameLog;

public abstract class LogThreadBase : BaseDisposable
{
	private ObservableList<CombatLogMessage> m_ThreadMessages = new ObservableList<CombatLogMessage>();

	public static bool IsPreviousMessageUseSomething;

	protected static GameLogStrings Strings => GameLogStrings.Instance;

	protected static LogColors Colors => ConfigRoot.Instance.UIConfig.LogColors;

	public IReadOnlyList<CombatLogMessage> AllMessages => m_ThreadMessages;

	public virtual void StartThread()
	{
	}

	public Observable<CollectionAddEvent<CombatLogMessage>> ObserveAdd()
	{
		return m_ThreadMessages.ObserveAdd();
	}

	public Observable<CollectionRemoveEvent<CombatLogMessage>> ObserveRemove()
	{
		return m_ThreadMessages.ObserveRemove();
	}

	protected void AddMessage(CombatLogMessage newMessage)
	{
		IsPreviousMessageUseSomething = false;
		if (!ContextData<GameLogDisabled>.Current && newMessage != null)
		{
			m_ThreadMessages.Add(newMessage);
		}
	}

	public static ITooltipBrick CreateBrickModifier(Modifier modifier, bool valueIsPercent = false, string additionText = null, int nestedLevel = 0, bool isResultValue = false, bool isWithoutPlus = false, bool noName = false)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> createBrickTextValue = CombatLogTooltipService.CreateBrickTextValue;
		if (createBrickTextValue == null)
		{
			return null;
		}
		if (modifier.Value == 0)
		{
			return null;
		}
		string text = string.Empty;
		if (!noName)
		{
			text = modifier.GetFormattedName();
		}
		ModifierFormatFlags modifierFormatFlags = ModifierFormatFlags.None;
		if (valueIsPercent)
		{
			modifierFormatFlags |= ModifierFormatFlags.IsPercent;
		}
		if (isWithoutPlus)
		{
			modifierFormatFlags |= ModifierFormatFlags.NoPlusSign;
		}
		string formattedValue = modifier.GetFormattedValue(additionText, modifierFormatFlags);
		return createBrickTextValue(new TooltipBrickTextValueArgs(text, formattedValue, nestedLevel, isResultValue));
	}

	public void Cleanup()
	{
		m_ThreadMessages.Clear();
	}

	protected static IEnumerable<ITooltipBrick> CreateBrickModifiers(IEnumerable<Modifier> allModifiers, bool valueIsPercent = false, string additionText = null, int nestedLevel = 0, bool isResultValue = false, bool isFirstWithoutPlus = false)
	{
		foreach (Modifier allModifier in allModifiers)
		{
			ITooltipBrick tooltipBrick = CreateBrickModifier(allModifier, valueIsPercent, additionText, nestedLevel, isResultValue, isFirstWithoutPlus);
			isFirstWithoutPlus = false;
			if (tooltipBrick != null)
			{
				yield return tooltipBrick;
			}
		}
	}

	public static IEnumerable<ITooltipBrick> GetMinMaxDamageModifiers(IReadonlyModifiersValue min, IReadonlyModifiersValue max, int nestedLevel)
	{
		Func<BrickIconTextValueArgs, ITooltipBrick> tooltipBrickIconTextValue = CombatLogTooltipService.CreateBrickIconTextValue;
		if (tooltipBrickIconTextValue == null)
		{
			yield break;
		}
		TooltipBrickStrings s = Strings.TooltipBrickStrings;
		List<Modifier> copyList = TempList.Get<Modifier>();
		foreach (Modifier item in min.List)
		{
			bool flag = false;
			foreach (Modifier item2 in max.List)
			{
				if (item.Type == item2.Type && item.Value == item2.Value && item.Fact == item2.Fact && item.Item == item2.Item && item.Bonus == item2.Bonus && item.Stat == item2.Stat && item.Descriptor == item2.Descriptor)
				{
					flag = true;
					copyList.Add(item2);
				}
			}
			TooltipModifiersUtility.ModifierDescription modifierDescription = new TooltipModifiersUtility.ModifierDescription(item);
			yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(modifierDescription.LocalizedName, flag ? modifierDescription.Value : (modifierDescription.Value + " " + s.MinDamage.Text), nestedLevel, isResultValue: true));
		}
		foreach (Modifier item3 in max.List)
		{
			if (!copyList.Contains(item3))
			{
				TooltipModifiersUtility.ModifierDescription modifierDescription2 = new TooltipModifiersUtility.ModifierDescription(item3);
				yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(modifierDescription2.LocalizedName, modifierDescription2.Value + " " + s.MaxDamage.Text, nestedLevel, isResultValue: true));
			}
		}
	}

	public static IEnumerable<ITooltipBrick> GetModifiersSummary(IReadonlyModifiersComposite modifiers, int nestedLevel, IReadonlyModifiersComposite baseValueModifiers = null, IReadonlyModifiersComposite vitalModifiers = null)
	{
		return GetModifiers(modifiers, nestedLevel, showSummary: true, showModifiers: false, baseValueModifiers, vitalModifiers);
	}

	private static IEnumerable<ITooltipBrick> GetModifiers(IReadonlyModifiersComposite modifiers, int nestedLevel, bool showSummary, bool showModifiers, IReadonlyModifiersComposite baseValueModifiers = null, IReadonlyModifiersComposite vitalModifiers = null)
	{
		Func<BrickIconTextValueArgs, ITooltipBrick> tooltipBrickIconTextValue = CombatLogTooltipService.CreateBrickIconTextValue;
		if (tooltipBrickIconTextValue == null)
		{
			yield break;
		}
		bool baseValueAdded = false;
		foreach (TooltipModifiersUtility.ModifiersListDescription modifiersList in TooltipModifiersUtility.GetDescription(modifiers, baseValueModifiers, vitalModifiers))
		{
			if (modifiersList.Count == 0)
			{
				continue;
			}
			if (showSummary)
			{
				if (!baseValueAdded)
				{
					TooltipModifiersUtility.ModifierDescription modifierDescription = new TooltipModifiersUtility.ModifierDescription(modifiers.GetModifier((Modifier i) => i.Type == ModifierType.ValAdd && i.Descriptor == ModifierDescriptor.BaseValue) ?? new Modifier(ModifierType.ValAdd, 0, null, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue));
					yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(modifierDescription.LocalizedName, modifierDescription.PlainValue, nestedLevel, isResultValue: true));
					baseValueAdded = true;
				}
				yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(modifiersList.LocalizedTitle, modifiersList.TitleValue, nestedLevel, isResultValue: true, null, CombatLogIcon.None, BrickElementPalette.Normal, new TooltipTemplateModifiers(modifiersList, excludeBaseValue: true)));
			}
			if (!showModifiers)
			{
				continue;
			}
			foreach (TooltipModifiersUtility.ModifierDescription modifier in modifiersList)
			{
				int nested = (showSummary ? (nestedLevel + 1) : nestedLevel);
				yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(modifier.LocalizedName, modifier.Value, nested, isResultValue: true));
				IReadonlyModifiersComposite details = modifier.Details;
				if (details == null || details.List.Count <= 0)
				{
					continue;
				}
				foreach (ITooltipBrick modifier2 in GetModifiers(details, nested + 1, showSummary: false, showModifiers: true))
				{
					yield return modifier2;
				}
			}
		}
	}

	public static IEnumerable<ITooltipBrick> GetCommonDamageModifiers(IReadonlyModifiersComposite modifiers, int nestedLevel)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> tooltipBrickTextValue = CombatLogTooltipService.CreateBrickTextValue;
		Func<BrickIconTextValueArgs, ITooltipBrick> tooltipBrickIconTextValue = CombatLogTooltipService.CreateBrickIconTextValue;
		if (tooltipBrickTextValue == null || tooltipBrickIconTextValue == null)
		{
			yield break;
		}
		TooltipBrickStrings s = Strings.TooltipBrickStrings;
		List<Modifier> valAddList = modifiers.ValueModifiersList.ToList();
		if (valAddList.Count > 0)
		{
			int num = 0;
			bool flag = false;
			foreach (Modifier item in valAddList)
			{
				if (item.Value != 0)
				{
					flag = true;
					num += item.Value;
				}
			}
			if (num != 0 || flag)
			{
				yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(s.ValAdd.Text, UtilityText.AddSign(num).ToString(CultureInfo.InvariantCulture) ?? "", nestedLevel + 1, isResultValue: true));
				bool needPrefix = false;
				foreach (Modifier item2 in valAddList)
				{
					if (item2.Value != 0)
					{
						string text = ((!needPrefix) ? "" : ((item2.Value > 0) ? "+" : ""));
						needPrefix = true;
						string modifierName = GetModifierName(item2);
						int value = item2.Value;
						yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(modifierName, text + value.ToString(CultureInfo.InvariantCulture), nestedLevel + 1, isResultValue: true));
					}
				}
			}
		}
		List<Modifier> pctAddList = modifiers.PercentModifiersList.ToList();
		if (pctAddList.Count > 0)
		{
			float num2 = 0f;
			bool flag2 = false;
			foreach (Modifier item3 in pctAddList)
			{
				if (item3.Value != 0)
				{
					flag2 = true;
					num2 += (float)item3.Value / 100f;
				}
			}
			num2 *= 100f;
			if (num2 != 0f || flag2)
			{
				num2 += 100f;
				yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(s.PctAdd.Text, $"×{(num2 / 100f).ToString(CultureInfo.InvariantCulture)} ({num2}%)", nestedLevel + 1, isResultValue: true));
				yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(s.BaseModifier.Text, "100%", nestedLevel + 1, isResultValue: true));
				foreach (Modifier item4 in pctAddList)
				{
					if (item4.Value != 0)
					{
						yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(GetModifierName(item4), UtilityText.AddSign(item4.Value).ToString(CultureInfo.InvariantCulture) + "%", nestedLevel + 1, isResultValue: true));
					}
				}
			}
		}
		List<Modifier> pctMulList = modifiers.PercentMultipliersList.ToList();
		if (pctMulList.Count > 0)
		{
			float num3 = 1f;
			foreach (Modifier item5 in pctMulList)
			{
				num3 *= (float)item5.Value / 100f;
			}
			num3 *= 100f;
			yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(s.PctMul.Text, "×" + (num3 / 100f).ToString(CultureInfo.InvariantCulture) + " (" + num3 + "%)", nestedLevel + 1, isResultValue: true));
			yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(s.BaseModifier.Text, "100%", nestedLevel + 1, isResultValue: true));
			foreach (Modifier item6 in pctMulList)
			{
				string modifierName2 = GetModifierName(item6);
				string[] obj = new string[5]
				{
					"×",
					((float)item6.Value / 100f).ToString(CultureInfo.InvariantCulture),
					" (",
					null,
					null
				};
				int value = item6.Value;
				obj[3] = value.ToString();
				obj[4] = "%)";
				yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(modifierName2, string.Concat(obj), nestedLevel + 1, isResultValue: true));
			}
		}
		List<Modifier> valAddExtraList = modifiers.ValueModifiersExtraList.ToList();
		if (valAddExtraList.Count > 0)
		{
			int num4 = 0;
			bool flag3 = false;
			foreach (Modifier item7 in valAddExtraList)
			{
				if (item7.Value != 0)
				{
					flag3 = true;
					num4 += item7.Value;
				}
			}
			if (num4 != 0 || flag3)
			{
				yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(s.ValAddExtra.Text, UtilityText.AddSign(num4).ToString(CultureInfo.InvariantCulture) ?? "", nestedLevel + 1, isResultValue: true));
				bool needPrefix = false;
				foreach (Modifier item8 in valAddExtraList)
				{
					if (item8.Value != 0)
					{
						string text2 = ((!needPrefix) ? "" : ((item8.Value > 0) ? "+" : ""));
						needPrefix = true;
						string modifierName3 = GetModifierName(item8);
						int value = item8.Value;
						yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(modifierName3, text2 + value.ToString(CultureInfo.InvariantCulture), nestedLevel + 1, isResultValue: true));
					}
				}
			}
		}
		List<Modifier> pctMulExtraList = modifiers.PercentMultipliersExtraList.ToList();
		if (pctMulExtraList.Count <= 0)
		{
			yield break;
		}
		float num5 = 1f;
		foreach (Modifier item9 in pctMulExtraList)
		{
			num5 *= (float)item9.Value / 100f;
		}
		num5 *= 100f;
		yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(s.PctMulExtra.Text, "×" + (num5 / 100f).ToString(CultureInfo.InvariantCulture) + " (" + num5 + "%)", nestedLevel + 1, isResultValue: true));
		yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(s.BaseModifier.Text, "100%", nestedLevel + 1, isResultValue: true));
		foreach (Modifier item10 in pctMulExtraList)
		{
			string modifierName4 = GetModifierName(item10);
			string[] obj2 = new string[5]
			{
				"×",
				((float)item10.Value / 100f).ToString(CultureInfo.InvariantCulture),
				" (",
				null,
				null
			};
			int value = item10.Value;
			obj2[3] = value.ToString();
			obj2[4] = "%)";
			yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(modifierName4, string.Concat(obj2), nestedLevel + 1, isResultValue: true));
		}
	}

	public static IEnumerable<ITooltipBrick> GetDamageModifiers(RolledDamage damageData, int nestedLevel, bool minMax, bool common)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> createBrickTextValue = CombatLogTooltipService.CreateBrickTextValue;
		Func<BrickIconTextValueArgs, ITooltipBrick> createBrickIconTextValue = CombatLogTooltipService.CreateBrickIconTextValue;
		if (createBrickTextValue == null || createBrickIconTextValue == null)
		{
			yield break;
		}
		if (minMax)
		{
			foreach (ITooltipBrick minMaxDamageModifier in GetMinMaxDamageModifiers(damageData.BaseDamageMin, damageData.BaseDamageMax, nestedLevel))
			{
				yield return minMaxDamageModifier;
			}
		}
		if (!common)
		{
			yield break;
		}
		foreach (ITooltipBrick commonDamageModifier in GetCommonDamageModifiers(damageData.Modifiers, nestedLevel))
		{
			yield return commonDamageModifier;
		}
	}

	public static string GetModifierName(Modifier modifier)
	{
		string text = StatModifiersBreakdown.GetBonusSourceText(modifier);
		if (text.IsNullOrEmpty())
		{
			text = LocalizedTexts.Instance.Descriptors.GetText(modifier.Descriptor);
		}
		return text;
	}

	protected static IEnumerable<ITooltipBrick> ShowReroll(RuleRollChance roll, int chance, bool isTargetHitIcon = false, bool isProtectionIcon = false)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> tooltipBrickTextValue = CombatLogTooltipService.CreateBrickTextValue;
		Func<BrickChanceArgs, ITooltipBrick> tooltipBrickChance = CombatLogTooltipService.CreateBrickChance;
		Func<TooltipBrickTriggeredAutoArgs, ITooltipBrick> createBrickTriggeredAuto = CombatLogTooltipService.CreateBrickTriggeredAuto;
		if (tooltipBrickTextValue != null && tooltipBrickChance != null && createBrickTriggeredAuto != null && roll.RerollSuccessChance.HasValue)
		{
			int num = roll.RollHistory[0];
			yield return createBrickTriggeredAuto(new TooltipBrickTriggeredAutoArgs(Strings.TooltipBrickStrings.TriggeredReroll.Text, null, num <= chance));
			yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(roll.RerollSourceFactName, null, 2));
			for (int i = roll.RollHistory.Count - 2; i >= 0; i--)
			{
				yield return tooltipBrickChance(new BrickChanceArgs((i == 0) ? Strings.TooltipBrickStrings.InitialRoll.Text : Strings.TooltipBrickStrings.CheckRoll.Text, (i == 0) ? chance : roll.RerollSuccessChance.Value, roll.RollHistory[i], 2, isResultValue: false, null, CombatLogIcon.Protection));
			}
		}
	}

	protected static ITooltipBrick MinMaxValueBorder(int value, int min, int max, int nestedLevel, bool percent, string minText = null, string maxText = null)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> createBrickTextValue = CombatLogTooltipService.CreateBrickTextValue;
		if (createBrickTextValue == null)
		{
			return null;
		}
		TooltipBrickStrings tooltipBrickStrings = Strings.TooltipBrickStrings;
		if (minText == null)
		{
			minText = tooltipBrickStrings.MinValue.Text;
		}
		if (maxText == null)
		{
			maxText = tooltipBrickStrings.MaxValue.Text;
		}
		string text = (percent ? "%" : "");
		if (value < min)
		{
			return createBrickTextValue(new TooltipBrickTextValueArgs(minText, $"{tooltipBrickStrings.MinValue.Text} {min}{text} ({value}{text})", nestedLevel));
		}
		if (value > max)
		{
			return createBrickTextValue(new TooltipBrickTextValueArgs(maxText, $"{tooltipBrickStrings.MaxValue.Text} {max}{text} ({value}{text})", nestedLevel));
		}
		return null;
	}

	protected static ITooltipBrick AddMinMaxValue(float value, int nestedLevel, int minValue = 0, bool isResultValue = false)
	{
		Func<BrickIconTextValueArgs, ITooltipBrick> createBrickIconTextValue = CombatLogTooltipService.CreateBrickIconTextValue;
		if (createBrickIconTextValue == null)
		{
			return null;
		}
		if (value < (float)minValue)
		{
			return createBrickIconTextValue(new BrickIconTextValueArgs("<b>" + Strings.TooltipBrickStrings.ChanceBorderMin.Text + "</b>", "<b>" + Strings.TooltipBrickStrings.MinValue.Text + " " + minValue + "% (" + value.ToString(CultureInfo.InvariantCulture) + "%</b>)", nestedLevel, isResultValue, null, CombatLogIcon.BorderChance));
		}
		if (value > 100f)
		{
			return createBrickIconTextValue(new BrickIconTextValueArgs("<b>" + Strings.TooltipBrickStrings.ChanceBorder.Text + "</b>", "<b>" + Strings.TooltipBrickStrings.MaxValue.Text + " " + 100 + "% (" + value.ToString(CultureInfo.InvariantCulture) + "%</b>)", nestedLevel, isResultValue, null, CombatLogIcon.BorderChance));
		}
		return null;
	}

	protected override void DisposeImplementation()
	{
		m_ThreadMessages.Clear();
		m_ThreadMessages = null;
	}
}
