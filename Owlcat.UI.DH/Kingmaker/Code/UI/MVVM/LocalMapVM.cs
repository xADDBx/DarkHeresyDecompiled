using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.LocalMap;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapVM : ViewModel, INetPingPosition, ISubscriber, IServiceWindow
{
	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	private readonly ReactiveProperty<WarhammerLocalMapRenderer.DrawResults> m_DrawResult = new ReactiveProperty<WarhammerLocalMapRenderer.DrawResults>();

	private readonly ReactiveProperty<float> m_CompassAngle = new ReactiveProperty<float>(0f);

	private readonly ReactiveProperty<bool> m_CanOpenMap = new ReactiveProperty<bool>();

	public readonly LocalMapLegendBlockVM LocalMapLegendBlockVM;

	public readonly ObservableList<LocalMapMarkerVM> MarkersVm = new ObservableList<LocalMapMarkerVM>();

	public readonly ReactiveProperty<bool> SwitchedFromServiceWindow = new ReactiveProperty<bool>();

	private Action m_Close;

	private readonly ReactiveCommand<(NetPlayer, Vector3)> m_CoopPingPosition = new ReactiveCommand<(NetPlayer, Vector3)>();

	private static readonly int LocalMapColorTex = Shader.PropertyToID("_LocalMapColorTex");

	private static readonly int LocalMapFowScaleOffset = Shader.PropertyToID("LocalMapFowScaleOffset");

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public ReadOnlyReactiveProperty<WarhammerLocalMapRenderer.DrawResults> DrawResult => m_DrawResult;

	public ReadOnlyReactiveProperty<float> CompassAngle => m_CompassAngle;

	public ReadOnlyReactiveProperty<bool> CanOpenMap => m_CanOpenMap;

	public BlueprintAreaPart.LocalMapRotationDegree LocalMapRotation => Game.Instance.CurrentlyLoadedAreaPart.LocalMapRotationDeg;

	public float ZoomMin => Game.Instance.CurrentlyLoadedAreaPart.LocalMapZoomMin;

	public float ZoomMax => Game.Instance.CurrentlyLoadedAreaPart.LocalMapZoomMax;

	public Observable<(NetPlayer, Vector3)> CoopPingPosition => m_CoopPingPosition;

	public LocalMapVM()
	{
		Game.Instance.Player.UISettings.IsLocalMapOpen = true;
		EventBus.RaiseEvent(delegate(IFullScreenLocalMapUIHandler h)
		{
			h.HandleFullScreenLocalMapChanged(state: true);
		});
		m_Title.Value = Game.Instance.State.LoadedAreaState.Area.Blueprint.AreaDisplayName;
		SetDrawResult();
		SetMarkers();
		LocalMapLegendBlockVM = new LocalMapLegendBlockVM().AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			OnUpdateHandler();
		}).AddTo(this);
		m_CanOpenMap.Value = Game.Instance.LoadedAreaState.Settings.DisableLocalMap;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		Game.Instance.Player.UISettings.IsLocalMapOpen = false;
		MarkersVm.ForEach(delegate(LocalMapMarkerVM m)
		{
			m.Dispose();
		});
		MarkersVm.Clear();
		EventBus.RaiseEvent(delegate(IFullScreenLocalMapUIHandler h)
		{
			h.HandleFullScreenLocalMapChanged(state: false);
		});
	}

	private void SetDrawResult()
	{
		if (!(WarhammerLocalMapRenderer.Instance == null))
		{
			WarhammerLocalMapRenderer.DrawResults value = WarhammerLocalMapRenderer.Instance.Draw();
			m_DrawResult.Value = value;
			Shader.SetGlobalTexture(LocalMapColorTex, value.ColorRT);
			Shader.SetGlobalVector(LocalMapFowScaleOffset, value.LocalMapFowScaleOffset);
		}
	}

	private void SetMarkers()
	{
		foreach (ILocalMapMarker mapObjectMarker in LocalMapModel.MapObjectMarkers)
		{
			MarkersVm.Add(new LocalMapCommonMarkerVM(mapObjectMarker).AddTo(this));
		}
		foreach (BaseUnitEntity partyForMarker in LocalMapModel.PartyForMarkers)
		{
			MarkersVm.Add(new LocalMapCharacterMarkerVM(partyForMarker).AddTo(this));
			MarkersVm.Add(new LocalMapDestinationMarkerVM(partyForMarker).AddTo(this));
		}
		foreach (UnitGroupMemory.UnitInfo validatedNonPartyUnitsForMarker in LocalMapModel.ValidatedNonPartyUnitsForMarkers)
		{
			LocalMapMarkerPart optional = validatedNonPartyUnitsForMarker.Unit.Parts.GetOptional<LocalMapMarkerPart>();
			LocalMapMarkType? localMapMarkType = optional?.GetMarkerType();
			LocalMapMarkerVM item = ((localMapMarkType.HasValue && localMapMarkType.GetValueOrDefault() == LocalMapMarkType.PointOfInterest) ? ((LocalMapMarkerVM)new LocalMapCommonMarkerVM(optional).AddTo(this)) : ((LocalMapMarkerVM)new LocalMapUnitMarkerVM(validatedNonPartyUnitsForMarker).AddTo(this)));
			MarkersVm.Add(item);
		}
	}

	private void OnUpdateHandler()
	{
		m_DrawResult.Value = WarhammerLocalMapRenderer.Instance.Draw();
		CameraRig instance = CameraRig.Instance;
		m_CompassAngle.Value = instance.transform.eulerAngles.y - (float)LocalMapRotation;
		List<LocalMapUnitMarkerVM> source = MarkersVm.OfType<LocalMapUnitMarkerVM>().ToList();
		List<UnitGroupMemory.UnitInfo> list = new List<UnitGroupMemory.UnitInfo>();
		foreach (UnitGroupMemory.UnitInfo unit in LocalMapModel.PossibleNonPartyUnitsForMarkers)
		{
			if (!LocalMapModel.CheckValidForLocalMap(unit.Unit))
			{
				list.Add(unit);
			}
			else if (source.FirstOrDefault((LocalMapUnitMarkerVM vm) => vm.UnitInfo == unit) == null)
			{
				MarkersVm.Add(new LocalMapUnitMarkerVM(unit).AddTo(this));
			}
		}
		for (int i = 0; i < MarkersVm.Count; i++)
		{
			if (MarkersVm[i] is LocalMapUnitMarkerVM localMapUnitMarkerVM && list.Contains(localMapUnitMarkerVM.UnitInfo))
			{
				MarkersVm[i].Dispose();
				MarkersVm.RemoveAt(i);
			}
		}
		list.Clear();
	}

	public void OnClick(Vector2 viewPortPos, bool state, Entity entity = null, bool canPing = true)
	{
		Vector3 worldPos = WarhammerLocalMapRenderer.Instance.ViewportToWorldPoint(viewPortPos);
		if (!LocalMapModel.IsInCurrentArea(worldPos))
		{
			worldPos = Game.Instance.CurrentlyLoadedAreaPart.Bounds.LocalMapBounds.ClosestPoint(worldPos);
		}
		if (!canPing || !PhotonManager.Ping.CheckPingCoop(delegate
		{
			if (entity != null)
			{
				PhotonManager.Ping.PingEntity(entity);
			}
			else
			{
				PhotonManager.Ping.PingPosition(worldPos);
			}
		}))
		{
			if (state)
			{
				CameraRig.Instance.ScrollTo(worldPos);
				return;
			}
			UnitCommandsRunner.MoveSelectedUnitsToPoint(worldPos);
			Game.Instance.Controllers.CameraController.Follower.Follow();
		}
	}

	public void ScrollCameraToRogueTrader()
	{
		CameraRig.Instance.ScrollTo(Game.Instance.Player.MainCharacter.Entity.Position);
	}

	public void HandlePingPosition(NetPlayer player, Vector3 position)
	{
		m_CoopPingPosition.Execute((player, position));
	}

	public void HandlePingPositionSound(GameObject gameObject)
	{
	}

	public void HandleOnSwitchedFromWindow()
	{
		SwitchedFromServiceWindow.Value = true;
	}
}
