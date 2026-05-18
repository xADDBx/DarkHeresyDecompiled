using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipsContext : ViewModel, IVariativeInteractionUIHandler, ISubscriber
{
	private readonly ReactiveProperty<VariativeInteractionVM> m_VariativeInteractionVM;

	private readonly ReactiveProperty<SurfaceOvertipsVM> m_SurfaceOvertipsVM;

	private readonly ReactiveProperty<PointMarkersVM> m_PointMarkersVM;

	public MechanicEntity OpenedVariantsMapEntity => m_VariativeInteractionVM.Value?.MechanicEntity;

	public OvertipsContext(ReactiveProperty<VariativeInteractionVM> variativeInteractionVM, ReactiveProperty<SurfaceOvertipsVM> surfaceOvertipsVM, ReactiveProperty<PointMarkersVM> pointMarkersVM)
	{
		m_VariativeInteractionVM = variativeInteractionVM;
		m_SurfaceOvertipsVM = surfaceOvertipsVM;
		m_PointMarkersVM = pointMarkersVM;
		CreateOvertips();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleInteractionRequest(MechanicEntity mechanicEntity, IEnumerable<InteractionActorWithConditions> actors)
	{
		if (m_VariativeInteractionVM.Value != null && m_VariativeInteractionVM.Value.MechanicEntity == mechanicEntity)
		{
			DisposeVariativeInteraction();
		}
		else
		{
			m_VariativeInteractionVM.Value = new VariativeInteractionVM(mechanicEntity, actors, DisposeVariativeInteraction);
		}
	}

	public void HandleInteractionRequest(InteractionVariativePart interactionPart)
	{
		HandleInteractionRequest(interactionPart.Owner, interactionPart.InteractionSettings.Interactions.Select((InteractionWithConditions c) => c.ToActorWithConditions()));
	}

	protected override void OnDispose()
	{
		DisposeOvertips();
	}

	private void DisposeVariativeInteraction()
	{
		m_VariativeInteractionVM.Value?.Dispose();
		m_VariativeInteractionVM.Value = null;
	}

	private void CreateOvertips()
	{
		DisposeOvertips();
		m_SurfaceOvertipsVM.Value = new SurfaceOvertipsVM();
		m_PointMarkersVM.Value = new PointMarkersVM();
	}

	private void DisposeOvertips()
	{
		m_SurfaceOvertipsVM.Value?.Dispose();
		m_SurfaceOvertipsVM.Value = null;
		m_PointMarkersVM.Value?.Dispose();
		m_PointMarkersVM.Value = null;
	}
}
