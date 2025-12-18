using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.InputSystems;
using R3;
using UnityEngine;

namespace Code.View.UI.MVVM.Dialog;

public class DetectiveEpilogPCView : DetectiveEpilogBaseView, IHasBlueprintInfo
{
	[SerializeField]
	private List<DetectivePaperView> m_Papers = new List<DetectivePaperView>();

	public BlueprintScriptableObject Blueprint => Game.Instance.Controllers.DialogController.Dialog;

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			if (!base.IsShowHistory.CurrentValue)
			{
				EventBus.RaiseEvent(delegate(IEscMenuHandler h)
				{
					h.HandleOpen();
				});
			}
		}).AddTo(this);
		m_Papers.ForEach(delegate(DetectivePaperView p)
		{
			p.Bind(default(Unit));
		});
	}
}
