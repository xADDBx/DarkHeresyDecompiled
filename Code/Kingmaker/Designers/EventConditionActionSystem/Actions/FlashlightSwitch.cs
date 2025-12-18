using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("75170b611fec4a2abeff6faf69e0e339")]
public class FlashlightSwitch : GameAction
{
	[SerializeField]
	private bool _turnOn;

	public override string GetCaption()
	{
		if (!_turnOn)
		{
			return "Flashlight OFF";
		}
		return "Flashlight ON";
	}

	protected override void RunAction()
	{
		UnitPartFlashlight unitPartFlashlight = Game.Instance.Player.MainCharacterEntity?.GetOrCreate<UnitPartFlashlight>();
		if (unitPartFlashlight != null)
		{
			if (_turnOn)
			{
				unitPartFlashlight.FlashlightOn();
			}
			else
			{
				unitPartFlashlight.FlashlightOff();
			}
		}
	}
}
