using Kingmaker.Visual;
using UnityEngine;

public class RunningBackgroundCameraShake : CameraShakeFx
{
	private RunningBackgroundSettings _settings;

	private float _currentSpeed;

	private float _noiseSeedX;

	private float _noiseSeedY;

	private float _impulseAmplitude;

	private float _impulseDuration;

	private float _impulseTimer;

	public void Setup(RunningBackgroundSettings settings)
	{
		_settings = settings;
		_noiseSeedX = Random.value * 1000f;
		_noiseSeedY = Random.value * 1000f;
	}

	public void SetSpeed(float speed)
	{
		_currentSpeed = speed;
	}

	public void Impulse(float amplitudeMultiplier, float duration)
	{
		_impulseAmplitude = amplitudeMultiplier;
		_impulseDuration = duration;
		_impulseTimer = 0f;
	}

	public override Vector2 CalculateDelta(Vector3 camPos)
	{
		if (_settings == null || !_settings.enableCameraShake)
		{
			return Vector2.zero;
		}
		float time = ((_settings.shakeMaxSpeed > 0f) ? Mathf.Clamp01(_currentSpeed / _settings.shakeMaxSpeed) : 1f);
		float num = _settings.shakeBySpeed.Evaluate(time);
		float num2 = 0f;
		float num3 = 0f;
		float num4 = Time.time * _settings.ambientFrequency;
		float num5 = _settings.ambientAmplitude * num;
		num2 += (Mathf.PerlinNoise(num4 + _noiseSeedX, _noiseSeedY) - 0.5f) * 2f * num5;
		num3 += (Mathf.PerlinNoise(_noiseSeedX, num4 + _noiseSeedY) - 0.5f) * 2f * num5;
		float num6 = Time.time * _settings.periodicFrequency;
		float time2 = ((_settings.periodicCycleDuration > 0f) ? (Time.time % _settings.periodicCycleDuration / _settings.periodicCycleDuration) : 1f);
		float num7 = _settings.periodicCycleEnvelope.Evaluate(time2);
		float num8 = _settings.periodicAmplitude * num * num7;
		num2 += (Mathf.PerlinNoise(num6 + _noiseSeedX + 300f, _noiseSeedY + 300f) - 0.5f) * 2f * num8;
		num3 += (Mathf.PerlinNoise(_noiseSeedX + 300f, num6 + _noiseSeedY + 300f) - 0.5f) * 2f * num8;
		if (_impulseTimer < _impulseDuration)
		{
			_impulseTimer += Time.deltaTime;
			float num9 = 1f - Mathf.Clamp01(_impulseTimer / _impulseDuration);
			num2 += (Mathf.PerlinNoise(num6 * 2f + _noiseSeedX + 500f, _noiseSeedY) - 0.5f) * 2f * _impulseAmplitude * num9;
			num3 += (Mathf.PerlinNoise(_noiseSeedX, num6 * 2f + _noiseSeedY + 500f) - 0.5f) * 2f * _impulseAmplitude * num9;
		}
		return new Vector2(num2, num3);
	}
}
