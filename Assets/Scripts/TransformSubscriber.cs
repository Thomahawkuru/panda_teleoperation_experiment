using UnityEngine;
using RosSharp.RosBridgeClient;
 
[RequireComponent(typeof(RosConnector))]
public class TransformSubscriber : UnitySubscriber<RosSharp.RosBridgeClient.MessageTypes.Geometry.Transform>
{
    private bool isMessageReceived;
    private static float[] transform = new float[7];

    protected override void Start()
    {
		base.Start();
    }
    private void Update()
    {
        if (isMessageReceived)
            isMessageReceived = false;
    }

    protected override void ReceiveMessage(RosSharp.RosBridgeClient.MessageTypes.Geometry.Transform msg)
    {
        transform[0] = (float)msg.translation.y;
        transform[1] = (float)msg.translation.z;
        transform[2] = (float)msg.translation.x;

        transform[3] = (float)msg.rotation.y;
        transform[4] = (float)msg.rotation.z;
        transform[5] = (float)msg.rotation.x;
        transform[6] = (float)msg.rotation.w;

        isMessageReceived = true;
/*        Debug.Log(transform[0]);
        Debug.Log(transform[1]);
        Debug.Log(transform[2]);
        Debug.Log(transform[3]);
        Debug.Log(transform[4]);
        Debug.Log(transform[5]);
        Debug.Log(transform[6]);*/

    }

    public float[] Transform()
    {
        return transform;
    }

}