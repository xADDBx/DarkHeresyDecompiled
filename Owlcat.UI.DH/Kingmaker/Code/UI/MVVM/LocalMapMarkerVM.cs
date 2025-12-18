using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class LocalMapMarkerVM : ViewModel, INetPingEntity, ISubscriber
{
	private readonly ReactiveCommand<(NetPlayer player, Entity entity)> m_CoopPingEntity = new ReactiveCommand<(NetPlayer, Entity)>();

	protected readonly ReactiveProperty<bool> m_IsMapObject = new ReactiveProperty<bool>(value: false);

	protected readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>(null);

	protected readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>(value: false);

	protected readonly ReactiveProperty<Vector3> m_Position = new ReactiveProperty<Vector3>();

	protected readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> m_IsEnemy = new ReactiveProperty<bool>(value: false);

	public LocalMapMarkType MarkerType { get; protected set; } = LocalMapMarkType.Invalid;


	public ReadOnlyReactiveProperty<Vector3> Position => m_Position;

	public ReadOnlyReactiveProperty<string> Description => m_Description;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<bool> IsEnemy => m_IsEnemy;

	public ReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool> IsMapObject => m_IsMapObject;

	public Observable<(NetPlayer player, Entity entity)> CoopPingEntity => m_CoopPingEntity;

	protected LocalMapMarkerVM()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			OnUpdateHandler();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected abstract void OnUpdateHandler();

	public abstract Entity GetEntity();

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		m_CoopPingEntity.Execute((player, entity));
	}
}
