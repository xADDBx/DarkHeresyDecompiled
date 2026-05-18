using System;
using System.Threading.Tasks;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Framework.Interaction;

[Serializable]
public abstract class InteractionModule : IHaveCaption
{
	public bool Required = true;

	string IHaveCaption.Caption => GetCaption();

	public abstract string GetCaption();

	public abstract Task Execute(BaseUnitEntity initiator, MapObjectEntity target);

	public virtual bool CanInteract(MapObjectEntity owner)
	{
		return true;
	}

	public virtual bool CanBeSelected(BaseUnitEntity unit)
	{
		return true;
	}

	public virtual AbstractUnitEntity? GetProcessUser(BaseUnitEntity initiator)
	{
		return null;
	}
}
