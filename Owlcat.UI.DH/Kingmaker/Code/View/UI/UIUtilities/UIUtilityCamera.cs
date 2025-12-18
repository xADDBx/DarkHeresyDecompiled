using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;

namespace Kingmaker.Code.View.UI.UIUtilities;

public class UIUtilityCamera
{
	public static void ShowUnit(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			CameraRig.Instance.ScrollToImmediately(unit.Position);
		}
	}
}
