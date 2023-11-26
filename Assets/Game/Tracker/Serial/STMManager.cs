using System;
using System.Threading;
using MyBox;
using UnityEngine;

public class STMManager : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private TrackerBehaviour trackerBehaviour;
    
    private SerialPortManager _serialPort;
    private bool _continueThread;
    private Thread _thread;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (!trackerBehaviour)
        {
            trackerBehaviour = FindObjectOfType<TrackerBehaviour>();
        }
    }
    #endif

    public void Initialize(Settings settings)
    {
        _serialPort = new SerialPortManager();

        _serialPort.AutoConnect(this, s=> s.Contains("b:"), 
            settings.BaudRate, 999, 999);

        StartThread();
    }

    private void StartThread()
    {
        _continueThread = true;
        _thread = new Thread(Read);
        _thread.Start();   
    }

    private void Read()
    {
        while (_continueThread)
        {
            if (_serialPort == null)
            {
                continue;
            }

            var read = "";

            try
            {
                read = _serialPort.Read();
            }
            catch (Exception e)
            {
               Debug.Log(e.Message);
            }
            
            if (string.IsNullOrEmpty(read))
            {
                continue;
            }
            
            trackerBehaviour.ReceivedData(read);
        }
    }

    private void OnDisable()
    {
        if (_thread != null)
        {
            _continueThread = false;
            _thread.Abort();
        }
    }
}