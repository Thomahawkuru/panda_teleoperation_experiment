using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

namespace RosSharp.RosBridgeClient
{
    public class GripcmdPublisher : UnityPublisher<MessageTypes.Std.Float64MultiArray>
    {
        
        Controller controller;
        private MessageTypes.Std.Float64MultiArray message;
        private string Hand;

        public cmdenum CMDType;
        public float CMDHigh = 0.3f;
        public float CMDLow = 0.05f;

        [HideInInspector]
        public static bool gripping = false;
        public static double grip;
        public static double pinch;

        public void Start()
        {
            Hand = LogPrepper.Hand;
            controller = new Controller();

            base.Start();
            InitializeMessage();
            UpdateMessage(0);
        }

        public void Update()
        {
            Frame frame = controller.Frame(); 

            if (frame.Hands.Count > 0)
            {
                List<Hand> hands = frame.Hands;
                foreach (Hand hand in hands)
                {
                    //Debug.Log(hand.IsRight);
                    //Debug.Log(Hand);
                    if (Hand == "R" && hand.IsRight)
                    {
                        grip = hand.GrabStrength;
                        pinch = hand.PinchStrength;
                        //Debug.Log(grip);

                        if (CMDType.ToString() == "gripping")
                        {
                            if ((grip > CMDHigh) && !gripping)
                            {
                                gripping = true;
                                UpdateMessage(1);
                                break;
                            }
                            else if ((grip < CMDLow) && gripping)
                            {
                                gripping = false;
                                UpdateMessage(0);
                                break;
                            }
                        }
                        else if (CMDType.ToString() == "pinching")
                        {
                            if ((pinch > CMDHigh) && !gripping)
                            {
                                gripping = true;
                                UpdateMessage(1);
                                break;
                            }
                            else if ((pinch < CMDLow) && gripping)
                            {
                                gripping = false;
                                UpdateMessage(0);
                                break;
                            }
                        }
                    }
                    else if(Hand == "L" && hand.IsLeft)
                    {
                        grip = hand.GrabStrength;
                        pinch = hand.PinchStrength;

                        if (CMDType.ToString() == "gripping")
                        {
                            if ((grip > CMDHigh) && !gripping)
                            {
                                gripping = true;
                                UpdateMessage(1);
                                break;
                            }
                            else if ((grip < CMDLow) && gripping)
                            {
                                gripping = false;
                                UpdateMessage(0);
                                break;
                            }
                        }
                        else if (CMDType.ToString() == "pinching")
                        {
                            if ((pinch > CMDHigh) && !gripping)
                            {
                                gripping = true;
                                UpdateMessage(1);
                                break;
                            }
                            else if ((pinch < CMDLow) && gripping)
                            {
                                gripping = false;
                                UpdateMessage(0);
                                break;
                            }
                        }
                    }
                }
            }      
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Float64MultiArray
            {
                data = new double[2]
            };
        }
        private void UpdateMessage(double cmd)
        {
            message.data[0] = 0.04 - cmd * 0.04;
            message.data[1] = 0.04 - cmd * 0.04;
            
            Publish(message);
        }

        public enum cmdenum
        {
            gripping,
            pinching
        }
    }
}
;