using System.Collections.Generic;
using UnityEngine;
using Leap;
using System;


namespace RosSharp.RosBridgeClient
{
    public class PoscmdPublisherDirect : UnityPublisher<MessageTypes.Geometry.PoseStamped>
    {
        Controller controller;
        //public Transform LeapTracker;
        public Transform handtransform;
        public Transform Lindex;
        public Transform Lthumb;
        public Transform Lrot;
        public Transform Rindex;
        public Transform Rthumb;
        public Transform Rrot;
        public Transform Base;
        public Transform Panda;

        public bool Connect = true;

        [Header("Position input")]
        public float Zzero = 0.5F;
        public Vector3 LinOffset = new Vector3(0.0f, 0.0f, 0.3f);

        // unity:  z     x     y     z     x     y
        // robot:  x     y     z     x     y     z
        [Header("Angle input")]
        public bool x = true;
        public bool y = true;
        public bool z = true;

        [Header("Euler Angles")]
        public Vector3 AngleOffset = new Vector3(10f, 90f, 20f);
        //public Vector3 AngleDirection = new Vector3(-1,-1,1);

        [HideInInspector]
        private float[] readystate = { 0.4F, 0f, 0.5f, 1F, 0F, 0F, 0F };
        //private float[] readystate = { 0.5F, 0f, 0.5f, 0.888673961163F, 0.00819689128548F, 0.0145936477929F, -0.458234190941F };
        //private float[] readystate = { 0.5F, 0f, 0.5f, -0.888673961163F, -0.00819689128548F, -0.0145936477929F, 0.458234190941F };

        private MessageTypes.Geometry.PoseStamped message;
        public static bool Controlling;
        public static bool tracked;
        public static bool connect;
        public static Vector3 OutputVec = new Vector3();
        public static Quaternion OutputQuat = new Quaternion();
        private string Hand;
        public static Transform LeapHand;
        public static Transform OutputHand;
        private KeyCode toggleInput = KeyCode.KeypadEnter;
        

        private void Start()
        {
            connect = Connect;
            Hand = LogPrepper.Hand;
            controller = new Controller();

            if (Hand == "R") { LeapHand = Rrot; }
            if (Hand == "L") { LeapHand = Lrot; }

            if (LogPrepper.handedness == HandEnum.L)
            {
                AngleOffset.x = -AngleOffset.x;
                AngleOffset.z = -AngleOffset.z;
            }

            base.Start();
            InitializeMessage();
        }

        private void Update()
        {
            Frame frame = controller.Frame();

            if (Input.GetKeyDown(toggleInput))
            {
                if (connect == true) { connect = false; }
                else if (connect == false) { connect = true; }
            }

            if (frame.Hands.Count > 0)
            {
                List<Hand> hands = frame.Hands;
                Controlling = false;

                foreach (Hand hand in hands)
                {
                    if ((hand.IsRight) && (Hand == "R"))
                    {
                        tracked = true;
                        //Rhand = Rrot;
                        handtransform.position = (Rindex.position + Rthumb.position) / 2f;
                        handtransform.rotation = Rrot.rotation;
                        //Quaternion.Slerp(Rindex.rotation, Rthumb.rotation, 0.5f);
                        CheckInput(handtransform, Base.position, "R");
                    }
                    else if ((hand.IsLeft) && (Hand == "L"))
                    {
                        tracked = true;
                        //Lhand = Lrot;
                        handtransform.position = (Lindex.position + Lthumb.position) / 2f;
                        handtransform.rotation = Lrot.rotation;
                        CheckInput(handtransform, Base.position, "L");
                    }
                    else { Controlling = false; tracked = false; }

                    if (Controlling == true)
                    {
                        break;
                    }
                }
            }

            else { Controlling = false; }

            //Debug.Log("Input = " + String.Join(", ", new List<float>(Input).ConvertAll(i => i.ToString()).ToArray()));
            UpdateMessage();
        }
        private void InitializeMessage()
        {
            message = new MessageTypes.Geometry.PoseStamped();
            message.pose.position.x = new double();
            message.pose.position.y = new double();
            message.pose.position.z = new double();
            message.pose.orientation.x = new double();
            message.pose.orientation.y = new double();
            message.pose.orientation.z = new double();
            message.pose.orientation.w = new double();
        }

        private void CheckInput(Transform hand, Vector3 p, string handedness)
        {
            //Debug.Log("Input: " + hand.rotation.ToString("0.000"));

            //control volume size
            var sx = 1.0;
            var sy = -0.1;
            var sz = 1.5;

            if (Math.Abs(hand.position.x - p.x) < sx / 2)
            {
                if (hand.position.y - p.y > sy)
                {
                    if (Math.Abs(hand.position.z + LinOffset.z + Zzero - p.z) < sz / 2)
                    {
                        //Debug.Log("controlling" + connect.ToString());
                        if (connect)
                        {
                            Controlling = true;
                            CalculateInput(hand, Base.position, AngleOffset, handedness);
                        }
                    }
                }
            }
        }

        private void CalculateInput(Transform hand, Vector3 Base, Vector3 Offset, string handedness)
        {
            var LtoU = new Quaternion();
            var quat_unity = new Quaternion();
            var quat_rh = new Quaternion();
            var UtoE = Quaternion.Euler(new Vector3(-180, 0, 0));

            if (handedness == "R")
            {
                LtoU = Quaternion.Euler(new Vector3(180, 0, 0));
                quat_unity = LtoU * hand.rotation;
                quat_rh = new Quaternion(quat_unity.x, quat_unity.z, -quat_unity.y, quat_unity.w);
            } //ZXY
            else if (handedness == "L")
            {
                LtoU = Quaternion.Euler(new Vector3(180, 90, 0));
                quat_unity = LtoU * hand.rotation;
                quat_rh = new Quaternion(-quat_unity.x, -quat_unity.z, -quat_unity.y, quat_unity.w);
            } //ZXY

            var quat = UtoE * quat_rh;

            quat *= Quaternion.Inverse(Quaternion.Euler(Offset.x, 0, 0));
            quat *= Quaternion.Inverse(Quaternion.Euler(0, Offset.y, 0));
            quat *= Quaternion.Inverse(Quaternion.Euler(0, 0, Offset.z));

            quat = quat.normalized;

            //robot: x y z = unity: -z, x, y
            OutputVec.y = -(hand.position.z - Base[2] + LinOffset.z);  //X
            OutputVec.x = -(hand.position.x - Base[0] + LinOffset.x - Panda.position.x);  //Y
            OutputVec.z = hand.position.y - Base[1] + LinOffset.y;     //Z

            if (quat.x < 0) 
            { 
                quat.x *= -1;
                quat.y *= -1;
                quat.z *= -1;
                quat.w *= -1;
            }

            OutputQuat.x = quat.x;
            OutputQuat.y = quat.y;
            OutputQuat.z = quat.z;
            OutputQuat.w = quat.w;

            //Debug.Log("Output: " + quat.ToString("0.000"));
        }

        private void UpdateMessage()
        {
            if (Controlling == true)
            {
                //Debug.Log("Controlling");
                message.pose.position.x = OutputVec.x;
                message.pose.position.y = OutputVec.y;
                message.pose.position.z = OutputVec.z;
                message.pose.orientation.x = OutputQuat.x;
                message.pose.orientation.y = OutputQuat.y;
                message.pose.orientation.z = OutputQuat.z;
                message.pose.orientation.w = OutputQuat.w;
            }
            else if (Controlling == false)
            {
                //Debug.Log("Readystate");
                message.pose.position.x = readystate[0];
                message.pose.position.y = readystate[1];
                message.pose.position.z = readystate[2];
                message.pose.orientation.x = readystate[3];
                message.pose.orientation.y = readystate[4];
                message.pose.orientation.z = readystate[5];
                message.pose.orientation.w = readystate[6];
            }

            Publish(message);
        }
    }
}