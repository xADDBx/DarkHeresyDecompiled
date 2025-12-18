using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("52ded17ca9754ae995489b64825c6378")]
public class SetAnimationSet : GameAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private AnimationSetLink m_AnimationSet;

	public override string GetCaption()
	{
		return $"Set {m_Unit} AnimationSet to '{m_AnimationSet.Load().NameSafe()}'";
	}

	protected override void RunAction()
	{
		(m_Unit?.GetValue())?.GetOrCreate<UnitPartVisualChange>().SetAnimationSet(m_AnimationSet);
	}
}
