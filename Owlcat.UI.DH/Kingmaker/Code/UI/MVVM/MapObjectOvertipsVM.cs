using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class MapObjectOvertipsVM : ViewModel
{
	public readonly TransitionOvertipsCollectionVM TransitionOvertipsCollectionVM;

	public readonly MapInteractionObjectOvertipsCollectionVM MapInteractionObjectOvertipsCollectionVM;

	public readonly DestructibleObjectOvertipsCollectionVM DestructibleObjectOvertipsCollectionVM;

	public MapObjectOvertipsVM()
	{
		TransitionOvertipsCollectionVM = new TransitionOvertipsCollectionVM().AddTo(this);
		MapInteractionObjectOvertipsCollectionVM = new MapInteractionObjectOvertipsCollectionVM().AddTo(this);
		DestructibleObjectOvertipsCollectionVM = new DestructibleObjectOvertipsCollectionVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void ShowBark(Entity entity, string text)
	{
		MapInteractionObjectOvertipsCollectionVM.ShowBark(entity, text);
		DestructibleObjectOvertipsCollectionVM.ShowBark(entity, text);
	}

	public void HideBark(Entity entity)
	{
		MapInteractionObjectOvertipsCollectionVM.HideBark(entity);
		DestructibleObjectOvertipsCollectionVM.HideBark(entity);
	}
}
