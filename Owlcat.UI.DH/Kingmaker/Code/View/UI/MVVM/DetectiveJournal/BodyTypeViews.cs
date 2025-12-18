using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class BodyTypeViews : View<BodyType>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiSelectable m_MaleType;

	[SerializeField]
	private OwlcatMultiSelectable m_FemaleType;

	[SerializeField]
	private OwlcatMultiSelectable m_OtherType;

	protected override void OnBind()
	{
		m_MaleType.SetActiveLayer(GetLayerName(BodyType.Male));
		m_FemaleType.SetActiveLayer(GetLayerName(BodyType.Female));
		m_OtherType.SetActiveLayer(GetLayerName(BodyType.Other));
	}

	protected override void OnUnbind()
	{
		m_MaleType.SetActiveLayer("Off");
		m_FemaleType.SetActiveLayer("Off");
		m_OtherType.SetActiveLayer("Off");
	}

	private string GetLayerName(BodyType type)
	{
		if (base.ViewModel != type)
		{
			return "Off";
		}
		return "On";
	}
}
