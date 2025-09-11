using UnityEngine;
using TMPro;

/// <summary>Monitors frame rate per second and displays it in a <see cref="TMP_Text"/> on the same object.</summary>
[RequireComponent(typeof(TMP_Text))]
public class FPSMonitor : MonoBehaviour
{
    public float Interval = 1f;

    private TMP_Text m_text;
    private float m_timer;
    private int m_frames;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        m_frames++;
        m_timer += Time.deltaTime;
        if (m_timer >= Interval)
        {
            float fps = m_frames / m_timer;
            m_frames = 0;
            m_timer = 0f;
            m_text.text = fps.ToString("0.0") + " fps";
        }
    }
}
