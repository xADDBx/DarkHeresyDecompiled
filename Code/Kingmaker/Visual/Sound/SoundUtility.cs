using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public static class SoundUtility
{
	private const string SexGroupName = "CharacterSex";

	private const string TitleGroupName = "CharacterTitle";

	private const string RaceGroupName = "CharacterRace";

	public static void SetGenderFlags(GameObject go)
	{
		BlueprintUnlockableFlag kingFlag = ConfigRoot.Instance.SystemMechanics.KingFlag;
		if (kingFlag != null)
		{
			if (!Game.Instance.Player.UnlockableFlags.IsUnlocked(kingFlag))
			{
				AkUnitySoundEngine.SetSwitch("CharacterTitle", "Baron", go);
			}
			else
			{
				AkUnitySoundEngine.SetSwitch("CharacterTitle", "King", go);
			}
		}
		if (Game.Instance.Player != null && !Game.Instance.Player.MainCharacter.IsNull())
		{
			switch (Game.Instance.Player.MainCharacter.Entity.Gender)
			{
			case Gender.Male:
				AkUnitySoundEngine.SetSwitch("CharacterSex", "Male", go);
				break;
			case Gender.Female:
				AkUnitySoundEngine.SetSwitch("CharacterSex", "Female", go);
				break;
			}
		}
	}

	public static void SetRaceFlags(GameObject go)
	{
		string text = Game.Instance.Player.MainCharacterEntity.Progression.Race?.SoundKey;
		if (text != null)
		{
			AkUnitySoundEngine.SetSwitch("CharacterRace", text, go);
		}
	}
}
