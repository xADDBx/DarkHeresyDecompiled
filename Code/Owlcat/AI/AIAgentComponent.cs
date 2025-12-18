using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("AI/AIAgentComponent")]
[TypeId("8cc11f5e187b48f58cc454bdb5a128c5")]
public class AIAgentComponent : EntityFactComponentDelegate<MechanicEntity>, IParameterizedBehaviourTreeProvider, IBehaviourTreeProvider
{
	[SerializeField]
	private ParameterizedBehaviourTree m_ParameterizedBehaviourTree;

	public ParameterizedBehaviourTree ParameterizedBehaviourTree => m_ParameterizedBehaviourTree;

	public BehaviourTreeSerializableData BehaviourTree => ParameterizedBehaviourTree.BehaviourTree?.Get();

	protected override void OnActivateOrPostLoad()
	{
		if (BehaviourTree == null)
		{
			PFLog.AI.Error($"{base.Owner}: BehaviourTree is not set");
		}
		else
		{
			Game.Instance.Controllers.BehaviourTreeTickController.Storage.Register(base.Owner, this);
		}
	}

	protected override void OnViewDidAttach()
	{
		if (BehaviourTree != null)
		{
			Game.Instance.Controllers.BehaviourTreeTickController.Storage.RegisterViewForDebug(base.Owner, this);
		}
	}

	protected override void OnDeactivate()
	{
		Game.Instance.Controllers.BehaviourTreeTickController.Storage.UnRegister(base.Owner, this);
	}
}
