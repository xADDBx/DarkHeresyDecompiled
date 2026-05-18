using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionHeaderLevelView : View<ChargenProgressionHeaderLevelVM>
{
	private readonly Color HoveredColor = new Color(0.169f, 0.059f, 0.043f, 1f);

	private readonly Color DefaultColor = new Color(0.396f, 0.345f, 0.247f, 1f);

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private GameObject[] m_DoneStateObjects;

	[SerializeField]
	private RandomSpritesPicker m_IconPicker;

	protected override void OnBind()
	{
		base.OnBind();
		TextMeshProUGUI title = m_Title;
		int level = base.ViewModel.Level;
		title.text = level.ToString();
		m_IconPicker.Randomize(m_Title.text);
		base.ViewModel.IsCompleted.Subscribe(UpdateDoneState).AddTo(this);
		base.ViewModel.Hovered.Subscribe(UpdateHoveredState).AddTo(this);
	}

	private void UpdateDoneState(bool isDone)
	{
		GameObject[] doneStateObjects = m_DoneStateObjects;
		for (int i = 0; i < doneStateObjects.Length; i++)
		{
			doneStateObjects[i].SetActive(isDone);
		}
	}

	private void UpdateHoveredState(bool isHovered)
	{
		m_Title.color = (isHovered ? HoveredColor : DefaultColor);
	}
}
