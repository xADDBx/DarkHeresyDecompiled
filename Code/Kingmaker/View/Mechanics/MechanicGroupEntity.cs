using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View.Spawners;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.View.Mechanics;

[HashNoGenerate]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicGroupEntity : AbstractEntityGroup, IOwlPackable<MechanicGroupEntity>
{
	protected MechanicGroupEntity(IEntityGroupConfig config)
		: base(config)
	{
	}

	protected MechanicGroupEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		foreach (Entity member in base.Members)
		{
			member.IsInGame = base.IsInGame;
		}
		if (base.IsInGame)
		{
			OnActivate();
		}
		else
		{
			OnDeactivate();
		}
	}

	protected override void OnSetConfig(IEntityConfig entityConfig)
	{
		base.OnSetConfig(entityConfig);
		foreach (Entity member in base.Members)
		{
			member.IsInGame = base.IsInGame;
		}
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnDeactivate()
	{
	}
}
