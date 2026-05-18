using Kingmaker.DialogSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("8f6c7829dcc6428bb0256f3572955519")]
public class SetMood : GameAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private Mood m_Mood;

	public override string GetCaption()
	{
		return $"Set {m_Unit} mood to '{m_Mood}'";
	}

	protected override void RunAction()
	{
		if (m_Unit?.GetValue() is BaseUnitEntity entity)
		{
			entity.SetMood(m_Mood);
		}
	}
}
