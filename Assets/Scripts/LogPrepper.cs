using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LogPrepper : MonoBehaviour
{
    public int subjectID = 0;
    public HandEnum Handedness;
    public int Timer = 60;
    public static int timer = 60;
    public string DataPath = "C:/Users/thoma/Documents/Unity/Panda-manipulation/Data/";
    public static KeyCode toggleLoggingKey = KeyCode.L;

    [HideInInspector]
    public static string Hand;
    public static string subjectDataFolder;
    public static bool loaded = false;
    public static bool IsStarted = false;
    public static HandEnum handedness;

    private void Start()
    {
        handedness = Handedness;
        timer = Timer;
        Hand = Handedness.ToString();
        subjectDataFolder = DataPath + subjectID + "/" + System.DateTime.Now.ToString("MM-dd_HH-mm");
        Debug.Log($"Created log folder: {subjectDataFolder}...");

        System.IO.Directory.CreateDirectory(subjectDataFolder);
        IsStarted = true;
    }
}

public enum HandEnum
{
    R,
    L
}