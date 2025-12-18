using System;
using System.Collections.Generic;
using Kingmaker.Settings;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints;

[Serializable]
public class ModifiableByDifficultyParameter
{
	[Serializable]
	public class DifficultyToIntElement
	{
		public GameDifficultyOption Difficulty;

		public int Modifier;
	}

	public int BaseValue;

	public List<DifficultyToIntElement> DifficultyModifiers = new List<DifficultyToIntElement>();

	public int GetValue()
	{
		GameDifficultyOption difficulty = SettingsRoot.Difficulty.GameDifficulty.GetValue();
		if (!DifficultyModifiers.TryFind((DifficultyToIntElement e) => e.Difficulty == difficulty, out var result))
		{
			return BaseValue;
		}
		return BaseValue + result.Modifier;
	}
}
