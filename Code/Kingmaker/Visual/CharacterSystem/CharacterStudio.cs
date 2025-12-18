using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterStudio : MonoBehaviour
{
	public enum Gender
	{
		Male,
		Female,
		None,
		NotDetermined
	}

	public enum Race
	{
		Human,
		Spacemarine,
		Eldar,
		Kroot,
		Ogryn,
		NotDetermined
	}

	public enum SaveCharacterAction
	{
		CharacterRegularAs,
		CharacterRegularOverwrite,
		CharacterCutsceneAs,
		CharacterCutsceneOverwrite
	}
}
