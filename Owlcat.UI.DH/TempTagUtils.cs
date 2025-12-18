using System;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

public static class TempTagUtils
{
	public static void GetTagNameAndDescription<T>(T tag, out string tagName, out string tagDescription) where T : BlueprintComponent
	{
		if (!(tag.OwnerBlueprint is BlueprintFeature { Description: var description } blueprintFeature))
		{
			tagName = null;
			tagDescription = null;
			return;
		}
		ReadOnlySpan<char> span = description.AsSpan(0, Math.Min(30, description.Length));
		int num = span.IndexOf('—');
		if (num < 0)
		{
			num = span.IndexOf(':');
		}
		if (num > 0)
		{
			tagName = CapitalizeWords(StripTags(description.Substring(0, num)).ToLower()).Trim();
			string text = description;
			int num2 = num + 1;
			tagDescription = text.Substring(num2, text.Length - num2).Trim();
		}
		else
		{
			tagName = StripTags(blueprintFeature.Name);
			tagDescription = description;
		}
	}

	private static string Capitalize(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		input = input.ToLower();
		string text = char.ToUpper(input[0]).ToString();
		string text2 = input;
		return text + text2.Substring(1, text2.Length - 1);
	}

	private static string CapitalizeWords(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		bool flag = true;
		foreach (char c in input)
		{
			if (char.IsWhiteSpace(c))
			{
				flag = true;
				stringBuilder.Append(c);
			}
			else if (flag)
			{
				stringBuilder.Append(char.ToUpperInvariant(c));
				flag = false;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private static string StripTags(string input)
	{
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		bool flag = false;
		foreach (char c in input)
		{
			switch (c)
			{
			case '<':
				flag = true;
				continue;
			case '>':
				flag = false;
				continue;
			}
			if (!flag)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
