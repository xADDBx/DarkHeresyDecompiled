using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharInfoComponentVM : ViewModel, ILevelUpManagerUIHandler, ISubscriber
{
	public readonly ReadOnlyReactiveProperty<BaseUnitEntity> Unit;

	public MechanicEntityUIWrapper UnitUIWrapper;

	protected CharInfoComponentVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
	{
		Unit = unit;
		EventBus.Subscribe(this).AddTo(this);
		Unit?.Subscribe(delegate(BaseUnitEntity descriptor)
		{
			if (descriptor != null)
			{
				UnitUIWrapper = new MechanicEntityUIWrapper(descriptor);
				RefreshData();
			}
		}).AddTo(this);
	}

	protected virtual void RefreshData()
	{
	}

	public virtual void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public virtual void HandleDestroyLevelUpManager()
	{
	}

	public virtual void HandleUISelectCareerPath()
	{
	}

	public virtual void HandleUICommitChanges()
	{
		RefreshData();
	}

	public virtual void HandleUISelectionChanged()
	{
	}
}
