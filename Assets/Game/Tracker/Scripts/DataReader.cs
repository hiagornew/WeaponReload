using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MyBox;
using UnityEngine;

[Serializable]
public class JsonWrapper 
{
    public Settings settings;
}

[Serializable]
public class Settings 
{
    public int BaudRate;
    public bool TrackerScene;
}

public class DataReader : MonoBehaviour
{
    [Foldout("References", true)] 
    [SerializeField, ReadOnly]
    private STMManager stmManager;
    [SerializeField, ReadOnly]
    private TrackerSceneController trackerSceneController;
    
    [Foldout("Information", true)]
    public Settings currentSettings;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (!stmManager)
        {
            stmManager = FindObjectOfType<STMManager>();
        }
        
        if (!trackerSceneController)
        {
            trackerSceneController = FindObjectOfType<TrackerSceneController>();
        }
    }
    #endif

    private void Start()
    {
        ReadSettings();
    }

    public void ReadSettings()
    {
        currentSettings = ReadData("ArduinoPortSettings.json");

        trackerSceneController.Initialize(currentSettings.TrackerScene);
        
        if (!currentSettings.TrackerScene)
        {
            return;
        }

        stmManager.Initialize(currentSettings);
    }

    private Settings ReadData(string settingsName)
    {
        var path = Path.Combine(Application.streamingAssetsPath, settingsName);

        var contents = File.ReadAllText(path);
                
        var wrapper = JsonUtility.FromJson<JsonWrapper>(contents);
                
        return wrapper.settings;
    }
    
    public void SaveIntoJson(){
        var path = Path.Combine(Application.streamingAssetsPath, "ArduinoPortSettings.json");
        
        var wrapper = new JsonWrapper();
        wrapper.settings = currentSettings;
        
        var contents = JsonUtility.ToJson(wrapper);
        
        File.WriteAllText(path, contents);
    }

    private void OnApplicationQuit()
    {
        SaveIntoJson();
    }
}
