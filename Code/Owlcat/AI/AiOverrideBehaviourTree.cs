using Kingmaker;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/AiOverrideBehaviourTree")]
[TypeId("fc89cb965d6937143bee436ce34948d7")]
public class AiOverrideBehaviourTree : EntityFactComponentDelegate<MechanicEntity>, IParameterizedBehaviourTreeProvider, IBehaviourTreeProvider
{
	[SerializeField]
	private ParameterizedBehaviourTree m_ParameterizedBehaviourTree;

	public ParameterizedBehaviourTree ParameterizedBehaviourTree => m_ParameterizedBehaviourTree;

	public BehaviourTreeSerializableData BehaviourTree => m_ParameterizedBehaviourTree.BehaviourTree?.Get();

	protected override void OnActivateOrPostLoad()
	{
		if (BehaviourTree == null)
		{
			PFLog.AI.Error($"{base.Owner}: BehaviourTree is not set");
			return;
		}
		PFLog.AI.Log($"Activate {BehaviourTree.Title} brain for unit {base.Owner}");
		Game.Instance.Controllers.BehaviourTreeTickController.Storage.Register(base.Owner, this);
		Game.Instance.Controllers.BehaviourTreeTickController.Storage.RegisterViewForDebug(base.Owner, this);
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
		PFLog.AI.Log(base.Owner.View.AsMechanicEntityView(), $"Deactivate {BehaviourTree.Title} brain for unit {base.Owner}");
		Game.Instance.Controllers.BehaviourTreeTickController.Storage.UnRegister(base.Owner, this);
	}
}
