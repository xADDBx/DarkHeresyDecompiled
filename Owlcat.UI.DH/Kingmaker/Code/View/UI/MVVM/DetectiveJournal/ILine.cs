using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public interface ILine
{
	GameObject GameObject { get; }

	void Initialize(List<LineDirectionData> directions, bool isRefuted = false, float colorLerp = 0.5f);

	void DoUpdate();

	void SetDirection(int id, LineDirectionData direction);

	void SetDirection(int id, LineDirection direction);
}
