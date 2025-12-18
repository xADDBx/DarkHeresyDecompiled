using Kingmaker.Blueprints.Attributes;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[ComponentName("UI/AdditionalDescriptionComponent")]
[TypeId("3c5267edd045432ab5bd6dbe606d4da3")]
public class AdditionalDescriptionComponent : MechanicEntityFactComponentDelegate
{
	public LocalizedString AdditionalDescription;
}
