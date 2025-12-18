using System;
using Kingmaker.Blueprints;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
public class MismatchEntitiesEntry
{
	public BlueprintReference<BlueprintScriptableObject> Blueprint;

	public EntityReference Entity;
}
