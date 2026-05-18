using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools.Core;

public abstract class BaseTextTemplateEngine : ITextTemplateEngine
{
	protected static readonly Dictionary<string, TextTemplate> TemplatesByTag = new Dictionary<string, TextTemplate>();

	protected void AddTemplate(string tag, TextTemplate template)
	{
		TemplatesByTag[tag.ToLowerInvariant()] = template;
	}

	public virtual string Process(string text)
	{
		if (!text.Contains('{'))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>();
		try
		{
			int i = 0;
			while (i < text.Length)
			{
				char c = text[i];
				if (c != '{')
				{
					stringBuilder.Append(c);
					i++;
					continue;
				}
				int num = i;
				int num2 = i;
				do
				{
					i++;
				}
				while (i < text.Length && text[i] != '}' && text[i] != '|' && text[i] != '{');
				if (i >= text.Length || text[i] == '{')
				{
					stringBuilder.Append(text, num, i - num);
					continue;
				}
				string text2 = text.Substring(num + 1, i - num - 1);
				bool capitalized = text2.Length > 0 && char.IsUpper(text2[0]);
				TemplatesByTag.TryGetValue(text2.ToLowerInvariant(), out var value);
				if (value == null)
				{
					for (; i < text.Length && text[i] != '}'; i++)
					{
					}
					stringBuilder.Append(text, num, i - num);
					continue;
				}
				list.Clear();
				if (text[i] == '}')
				{
					stringBuilder.Append(value.Generate(capitalized, list));
					i++;
					continue;
				}
				num = i;
				while (i < text.Length - 1)
				{
					i++;
					if (text[i] == '|' || text[i] == '}')
					{
						list.Add(text.Substring(num + 1, i - num - 1));
						num = i;
					}
					if (text[i] == '}' || text[i] == '{')
					{
						break;
					}
				}
				if (text[i] == '}')
				{
					stringBuilder.Append(value.Generate(capitalized, list));
					i++;
				}
				else if (i >= text.Length - 1 || text[i] == '{')
				{
					stringBuilder.Append(text, num2, i - num2);
				}
			}
			return stringBuilder.ToString();
		}
		catch (Exception innerException)
		{
			throw new Exception("Exception in string \"" + text + "\"", innerException);
		}
	}

	[CanBeNull]
	public virtual TextTemplate GetTemplate(Match match, out string tag, out string[] parameters, out bool capitalized)
	{
		string[] array = match.Groups[1].Value.Split('|');
		tag = array[0];
		if (TemplatesByTag.TryGetValue(tag.ToLowerInvariant(), out var value))
		{
			parameters = array.Skip(1).ToArray();
			capitalized = char.IsUpper(tag[0]);
			return value;
		}
		parameters = null;
		capitalized = false;
		return null;
	}

	public virtual string[] GetTemplateTags()
	{
		return Array.Empty<string>();
	}
}
