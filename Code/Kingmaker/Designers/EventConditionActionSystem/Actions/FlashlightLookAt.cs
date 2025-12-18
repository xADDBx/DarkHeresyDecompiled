using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("16a553846d144b599b8180778ec46ee0")]
public class FlashlightLookAt : GameAction
{
	[SerializeField]
	[SerializeReference]
	[HideIf("_releaseLookAt")]
	private PositionEvaluator _lookAtPosition;

	[SerializeField]
	[HideIf("_releaseLookAt")]
	private Vector3 _offset;

	[SerializeField]
	private bool _releaseLookAt;

	public override string GetCaption()
	{
		if (!_releaseLookAt)
		{
			return "Flashlight look at " + _lookAtPosition?.GetCaption();
		}
		return "Release Flashlight";
	}

	protected override void RunAction()
	{
		UnitPartFlashlight unitPartFlashlight = Game.Instance.Player.MainCharacterEntity?.GetOrCreate<UnitPartFlashlight>();
		if (unitPartFlashlight != null)
		{
			if (_releaseLookAt)
			{
				unitPartFlashlight.ForcedLookAtPosition = null;
			}
			else
			{
				unitPartFlashlight.ForcedLookAtPosition = _lookAtPosition.GetValue() + _offset;
			}
		}
	}
}
