using DG.Tweening;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Code.UI.MVVM.SignalDevice;
using R3;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public class SignalOwnerUnitMark : MonoBehaviour
{
	private static readonly int Color = Shader.PropertyToID("_BaseColor");

	[SerializeField]
	private Transform m_UnitMark;

	[SerializeField]
	private Material m_UnitMarkMaterial;

	[SerializeField]
	private Vector2 m_UnitMarkSizeMinMax;

	[SerializeField]
	private ParticleSystem m_Signal;

	[SerializeField]
	private float alpha = 0.5f;

	private DetectiveRadarController Controller => Game.Instance.Controllers.DetectiveRadarController;

	private SignalUISettings Settings => DetectiveClueSignalRoot.Instance.UISettings;

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(SignalFxContext.Instance.DoPulse, delegate
		{
			DoPulse();
		}).AddTo(this);
	}

	private void Update()
	{
		Color color = Settings.GetColor(Controller.SignalPowerClamped01);
		color.a = alpha;
		m_UnitMarkMaterial.SetColor(Color, color);
	}

	private void DoPulse()
	{
		if (SignalDeviceUtils.CanPulse())
		{
			DOTween.Kill(m_UnitMark);
			m_UnitMark.localScale = Vector3.one * m_UnitMarkSizeMinMax.x;
			float wavesTime = Settings.GetWavesTime(Controller.SignalPowerClamped01);
			m_UnitMark.DOScale(m_UnitMarkSizeMinMax.y, wavesTime * 0.5f).SetLoops(2, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
				.OnComplete(delegate
				{
					m_UnitMark.localScale = Vector3.one * m_UnitMarkSizeMinMax.x;
				});
			ParticleSystem.MainModule main = m_Signal.main;
			main.startColor = Settings.ColorGradient.Evaluate(Controller.SignalPowerClamped01);
			m_Signal.Stop();
			m_Signal.Play();
		}
	}
}
