using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipPointBlockPCView : View<OvertipPointBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ActivePointsText;

	[SerializeField]
	private TextMeshProUGUI m_MovePointsText;

	protected override void OnBind()
	{
		base.ViewModel.ActionPoints.Subscribe(delegate(float value)
		{
			m_ActivePointsText.text = $"{value} ap";
		}).AddTo(this);
		base.ViewModel.MovePoints.Subscribe(delegate(float value)
		{
			m_MovePointsText.text = $"{value} mp";
		}).AddTo(this);
		base.ViewModel.NeedToShow.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
		}).AddTo(this);
	}
}
