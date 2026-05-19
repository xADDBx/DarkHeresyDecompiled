using System;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipMapObjectInteractionPCView : OvertipMapObjectInteractionView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IGameModeHandler, ISubscriber
{
	[Header("Hint")]
	[SerializeField]
	private FadeAnimator m_HintAnimator;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	[SerializeField]
	private OvertipState TestState;

	protected override void OnBind()
	{
		OnGameModeStart(Game.Instance.CurrentModeType);
		base.OnBind();
		m_HintText.text = GetHint();
		this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			ShowHint();
		}).AddTo(this);
		this.OnPointerExitAsObservable().Subscribe(delegate
		{
			HideHint();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		HideHint();
	}

	private void ShowHint()
	{
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			m_HintAnimator.AppearAnimation();
		}
	}

	private void HideHint()
	{
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			m_HintAnimator.DisappearAnimation();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.ViewModel != null)
		{
			base.ViewModel.MapObjectEntity.View.SetGlobalHighlight(value: true);
			base.ViewModel.SetMouseOverUI(isOverUI: true);
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View.AsMapObjectView(), isHighlighted: true);
			EventBus.RaiseEvent((IMapObjectEntity)base.ViewModel.MapObjectEntity, (Action<IDirectInteractionObjectUIHandler>)delegate(IDirectInteractionObjectUIHandler h)
			{
				h.HandleObjectInteract(isOn: true);
			}, isCheckRuntime: true);
			Game.Instance.GameCommandQueue.MarkHighlightedAndNoticed(base.ViewModel.MapObjectEntity);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.ViewModel != null)
		{
			base.ViewModel.MapObjectEntity.View.SetGlobalHighlight(value: false);
			base.ViewModel.SetMouseOverUI(isOverUI: false);
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View.AsMapObjectView(), isHighlighted: false);
			EventBus.RaiseEvent((IMapObjectEntity)base.ViewModel.MapObjectEntity, (Action<IDirectInteractionObjectUIHandler>)delegate(IDirectInteractionObjectUIHandler h)
			{
				h.HandleObjectInteract(isOn: false);
			}, isCheckRuntime: true);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_Button.Interactable = gameMode == GameModeType.Default || gameMode == GameModeType.GlobalMap || gameMode == GameModeType.Pause;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private string GetHint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.ViewModel.ResourceName + "\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.HasResourceCount.Text}: {base.ViewModel.HasResourceCount}\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.RequiredResourceCount.Text}: {base.ViewModel.RequiredResourceCount}\n");
		return stringBuilder.ToString();
	}

	[ContextMenu("UpdateState")]
	private void UpdateState()
	{
		m_AppearanceStateSelectable.SetActiveLayer(TestState.ToString());
	}
}
