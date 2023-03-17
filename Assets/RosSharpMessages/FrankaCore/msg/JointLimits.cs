/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



namespace RosSharp.RosBridgeClient.MessageTypes.FrankaCore
{
    public class JointLimits : Message
    {
        public const string RosMessageName = "franka_core_msgs/JointLimits";

        //  names of the joints
        public string[] joint_names { get; set; }
        //  lower bound on the angular position in radians
        public double[] position_lower { get; set; }
        //  upper bound on the angular position in radians
        public double[] position_upper { get; set; }
        //  symmetric maximum joint velocity in radians/second
        public double[] velocity { get; set; }
        //  symmetric maximum joint acceleration in radians/second^2
        public double[] accel { get; set; }
        //  symmetric maximum joint torque in Newton-meters
        public double[] effort { get; set; }

        public JointLimits()
        {
            this.joint_names = new string[0];
            this.position_lower = new double[0];
            this.position_upper = new double[0];
            this.velocity = new double[0];
            this.accel = new double[0];
            this.effort = new double[0];
        }

        public JointLimits(string[] joint_names, double[] position_lower, double[] position_upper, double[] velocity, double[] accel, double[] effort)
        {
            this.joint_names = joint_names;
            this.position_lower = position_lower;
            this.position_upper = position_upper;
            this.velocity = velocity;
            this.accel = accel;
            this.effort = effort;
        }
    }
}
