using System;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[Serializable]
public class AwarenessCheckSettings
{
	[Tooltip("Тип модификатора к сложности awareness check. AutoPass открывает объект без броска когда партия подойдет на заданное в Radius расстояние.")]
	[SkillCheckActualDifficulty]
	public SkillCheckDifficulty Difficulty;

	[ShowIf("IsCustomDifficulty")]
	public int CustomDifficulty;

	[Tooltip("На каком расстоянии делать awareness check. Значение 0 означает, что чек не будет сделан никогда, так как нельзя приблизиться к объекту ближе чем на расстояние в 0 метров.")]
	public float Radius = 7f;

	[Tooltip("Объект скрыт до тех пор пока на него не посветят фонариком. Комбинируется вместе с awareness check.")]
	public bool HiddenInDarkness;

	private bool IsCustomDifficulty => Difficulty == SkillCheckDifficulty.Custom;

	public int GetDifficulty()
	{
		if (Difficulty != 0)
		{
			return Difficulty.GetDC();
		}
		return CustomDifficulty;
	}
}
