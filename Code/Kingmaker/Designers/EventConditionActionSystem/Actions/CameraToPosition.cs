using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/CameraToPosition")]
[AllowMultipleComponents]
[TypeId("1a0449d4049c34149a17869dd62dc64a")]
public class CameraToPosition : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public TransformEvaluator Position;

	public bool RotateAsWell;

	public override string GetDescription()
	{
		return $"Моментально перемещает камеру в указанную позицию {Position}";
	}

	protected override void RunAction()
	{
		Transform value = Position.GetValue();
		CameraRig.Instance.ScrollTo(value.position);
		if (RotateAsWell)
		{
			CameraRig.Instance.RotateToImmediately(value.rotation.eulerAngles.y);
		}
	}

	public override string GetCaption()
	{
		return $"Camera To Position ({Position})";
	}
}
