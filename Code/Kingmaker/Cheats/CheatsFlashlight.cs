using Core.Cheats;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Cheats;

public static class CheatsFlashlight
{
	[Cheat(Name = "flashlight_toggle", ExecutionPolicy = ExecutionPolicy.PlayMode, Description = "Переключает состояние фонарика главного персонажа")]
	public static void ToggleFlashlight()
	{
		UnitPartFlashlight unitPartFlashlight = Game.Instance.Player.MainCharacterEntity?.GetOrCreate<UnitPartFlashlight>();
		if (unitPartFlashlight != null)
		{
			if (unitPartFlashlight.IsFlashlightEnabled)
			{
				unitPartFlashlight.FlashlightOff();
			}
			else
			{
				unitPartFlashlight.FlashlightOn();
			}
		}
	}

	[Cheat(Name = "flashlight_on", ExecutionPolicy = ExecutionPolicy.PlayMode, Description = "Включает фонарик главного персонажа")]
	public static void FlashlightOn()
	{
		(Game.Instance.Player.MainCharacterEntity?.GetOrCreate<UnitPartFlashlight>())?.FlashlightOn();
	}

	[Cheat(Name = "flashlight_off", ExecutionPolicy = ExecutionPolicy.PlayMode, Description = "Выключает фонарик главного персонажа")]
	public static void FlashlightOff()
	{
		(Game.Instance.Player.MainCharacterEntity?.GetOrCreate<UnitPartFlashlight>())?.FlashlightOff();
	}
}
