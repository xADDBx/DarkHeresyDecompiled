using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.AI;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("d5b37a0ab70d4db2acb91b47a31a5754")]
public class SetBehaviourTree : GameAction, IParameterizedBehaviourTreeProvider, IBehaviourTreeProvider
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private ParameterizedBehaviourTree m_ParameterizedBehaviourTree;

	public ParameterizedBehaviourTree ParameterizedBehaviourTree => m_ParameterizedBehaviourTree;

	public BehaviourTreeSerializableData BehaviourTree => m_ParameterizedBehaviourTree.BehaviourTree?.Get();

	public override string GetCaption()
	{
		return string.Format("Set {0} mood to '{1}'", m_Unit, BehaviourTree?.Title ?? "-not set-");
	}

	protected override void RunAction()
	{
		if (m_Unit?.GetValue() is BaseUnitEntity baseUnitEntity)
		{
			PFLog.AI.Log(baseUnitEntity.View, $"Activate {BehaviourTree.Title} brain for unit {baseUnitEntity}");
			Game.Instance.Controllers.BehaviourTreeTickController.Storage.Register(baseUnitEntity, this);
			Game.Instance.Controllers.BehaviourTreeTickController.Storage.RegisterViewForDebug(baseUnitEntity, this);
		}
	}
}
