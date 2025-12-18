using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SurfaceOvertipsVM : OvertipsVM
{
	public readonly UnitOvertipsCollectionVM UnitOvertipsCollectionVM;

	public readonly LightweightUnitOvertipsCollectionVM LightweightUnitOvertipsCollectionVM;

	public readonly MapObjectOvertipsVM MapObjectOvertipsVM;

	public SurfaceOvertipsVM()
	{
		UnitOvertipsCollectionVM = new UnitOvertipsCollectionVM().AddTo(this);
		LightweightUnitOvertipsCollectionVM = new LightweightUnitOvertipsCollectionVM().AddTo(this);
		MapObjectOvertipsVM = new MapObjectOvertipsVM().AddTo(this);
	}

	public override void HandleOnShowBark(string text)
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (!(entity is LightweightUnitEntity entity2))
		{
			if (entity is AbstractUnitEntity entity3)
			{
				UnitOvertipsCollectionVM.ShowBark(entity3, text);
			}
			else
			{
				MapObjectOvertipsVM.ShowBark(entity, text);
			}
		}
		else
		{
			LightweightUnitOvertipsCollectionVM.ShowBark(entity2, text);
		}
	}

	public override void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
	}

	public override void HandleOnHideBark()
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (!(entity is LightweightUnitEntity entity2))
		{
			if (entity is AbstractUnitEntity entity3)
			{
				UnitOvertipsCollectionVM.HideBark(entity3);
			}
			else
			{
				MapObjectOvertipsVM.HideBark(entity);
			}
		}
		else
		{
			LightweightUnitOvertipsCollectionVM.HideBark(entity2);
		}
	}
}
