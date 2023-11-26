using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using System.IO.Ports;

public class SerialPortManager
{
    private SerialPort _currentPort;

    public bool IsOpen => _currentPort != null && _currentPort.IsOpen;

    private Coroutine _waitRoutine;

    public void AutoConnect(MonoBehaviour caller, Func<string, bool> query, int baudRate, int readTimeout = 0,
        int writeTimeOut = 0)
    {
        caller.StartCoroutine(SearchConnection(caller, query, baudRate, readTimeout, writeTimeOut));
    }

    IEnumerator SearchConnection(MonoBehaviour caller, Func<string, bool> query, int baudRate, int readTimeout = 0,
        int writeTimeOut = 0)
    {
        var ports = GetSerialPorts(baudRate, readTimeout, writeTimeOut);

        float timeOut = 1;
        
        foreach (var port in ports)
        {
            var readLine = "";
            var newTime = Time.realtimeSinceStartup + timeOut;
            
            while (!query(readLine) && Time.realtimeSinceStartup <= newTime)
            {
                try
                {
                    readLine = port.ReadLine();
                    port.BaseStream.Flush();
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                yield return new WaitForEndOfFrame();
            }

            if (!query(readLine))
            {
                port.Close();
            }
            else
            {
                _currentPort = port;

                break;
            }
        }
    }

    private List<SerialPort> GetSerialPorts(int baudRate, int readTimeout = 0, int writeTimeOut = 0)
    {
        var portsNames = GetPortsName();
        List<SerialPort> ports = new List<SerialPort>();

        for (int i = 0; i < portsNames.Length; i++)
        {
            var currentPort = GetAndOpenConnection(portsNames[i], baudRate, readTimeout, writeTimeOut);

            if (currentPort != null && currentPort.IsOpen)
                ports.Add(currentPort);
        }

        return ports;
    }

    private string[] GetPortsName()
    {
        return SerialPort.GetPortNames();
    }
    
    public void Connect(string portName, int baudRate, int readTimeout = 0, int writeTimeOut = 0)
    {
        _currentPort = GetAndOpenConnection(portName, baudRate, readTimeout, writeTimeOut);
    }

    private SerialPort GetAndOpenConnection(string portName, int baudRate, int readTimeout = 0, int writeTimeOut = 0)
    {
        var port = new SerialPort(portName, baudRate);

        try
        {
            port.Open();
            port.ReadTimeout = readTimeout;
            port.WriteTimeout = writeTimeOut;

            return port;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return null;
        }
    }

    public void Close()
    {
        _currentPort.Close();
    }

    public string Read()
    {
        if (!IsOpen) return "";

        var data = _currentPort.ReadLine();

        _currentPort.BaseStream.Flush();

        return data;
    }

    public void Write(string data)
    {
        if (!IsOpen) return;

        _currentPort.WriteLine(data);

        _currentPort.BaseStream.Flush();
    }
}