using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Code.Framework.VO;

[Serializable]
public class MismatchActionEntry
{
	public GameAction Action;

	public EntityReference Entity;

	public BlueprintReference<BlueprintScriptableObject> SourceBlueprint;
}
