using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Levelup.Components;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("5474d502f4f54412b9cd7e7bbd0ddeec")]
public class CharacterLevelLimit : BlueprintComponent
{
	public int LevelLimit;
}
