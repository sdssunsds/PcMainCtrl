using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.Network
{
    public class Network
    {
        /// <summary>
        /// TcpServer服务器角色
        /// </summary>
        public class TcpServiceSocket
        {
            //连接信息事件
            public Action<Socket> accpetInfoEvent = null;

            //接收数据事件
            public Action<Socket, string> recvMessageEvent = null;

            //发送结果事件
            public Action<int> sendResultEvent = null;

            //允许连接到tcp服务器的tcp客户端数量
            private int numConnections = 0;

            //连接socket
            private Socket listenSocket = null;

            //tcp服务器ip
            private string host = "";

            //tcp服务器端口
            private int port = 0;

            //控制tcp客户端连接数量的信号量
            private Semaphore maxNumberAcceptedClients = null;

            //tcp连接缓冲区
            private int bufferSize = 1024;

            //客户端session列表
            public List<Socket> clientSockets = null;

            /// <summary>
            /// 初始化socket参数
            /// </summary>
            public TcpServiceSocket(string host, int port, int numConnections)
            {
                if (string.IsNullOrEmpty(host))
                    throw new ArgumentNullException("host cannot be null");

                if (port < 1 || port > 65535)
                    throw new ArgumentOutOfRangeException("port is out of range");

                if (numConnections <= 0 || numConnections > int.MaxValue)
                    throw new ArgumentOutOfRangeException("_numConnections is out of range");

                this.host = host;
                this.port = port;
                this.numConnections = numConnections;
                clientSockets = new List<Socket>();
                maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
            }

            /// <summary>
            /// 创建TcpServer角色
            /// </summary>
            public void Start()
            {
                try
                {
                    listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listenSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
                    listenSocket.Listen(numConnections);
                    AcceptAsync();
                }
                catch (Exception e)
                {
                    AddLog("Network  Exception (Start): " + e.Message, -1);
                    throw e;
                }
            }

            /// <summary>
            /// 接收socket连接
            /// </summary>
            private void AcceptAsync()
            {
                TaskRun(new Action(() =>
                {
                    while (true)
                    {
                        maxNumberAcceptedClients.WaitOne();

                        try
                        {
                            Socket acceptSocket = listenSocket.Accept();
                            if (acceptSocket == null)
                                continue;

                            //添加session
                            lock (clientSockets)
                            {
                                clientSockets.RemoveAll(c => !c.Connected);
                                clientSockets.Add(acceptSocket);
                            }

                            //通知UI已经有UI连接进来
                            AccpetClientInfoAsync(acceptSocket);

                            //拉起任务
                            RecvAsync(acceptSocket);
                        }
                        catch (Exception e)
                        {
                            AddLog("Service Exception (AcceptAsync1): " + e.Message, -1);
                            try
                            {
                                maxNumberAcceptedClients.Release();
                            }
                            catch (Exception ex)
                            {
                                AddLog("Service Exception (AcceptAsync2): " + ex.Message, -1);
                            }
                        }
                    }
                }));
            }

            /// <summary>
            /// 通知客户端连接信息
            /// </summary>
            private void AccpetClientInfoAsync(Socket acceptSocket)
            {
                TaskRun(new Action(() =>
                {
                    try
                    {
                        accpetInfoEvent(acceptSocket); //返回字符串
                    }
                    catch (Exception e)
                    {
                        AddLog("Service Exception (AccpetClientInfoAsync): " + e.Message, -1);
                    }
                }));
            }

            /// <summary>
            /// 接收数据
            /// </summary>
            private void RecvAsync(Socket acceptSocket)
            {
                TaskRun(new Action(() =>
                {
                    int len = 0;
                    byte[] buffer = new byte[bufferSize];

                    try
                    {
                        //表示收取数据成功
                        while ((len = acceptSocket.Receive(buffer, bufferSize, SocketFlags.None)) > 0)
                        {
                            if (recvMessageEvent != null)
                            {
                                recvMessageEvent(acceptSocket, Encoding.UTF8.GetString(buffer, 0, len)); //返回字符串
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CloseClientSocket(acceptSocket);
                        AddLog("Service Exception (RecvAsync): " + e.Message, -1);
                    }
                }));
            }

            /// <summary>
            /// 发送数据
            /// </summary>
            public void SendAsync(Socket acceptSocket, string message)
            {
                TaskRun(new Action(() =>
                {
                    int len = 0;
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    try
                    {
                        if ((len = acceptSocket.Send(buffer, buffer.Length, SocketFlags.None)) > 0)
                        {
                            if (sendResultEvent != null)
                            {
                                sendResultEvent(len);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CloseClientSocket(acceptSocket);
                        AddLog("Service Exception (SendAsync): " + e.Message, -1);
                    }
                }));
            }

            /// <summary>
            /// 向所有客户端广播消息
            /// </summary>
            public void SendMessageToAllClientsAsync(string message)
            {
                TaskRun(new Action(() =>
                {
                    lock (clientSockets)
                    {
                        foreach (var socket in clientSockets)
                        {
                            SendAsync(socket, message);
                        } 
                    }
                }));
            }

            /// <summary>
            /// 关闭一个客户端连接
            /// </summary>
            private void CloseClientSocket(Socket acceptSocket)
            {
                try
                {
                    acceptSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    AddLog("Service Exception (CloseClientSocket1): " + e.Message, -1);
                }

                try
                {
                    acceptSocket.Close();
                }
                catch (Exception e)
                {
                    AddLog("Service Exception (CloseClientSocket2): " + e.Message, -1);
                }

                try
                {
                    maxNumberAcceptedClients.Release();
                }
                catch (Exception e)
                {
                    AddLog("Service Exception (CloseClientSocket3): " + e.Message, -1);
                }
            }

            /// <summary>
            /// 关闭所有客户端连接
            /// </summary>
            public void CloseAllClientSocket()
            {
                lock (clientSockets)
                {
                    try
                    {
                        foreach (var socket in clientSockets)
                        {
                            socket.Shutdown(SocketShutdown.Both);
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("Service Exception (CloseAllClientSocket1): " + e.Message, -1);
                        throw e;
                    }

                    try
                    {
                        foreach (var socket in clientSockets)
                        {
                            socket.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("Service Exception (CloseAllClientSocket2): " + e.Message, -1);
                        throw e;
                    }

                    try
                    {
                        listenSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception e)
                    {
                        AddLog("Service Exception (CloseAllClientSocket3): " + e.Message, -1);
                        throw e;
                    }

                    try
                    {
                        listenSocket.Close();
                    }
                    catch (Exception e)
                    {
                        AddLog("Service Exception (CloseAllClientSocket4): " + e.Message, -1);
                        throw e;
                    }

                    try
                    {
                        maxNumberAcceptedClients.Release(clientSockets.Count);
                        clientSockets.Clear();
                    }
                    catch (Exception e)
                    {
                        AddLog("Service Exception (CloseAllClientSocket5): " + e.Message, -1);
                        throw e;
                    } 
                }
            }
        }

        /// <summary>
        /// Tcp客户端角色
        /// </summary>
        public class TcpClientSocket
        {
            //接收数据事件
            public Action<string> recvMessageEvent = null;
            public Action<byte[], int> recvByteEvent = null;

            //发送结果事件
            public Action<int> sendResultEvent = null;
            
            //连接socket
            public Socket connectSocket = null;
            
            //tcp服务器ip
            private string host = "";
            
            //tcp服务器端口
            private int port = 0;

            //socket缓冲区
            private int bufferSize = 1024;

            public bool? IsLink
            {
                get { return connectSocket?.Connected; }
            }

            public TcpClientSocket(string host, int port)
            {
                if (string.IsNullOrEmpty(host))
                    throw new Exception("IP地址非法【IP: " + host + "】");
                if (port < 1 || port > 65535)
                    throw new Exception("端口号非法【Port: " + port + "】");

                this.host = host;
                this.port = port;
            }

            public bool Start(int protocol)
            {
                try
                {
                    connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    connectSocket.Connect(host, port);
                    if (protocol == 0)
                    {
                        RecvAsync();
                    }
                    else if(protocol == 1)
                    {
                        RecvByteAsync();
                    }
                }
                catch (Exception e)
                {
                    AddLog("Client Exception (Start): " + e.Message, -1);
                    return false;
                }
                return true;
            }

            private void RecvByteAsync()
            {
                TaskRun(new Action(() =>
                {
                    int len = 0;
                    byte[] buffer = new byte[bufferSize];
                    try
                    {
                        //表示收取数据成功
                        while (connectSocket != null &&
                              (len = connectSocket.Receive(buffer, bufferSize, SocketFlags.None)) > 0)
                        {
                            if (recvByteEvent != null)
                                recvByteEvent(buffer, len);
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("Client Exception (RecvByteAsync): " + e.Message, -1);
                        Restart(1);
                    }
                }));
            }

            private void RecvAsync()
            {
                TaskRun(new Action(() =>
                {
                    int len = 0;
                    byte[] buffer = new byte[bufferSize];
                    try
                    {
                        //表示收取数据成功
                        while (connectSocket != null &&
                              (len = connectSocket.Receive(buffer, bufferSize, SocketFlags.None)) > 0)
                        {
                            if (recvMessageEvent != null)
                                recvMessageEvent(Encoding.UTF8.GetString(buffer, 0, len));
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("Client Exception (RecvAsync): " + e.Message, -1);
                        Restart(0);
                    }
                }));
            }

            public void SendByteAsync(byte[] buffer, int bufferlen)
            {
                TaskRun(new Action(() =>
                {
                    int len = 0;
                    try
                    {
                        if (connectSocket != null &&
                           (len = connectSocket.Send(buffer, bufferlen, SocketFlags.None)) > 0)
                        {
                            if (sendResultEvent != null)
                                sendResultEvent(len);
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("Client Exception (SendByteAsync): " + e.Message, -1);
                        Restart(1);
                    }
                }));
            }

            public void SendAsync(string message)
            {
                TaskRun(new Action(() =>
                {
                    int len = 0;
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    try
                    {
                        if (connectSocket != null &&
                           (len = connectSocket.Send(buffer, buffer.Length, SocketFlags.None)) > 0)
                        {
                            if (sendResultEvent != null)
                                sendResultEvent(len);
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog("Client Exception (SendAsync): " + e.Message, -1);
                        Restart(0);
                    }
                }));
            }

            public void CloseClientSocket()
            {
                try
                {
                    connectSocket?.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    AddLog("Client Exception (CloseClientSocket1): " + e.Message, -1);
                }
                try
                {
                    connectSocket?.Close();
                }
                catch (Exception e)
                {
                    AddLog("Client Exception (CloseClientSocket2): " + e.Message, -1);
                }
            }

            public void Restart(int protocol)
            {
                CloseClientSocket();
                while (!Start(protocol))
                {
                    AddLog("Socket尝试重连...");
                    ThreadSleep(3000);
                }
            }
        }
    }
}
