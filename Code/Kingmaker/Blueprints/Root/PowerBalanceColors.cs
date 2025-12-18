using System;
using Kingmaker.Code.Gameplay.Controllers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class PowerBalanceColors
{
	public Color PlayerSliderColor;

	public Color EnemySliderColor;

	public Color PlayerGroupColor;

	public Color EnemyGroupColor;

	public Color StateRegularColor;

	public Color StateLosingBattleColor;

	public Color StateShatteredColor;

	public Color GetGroupColor(bool isPLayer)
	{
		if (!isPLayer)
		{
			return EnemyGroupColor;
		}
		return PlayerGroupColor;
	}

	public Color GetStateColor(PowerBalanceState powerBalanceState)
	{
		return powerBalanceState switch
		{
			PowerBalanceState.Regular => StateRegularColor, 
			PowerBalanceState.LosingBattle => StateLosingBattleColor, 
			PowerBalanceState.Shattered => StateShatteredColor, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
