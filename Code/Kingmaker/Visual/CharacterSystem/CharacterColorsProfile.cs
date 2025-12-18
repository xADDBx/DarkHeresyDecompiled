using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[CreateAssetMenu(menuName = "Character System/Colors Profile")]
public class CharacterColorsProfile : ScriptableObject
{
	public enum ColorsProfileMode
	{
		Replace,
		Overlay,
		Multiply
	}

	public ColorsProfileMode Mode;

	public List<Texture2D> Ramps = new List<Texture2D>();
}
