using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public static class DetectiveInfoEncryption
{
	public static readonly string AddendumsTag = "a|";

	public static readonly string ConclusionsTag = "c|";

	public static string EncryptAddendums(BlueprintClue clue, IEnumerable<BlueprintClueAddendum> addendums)
	{
		return (from a in addendums
			where a.ParentClue.Blueprint == clue
			select a.ParentClue.Blueprint.Addendums.Dereference().IndexOf(a)).Aggregate(AddendumsTag, (string current, int s) => current + $"{s},");
	}

	public static bool TryDecryptAddendums(BlueprintClue clue, string encrypted, out List<string> descs)
	{
		if (!encrypted.StartsWith(AddendumsTag))
		{
			descs = null;
			return false;
		}
		string[] array = encrypted.Remove(0, AddendumsTag.Length).Split(',');
		descs = new List<string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (int.TryParse(array2[i], out var result))
			{
				string item = clue.Addendums.ElementAtOrDefault(result)?.Blueprint.Description.Text;
				descs.Add(item);
			}
		}
		return descs.Any();
	}

	public static string EncryptConclusions(IEnumerable<BlueprintConclusion> conclusions)
	{
		return conclusions.Select((BlueprintConclusion c) => c.ParentCase.Blueprint.Conclusions.Dereference().IndexOf(c)).Aggregate(ConclusionsTag, (string current, int s) => current + $"{s},");
	}

	public static bool TryDecryptConclusions(BlueprintCase parentCase, string encrypted, out List<string> descs)
	{
		if (!encrypted.StartsWith(ConclusionsTag))
		{
			descs = null;
			return false;
		}
		string[] array = encrypted.Remove(0, ConclusionsTag.Length).Split(',');
		descs = new List<string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (int.TryParse(array2[i], out var result))
			{
				string item = parentCase.Conclusions.ElementAtOrDefault(result)?.Blueprint.Description.Text;
				descs.Add(item);
			}
		}
		return descs.Any();
	}
}
