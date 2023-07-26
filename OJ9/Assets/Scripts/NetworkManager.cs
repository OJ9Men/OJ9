using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

public class NetworkManager
{
    private Socket socket;
    private byte[] buffer;
    public NetState netState;
    public Action<NetState> netStateChangedHandler;
    private Action<byte[]>[] packetHandlers;
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
        packetHandlers = new Action<byte[]>[(int)PacketType.Max];
        blockAction = _blockAction;
    }

    public void Disconnect()
    {
        socket.Disconnect(false);
    }

    private void BindPacketHandler(PacketType _packetType, Action<byte[]> _action)
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
            netStateChangedHandler(NetState.Closed);
            return;
        }
        
        socket.BeginReceive(
            buffer,
            0,
            buffer.Length,
            SocketFlags.None,
            OnReceived,
            null
        );

        netState = NetState.Connected;
        netStateChangedHandler(NetState.Connected);
        Debug.Log("Server connected");
    }

    public void SendAndBindHandler(PacketBase _packet, Action<byte[]> _action)
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
        var received = socket.EndReceive(_asyncResult);
        if (received <= 0)
        {
            return;
        }

        var newBuffer = new byte[received];
        Array.Copy(buffer, 0, newBuffer, 0, received);
        var packetBase = OJ9Function.ByteArrayToObject<PacketBase>(newBuffer);
        var packetHandler = packetHandlers[(int)packetBase.packetType]; 
        
        if (packetHandler is null)
        {
            Debug.LogError("Need to be binded. Packet type is " + packetBase.packetType);
        }
        else
        {
            packetHandler(newBuffer);
            blockAction(false);
        }
    }
}