using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("adf7772d3bb1143488b06d12d53d00ed")]
public class ForEachEntityInEntityGroup : GameAction
{
	[SerializeField]
	[AllowedEntityType(typeof(AbstractEntityGroupView))]
	private EntityReference m_Group;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Do Action for each Entity in group " + m_Group;
	}

	protected override void RunAction()
	{
		if (!(m_Group.FindData() is AbstractEntityGroup abstractEntityGroup))
		{
			return;
		}
		foreach (Entity member in abstractEntityGroup.Members)
		{
			if (member is MechanicEntity entity)
			{
				using (ContextData<MechanicEntityData>.Request().Setup(entity))
				{
					Actions.Run();
				}
			}
		}
	}
}
