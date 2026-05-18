using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.View.Spawners;

public interface IEntityGroupConfig : IEntityConfig
{
	IEnumerable<string> MemberIds { get; }
}
