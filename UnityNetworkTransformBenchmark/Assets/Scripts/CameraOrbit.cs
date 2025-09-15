using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using KS.Reactor;

/// <summary>
/// Camera controller that uses axis, mouse, and touch input to move the camera around the origin of this object. The
/// camera should be a child of this object. The camera always faces this object's origin.
/// </summary>
public class CameraOrbit : MonoBehaviour
{
    /// <summary>Min camera distance from the origin.</summary>
    [Tooltip("Min camera distance from the origin.")]
    public float MinZoom = 15f;
    /// <summary>Max camera distance from the origin.</summary>
    [Tooltip("Max camera distance from the origin.")]
    public float MaxZoom = 90f;
    /// <summary>Min camera pitch in degrees.</summary>
    [Tooltip("Min camera pitch in degrees.")]
    public float MinPitch = -80f;
    /// <summary>Max camera pitch in degrees.</summary>
    [Tooltip("Max camera pitch in degrees.")]
    public float MaxPitch = 80f;

    /// <summary>Deceleration after a swipe.</summary>
    [Tooltip("Deceleration after a swipe.")]
    public float Decel = 100f;
    /// <summary>Reduction in speed after a swipe as a multiple of the current speed times the time step.</summary>
    [Tooltip("Reduction in speed after a swipe as a multiple of the current speed times the time step.")]
    public float Drag = 1f;
    /// <summary>Maximum speed after a swipe.</summary>
    [Tooltip("Maximum speed after a swipe.")]
    public float MaxSwipeSpeed = 1000f;

    private float m_pitch;
    private float m_yaw;
    private float m_rotateSpeed = 100f;
    private float m_perspectiveZoomSpeed = 1.0f;

    private const int MAX_TOUCH_NUM = 6;
    private const float MIN_SWIPE_TIME = 0.2f;
    private float m_swipeSpeedX = 0f;
    private float m_swipeSpeedY = 0f;
    private float m_swipeTime = 0f;
    private Queue<Touch> m_touchQueue = new Queue<Touch>();

    private void Start()
    {
        Input.simulateMouseWithTouches = false;
    }

    void LateUpdate()
    {
        float turnX, turnY, move;

        turnX = -Input.GetAxis("Horizontal") * 60.0f * Time.deltaTime;
        turnY = Input.GetAxis("Vertical") * 60.0f * Time.deltaTime;
        move = Input.GetAxis("Mouse ScrollWheel") * 15.0f * m_perspectiveZoomSpeed;

        if (turnX != 0f)
        {
            m_swipeSpeedX = 0f;
        }
        if (turnY != 0f)
        {
            m_swipeSpeedY = 0f;
        }

        if (Input.touchCount == 1 || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            Touch touch = Input.touchCount == 1 ? Input.GetTouch(0) : GetMouseTouch();

            if (touch.phase == TouchPhase.Began)
            {
                m_swipeTime = 0f;
            }
            else
            {
                if (touch.deltaTime > 0)
                {
                    m_swipeSpeedX = (-touch.deltaPosition.x / touch.deltaTime) * m_rotateSpeed / Screen.width;
                    m_swipeSpeedY = (touch.deltaPosition.y / touch.deltaTime) * m_rotateSpeed / Screen.width;

                    m_touchQueue.Enqueue(touch);
                    if (m_touchQueue.Count > MAX_TOUCH_NUM)
                    {
                        m_touchQueue.Dequeue();
                    }
                }

                m_swipeTime += Time.deltaTime;
                if (touch.phase == TouchPhase.Ended && m_swipeTime >= MIN_SWIPE_TIME)
                {
                    CalculateSwipeSpeed();
                }
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            move = deltaMagnitudeDiff * 0.215f * m_perspectiveZoomSpeed;
        }
        else
        {
            Decelerate(ref m_swipeSpeedX);
            Decelerate(ref m_swipeSpeedY);
        }

        if (m_swipeSpeedX != 0f)
        {
            turnX = m_swipeSpeedX * Time.deltaTime;
        }
        if (m_swipeSpeedY != 0f)
        {
            turnY = m_swipeSpeedY * Time.deltaTime;
        }

        if (turnX != 0f || turnY != 0f)
        {
            m_yaw = ksRange.Degrees.Wrap(m_yaw + turnX);
            m_pitch = m_pitch + turnY;
            m_pitch = ksMath.Clamp(m_pitch, MinPitch, MaxPitch);
            transform.localEulerAngles = new Vector3(ksRange.Degrees.Wrap(m_pitch), m_yaw, 0f);
        }

        float zoom = -Camera.main.transform.localPosition.z;
        Camera.main.transform.localPosition = new Vector3(0, 0, -Mathf.Clamp(zoom - move, MinZoom, MaxZoom));
    }

    private void Decelerate(ref float speed)
    {
        if (speed == 0f)
        {
            return;
        }

        float sign = speed > 0f ? 1f : -1f;
        float abs = speed * sign;
        abs -= abs * Drag * Time.deltaTime;
        abs -= Decel * Time.deltaTime;
        abs = Mathf.Min(abs, MaxSwipeSpeed);
        if (abs <= 0f)
        {
            speed = 0;
        }
        else
        {
            speed = abs * sign;
        }
    }

    private void CalculateSwipeSpeed()
    {
        Vector2 sumDeltaPos = new Vector2();
        float sumDeltaTime = 0f;
        while (m_touchQueue.Count > 0)
        {
            Touch touch = m_touchQueue.Dequeue();
            sumDeltaPos += touch.deltaPosition;
            sumDeltaTime += touch.deltaTime;
        }
        float touchTurnSpeedX = (-sumDeltaPos.x / sumDeltaTime) * m_rotateSpeed / Screen.width;
        float touchTurnSpeedY = (sumDeltaPos.y / sumDeltaTime) * m_rotateSpeed / Screen.width;
        if (Mathf.Abs(touchTurnSpeedX) > Mathf.Abs(m_swipeSpeedX))
        {
            m_swipeSpeedX = touchTurnSpeedX;
        }
        if (Mathf.Abs(touchTurnSpeedY) > Mathf.Abs(m_swipeSpeedY))
        {
            m_swipeSpeedY = touchTurnSpeedY;
        }
    }

    private Touch GetMouseTouch()
    {
        Touch touch = new Touch();
        touch.deltaPosition = Input.mousePositionDelta * 10f;
        touch.deltaTime = Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            touch.phase = TouchPhase.Began;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            touch.phase = TouchPhase.Ended;
        }
        else
        {
            touch.phase = Input.mousePositionDelta != Vector3.zero ? TouchPhase.Moved : TouchPhase.Stationary;
        }
        return touch;
    }
}
