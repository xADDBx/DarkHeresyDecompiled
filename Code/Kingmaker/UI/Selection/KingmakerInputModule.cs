using Kingmaker.UI.Common;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Selection;

public class KingmakerInputModule : StandaloneInputModule
{
	private static KingmakerInputModule s_Instance;

	public static bool ScrollIsBusy
	{
		get
		{
			if (s_Instance == null)
			{
				return true;
			}
			return GetPointerEventData().HoverAt<IScrollHandler>();
		}
	}

	public static PointerEventData GetPointerEventData(int pointerId = -1)
	{
		s_Instance.GetPointerData(pointerId, out var data, create: true);
		return data;
	}

	protected override void Awake()
	{
		base.Awake();
		s_Instance = this;
	}

	protected override void Start()
	{
		base.Start();
	}
}
