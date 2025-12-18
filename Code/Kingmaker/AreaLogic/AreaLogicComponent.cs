using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic;

[AllowedOn(typeof(BlueprintArea))]
[TypeId("d7e59305372c7174e95c74f47a54ff89")]
public abstract class AreaLogicComponent : EntityFactComponentDelegate
{
}
