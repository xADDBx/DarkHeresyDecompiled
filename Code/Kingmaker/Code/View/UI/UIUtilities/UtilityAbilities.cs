using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityAbilities
{
	public static string GetLongOrShortText(string description, bool state)
	{
		string text = description;
		if (state)
		{
			if (text.Contains("[LONGSTART]"))
			{
				while (text.Contains("[LONGSTART]"))
				{
					int startIndex = text.IndexOf("[LONGSTART]", StringComparison.Ordinal);
					int startIndex2 = text.IndexOf("[LONGSTART]", StringComparison.Ordinal) + 11;
					string text2 = text.Substring(startIndex2);
					text = text.Remove(startIndex) + text2;
				}
			}
			if (text.Contains("[LONGEND]"))
			{
				while (text.Contains("[LONGEND]"))
				{
					int startIndex3 = text.IndexOf("[LONGEND]", StringComparison.Ordinal);
					int startIndex4 = text.IndexOf("[LONGEND]", StringComparison.Ordinal) + 9;
					string text3 = text.Substring(startIndex4);
					text = text.Remove(startIndex3) + text3;
				}
			}
		}
		else if (text.Contains("[LONGSTART]") && text.Contains("[LONGEND]"))
		{
			while (text.Contains("[LONGSTART]"))
			{
				int startIndex5 = text.IndexOf("[LONGSTART]", StringComparison.Ordinal);
				int num = text.IndexOf("[LONGEND]", StringComparison.Ordinal);
				string text4 = text.Remove(startIndex5);
				if (num < 0)
				{
					text = text4;
					continue;
				}
				int num2 = num + 9;
				string text5 = text;
				int num3 = num2;
				string text6 = text5.Substring(num3, text5.Length - num3);
				text = text4 + text6;
			}
		}
		return text;
	}

	public static string GetAbilityAcronym(BlueprintFeatureBase featureBase)
	{
		if (!(featureBase is BlueprintFeature { Acronym: var acronym }))
		{
			return GetAbilityAcronym(featureBase.Name);
		}
		if (string.IsNullOrEmpty(acronym))
		{
			return GetAbilityAcronym(featureBase.Name);
		}
		return acronym;
	}

	public static string GetAbilityAcronym(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		Locale currentLocale = LocalizationManager.Instance.CurrentLocale;
		name = UIConfig.Instance.AcronymsConfig.GetWordsToExcludeFor(currentLocale).Aggregate(name, (string current, string excludeWord) => current.Replace(excludeWord, " "));
		name = name.Replace("-", " ");
		if (LocalizationManager.Instance.CurrentLocale == Locale.enGB)
		{
			for (int i = 0; i < name.Length; i++)
			{
				if (char.IsUpper(name, i))
				{
					stringBuilder.Append(name[i]);
					stringBuilder.Append(" ");
				}
			}
		}
		else if (name.Length > 0)
		{
			bool flag = true;
			bool flag2 = false;
			string text = name;
			foreach (char c in text)
			{
				switch (c)
				{
				case ' ':
				case '(':
				case ')':
					flag = true;
					continue;
				case '<':
				case '[':
				case '{':
					flag2 = true;
					continue;
				}
				if (flag2)
				{
					if (c == ']')
					{
						flag2 = false;
					}
				}
				else if (flag)
				{
					stringBuilder.Append(c);
					stringBuilder.Append(" ");
					flag = false;
				}
			}
		}
		string text2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stringBuilder.ToString());
		text2 = new string((from s in text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
			select s[0]).ToArray());
		UIConfig.Instance.AcronymsConfig.GetLettersInAcronym(currentLocale, out var maxLettersCount, out var preferredLettersCount);
		maxLettersCount = Math.Max(1, maxLettersCount);
		if (text2.Length > maxLettersCount)
		{
			text2 = text2.Remove(preferredLettersCount);
		}
		return text2;
	}
}
