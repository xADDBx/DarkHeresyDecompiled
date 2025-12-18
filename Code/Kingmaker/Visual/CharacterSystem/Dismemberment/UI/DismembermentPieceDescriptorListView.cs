using Owlcat.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentPieceDescriptorListView : ViewBase<DismembermentPieceDescriptorListVM>
{
	public WidgetList WidgetList;

	public DismembermentPieceDescriptorView WidgetEntityView;

	protected override void BindViewImplementation()
	{
		DrawEntities();
	}

	private void DrawEntities()
	{
		WidgetList.DrawEntries(base.ViewModel.Pieces.ToArray(), WidgetEntityView);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
