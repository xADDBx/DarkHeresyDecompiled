using Kingmaker.EntitySystem.Entities;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BaseOvertipMapObjectVM : OvertipEntityVM
{
	public readonly MapObjectEntity MapObjectEntity;

	protected readonly ReactiveProperty<bool> m_IsEnabled = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> m_MapObjectIsHighlighted = new ReactiveProperty<bool>();

	protected readonly ReactiveCommand<Unit> m_VisibilityChanged = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_IsMouseOverUI = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasSurrounding = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsChosen = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsEnabled => m_IsEnabled;

	public ReadOnlyReactiveProperty<bool> MapObjectIsHighlighted => m_MapObjectIsHighlighted;

	public ReadOnlyReactiveProperty<bool> IsMouseOverUI => m_IsMouseOverUI;

	public Observable<Unit> VisibilityChanged => m_VisibilityChanged;

	public ReadOnlyReactiveProperty<bool> HasSurrounding => m_HasSurrounding;

	public ReadOnlyReactiveProperty<bool> IsChosen => m_IsChosen;

	public bool HideFromScreen
	{
		get
		{
			if (MapObjectEntity.IsRevealed && MapObjectEntity.IsVisibleForPlayer && MapObjectEntity.IsInState && MapObjectEntity.IsInGame)
			{
				return MapObjectEntity.IsInFogOfWar;
			}
			return true;
		}
	}

	public bool IsInCameraFrustum => MapObjectEntity.IsInCameraFrustum;

	protected BaseOvertipMapObjectVM(MapObjectEntity mapObjectEntity)
	{
		MapObjectEntity = mapObjectEntity;
		IsEnabled.CombineLatest(MapObjectIsHighlighted, IsMouseOverUI, (bool isEnabled, bool hover, bool mouseOver) => new { isEnabled, hover, mouseOver }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(_ =>
		{
			m_VisibilityChanged.Execute();
		})
			.AddTo(this);
	}

	protected override Vector3 GetEntityPosition()
	{
		return Vector3.zero;
	}

	public void HandleSurroundingObjectsChanged(bool isInNavigation, bool isChosen)
	{
		m_HasSurrounding.Value = isInNavigation;
		m_IsChosen.Value = isChosen;
	}

	public void SetMouseOverUI(bool isOverUI)
	{
		m_IsMouseOverUI.Value = isOverUI;
	}
}
