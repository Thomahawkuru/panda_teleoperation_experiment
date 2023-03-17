

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Varjo;
using System.Collections;
using RosSharp.RosBridgeClient;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Logger : MonoBehaviour
{
    StreamWriter GAZEwriter = null;
    StreamWriter HMDwriter = null;
    StreamWriter GRIPPERwriter = null;
    StreamWriter ROBOTwriter = null;
    StreamWriter HANDwriter = null;
    StreamWriter EXPERIMENTwriter = null;

    List<VarjoPlugin.GazeData> dataSinceLastUpdate;

    private string subjectDataFolder;
    //private string Scene;
    //private List<SceneAsset> Scenes;

    Vector3 hmdPosition;
    Quaternion hmdRotation;

    [Header("Should only the latest data be logged on each update")]
    public bool oneGazeDataPerFrame = true;

    [Header("Start logging after calibration")]
    public bool startAutomatically = true;

    [Header("Press to start or end logging")]
    public static KeyCode toggleLoggingKey = KeyCode.L;

    [HideInInspector]
    public static bool logging = false;
    static double time;

    static readonly string[] ColumnNamesGaze = { "F", "DT", "T", "Status", "GazeX", "GazeY", "GazeZ", "GazePosition", "FocusDistance", "FocusStability", "StatusL", "LeftX", "LeftY", "LeftZ", "PositionL", "PupilL", "StatusR", "RightX", "RightY", "RightZ", "PositionR", "PupilR"};
    static readonly string[] ColumnNamesHMD = { "F", "DT", "T", "HMDPositionX", "HMDPositionY", "HMDPositionZ", "HMDRotationX", "HMDRotationY", "HMDRotationZ", "HMDRotationW" };
    static readonly string[] ColumnNamesGripper = { "F", "DT", "T", "Pos_X", "Pos_Y", "Pos_Z", "Rot_X", "Rot_Y", "Rot_Z", "Rot_W", "Grip_pos", "Grip_vel", "Grip_eff" };
    static readonly string[] ColumnNamesRobot = { "F", "DT", "T", "tau1", "tau2", "tau3", "tau4", "tau5", "tau6", "tau7" , "con_x", "con_y", "con_z"};
    static readonly string[] ColumnNamesHands = { "F", "DT", "T", "LeapPos_X", "LeapPos_Y", "LeapPos_Z", "LeapRot_X", "LeapRot_Y", "LeapRot_Z", "LeapRot_W", "Grab", "Pinch", "cmdPos_X", "cmdPos_Y", "cmdPos_Z", "cmdRot_X", "cmdRot_Y", "cmdRot_Z", "cmdRot_W" };
    static readonly string[] ColumnNamesExperiment = { "F", "DT", "T", "fps", "Start", "Hand", "Grip" , "Tracked", "Controlling"};
            
    const string ValidString = "VALID";
    const string InvalidString = "INVALID";

    private RobotstateSubscriber robotstatesubscriber;
    private TransformSubscriber transformsubscriber;
    private GripperSubscriber grippersubscriber;
    private KeyCode toggleCountdownKey = Countdown.ToggleCountdown;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => LogPrepper.IsStarted);

        if (!VarjoPlugin.InitGaze())
        {
            Debug.LogError("Failed to initialize gaze");
        }

        robotstatesubscriber = GameObject.Find("RosConnector").GetComponent<RobotstateSubscriber>();
        transformsubscriber = GameObject.Find("RosConnector").GetComponent<TransformSubscriber>();
        grippersubscriber = GameObject.Find("RosConnector").GetComponent<GripperSubscriber>();

        subjectDataFolder = LogPrepper.subjectDataFolder;
        Debug.Log("Read subject data folder = " + subjectDataFolder);
        Directory.CreateDirectory(subjectDataFolder);
        Debug.Log("Log directory created");
    }

    void Update()
    {
        time = time + Time.deltaTime;

        // Do not run update if the application is not visible
/*        if (!VarjoManager.Instance.IsLayerVisible() || VarjoManager.Instance.IsInStandBy())
        {
            return;
        }*/

        if ((Input.GetKeyDown(toggleLoggingKey)) || (Input.GetKeyDown(toggleCountdownKey)))
        {
            if (!logging)
            {
                time = 0.0;
                StartLogging();
            }
            else
            {
                StopLogging(EXPERIMENTwriter);
                StopLogging(GAZEwriter);
                StopLogging(HMDwriter);
                StopLogging(HANDwriter);
                StopLogging(GRIPPERwriter);
                StopLogging(ROBOTwriter);
                
                
            }
            return;
        }

        if (logging)
        {
            if (oneGazeDataPerFrame)
            {
                // Get and log latest gaze data
                LogExperimentData(VarjoPlugin.GetGaze());
                LogGazeData(VarjoPlugin.GetGaze());
                LogHandData(VarjoPlugin.GetGaze());
                LogHMDData(VarjoPlugin.GetGaze());
                LogGripperData(VarjoPlugin.GetGaze());
                LogRobotData(VarjoPlugin.GetGaze());           
            }
            else
            {
                // Get and log all gaze data since last update
                dataSinceLastUpdate = VarjoPlugin.GetGazeList();
                foreach (var data in dataSinceLastUpdate)
                {
                    LogExperimentData(data);
                    LogGazeData(data);
                    LogHMDData(data);
                    LogHandData(data);
                    LogGripperData(data);
                    LogRobotData(data);
                }
            }
        }
        else if (startAutomatically)
        {
            if (VarjoPlugin.GetGaze().status == VarjoPlugin.GazeStatus.VALID)
            {
                StartLogging();
                Debug.Log("automatic start");
            }
        }
    }

    void LogExperimentData(VarjoPlugin.GazeData data)
    {
        string[] logData = new string[9];


        // Gaze data frame number
        logData[0] = data.frameNumber.ToString();

        // Gaze data capture time (nanoseconds)
        logData[1] = Time.deltaTime.ToString(); //data.DeltaTime.ToString();

        // Log time (milliseconds)
        logData[2] = time.ToString(); //DateTime.Now.Ticks.ToString();

        // Experiment
        logData[3] = FPSLimiter.fps.ToString();
        logData[4] = Countdown.started.ToString();
        logData[5] = LogPrepper.Hand;
        logData[6] = GripcmdPublisher.gripping.ToString();
        logData[7] = PoscmdPublisher.tracked.ToString();
        logData[8] = PoscmdPublisher.Controlling.ToString();

        Log(logData, EXPERIMENTwriter);
    }

    void LogGazeData(VarjoPlugin.GazeData data)
    {
        string[] logData = new string[22];

        // Gaze data frame number
        logData[0] = data.frameNumber.ToString();

        // Gaze data capture time (nanoseconds)
        logData[1] = Time.deltaTime.ToString(); //data.DeltaTime.ToString();

        // Log time (milliseconds)
        logData[2] = time.ToString(); //DateTime.Now.Ticks.ToString();

        // Combined gaze
        bool invalid = data.status == VarjoPlugin.GazeStatus.INVALID;
        logData[3] = invalid ? InvalidString : ValidString;
        logData[4] = invalid ? "" : data.gaze.forward[0].ToString();
        logData[5] = invalid ? "" : data.gaze.forward[1].ToString();
        logData[6] = invalid ? "" : data.gaze.forward[2].ToString();
        logData[7] = invalid ? "" : data.gaze.position[0].ToString();

        // Focus
        logData[8] = invalid ? "" : data.focusDistance.ToString();
        logData[9] = invalid ? "" : data.focusStability.ToString();

        // Left eye
        bool leftInvalid = data.leftStatus == VarjoPlugin.GazeEyeStatus.EYE_INVALID;
        logData[10] = leftInvalid ? InvalidString : ValidString;
        logData[11] = leftInvalid ? "" : data.left.forward[0].ToString();
        logData[12] = leftInvalid ? "" : data.left.forward[1].ToString();
        logData[13] = leftInvalid ? "" : data.left.forward[2].ToString();
        logData[14] = leftInvalid ? "" : data.left.position[0].ToString();
        logData[15] = leftInvalid ? "" : data.leftPupilSize.ToString();

        // Right eye
        bool rightInvalid = data.rightStatus == VarjoPlugin.GazeEyeStatus.EYE_INVALID;
        logData[16] = rightInvalid ? InvalidString : ValidString;
        logData[17] = rightInvalid ? "" : data.right.forward[0].ToString();
        logData[18] = rightInvalid ? "" : data.right.forward[1].ToString();
        logData[19] = rightInvalid ? "" : data.right.forward[2].ToString();
        logData[20] = rightInvalid ? "" : data.right.position[0].ToString();
        logData[21] = rightInvalid ? "" : data.rightPupilSize.ToString();

        

        Log(logData,GAZEwriter);
    }

    void LogHMDData(VarjoPlugin.GazeData data)
    {
        // Get HMD position and rotation
        hmdPosition = VarjoManager.Instance.HeadTransform.position;
        hmdRotation = VarjoManager.Instance.HeadTransform.rotation;

        string[] logData = new string[10];

        // Gaze data frame number
        logData[0] = data.frameNumber.ToString();

        // Gaze data capture time (nanoseconds)
        logData[1] = Time.deltaTime.ToString(); //data.DeltaTime.ToString();

        // Log time (milliseconds)
        logData[2] = time.ToString(); //DateTime.Now.Ticks.ToString();

        // HMD
        logData[3] = hmdPosition.x.ToString();
        logData[4] = hmdPosition.y.ToString();
        logData[5] = hmdPosition.z.ToString();

        logData[6] = hmdRotation.x.ToString();
        logData[7] = hmdRotation.y.ToString();
        logData[8] = hmdRotation.z.ToString();
        logData[9] = hmdRotation.w.ToString();

        Log(logData, HMDwriter);

    }

    void LogHandData(VarjoPlugin.GazeData data)
    {
        string[] logData = new string[19];
        double HandGrip = GripcmdPublisher.grip;
        double Handpinch = GripcmdPublisher.pinch;

        // Gaze data frame number
        logData[0] = data.frameNumber.ToString();

        // Gaze data capture time (nanoseconds)
        logData[1] = Time.deltaTime.ToString(); //data.DeltaTime.ToString();

        // Log time (milliseconds)
        logData[2] = time.ToString(); //DateTime.Now.Ticks.ToString();

        // hand
        logData[3] = PoscmdPublisher.LeapHand.position.x.ToString();
        logData[4] = PoscmdPublisher.LeapHand.position.y.ToString();
        logData[5] = PoscmdPublisher.LeapHand.position.z.ToString();
        logData[6] = PoscmdPublisher.LeapHand.rotation.x.ToString();
        logData[7] = PoscmdPublisher.LeapHand.rotation.y.ToString();
        logData[8] = PoscmdPublisher.LeapHand.rotation.z.ToString();
        logData[9] = PoscmdPublisher.LeapHand.rotation.w.ToString();
        logData[10] = HandGrip.ToString();
        logData[11] = Handpinch.ToString();

        logData[12] = PoscmdPublisher.OutputVec.x.ToString();
        logData[13] = PoscmdPublisher.OutputVec.y.ToString();
        logData[14] = PoscmdPublisher.OutputVec.z.ToString();
        logData[15] = PoscmdPublisher.OutputQuat.x.ToString();
        logData[16] = PoscmdPublisher.OutputQuat.y.ToString();
        logData[17] = PoscmdPublisher.OutputQuat.z.ToString();
        logData[18] = PoscmdPublisher.OutputQuat.w.ToString();

        Log(logData, HANDwriter);
    }

    void LogGripperData(VarjoPlugin.GazeData data)
    {
        float[] Panda = transformsubscriber.Transform();
        double[] Gripper_pos = grippersubscriber.Position();
        double[] Gripper_vel = grippersubscriber.Velocity();
        double[] Gripper_eff = grippersubscriber.Effort();
        string[] logData = new string[13];

        // Gaze data frame number
        logData[0] = data.frameNumber.ToString();

        // Gaze data capture time (nanoseconds)
        logData[1] = Time.deltaTime.ToString(); //data.DeltaTime.ToString();

        // Log time (milliseconds)
        logData[2] = time.ToString(); //DateTime.Now.Ticks.ToString();

        // Panda
        logData[3] = Panda[0].ToString();
        logData[4] = Panda[1].ToString();
        logData[5] = Panda[2].ToString();
        logData[6] = Panda[3].ToString();
        logData[7] = Panda[4].ToString();
        logData[8] = Panda[5].ToString();
        logData[9] = Panda[6].ToString();

        var grip = Gripper_pos[0] * 2;
        logData[10] = grip.ToString();
        logData[11] = Gripper_vel[0].ToString();
        logData[12] = Gripper_eff[0].ToString();


        Log(logData, GRIPPERwriter);
    }

    void LogRobotData(VarjoPlugin.GazeData data)
    {
        RosSharp.RosBridgeClient.MessageTypes.Franka.FrankaState Robotstate = robotstatesubscriber.Message();
        string[] logData = new string[13];

        // Gaze data frame number
        logData[0] = data.frameNumber.ToString();

        // Gaze data capture time (nanoseconds)
        logData[1] = Time.deltaTime.ToString(); //data.DeltaTime.ToString();

        // Log time (milliseconds)
        logData[2] = time.ToString(); //DateTime.Now.Ticks.ToString();

        // Panda
        logData[3] = Robotstate.tau_J[0].ToString();
        logData[4] = Robotstate.tau_J[1].ToString();
        logData[5] = Robotstate.tau_J[2].ToString();
        logData[6] = Robotstate.tau_J[3].ToString();
        logData[7] = Robotstate.tau_J[4].ToString();
        logData[8] = Robotstate.tau_J[5].ToString();
        logData[9] = Robotstate.tau_J[6].ToString();

        logData[10] = Robotstate.O_F_ext_hat_K[0].ToString();
        logData[11] = Robotstate.O_F_ext_hat_K[1].ToString();
        logData[12] = Robotstate.O_F_ext_hat_K[2].ToString();

        Log(logData, ROBOTwriter);
    }   
   
    // Write given values in the log file
    void Log(string[] values, StreamWriter writer)
    {
        if (!logging || writer == null)
            return;

        string line = "";
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = values[i].Replace("\r", "").Replace("\n", ""); // Remove new lines so they don't break csv
            line += values[i] + (i == (values.Length - 1) ? "" : ","); // Do not add colon to last data string
        }
        writer.WriteLine(line);
    }

    public void StartLogging()
    {
        if (logging)
        {
            Debug.LogWarning("Logging was on when StartLogging was called. No new log was started.");
            return;
        }

        GAZEwriter = new StreamWriter(subjectDataFolder + "/Gaze.csv");
        HMDwriter = new StreamWriter(subjectDataFolder + "/HMD.csv");
        GRIPPERwriter = new StreamWriter(subjectDataFolder + "/Gripper.csv");
        ROBOTwriter = new StreamWriter(subjectDataFolder + "/Robot.csv");
        HANDwriter = new StreamWriter(subjectDataFolder + "/Hand.csv");
        EXPERIMENTwriter = new StreamWriter(subjectDataFolder + "/Experiment.csv");

        logging = true;

        Log(ColumnNamesExperiment, EXPERIMENTwriter);
        Log(ColumnNamesGaze, GAZEwriter);
        Log(ColumnNamesHMD, HMDwriter);
        Log(ColumnNamesHands, HANDwriter);
        Log(ColumnNamesGripper, GRIPPERwriter);
        Log(ColumnNamesRobot, ROBOTwriter);   
                        
        Debug.Log("Logs started at: " + subjectDataFolder);
    }

    void StopLogging(StreamWriter writer)
    {
        if (!logging)
            return;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
        logging = false;
        Debug.Log("Logging ended");
    }

    void OnApplicationQuit()
    {
        StopLogging(EXPERIMENTwriter);
        StopLogging(GAZEwriter);
        StopLogging(HMDwriter);
        StopLogging(HANDwriter);
        StopLogging(GRIPPERwriter);
        StopLogging(ROBOTwriter);
        
        
    }

    public static string Double3ToString(double[] doubles)
    {
        return doubles[0].ToString() + ". " + doubles[1].ToString() + ". " + doubles[2].ToString();
    }
}
