using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Actions;

[TypeId("7808a10ffaa84541984d4b6b77ea428f")]
public abstract class CompanionActionBase : GameAction
{
	[ValidateNotNull]
	[HideIf("Evaluate")]
	[SerializeField]
	private BlueprintUnitReference? m_CompanionBlueprint;

	[ShowIf("Evaluate")]
	[SerializeReference]
	public AbstractUnitEvaluator? CompanionEvaluator;

	[SerializeField]
	public bool Evaluate;

	public BlueprintUnit? CompanionBlueprint
	{
		get
		{
			return m_CompanionBlueprint?.Get();
		}
		set
		{
			m_CompanionBlueprint = value.ToReference<BlueprintUnitReference>();
		}
	}

	protected string CaptionName
	{
		get
		{
			if (!Evaluate)
			{
				return CompanionBlueprint.NameSafe();
			}
			if (CompanionEvaluator != null)
			{
				return CompanionEvaluator.GetCaption();
			}
			return "??";
		}
	}

	protected abstract void RunAction(BlueprintUnit unit);

	protected override void RunAction()
	{
		BlueprintUnit blueprintUnit = ((!Evaluate) ? CompanionBlueprint : CompanionEvaluator?.GetValue().Blueprint);
		if (blueprintUnit == null)
		{
			Element.LogError($"Unit evaluation failed for {CompanionEvaluator}");
		}
		else
		{
			RunAction(blueprintUnit);
		}
	}
}
