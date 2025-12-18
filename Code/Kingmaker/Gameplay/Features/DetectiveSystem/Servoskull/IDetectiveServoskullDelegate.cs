using System.Threading.Tasks;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

public interface IDetectiveServoskullDelegate
{
	bool IsVisualSyncedToAgent { get; }

	Vector3 CurrentVisualPosition { get; }

	Task PlayScanAnimation(MapObjectEntity target);

	void SetScanTargetEntity(MapObjectEntity? target);
}
