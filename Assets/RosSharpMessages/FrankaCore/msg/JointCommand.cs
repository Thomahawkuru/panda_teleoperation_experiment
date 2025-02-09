/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



using RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient.MessageTypes.FrankaCore
{
    public class JointCommand : Message
    {
        public const string RosMessageName = "franka_core_msgs/JointCommand";

        public Header header { get; set; }
        public int mode { get; set; }
        //  Mode in which to command arm
        public string[] names { get; set; }
        //  Joint names order for command
        //  Fields of commands indexed according to the Joint names vector.
        //  Command fields required for a desired mode are listed in the comments
        public double[] position { get; set; }
        //  (radians)       Required for POSITION_MODE and IMPEDANCE_MODE
        public double[] velocity { get; set; }
        //  (radians/sec)   Required for VELOCITY_MODE and IMPEDANCE_MODE
        public double[] acceleration { get; set; }
        //  (radians/sec^2) Required for                   
        public double[] effort { get; set; }
        //  (newton-meters) Required for TORQUE_MODE
        //  Modes available to command arm
        public const int POSITION_MODE = 1;
        public const int VELOCITY_MODE = 2;
        public const int TORQUE_MODE = 3;
        public const int IMPEDANCE_MODE = 4;

        public JointCommand()
        {
            this.header = new Header();
            this.mode = 0;
            this.names = new string[0];
            this.position = new double[0];
            this.velocity = new double[0];
            this.acceleration = new double[0];
            this.effort = new double[0];
        }

        public JointCommand(Header header, int mode, string[] names, double[] position, double[] velocity, double[] acceleration, double[] effort)
        {
            this.header = header;
            this.mode = mode;
            this.names = names;
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.effort = effort;
        }
    }
}
