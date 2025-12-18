using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[AllowedOn(typeof(BlueprintEtude))]
[TypeId("d15e4f718feb4d3faa36f8f6d2e98ffa")]
public sealed class EtudeBracketDisableDetectiveServoskull : EtudeBracketTrigger
{
	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.DisableDetectiveServoskull.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.DisableDetectiveServoskull.Release();
	}
}
