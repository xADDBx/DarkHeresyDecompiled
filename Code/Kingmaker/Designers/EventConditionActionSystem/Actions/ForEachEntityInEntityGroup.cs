using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("adf7772d3bb1143488b06d12d53d00ed")]
public class ForEachEntityInEntityGroup : GameAction
{
	[SerializeField]
	[AllowedEntityType(typeof(EntityGroupView))]
	private EntityReference m_Group;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Do Action for each Entity in Group";
	}

	protected override void RunAction()
	{
		IEntityViewBase entityViewBase = m_Group.FindView();
		if (entityViewBase == null)
		{
			return;
		}
		EntityViewBase[] componentsInChildren = entityViewBase.ViewTransform.transform.GetComponentsInChildren<EntityViewBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MechanicEntity mechanicEntity = (MechanicEntity)componentsInChildren[i].Data;
			if (mechanicEntity != null)
			{
				using (ContextData<MechanicEntityData>.Request().Setup(mechanicEntity))
				{
					Actions.Run();
				}
			}
		}
	}
}
