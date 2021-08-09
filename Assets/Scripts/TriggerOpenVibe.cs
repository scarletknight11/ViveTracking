using System;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public class TriggerOpenVibe : MonoBehaviour
{
    private static TcpClient _socketConnection1;
    private static TcpClient _socketConnection2;

    private void Awake()
    {
        try
        {
            /*
             * Connect to the OpenVibe Acquisition Server. Unless settings have been modified
             * or you're running OpenVibe on a different machine, the address should be
             * localhost and the port 15361. These settings are in Acquisition Server -> Preferences.
             */
            _socketConnection1 = new TcpClient("localhost", 15361);
            _socketConnection2 = new TcpClient("127.0.0.1", 15361);
            Debug.Log("Successfully connected to OpenVibe");
        }
        catch (SocketException se)
        {
            Debug.Log("Socket exception: " + se);
        }
    }

    /*
     * Call this from your experiment code to send a trigger. It can take
     * any integer. You can have as many different trigger codes as you like.
     */
    public static void SendTrigger(int eventId)
    {
        SendTrigger1(eventId);
        //SendTrigger2(eventId);
    }


    public static void SendTrigger1(int eventId)
    {
        var stream = _socketConnection1.GetStream();

        if (!stream.CanWrite) return;

        var buffer = BitConverter.GetBytes((ulong)0);
        var eventTag = BitConverter.GetBytes((ulong)eventId);

        var sendArray = buffer.Concat(eventTag.Concat(buffer)).ToArray();

        stream.Write(sendArray, 0, sendArray.Length);
    }

    /*public static void SendTrigger2(int eventId)
    {
        var stream = _socketConnection2.GetStream();

        if (!stream.CanWrite) return;

        var buffer = BitConverter.GetBytes((ulong)0);
        var eventTag = BitConverter.GetBytes((ulong)eventId);

        var sendArray = buffer.Concat(eventTag.Concat(buffer)).ToArray();

        stream.Write(sendArray, 0, sendArray.Length);
    }*/
}