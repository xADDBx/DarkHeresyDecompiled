using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.UnitLogic.Buffs;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SurfaceCombatMechanicEntityActionListVM : ViewModel
{
	public ObservableList<SurfaceCombatActionVM> Actions = new ObservableList<SurfaceCombatActionVM>();

	public ChannelingLogic.InitiativeHolder InitiativeHolder { get; }

	private Buff ChannelingBuff { get; }

	public SurfaceCombatMechanicEntityActionListVM(ChannelingLogic.InitiativeHolder initiativeHolder)
	{
		InitiativeHolder = initiativeHolder;
		ChannelingBuff = InitiativeHolder?.Buff;
		TryAddActionVM(ChannelingBuff);
	}

	private void TryAddActionVM(Buff buff)
	{
		if (buff != null)
		{
			Actions.Add(new SurfaceCombatActionVM(buff).AddTo(this));
		}
	}
}
