using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EtudeCompleteTrigger")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[AllowedOn(typeof(BlueprintComponentList))]
[TypeId("b20b3c539fa60ed44abbfc7e367245ea")]
public class EtudeCompleteTrigger : EntityFactComponentDelegate, IEtudeCompleteTrigger
{
	public ActionList Actions;

	private void OnCompleteInternal()
	{
		Actions.Run();
	}

	void IEtudeCompleteTrigger.OnComplete()
	{
		OnCompleteInternal();
	}
}
