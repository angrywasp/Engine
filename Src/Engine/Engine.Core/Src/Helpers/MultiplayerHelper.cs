using System.Net;
using System.Net.Sockets;
using System.Linq;
using Lidgren.Network;
using System.Numerics;

namespace Engine.Helpers
{
    public static class MultiplayerHelper
    {
        public static string GetLocalNetworkAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(
                ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
        }

        public static void Write(this NetOutgoingMessage msg, Vector2 p)
        {
            msg.Write(p.X);
            msg.Write(p.Y);
        }

        public static void Write(this NetOutgoingMessage msg, Vector3 p)
        {
            msg.Write(p.X);
            msg.Write(p.Y);
            msg.Write(p.Z);
        }

        public static void Write(this NetOutgoingMessage msg, Quaternion q)
        {
            msg.Write(q.X);
            msg.Write(q.Y);
            msg.Write(q.Z);
            msg.Write(q.W);
        }

        public static Vector2 ReadVector2(this NetIncomingMessage msg)
        {
            return new Vector2(
                msg.ReadFloat(),
                msg.ReadFloat());
        }

        public static Vector3 ReadVector3(this NetIncomingMessage msg)
        {
            return new Vector3(
                msg.ReadFloat(),
                msg.ReadFloat(),
                msg.ReadFloat());
        }

        public static Quaternion ReadQuaternion(this NetIncomingMessage msg)
        {
            return new Quaternion(
                msg.ReadFloat(),
                msg.ReadFloat(),
                msg.ReadFloat(),
                msg.ReadFloat());
        }
    }
}
