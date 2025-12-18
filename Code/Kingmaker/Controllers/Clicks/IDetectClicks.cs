using UnityEngine;

namespace Kingmaker.Controllers.Clicks;

public interface IDetectClicks
{
	GameObject Target { get; }

	void HandleClick();
}
