using UnityEngine;
using RosSharp.RosBridgeClient;

[RequireComponent(typeof(RosConnector))]
public class GripperSubscriber : UnitySubscriber<RosSharp.RosBridgeClient.MessageTypes.Sensor.JointState>
{
    private bool isMessageReceived;
    private static RosSharp.RosBridgeClient.MessageTypes.Sensor.JointState gripper = new RosSharp.RosBridgeClient.MessageTypes.Sensor.JointState();

    protected override void Start()
    {
       base.Start();
    }
    private void Update()
    {
        if (isMessageReceived)
            isMessageReceived = false;
    }

    protected override void ReceiveMessage(RosSharp.RosBridgeClient.MessageTypes.Sensor.JointState msg)
    {
        gripper.name = msg.name;
        gripper.position = msg.position;
        gripper.velocity = msg.velocity;
        gripper.effort = msg.effort;

        isMessageReceived = true;
    }

    public double[] Position()
    {
        return gripper.position;        
    }

    public double[] Velocity()
    {
        return gripper.velocity;
    }

    public double[] Effort()
    {
        return gripper.effort;
    }
}