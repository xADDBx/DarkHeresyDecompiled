using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("89677c030e9443d5b9fea192854d4d09")]
public abstract class AbstractEntityGroupView : EntityViewBase, IEntityGroupConfig, IEntityConfig
{
	private EntityViewBase[] _members;

	IEnumerable<string> IEntityGroupConfig.MemberIds => _members.Select((EntityViewBase i) => i.UniqueId);

	protected override void Awake()
	{
		base.Awake();
		_members = base.transform.GetComponentsInChildren<EntityViewBase>();
	}
}
