using System;
using System.Collections.Generic;

namespace Kingmaker.Localization;

public class LocalizedStringParamAttribute : Attribute
{
	public string Kind;

	public LocalizedStringGroup Group;

	private static readonly Dictionary<string, string> s_KindToPrettyName = new Dictionary<string, string>
	{
		{ "bark", "bark" },
		{ "buff", "buff" },
		{ "cue", "cue" },
		{ "answer", "answer" },
		{ "barkbanter", "BarkBanter" },
		{ "ask", "Ask" },
		{ "cases", "Cases" },
		{ "barkcutscene", "BarkCutscene" },
		{ "meta", "Meta" }
	};

	public bool TryGetPathPrefix(out string prefix)
	{
		prefix = string.Empty;
		if (!s_KindToPrettyName.TryGetValue(Kind, out var value))
		{
			throw new Exception("No pretty name for Kind!");
		}
		if (Kind == "meta")
		{
			prefix = $"{value}/{Group}";
		}
		return !string.IsNullOrEmpty(prefix);
	}
}
