using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickEventVM : TooltipBaseBrickVM
{
	public readonly string EventName;

	public readonly string EventDescription;

	public readonly EventRelationType Type;

	public TooltipBrickEventVM(BlueprintColonyEvent @event, EventRelationType type)
	{
		EventName = @event.Name;
		EventDescription = @event.Description;
		Type = type;
	}
}
