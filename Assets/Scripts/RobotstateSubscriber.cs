using UnityEngine;
using RosSharp.RosBridgeClient;
using System.Collections.Generic;
 
[RequireComponent(typeof(RosConnector))]
public class RobotstateSubscriber : UnitySubscriber<RosSharp.RosBridgeClient.MessageTypes.Franka.FrankaState>
{
    private bool isMessageReceived;
    private static RosSharp.RosBridgeClient.MessageTypes.Franka.FrankaState RobotState = new RosSharp.RosBridgeClient.MessageTypes.Franka.FrankaState();

    protected override void Start()
    {
		base.Start();
    }
    private void Update()
    {
        if (isMessageReceived)
            isMessageReceived = false;
    }

    protected override void ReceiveMessage(RosSharp.RosBridgeClient.MessageTypes.Franka.FrankaState msg)
    {
        RobotState = msg;
        isMessageReceived = true;

    }

    public RosSharp.RosBridgeClient.MessageTypes.Franka.FrankaState Message()
    {
        return RobotState;
    }

}