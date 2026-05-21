using System;
using System.Text;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;

namespace Kingmaker.Code.Middleware.Metrics;

public static class MetricsUtils
{
	public static string EnumToSnakeCase<T>(T enumValue) where T : Enum
	{
		string text = enumValue.ToString();
		StringBuilder stringBuilder = new StringBuilder(text.Length + 4);
		stringBuilder.Append(char.ToLower(text[0]));
		for (int i = 1; i < text.Length; i++)
		{
			char c = text[i];
			if (char.IsUpper(c))
			{
				stringBuilder.Append('_');
				stringBuilder.Append(char.ToLower(c));
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string GameDifficultyToString(GameDifficultyOption difficulty)
	{
		return difficulty switch
		{
			GameDifficultyOption.Casual => "casual", 
			GameDifficultyOption.Core => "core", 
			GameDifficultyOption.Custom => "custom", 
			GameDifficultyOption.Daring => "daring", 
			GameDifficultyOption.Hard => "hard", 
			GameDifficultyOption.Normal => "normal", 
			GameDifficultyOption.Story => "story", 
			GameDifficultyOption.Unfair => "unfair", 
			_ => EnumToSnakeCase(difficulty), 
		};
	}

	public static string GetMechanicalSelection(SelectionState selection)
	{
		if (selection is SelectionStateFeature { SelectionItem: var selectionItem })
		{
			return selectionItem?.Feature.AssetGuid;
		}
		if (selection is SelectionStateStats selectionStateStats)
		{
			foreach (StatType stat in selectionStateStats.Stats)
			{
				if (selectionStateStats.GetPointsSpent(stat) > 0)
				{
					return selectionStateStats.GetAdvancementBlueprint(stat)?.AssetGuid ?? stat.ToString();
				}
			}
		}
		return null;
	}
}
