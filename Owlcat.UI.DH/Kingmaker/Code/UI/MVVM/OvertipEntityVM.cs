using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipEntityVM : ViewModel, INetPingEntity, ISubscriber
{
	private readonly ReactiveCommand<(NetPlayer player, Entity entity)> m_CoopPingEntity = new ReactiveCommand<(NetPlayer, Entity)>();

	public float OvertipVerticalCorrection;

	public Vector3 Position { get; private set; }

	public bool IsCutscene => Game.Instance.CurrentModeType == GameModeType.Cutscene;

	public bool IsInDialog => Game.Instance.CurrentModeType == GameModeType.Dialog;

	public bool IsPreciseAttack => Game.Instance.Controllers.PreciseAttackController.HasTarget;

	public bool IsInCombatEndWindow => RootVM.Instance.HUDContext.CombatEndWindowVM.CurrentValue != null;

	protected virtual bool UpdateEnabled => true;

	public Observable<(NetPlayer player, Entity entity)> CoopPingEntity => m_CoopPingEntity;

	protected OvertipEntityVM()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate), delegate
		{
			InternalUpdate();
		}).AddTo(this);
	}

	protected abstract Vector3 GetEntityPosition();

	private void InternalUpdate()
	{
		if (Application.isPlaying && UpdateEnabled)
		{
			OnUpdateHandler();
		}
	}

	protected virtual void OnUpdateHandler()
	{
		Position = GetEntityPosition();
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		m_CoopPingEntity.Execute((player, entity));
	}
}
