using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[TypeId("2a0e55c10c0a4b9284d2a46eef49bc8d")]
[AllowedOn(typeof(BlueprintEtude))]
public class EnableDetectiveFlashlight : EtudeBracketTrigger
{
	[SerializeField]
	public bool TurnOnFlashlight = true;

	private UnitPartFlashlight Flashlight => Game.Instance.Player.Flashlight;

	protected override void OnEnter()
	{
		if (Flashlight == null)
		{
			return;
		}
		Flashlight.FlashlightInUse = true;
		if (TurnOnFlashlight)
		{
			Flashlight.FlashlightOn();
		}
		foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
		{
			mapObject.SuppressedByFlashlight = true;
		}
	}

	protected override void OnResume()
	{
		if (Flashlight != null)
		{
			Flashlight.FlashlightInUse = true;
			if (TurnOnFlashlight)
			{
				Flashlight.FlashlightOn();
			}
		}
	}

	protected override void OnExit()
	{
		if (Flashlight == null)
		{
			return;
		}
		Flashlight.FlashlightInUse = false;
		Flashlight.FlashlightOff();
		foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
		{
			mapObject.SuppressedByFlashlight = false;
		}
	}
}
