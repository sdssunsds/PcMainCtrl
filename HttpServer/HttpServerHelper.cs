using Newtonsoft.Json;
using PcMainCtrl.Common;
using PcMainCtrl.DataAccess.DataEntity;
using PcMainCtrl.HardWare.Robot;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using static PcMainCtrl.Common.GlobalValues;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.HttpServer
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer srv;

        private Stream inputStream;
        public StreamWriter outputStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.socket = s;
            this.srv = srv;
        }

        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { ThreadSleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        public void process()
        {
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            inputStream = new BufferedStream(socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
            try
            {
                parseRequest();
                readHeaders();
                if (http_method.Equals("GET"))
                {
                    handleGETRequest();
                }
                else if (http_method.Equals("POST"))
                {
                    handlePOSTRequest();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                writeFailure();
            }
            outputStream.Flush();
            // bs.Flush(); // flush any remaining output
            inputStream = null; outputStream = null; // bs = null;            
            socket.Close();
        }

        public void parseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_versionstring = tokens[2];

            Console.WriteLine("starting: " + request);
        }

        public void readHeaders()
        {
            Console.WriteLine("readHeaders()");
            String line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                string value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}", name, value);
                httpHeaders[name] = value;
            }
        }

        public void handleGETRequest()
        {
            srv.handleGETRequest(this);
        }

        public void handlePOSTRequest()
        {
            Console.WriteLine("get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.httpHeaders.ContainsKey("Content-Length"))
            {
                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));
                }
                byte[] buf = new byte[Properties.Settings.Default.HttpBufferSize];
                int to_read = content_len;
                while (to_read > 0)
                {
                    Console.WriteLine("starting Read, to_read={0}", to_read);

                    int numread = this.inputStream.Read(buf, 0, Math.Min(Properties.Settings.Default.HttpBufferSize, to_read));
                    Console.WriteLine("read finished, numread={0}", numread);
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            Console.WriteLine("get post data end");
            srv.handlePOSTRequest(this, new StreamReader(ms));
        }

        public void writeSuccess()
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type:application/json");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void writeFailure()
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }

    public abstract class HttpServer
    {

        protected int port;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            try
            {
                listener = new TcpListener(port);
                listener.Start();
                while (is_active)
                {
                    TcpClient s = listener.AcceptTcpClient();
                    HttpProcessor processor = new HttpProcessor(s, this);
                    ThreadStart(processor.process);
                    ThreadSleep(10);
                }
            }
            catch (Exception e)
            {
                AddLog("HttpServerHelper.class listen: " + e.Message, -1);
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);

        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }

    public class MyHttpServer : HttpServer
    {
        /// <summary>
        /// 委托+事件 = 回调函数，用于传递Stm32的消息
        /// </summary>
        /// <param name="stm32info"></param>
        public delegate void HttpServerModInfo(AppRemoteCtrl_DeviceFrame httpdata);
        public event HttpServerModInfo HttpServerModInfoEvent;

        public MyHttpServer(int port)
            : base(port)
        {

        }

        public override void handleGETRequest(HttpProcessor p)
        {
            Console.WriteLine("request: {0}", p.http_url);
            p.writeSuccess();
            p.outputStream.WriteLine("<html><body><h1>test server</h1>");
            p.outputStream.WriteLine("Current Time: " + DateTime.Now.ToString());
            p.outputStream.WriteLine("url : {0}", p.http_url);

            p.outputStream.WriteLine("<form method=post action=/form>");
            p.outputStream.WriteLine("<input type=text name=foo value=foovalue>");
            p.outputStream.WriteLine("<input type=submit name=bar value=barvalue>");
            p.outputStream.WriteLine("</form>");
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            Console.WriteLine("POST request: {0}", p.http_url);

            string data = inputData.ReadToEnd();
            Trace.WriteLine("\r\nPOST data: {0}\r\n", data);

            AppRemoteCtrl_RspFrame rsp = new AppRemoteCtrl_RspFrame();
            AppRemoteCtrl_DeviceFrame frame = new AppRemoteCtrl_DeviceFrame();
            //解析命令字
            try
            {
                //将收到的数据JSON序列化
                frame = JsonConvert.DeserializeObject<AppRemoteCtrl_DeviceFrame>(data);
                //开始任务
                if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_StartWork))
                {
                    //判断当前机器人状态
                    if (GlobalValues.UserInfo.myDeviceStat == UserEntity.key_DEVICE_INIT || GlobalValues.UserInfo.myDeviceStat == UserEntity.key_DEVICE_IDLE)
                    {
                        //得到数据,发送事件(用事件传递任务命令和任务参数)
                        HttpServerModInfoEvent(frame);

                        //回复
                        rsp.cmd = frame.cmd;
                        rsp.para.msg_code = UserEntity.key_DEVICE_IDLE;
                        rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                    }
                    else
                    {
                        //回复
                        rsp.cmd = frame.cmd;
                        rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                        rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                    }
                }
                //结束任务
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_StopWork))
                {
                    //判断当前机器人状态
                    if (GlobalValues.UserInfo.myDeviceStat == UserEntity.key_DEVICE_IDLE)
                    {
                        //得到数据,发送事件(用事件传递任务命令和任务参数)
                        HttpServerModInfoEvent(frame);

                        //回复
                        rsp.cmd = frame.cmd;
                        rsp.para.msg_code = UserEntity.key_DEVICE_BUSY;
                        rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                    }
                    else
                    {
                        //回复
                        rsp.cmd = frame.cmd;
                        rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                        rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                    }
                }
                //查询状态
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceQueryStat))
                {
                    //回复
                    rsp.cmd = frame.cmd;
                    rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                    rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                }
                //紧急停止
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceStop))
                {
                    //得到数据,发送事件(用事件传递任务命令和任务参数)
                    HttpServerModInfoEvent(frame);

                    //回复
                    rsp.cmd = frame.cmd;
                    rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                    rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                }
                //清除报警
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceClearAlarm))
                {
                    //得到数据,发送事件(用事件传递任务命令和任务参数)
                    HttpServerModInfoEvent(frame);

                    //回复
                    rsp.cmd = frame.cmd;
                    rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                    rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                }
                //自动充电
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DevicePowerCharge))
                {
                    //得到数据,发送事件(用事件传递任务命令和任务参数)
                    HttpServerModInfoEvent(frame);

                    //回复
                    rsp.cmd = frame.cmd;
                    rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                    rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                }
                //回归原点
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceRZSite))
                {
                    //得到数据,发送事件(用事件传递任务命令和任务参数)
                    HttpServerModInfoEvent(frame);

                    //回复
                    rsp.cmd = frame.cmd;
                    rsp.para.msg_code = UserEntity.key_DEVICE_BUSY;
                    rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);

                    for (int i = 0; i < 5; i++)
                    {
                        ThreadSleep(1000);
                        if (RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.FrontRobotRunStatMonitor != eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE &&
                            RobotModCtrlHelper.GetInstance().myRobotGlobalInfo.BackRobotRunStatMonitor != eROBOTMODRUNSTAT.ROBOTMODRUNSTAT_MOVE)
                        {
                            rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                            rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                            break;
                        }
                    }
                }
                //设备自检
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DeviceCheckSelf))
                {
                    //得到数据,发送事件(用事件传递任务命令和任务参数)
                    HttpServerModInfoEvent(frame);

                    //回复
                    rsp.cmd = frame.cmd;
                    rsp.para.msg_code = GlobalValues.UserInfo.myDeviceStat;
                    rsp.para.msg_content = GlobalValues.UserInfo.GetState(rsp.para.msg_code);
                }
                //设备关机
                else if (frame.cmd.StartsWith(AppRemoteCtrl_DeviceFrame.HostCmd_DevicePowerOff))
                {
                    //得到数据,发送事件(用事件传递任务命令和任务参数)
                    HttpServerModInfoEvent(frame);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddLog("AlarmHost服务器接收数据失败:" + ex.Message, -1);
            }

            //应答客户端
            string msg = JsonConvert.SerializeObject(rsp);
            AddLog("Http应答：" + msg);
            p.writeSuccess();
            p.outputStream.WriteLine("{0}", msg);
        }
    }

    public class HttpServerHelper
    {
        /// <summary>
        /// AppHttpserver模块
        /// </summary>
        private static HttpServerHelper instance;
        private HttpServerHelper() { }
        public static HttpServerHelper GetInstance()
        {
            return instance ?? (instance = new HttpServerHelper());
        }

        public MyHttpServer httpServer = new MyHttpServer(Properties.Settings.Default.HttpServerPort);

        public int CreatMyHttpserver()
        {
            TaskRun(new Action(() =>
            {
                httpServer.listen();
            }));

            return 0;
        }
    }
}
