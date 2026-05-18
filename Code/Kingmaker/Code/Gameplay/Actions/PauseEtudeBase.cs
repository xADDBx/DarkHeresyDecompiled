using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[TypeId("4368620fa0ef4a06b0d3c1d27ffef7dd")]
public abstract class PauseEtudeBase : GameAction
{
	[HideIf("Evaluate")]
	public BlueprintEtudeReference? Etude;

	[ShowIf("Evaluate")]
	[SerializeReference]
	public BlueprintEvaluator? EtudeEvaluator;

	public bool Evaluate;

	protected BlueprintEtude? Blueprint
	{
		get
		{
			if (!Evaluate)
			{
				return Etude?.Get();
			}
			return EtudeEvaluator?.GetValue() as BlueprintEtude;
		}
	}

	protected string Name => ((!Evaluate) ? Etude?.Get()?.NameSafe() : EtudeEvaluator?.GetCaption()) ?? "??";
}
