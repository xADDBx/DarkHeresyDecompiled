using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("a92a1c3c61b641ceac429a51dba8acc2")]
public class PostsAreBroken : Condition
{
	protected override string GetConditionCaption()
	{
		return "Posts are all supreme commanders";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
