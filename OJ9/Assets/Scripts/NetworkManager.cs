using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

public class NetworkManager
{
    private Socket socket;
    private byte[] buffer;
    public NetState netState;
    private Action<PacketBase>[] packetHandlers;
    private Action<bool> blockAction;

    public NetworkManager(Action<bool> _blockAction)
    {
        netState = NetState.None;

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = OJ9Function.CreateIPEndPoint(
            OJ9Const.SERVER_IP + ":" + Convert.ToString(OJ9Const.SERVER_PORT_NUM)
        );
        socket.BeginConnect(endPoint, OnConnect, null);
        buffer = new byte[OJ9Const.BUFFER_SIZE];
        packetHandlers = new Action<PacketBase>[(int)PacketType.Max];
        blockAction = _blockAction;
    }

    private void BindPacketHandler(PacketType _packetType, Action<PacketBase> _action)
    {
        packetHandlers[(int)_packetType] = _action;
    }
    
    private void OnConnect(IAsyncResult _asyncResult)
    {
        try
        {
            socket.EndConnect(_asyncResult);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        socket.BeginReceive(buffer, 0, OJ9Const.BUFFER_SIZE, SocketFlags.None, OnReceived, null);

        netState = NetState.Connected;
        Debug.Log("Server connected");
    }

    public void SendAndBindHandler(PacketBase _packet, Action<PacketBase> _action)
    {
        if (netState is not NetState.Connected)
        {
            throw new FormatException("Server is not connected");
        }
        
        socket.Send(OJ9Function.ObjectToByteArray(_packet));
        BindPacketHandler(_packet.packetType, _action);
        blockAction(true);
    }

    private void OnReceived(IAsyncResult _asyncResult)
    {
        socket.EndReceive(_asyncResult);
        var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(buffer);
        var packetHandler = packetHandlers[(int)packetBase.packetType]; 
        
        if (packetHandler is null)
        {
            Debug.LogError("Need to be binded. Packet type is " + packetBase.packetType);
        }
        else
        {
            packetHandler(packetBase);
            blockAction(false);
        }
    }
}