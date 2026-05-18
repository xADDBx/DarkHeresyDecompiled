using System.Collections;
using Dreamteck.Splines;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Visual.ConveyorBelt;

public class ConveyorBeltController : MonoBehaviour
{
	private const float MinSplineLength = 0.001f;

	[Header("Belt References")]
	[SerializeField]
	private ObjectController _beltLoopController;

	[SerializeField]
	private SplineComputer _deliverySpline;

	[Header("Belt Animation")]
	[SerializeField]
	private float _beltSpeed = 2f;

	[SerializeField]
	private float _accelerationTime = 0.5f;

	[SerializeField]
	private float _decelerationTime = 1f;

	[Header("Crate Delivery")]
	[SerializeField]
	private float _crateDeliveryDuration = 2f;

	[SerializeField]
	private Vector3 _deliveryRotationOffset;

	[Header("Sound")]
	[AkEventReference]
	[SerializeField]
	private string _beltStartEvent;

	[AkEventReference]
	[SerializeField]
	private string _beltLoopEvent;

	[AkEventReference]
	[SerializeField]
	private string _beltStopEvent;

	[AkEventReference]
	[SerializeField]
	private string _crateArriveEvent;

	[Header("Crates (to duplicate to belt)")]
	[SerializeField]
	private GameObject[] _crates;

	private bool _isRunning;

	private bool _initialized;

	private bool _initFailLogged;

	private float _currentSpeed;

	private Coroutine _speedCoroutine;

	private Coroutine _deliveryCoroutine;

	private int _currentCrateIndex = -1;

	private GameObject _tempDeliveryVisual;

	private float _beltSplineLength;

	private bool TryInitialize()
	{
		if (_initialized)
		{
			return true;
		}
		if (!ResolveBeltReference())
		{
			return false;
		}
		_initialized = true;
		return true;
	}

	private bool ResolveBeltReference()
	{
		if (_beltLoopController == null || _deliverySpline == null)
		{
			if (!_initFailLogged)
			{
				_initFailLogged = true;
				if (_beltLoopController == null)
				{
					UnityEngine.Debug.LogError("[ConveyorBelt] Belt loop controller is not assigned");
				}
				if (_deliverySpline == null)
				{
					UnityEngine.Debug.LogError("[ConveyorBelt] Delivery spline is not assigned");
				}
			}
			return false;
		}
		_beltSplineLength = ((_beltLoopController.spline != null) ? _beltLoopController.spline.CalculateLength() : 1f);
		return true;
	}

	private void Update()
	{
		if (TryInitialize() && _isRunning)
		{
			ScrollBelt();
		}
	}

	public void DeliverCrate(int crateIndex)
	{
		if (!TryInitialize())
		{
			UnityEngine.Debug.LogError("[ConveyorBelt] Cannot deliver — not initialized");
			return;
		}
		if (crateIndex < 0 || crateIndex >= _crates.Length || _crates[crateIndex] == null)
		{
			UnityEngine.Debug.LogError($"[ConveyorBelt] Invalid crate index {crateIndex} (array length={_crates?.Length})");
			return;
		}
		if (_deliveryCoroutine != null)
		{
			StopCoroutine(_deliveryCoroutine);
			CleanupDelivery();
		}
		_deliveryCoroutine = StartCoroutine(DeliverCrateSequence(crateIndex));
	}

	private void CleanupDelivery()
	{
		if (_tempDeliveryVisual != null)
		{
			Object.Destroy(_tempDeliveryVisual);
			_tempDeliveryVisual = null;
		}
		if (_currentCrateIndex >= 0 && _currentCrateIndex < _crates.Length && _crates[_currentCrateIndex] != null)
		{
			_crates[_currentCrateIndex].SetActive(value: true);
		}
		RequestBeltStop();
	}

	private IEnumerator DeliverCrateSequence(int newIndex)
	{
		GameObject gameObject = _crates[newIndex];
		PostSound(_beltStartEvent, base.gameObject);
		PostSound(_beltLoopEvent, base.gameObject);
		RequestBeltStart();
		_tempDeliveryVisual = Object.Instantiate(gameObject);
		_tempDeliveryVisual.name = "ConveyorBelt_Delivery_" + gameObject.name;
		_tempDeliveryVisual.SetActive(value: true);
		SplineFollower splineFollower = _tempDeliveryVisual.AddComponent<SplineFollower>();
		splineFollower.spline = _deliverySpline;
		splineFollower.followMode = SplineFollower.FollowMode.Time;
		splineFollower.motion.rotationOffset = _deliveryRotationOffset;
		if (_deliverySpline.CalculateLength() < 0.001f)
		{
			UnityEngine.Debug.LogError("[ConveyorBelt] Delivery spline has zero length");
			Object.Destroy(_tempDeliveryVisual);
			_tempDeliveryVisual = null;
			RequestBeltStop();
			_deliveryCoroutine = null;
			yield break;
		}
		splineFollower.followDuration = _crateDeliveryDuration;
		splineFollower.wrapMode = SplineFollower.Wrap.Default;
		splineFollower.follow = true;
		splineFollower.Restart();
		yield return new WaitForSeconds(_crateDeliveryDuration);
		Object.Destroy(_tempDeliveryVisual);
		_tempDeliveryVisual = null;
		_currentCrateIndex = newIndex;
		RequestBeltStop();
		_deliveryCoroutine = null;
	}

	private void ScrollBelt()
	{
		if (!(_beltLoopController == null) && !(_beltSplineLength < 0.001f))
		{
			float num = _beltLoopController.evaluateOffset + _currentSpeed * Time.deltaTime / _beltSplineLength;
			_beltLoopController.evaluateOffset = num - Mathf.Floor(num);
		}
	}

	private void RequestBeltStart()
	{
		if (_speedCoroutine != null)
		{
			StopCoroutine(_speedCoroutine);
		}
		_isRunning = true;
		_speedCoroutine = StartCoroutine(LerpBeltSpeed(_currentSpeed, _beltSpeed, _accelerationTime));
	}

	private void RequestBeltStop()
	{
		if (_speedCoroutine != null)
		{
			StopCoroutine(_speedCoroutine);
		}
		_speedCoroutine = StartCoroutine(LerpBeltSpeed(_currentSpeed, 0f, _decelerationTime));
	}

	private IEnumerator LerpBeltSpeed(float from, float to, float duration)
	{
		if (duration <= 0f)
		{
			_currentSpeed = to;
			if (Mathf.Approximately(to, 0f))
			{
				_isRunning = false;
			}
			_speedCoroutine = null;
			yield break;
		}
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			_currentSpeed = Mathf.Lerp(from, to, t);
			yield return null;
		}
		_currentSpeed = to;
		if (Mathf.Approximately(to, 0f))
		{
			_isRunning = false;
		}
		_speedCoroutine = null;
	}

	private void PostSound(string eventName, GameObject target)
	{
		if (!string.IsNullOrEmpty(eventName))
		{
			SoundEventsManager.PostEvent(eventName, target);
		}
	}
}
