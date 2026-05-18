using System.Collections;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Code.Framework.PubSubSystem;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[ComponentName("Actions/FocusCombatCamera")]
[TypeId("2ba0669cd610439e9860ea44612120e3")]
public class FocusCombatCamera : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public TransformEvaluator Position;

	public CameraFollowTaskParams Params;

	public bool PauseCombatTurnOrder;

	public override string GetDescription()
	{
		return $"Камера плавно перейдет в указанную позицию {Position}";
	}

	protected override void RunAction()
	{
		Transform target = Position.GetValue();
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			float delay = Params.BlendSettings.BlendTime + Params.CameraObserveTime;
			EventBus.RaiseEvent(delegate(ICameraFocusEventHandler h)
			{
				h.HandleCameraFocusEvent(target, Params, PauseCombatTurnOrder);
			});
			FogOfWarControllerData.AddRevealer(target);
			MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(HideRevealerDelayed(delay, target));
		}
		else
		{
			CameraRig.Instance.ScrollTo(target.position);
		}
	}

	private IEnumerator HideRevealerDelayed(float delay, Transform target)
	{
		yield return new WaitForSeconds(delay);
		FogOfWarControllerData.RemoveRevealer(target);
	}

	public override string GetCaption()
	{
		return $"Camera Focus ({Position})";
	}
}
