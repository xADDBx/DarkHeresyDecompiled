using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuConsoleView : ContextMenuView
{
	private ReadOnlyReactiveProperty<ContextMenuEntityConsoleView> m_CurrentEntity;

	public static readonly string InputLayerContextName = "ContextMenu";

	protected override void OnBind()
	{
		base.OnBind();
		CreateInputs();
	}

	private void CreateInputs()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(0, 1));
		m_CurrentEntity = gridConsoleNavigationBehaviour.DeepestFocusAsObservable.Select((IConsoleEntity e) => e as ContextMenuEntityConsoleView).ToReadOnlyReactiveProperty();
		List<ContextMenuEntityConsoleView> entitiesVertical = (from e in m_Entities
			select e as ContextMenuEntityConsoleView into e
			where e != null
			select e).ToList();
		gridConsoleNavigationBehaviour.SetEntitiesVertical(entitiesVertical);
		InputLayer inputLayer = gridConsoleNavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		inputLayer.AddButton(Close, 9);
		inputLayer.AddButton(ConfirmCurrentEntity, 8);
		GamePad.Instance.PushLayer(inputLayer).AddTo(this);
		gridConsoleNavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void ConfirmCurrentEntity(InputActionEventData data)
	{
		m_CurrentEntity.CurrentValue.Or(null)?.OnConfirm();
		Close(data);
	}

	private void Close(InputActionEventData data)
	{
		ContextMenuHelper.HideContextMenu();
	}
}
