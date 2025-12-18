using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.UI.Events;

public interface IMoveToCaseItemHandler : ISubscriber
{
	void HandleMoveToCaseItem(BlueprintCaseItem caseItem);

	void HandleMoveToItemPosition(Vector3 position);
}
