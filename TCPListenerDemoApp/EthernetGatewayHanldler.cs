using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPListenerDemoApp
{
    public class EthernetGatewayHandler
    {
        /// <summary>
        /// socket object gateway handler
        /// </summary>
        private Socket socketGatewayHandler;

        private bool isError = false;

        public ConcurrentStack<string> ErrorMessage = new ConcurrentStack<string>();

        private string result = String.Empty;

        string deviceAddress = string.Empty;

        /// <summary>
        /// Array of received bytes
        /// </summary>
        private ConcurrentQueue<byte[]> recBytes;
        private byte[] UnreadBytes;

        private StateObject state = new StateObject { BufferSize = 2048 };

        /// <summary>
        /// Initializes the connection
        /// </summary>
        /// <param name="objSocket">socket object</param>
        /// <returns>true or false</returns>
        public bool Initialize(TcpClient objSocket)
        {
            if (objSocket == null)
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: CP Error in Communication Plugin Socket Object is NULL, @DeviceId: " + DeviceId);
                return false;
            }

            try
            {
                // LogObj = logObj;
                this.recBytes = new ConcurrentQueue<byte[]>();
                this.UnreadBytes = new byte[] { };

                this.socketGatewayHandler = objSocket.Client;
                this.socketGatewayHandler.DontFragment = true;
                this.socketGatewayHandler.SendTimeout = 10000;
                this.socketGatewayHandler.ReceiveTimeout = 10000;
                // deviceAddress = IPAddress.Parse(((IPEndPoint)objSocket.Client.RemoteEndPoint).Address.ToString()) + ":" + ((IPEndPoint)objSocket.Client.RemoteEndPoint).Port.ToString();
            }
            catch (Exception ex)
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: Error Occurred Initialize() Method with @DeviceId: " + DeviceId, ex);
                throw ex;
            }
            return true;
        }

        public bool InitializeAsync()
        {
            /**********************************AsynReceive**************************/
            try
            {
                this.socketGatewayHandler.BeginReceive(state.Buffer, 0, state.BufferSize, 0, this.ReadCallback, state);
            }
            catch (Exception ex)
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: CP Error in InitializeAsync() Method with @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress, ex);
                return false;
            }
            /**********************************AsynReceive**************************/
            return true;
        }

        /// <summary>
        /// Sends the command to the device
        /// </summary>
        /// <param name="cmd">byte array</param>
        /// <returns>true or false</returns>
        public bool Send(byte[] cmd)
        {
            bool issent = false;

            if (this.socketGatewayHandler != null)
            {
                try
                {
                    this.socketGatewayHandler.Send(cmd);
                    //LogObj.WriteLog(LogLevel.Info, "EthernetGateWayHandler.Send: CP Data Sent to Device, @Cmd: " + BitConverter.ToString(cmd) + " @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                    issent = true;
                }
                catch (Exception ex)
                {
                    //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler.Send: CP Error in Communication Plugin -Send() Method @Cmd: " + BitConverter.ToString(cmd) + " @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress, ex);
                    ErrorMessage.Push(ex.Message);
                    isError = true;
                }
            }
            else
            {
                throw new NullReferenceException();
            }

            return issent;
        }

        /// <summary>
        /// Receive the data from the device
        /// </summary>
        /// <returns>byte array</returns>
        public List<byte[]> Receive()
        {
            /**********************************AsynReceive**************************/
            var buf = new List<byte[]>();

            //this.LogObj.WriteLog(LogLevel.Info, $"EthernetGateWayHandler.Receive: Receiving #{this.recBytes.Count} messages @DeviceId: {this.DeviceId} @DeviceAddress: {this.DeviceAddress}");

            var messages = new StringBuilder();

            while (this.recBytes.TryDequeue(out byte[] message))
            {
                buf.Add(message);
                messages.Append($" {BitConverter.ToString(message)}, ");
            }

            //this.LogObj.WriteLog(LogLevel.Info, $"EthernetGateWayHandler.Receive: #{buf.Count} messages Dequeued, All messages Dequeued: {messages} @DeviceId: {this.DeviceId} @DeviceAddress: {this.DeviceAddress}");

            return buf;
            /**********************************AsynReceive**************************/
        }

        /// <summary>
        /// Receive the data from the device
        /// </summary>
        /// <returns>byte array</returns>
        public List<byte[]> SyncReceive()
        {
            // Receive the response from the remote device.
            /**********************************AsynReceive**************************/
            // Data buffer for incoming data.
            var recByte = new List<byte[]>();
            byte[] bytes = new byte[1024];
            int bytesRec = this.socketGatewayHandler.Receive(bytes);
            if (bytesRec > 0)
            {
                Array.Resize(ref bytes, bytesRec);
                recByte.Add(bytes);
                //LogObj.WriteLog(LogLevel.Info, "EthernetGateWayHandler: CP Data Received as WorkItem Response CP: " + BitConverter.ToString(bytes) + " @DeviceId: " + DeviceId);
            }
            return recByte;
            /**********************************AsynReceive**************************/
        }

        /// <summary>
        /// disconnect the connection from the device
        /// </summary>
        /// <returns>true or false</returns>
        public bool Disconnect()
        {
            bool isDisconnected = false;
            if (this.socketGatewayHandler == null)
            {
                throw new NullReferenceException();
            }
            try
            {

                if (this.socketGatewayHandler.Connected)
                {
                    this.socketGatewayHandler.Disconnect(false);
                    this.socketGatewayHandler.Close();
                    this.socketGatewayHandler.Dispose();
                    this.socketGatewayHandler = null;
                    isDisconnected = true;
                }
            }
            catch (Exception e)
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: CP Error in Communication Plugin - Disconnect() Method With @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress + e.Message, e);
                ErrorMessage.Push(e.Message);

                isError = true;
            }
            return isDisconnected;
        }

        /// <summary>
        /// Is connected
        /// </summary>
        /// <returns>returns true or false</returns>
        public bool IsConnected()
        {
            if (socketGatewayHandler == null)
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: Socket  object is NULL, @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                //if (!string.IsNullOrEmpty(DeviceId))
                //    if (QueueManager.GateMap.ContainsKey(DeviceId))
                //        QueueManager.GateMap[DeviceId].Set();
                return false;
            }

            if ((!socketGatewayHandler.Connected || isError))
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: Socket is not Connected isError, @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                //if (!string.IsNullOrEmpty(DeviceId))
                //    if (QueueManager.GateMap.ContainsKey(DeviceId))
                //        QueueManager.GateMap[DeviceId].Set();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Asynchronous read call back
        /// </summary>
        /// <param name="ar">Asynchronous result</param>
        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject)ar.AsyncState;
                if (this.socketGatewayHandler != null)
                {
                    if (this.socketGatewayHandler.Connected)
                    {
                        int bytesRead = this.socketGatewayHandler.EndReceive(ar);



                        //LogObj.WriteLog(LogLevel.Info, $"EthernetGateWayHandler: CP Data Received. Byte Count: {bytesRead} @DeviceId: {this.DeviceId} @DeviceAddress: {this.DeviceAddress}");
                        if (bytesRead > 0)
                        {
                            byte[] rec = state.Buffer;
                            Array.Resize(ref rec, bytesRead);

                            this.recBytes.Enqueue(rec);


                            //LogObj.WriteLog(LogLevel.Info, "EthernetGateWayHandler.ReadCallback: CP Data Received as Notification CP: " + BitConverter.ToString(rec) + " @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                            if (!string.IsNullOrEmpty(DeviceId))
                            {
                                //if (QueueManager.GateMap.ContainsKey(DeviceId))
                                //{
                                //    QueueManager.GateMap[DeviceId].Set();
                                //    //LogObj.WriteLog(LogLevel.Info, "EthernetGateWayHandler.ReadCallback: CP Data Received Event Triggered, @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                                //}
                                //else
                                //    //LogObj.WriteLog(LogLevel.Info, "EthernetGateWayHandler.ReadCallback: CP Data Received Failed to Trigger Event - Map dont have value, @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                            }
                            else
                            {
                                //LogObj.WriteLog(LogLevel.Info, "EthernetGateWayHandler.ReadCallback: CP Data Received Failed to Trigger Event, @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                            }
                            StringBuilder messageBuilder = new StringBuilder();
                            messageBuilder.Append(Encoding.UTF8.GetString(rec));
                            Console.WriteLine(messageBuilder.ToString());
                            //foreach (var item in stt)
                            //{
                            // }
                        }
                        // shu:  The available data is the total amount of data queued in the network buffer for reading
                        if (socketGatewayHandler.Poll(0, SelectMode.SelectRead)
                            && socketGatewayHandler.Available == 0)
                        {
                            //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: Socket is not Connected - POLL. @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                            HandleSocketError("EthernetGateWayHandler: Socket is not Connected - POLL , @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress);
                            return;
                        }

                        var stateObj = new StateObject { BufferSize = 2048 };
                        this.socketGatewayHandler.BeginReceive(stateObj.Buffer, 0, stateObj.BufferSize, 0, this.ReadCallback, stateObj);
                    }
                }
            }
            catch (SocketException ex)
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: CP Error- SocketEx Read Call Back,ReadCallback() Method Ex with @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress + " @ErrorCode: " + ex.ErrorCode + " @Ex: " + ex.Message, ex);
                HandleSocketError(ex.Message);
            }
            catch (Exception ex)
            {
                //LogObj.WriteLog(LogLevel.Error, "EthernetGateWayHandler: CP Error-Ex in Read Call Back,ReadCallback() Method with @DeviceId: " + DeviceId + " @DeviceAddress: " + DeviceAddress + " @Ex: " + ex.Message, ex);
                HandleSocketError(ex.Message);
            }
        }

        private void HandleSocketError(string message)
        {
            ErrorMessage.Push(message);
            isError = true;
            //if (!string.IsNullOrEmpty(DeviceId))
            //    if (QueueManager.GateMap.ContainsKey(DeviceId))
            //        QueueManager.GateMap[DeviceId].Set();
        }

        public string GetLastError()
        {
            if (ErrorMessage.TryPop(out result))
                return result;
            return string.Empty;
        }

        private string deviceId;

        public string DeviceId
        {
            get
            {
                return deviceId;
            }
            set
            {
                deviceId = value;
            }
        }

        public string DeviceAddress
        {
            get
            {
                return deviceAddress;
            }

            set
            {
                deviceAddress = value;
            }
        }
    }

    internal class StateObject
    {
        /// <summary>
        /// buffer size
        /// </summary>
        private int bufferSize = 0;

        /// <summary>
        /// Byte array buffer
        /// </summary>
        private byte[] buffer = null;

        /// <summary>
        /// Gets or sets Buffer size
        /// </summary>
        public int BufferSize
        {
            get
            {
                return this.bufferSize;
            }

            set
            {
                this.bufferSize = value;
                this.buffer = new byte[this.bufferSize];
            }
        }

        /// <summary>
        /// Gets array Buffer 
        /// </summary>
        public byte[] Buffer
        {
            get { return this.buffer; }
        }
    }
}
