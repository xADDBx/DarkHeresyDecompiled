using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("0b7e8c6bec64d09469f436b66f2709de")]
public class CheckNewStatsComponent : EntityFactComponentDelegate
{
}
