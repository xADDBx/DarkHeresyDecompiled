using Kingmaker.Code.View.Bridge.Root;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CommonBaseView : ViewBase<CommonVM>, IInitializable
{
	protected InputLayer SurfaceBaseInputLayer;

	protected SurfaceMainInputLayer SurfaceMainInputLayer;

	protected SurfaceCombatInputLayer SurfaceCombatInputLayer;

	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		SurfaceBaseInputLayer = new InputLayer
		{
			ContextName = "SurfaceBaseInputLayer"
		};
		CreateBaseInputImpl(SurfaceBaseInputLayer);
		GamePad.Instance.SetBaseLayer(SurfaceBaseInputLayer);
		SurfaceMainInputLayer = new SurfaceMainInputLayer
		{
			ContextName = "SurfaceMainInputLayer"
		};
		CreateMainInputImpl(SurfaceMainInputLayer);
		GamePad.Instance.PushLayer(SurfaceMainInputLayer);
		AddDisposable(base.ViewModel.IsCombatInputModeActive.Subscribe(delegate(bool isActive)
		{
			if (isActive)
			{
				ActivateCombatInputLayer();
			}
			else
			{
				DeactivateCombatInputLayer();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		GamePad.Instance.SetBaseLayer(null);
		GamePad.Instance.PopLayer(SurfaceMainInputLayer);
		SurfaceBaseInputLayer = null;
		SurfaceMainInputLayer.Dispose();
		SurfaceMainInputLayer = null;
		DeactivateCombatInputLayer();
	}

	private void ActivateCombatInputLayer()
	{
		SurfaceCombatInputLayer = new SurfaceCombatInputLayer
		{
			ContextName = "SurfaceCombatInputLayer"
		};
		CreateCombatInputImpl(SurfaceCombatInputLayer);
		GamePad.Instance.PushLayer(SurfaceCombatInputLayer);
	}

	private void DeactivateCombatInputLayer()
	{
		if (SurfaceCombatInputLayer != null)
		{
			GamePad.Instance.PopLayer(SurfaceCombatInputLayer);
			SurfaceCombatInputLayer.Dispose();
			SurfaceCombatInputLayer = null;
		}
	}

	protected virtual void CreateBaseInputImpl(InputLayer baseInputLayer)
	{
	}

	protected virtual void CreateMainInputImpl(InputLayer mainInputLayer)
	{
	}

	protected virtual void CreateCombatInputImpl(InputLayer combatInputLayer)
	{
	}
}
