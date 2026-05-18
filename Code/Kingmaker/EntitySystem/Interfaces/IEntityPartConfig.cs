using System;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IEntityPartConfig
{
	Type EntityPartType { get; }

	string EntityId { get; }

	object GetSettings();
}
