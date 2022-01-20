// Traffic-Man Version 0.0.2-a 
// Copyright 2018 Daniel J. Thompson (a.k.a.) Cryptoalien 
// Get traffic with proxys - threaded goodness
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;


namespace Traffic_Man
{
    public partial class Form1 : Form
    {        
        // Variables and shiz
        // datatables
        public DataTable ThreadData = new DataTable();
        public AddThreads Add_Threads = new AddThreads();
        // Integers
        public int TogMove;
        public int MValX;
        public int MValY;
        public int threadCount = 0;
        public int numThread = 0;
        public int currentThreads = 0;
        public int runningThreads = 0;
        public int totalHits = 0;
        private int DisCount = 0;
        private int updaterThreads = 0;
        // Booleans
        public bool threadsStarted;
        public bool startingThreads { get; private set; }
        public bool isClosing = false;
        public bool threadsStopped = false;
        public bool wsConnected;
        public bool txtProxyListloaded;
        private bool bupThread0;
        private bool bupThread1;
        private bool bupThread2;
        private bool bupThread3;
        private bool bupThread4;
        // Lists
        private IList<Thread> _threads; // List to index threads
        public List<string> list_lines; // = new List<string>(lines); --Proxy List
        //public List<Tuple<string, string, int>> RowUUpdateList; // List of updates for RowUpdater thread
        public List<Tuple<string, string, int>> RowUUpdateList = new List<Tuple<string, string, int>>();
        public List<DataRow> RowUUpdateListR = new List<DataRow>();
        public ConcurrentQueue<DataRow> queue = new ConcurrentQueue<DataRow>(); 
        // Forms
        public PasswordForm _PasswordForm = new PasswordForm(); // Login Username & Password form
        // Threads
        public Thread threadWatcher;// ThreadWatcher
        public Thread rthread; // Thread to remove
        private Thread UpThread0; // row updater 0
        private Thread UpThread1; // row updater 1
        private Thread UpThread2; // row updater 2
        private Thread UpThread3; // row updater 3
        private Thread UpThread4; // row updater 4
        // Strings & Hashes
        public string txtProxyListPath;
        public string[] lines;// = File.ReadAllLines(txtProxyListPath);
        private string cnfPassphrase = "lllooonnngggaaassssssSSTTRRIINNGG12345678900987654321";
        private string wsPassphrase = "lllooonnngggaaassssssSSTTRRIINNGG12345678900987654321";
        // WebSocket & Shiz
        public WebSocket web = new WebSocket("ws://127.0.0.1:8980");
        private bool stoppingThreads;


        // Characters & Symbols
        public const char quoteSingleRight = (char)0x274c;
        public const char BallotBox = (char)0x2610;
        public const char HeavyMinus = (char)0x2582;   //268a        - 271a 2795 2796
        public const char Square4Corners = (char)0x25a2;        
        // Other Shiz
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect, // x-coordinate of upper-left corner
        int nTopRect, // y-coordinate of upper-left corner
        int nRightRect, // x-coordinate of lower-right corner
        int nBottomRect, // y-coordinate of lower-right corner
        int nWidthEllipse, // height of ellipse
        int nHeightEllipse // width of ellipse
        );
        
        public Form1()
        {
            InitializeComponent();
            // Radius corners of form
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
            // create lists
            _threads = new List<Thread>();
            

        }

        public class WebClientWithTimeout : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest wr = base.GetWebRequest(address);
                wr.Timeout = 3000; // timeout in milliseconds (ms)
                return wr;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DebugBox("Traffic-Man Loaded...");
            // Set special chars and symbols
            label8.Text = "Start";
            //label10.Text = Square4Corners.ToString();
            label11.Text = HeavyMinus.ToString();
            label9.Text = quoteSingleRight.ToString();
            // Add columns to datatable
            ThreadData.Columns.Add("Thread", typeof(String));
            ThreadData.Columns.Add("Thread Status", typeof(String));
            ThreadData.Columns.Add("Thread Data", typeof(String));
            //richTextBox1.Font = new System.Drawing.Font("System", 10);
            // Set datagrid source and format
            // test shiz 
            //Create New DataGridViewTextBoxColumn
            //DataGridViewTextBoxColumn ThreadColumn = new DataGridViewTextBoxColumn();
            //DataGridViewTextBoxColumn ThreadStatusColumn = new DataGridViewTextBoxColumn();
            //DataGridViewTextBoxColumn ThreadDataColumn = new DataGridViewTextBoxColumn();
            //dataGridView1.Columns.Add(ThreadColumn);
            //dataGridView1.Columns.Add(ThreadStatusColumn);
            //dataGridView1.Columns.Add(ThreadDataColumn);
            
            //Loop through DataGridView
            //foreach (DataGridViewRow row in dataGridView1.Rows)

            //{

            //Do your task here
            //  string fourthColumn = row.Cells[4].Value.toString();

            //}
            // end test
            dataGridView1.DataSource = ThreadData;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.Black;
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.Black;
            dataGridView1.DefaultCellStyle.ForeColor = Color.LightGray;
            dataGridView1.DefaultCellStyle.BackColor = Color.Black;
            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[1].Width = 108;
            dataGridView1.Columns[2].Width = 598;
            dataGridView1.Columns[0].HeaderCell.Style.SelectionForeColor = Color.Black;
            dataGridView1.Columns[0].HeaderCell.Style.SelectionBackColor = Color.LightGray;
            dataGridView1.Columns[0].HeaderCell.Style.ForeColor = Color.LightGray;
            dataGridView1.Columns[0].HeaderCell.Style.BackColor = Color.Black;
            dataGridView1.Columns[1].HeaderCell.Style.SelectionForeColor = Color.Black;
            dataGridView1.Columns[1].HeaderCell.Style.SelectionBackColor = Color.LightGray;
            dataGridView1.Columns[1].HeaderCell.Style.ForeColor = Color.LightGray;
            dataGridView1.Columns[1].HeaderCell.Style.BackColor = Color.Black;
            dataGridView1.Columns[2].HeaderCell.Style.SelectionForeColor = Color.Black;
            dataGridView1.Columns[2].HeaderCell.Style.SelectionBackColor = Color.LightGray;
            dataGridView1.Columns[2].HeaderCell.Style.ForeColor = Color.LightGray;
            dataGridView1.Columns[2].HeaderCell.Style.BackColor = Color.Black;
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                //Change cell font
                c.DefaultCellStyle.Font = new Font("Segoe UI", 11.75F, GraphicsUnit.Pixel);//Segoe UI Semibold, 8.25pt, style=Bold
            }
            isClosing = false;
            // websocket control shiz
            web.OnOpen += Web_OnOpen;
            web.OnClose += Web_OnClose;
            web.OnMessage += Web_OnMessage;
            web.OnError += Web_OnError;
            // thuple list
            
            Tuple<string, string, int> tuple = new Tuple<string, string, int>("PING", "PONG", 0);
            RowUUpdateList.Add(tuple);
            // Dispatch the main threads
            Task.Factory.StartNew(ThreadWatcher);
            Task.Factory.StartNew(SocketWatcher);
            Task.Factory.StartNew(RowUpdater);
            // Startup Helpers
            updateThreadCount();
        }
        // row updater -- to do  only one thread should access the datatable -- to fix errors hopefully
        private void RowUpdater()
        {


            // setup threads
            //TabControl1.TabPages.Remove(TabPage1) 'Could be male
            this.Invoke(new Action(() => tabControl1.TabPages.Remove(tabPage2))); //'Could be female
            //TabControl1.TabPages.Insert(0, TabPage1) 'Show male
            //TabControl1.TabPages.Insert(1, TabPage2) 'Show female
            void AddIt()
            {
                while (true)
                {
                    try
                    {
                        // for each loop in list 
                        //foreach (DataRow dtr in queue)//Tuple<string, string, int> upTuple in RowUUpdateList) // string cmd, string msg, int rowI
                        //{
                            queue.TryDequeue(out DataRow dtrO);
                            switch (dtrO[0])
                            {
                                case "ADD":
                                    // Add row to DataTable
                                    this.Invoke(new Action(() => addThreadRow(Convert.ToInt32(dtrO[2]))));
                                    //RowUUpdateListR.Remove(dtr);
                                    this.Invoke(new Action(() => DebugBox("RowCreator - Row: " + dtrO[2] + " Added")));
                                    break;
                                case "REM":
                                    // Remove row from DataTable
                                    this.Invoke(new Action(() => remThreadRow(Convert.ToInt32(dtrO[2]))));
                                    //RowUUpdateListR.Remove(dtr);
                                    this.Invoke(new Action(() => DebugBox("RowCreator - Row: " + dtrO[2] + " Removed")));
                                    break;
                                default:
                                    break;
                            }
                            //Thread.Sleep(1);
                        //}
                    }
                    catch
                    {
                        // list empty move on
                    }
                    Thread.Sleep(2);
                }
            }
            // start row creator thread
            //Thread AddThread = new Thread(AddIt);
            //UpThread0.IsBackground = true;
            //AddThread.Start();
            //this.Invoke(new Action(() => DebugBox("RowCreator - Started...")));
            // updater loop  
            this.Invoke(new Action(() => DebugBox("RowUpdater - Started...")));
            while (true)
            {

                // start first thread
                if (runningThreads > 0 && updaterThreads == 0)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Starting Thread 0...")));
                    UpThread0 = new Thread(RunIt);
                    //UpThread0.IsBackground = true;
                    UpThread0.Start();
                    bupThread0 = true;
                    updaterThreads = 1;
                }
                // start second thread
                if (runningThreads > 100 && updaterThreads == 1)
                {

                    this.Invoke(new Action(() => DebugBox("RowUpdater - Starting Thread 1...")));
                    UpThread1 = new Thread(RunIt);
                    //UpThread1.IsBackground = true;
                    UpThread1.Start();
                    bupThread1 = true;
                    updaterThreads = 2;
                }
                // start third thread
                if (runningThreads > 400 && updaterThreads == 2)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Starting Thread 2...")));
                    UpThread2 = new Thread(new ThreadStart(RunIt));
                    UpThread2.IsBackground = true;
                    UpThread2.Start();
                    bupThread2 = true;
                    updaterThreads = 3;
                }
                // start forth thread
                if (runningThreads > 600 && updaterThreads == 3)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Starting Thread 3...")));
                    UpThread3 = new Thread(new ThreadStart(RunIt));
                    UpThread3.IsBackground = true;
                    UpThread3.Start();
                    bupThread3 = true;
                    updaterThreads = 4;
                }
                // start fifth thread
                if (runningThreads > 800 && updaterThreads == 4)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Starting Thread 4...")));
                    UpThread4 = new Thread(new ThreadStart(RunIt));
                    UpThread4.IsBackground = true;
                    UpThread4.Start();
                    bupThread4 = true;
                    updaterThreads = 5;
                }
                // stop fifth thread
                if (runningThreads < 800 && updaterThreads == 5)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Stopping Thread 4...")));
                    UpThread4.Abort();
                    bupThread4 = false;
                    updaterThreads = 4;
                }
                // stop forth thread
                if (runningThreads < 600 && updaterThreads == 4)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Stopping Thread 3...")));
                    UpThread3.Abort();
                    bupThread3 = false;
                    updaterThreads = 3;
                }
                // stop third thread
                if (runningThreads < 400 && updaterThreads == 3)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Stopping Thread 2...")));
                    UpThread2.Abort();
                    bupThread2 = false;
                    updaterThreads = 2;
                }
                // stop second thread
                if (runningThreads < 200 && updaterThreads == 2)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Stopping Thread 1...")));
                    UpThread1.Abort();
                    bupThread1 = false;
                    updaterThreads = 1;
                }
                // stop first thread
                if (runningThreads < 1 && updaterThreads == 1)
                {
                    this.Invoke(new Action(() => DebugBox("RowUpdater - Stopping Thread 0...")));
                    UpThread0.Abort();
                    bupThread0 = false;
                    updaterThreads = 0;
                }

                //this.Invoke(new Action(() => DebugBox("RowUpdater - PING")));
                void RunIt()
                {
                    while (true)
                    {
                        try
                        {
                            // for each loop in list 
                            //foreach (DataRow dtr in queue)//Tuple<string, string, int> upTuple in RowUUpdateList) // string cmd, string msg, int rowI
                            //{
                                queue.TryDequeue(out DataRow dtrO);
                                switch (dtrO[0])
                                {
                                    case "ADD":
                                        // Add row to DataTable
                                        this.Invoke(new Action(() => addThreadRow(Convert.ToInt32(dtrO[2]))));
                                        //RowUUpdateListR.Remove(dtr);
                                        this.Invoke(new Action(() => DebugBox("RowCreator - Row: " + dtrO[2] + " Added")));
                                        break;
                                    case "REM":
                                        // Remove row from DataTable
                                        this.Invoke(new Action(() => remThreadRow(Convert.ToInt32(dtrO[2]))));
                                        //RowUUpdateListR.Remove(dtr);
                                        this.Invoke(new Action(() => DebugBox("RowCreator - Row: " + dtrO[2] + " Removed")));
                                        break;
                                    case "MSG":
                                        // Send message to row in DataTable
                                        try
                                        {
                                            //ThreadData.BeginLoadData();
                                            ThreadData.Rows[Convert.ToInt32(dtrO[2])].BeginEdit();
                                            ThreadData.Rows[Convert.ToInt32(dtrO[2])][2] = dtrO[1];
                                            ThreadData.Rows[Convert.ToInt32(dtrO[2])].EndEdit();
                                            //ThreadData.EndLoadData();
                                            //dataGridView1[upTuple.Item3, 2].Value = upTuple.Item2.ToString();
                                            //dataGridView1[1, 1].Value = "tes";
                                        }
                                        catch
                                        {
                                            this.Invoke(new Action(() => DebugBox("RowUpdater - Row: " + dtrO[2] + " Error-RowNotThere...")));
                                        }
                                        //this.Invoke(new Action(() => sndRow(upTuple.Item2, upTuple.Item3)));
                                        //RowUUpdateListR.Remove(dtr);
                                        //this.Invoke(new Action(() => DebugBox("RowUpdater - Row: " + upTuple.Item3 + " Message")));
                                        break;
                                    case "INFO":
                                        // Send status to row in DataTable
                                        try
                                        {
                                            //ThreadData.BeginLoadData();
                                            ThreadData.Rows[Convert.ToInt32(dtrO[2])].BeginEdit();                                            
                                            ThreadData.Rows[Convert.ToInt32(dtrO[2])][1] = dtrO[1];
                                            ThreadData.Rows[Convert.ToInt32(dtrO[2])].EndEdit();
                                            //ThreadData.EndLoadData();
                                            //dataGridView1[upTuple.Item3, 1].Value = upTuple.Item2.ToString();
                                        }
                                        catch
                                        {
                                        //DataTable data = queue.CopyToDataTable();
                                            this.Invoke(new Action(() => DebugBox("RowUpdater - Row: " + dtrO[2] + " Error-RowNotThere...")));
                                        }
                                        //RowUUpdateListR.Remove(dtr);
                                        this.Invoke(new Action(() => DebugBox("RowUpdater - Row: " + dtrO[2] + " Status")));
                                        break;
                                    default:
                                        break;
                                }
                                Thread.Sleep(1);
                            //}
                        }
                        catch
                        {
                            // list empty move on
                        }
                        //Thread.Sleep(2);
                    }
                }
                
                //Thread.Sleep(1);
            }
            
            
            //list[0].Item1 //Hello
            ///list[0].Item2 //1

        }

        // proxy shiz
        public string proxyGetter(int rowNum)
        {
            
            sndRow("ProxyGetter - Getting Random Proxy...", rowNum);

            //int Duplicate_Count = 0;
            bool proxyFound = false;
            int badProxy = 0;
            int CheckedCount = 0;
            int ProxyCount = list_lines.Count;
            string randomProxy = "";
            void getRnd()
            {
                Random rnd = new Random();
                int rndProxy = rnd.Next(1, ProxyCount);
                sndRow("ProxyGetter - Random Proxy Index: " + rndProxy, rowNum);
                void chkRnd(int line)// in list_lines)
                {
                    string[] line_char = list_lines[line].Split(':');
                    string ip = line_char[0];
                    string port = line_char[1];
                    sndRow("ProxyGetter - Checking Proxy : " + list_lines[line], rowNum);
                    if (CanPing(ip))
                    {
                        sndRow("ProxyGetter - Proxy Ip Ping Good...", rowNum);
                        if (SoketConnect(ip, port))
                        {
                            sndRow("ProxyGetter - Proxy Socket Good...", rowNum);
                            if (CheckProxy(ip, port))
                            {
                                sndRow("ProxyGetter - Proxy Test Successfull", rowNum);
                                string ipAndport = ip + ":" + port;
                           
                                CheckedCount++;
                                sndRow("ProxyGetter - Proxy Is Good! Continuing...", rowNum);
                                proxyFound = true;
                                randomProxy = ipAndport;
                                
                            }
                            else
                            {
                                sndRow("ProxyGetter - Proxy Test Failed, Retrying...", rowNum);
                                badProxy++;
                                CheckedCount++;
                                randomProxy = "";
                            }
                        }
                        else
                        {
                            sndRow("ProxyGetter - Proxy Socket Bad, Retrying...", rowNum);
                            badProxy++;
                            CheckedCount++;
                        }
                    }
                    else
                    {
                        sndRow("ProxyGetter - Proxy Ip Ping Bad, Retrying...", rowNum);
                        badProxy++;
                        CheckedCount++;
                    }
                }

                bool CanPing(string ip)
                {
                    sndRow("ProxyGetter - Pinging Proxy Ip: " + ip, rowNum);
                    Ping ping = new Ping();

                    try
                    {
                        PingReply reply = ping.Send(ip, 2000);
                        if (reply == null)
                            return false;

                        return (reply.Status == IPStatus.Success);
                    }
                    catch (PingException Ex)
                    {
                        return false;
                    }
                }

                bool SoketConnect(string ip, string port)
                {
                    sndRow("ProxyGetter - Checking Proxy Connection: " + ip + ":" + port, rowNum);
                    var is_success = false;
                    try
                    {
                        var connsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        connsock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 200);
                        System.Threading.Thread.Sleep(500);
                        var hip = IPAddress.Parse(ip);
                        var ipep = new IPEndPoint(hip, int.Parse(port));
                        connsock.Connect(ipep);
                        if (connsock.Connected)
                        {
                            is_success = true;
                        }
                        connsock.Close();
                    }
                    catch (Exception)
                    {
                        is_success = false;
                    }
                    return is_success;
                }

                bool CheckProxy(string ip, string port)
                {
                    sndRow("ProxyGetter - Testing Proxy: " + ip + ":" + port, rowNum);
                    try
                    {
                        WebClient WC = new WebClientWithTimeout();
                        WC.Proxy = new WebProxy(ip, int.Parse(port));
                        WC.DownloadString("https://myip.com");
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                chkRnd(rndProxy);
            }
            
            while (proxyFound == false)
            {
                getRnd();
            }
            return randomProxy;
        }

        // Websocket Watcher
        public void SocketWatcher()
        {
            this.Invoke(new Action(() => DebugBox("SocketWatcher - Started...")));
            // watcher loop
            while (true)
            {
                try
                {
                    //this.Invoke(new Action(() => DebugBox("SocketWatcher - PING...")));
                    // check connection 
                    if (wsConnected == false)
                    {
                        this.Invoke(new Action(() => DebugBox("SocketWatcher - Connecting...")));
                        web.Connect();
                        Thread.Sleep(100);

                    }
                    else if (wsConnected == true)
                    {
                        // do nothing
                    }
                    Thread.Sleep(15000);
                }
                catch
                {
                    // do nothing
                }
            }
        }

        private void Web_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            this.Invoke(new Action(() => DebugBox("SocketWatcher - Error - " + e.Exception)));
        }

        private void Web_OnMessage(object sender, MessageEventArgs e)
        {
            this.Invoke(new Action(() => DebugBox("SocketWatcher - Message - " + e.Data)));
            // decrypt message
            try
            {
                string dMsg = StringCipher.Decrypt(e.Data, wsPassphrase);
                this.Invoke(new Action(() => DebugBox("SocketWatcher - Message Decryption 0: " + dMsg)));
                string[] spMsg = dMsg.Split('@');
                // split0 then case
                this.Invoke(new Action(() => DebugBox("SocketWatcher - Command Received: " + spMsg[0])));
                switch (spMsg[0])
                {
                    case "DISCONNECT":
                        DisCount = DisCount + 1;
                        string dReason = StringCipher.Decrypt(spMsg[1], wsPassphrase);
                        this.Invoke(new Action(() => DebugBox("SocketWatcher - Disconnecting - Reason: " + dReason + " Count: " + DisCount)));
                        web.Close();
                        if (DisCount == 5)
                        {
                            //Kicked
                            
                            string KillReason = "This is a Warning, You have been told...";
                            this.Invoke(new Action(() => DebugBox("SocketWatcher - Killing Program - Reason: " + KillReason)));
                            this.Invoke(new Action(() => OnExit()));
                        }
                        break;
                    default:
                        this.Invoke(new Action(() => DebugBox("SocketWatcher - Command Not Found...")));
                        break;
                }
            }
            catch
            {
                // regular message do nothing
                return;
            }

            // split1 then do case of split0
        }

        private void Web_OnOpen(object sender, EventArgs e)//EventArgs e)
        {
            this.Invoke(new Action(() => DebugBox("SocketWatcher - Connected! - " + e.ToString())));
            wsConnected = true;
            // send info to server
        }

        private void Web_OnClose(object sender, CloseEventArgs e)
        {
            this.Invoke(new Action(() => DebugBox("SocketWatcher - Disconnected! - " + e.Reason)));
            wsConnected = false;
        }


        // Thread Watcher
        private void ThreadWatcher()
        {
            
            while (true)
            {
                Thread.Sleep(250);
                //this.Invoke(new Action(() => DebugBox("ThreadWatcher - Hit count: " + totalHits)));
                this.Invoke(new Action(() => updateHitCount()));
                
                // check if threads started 
                if (threadsStarted == true)
                {
                    // start thrreads
                    if (threadCount < currentThreads)
                    {
                        this.Invoke(new Action(() => DebugBox("ThreadWatcher - Thread Count Change Detected...")));
                        this.Invoke(new Action(() => DebugBox("ThreadWatcher - Running Threads: " + threadCount)));
                        int threadChange = currentThreads - threadCount;
                        this.Invoke(new Action(() => DebugBox("ThreadWatcher - Starting " + threadChange + " Threads...")));
                        while (threadChange > 0)
                        {
                            if (startingThreads == false)
                            {
                                this.Invoke(new Action(() => label8.Text = "Adding.."));
                            }
                            // Start Threads with index
                            this.Invoke(new Action(() => DebugBox("ThreadWatcher - Starting Thread " + threadCount + "...")));
                            // Create the thread  
                            //Tuple<string, string, int> tuple = new Tuple<string, string, int>("ADD","",threadCount);
                            //this.Invoke(new Action(() => RowUUpdateList.Add.("","",tuple)));
                            //RowUUpdateList.
                            this.Invoke(new Action(() => addThreadRowL(threadCount)));  // change to put in list
                            Thread.Sleep(100);
                            Thread thread = new Thread(new ThreadStart(doIt));
                            
                            thread.IsBackground = true;
                            thread.Name = string.Format("MyThread{0}", threadCount);
                            
                            // Add to List
                            _threads.Add(thread);
                            // Start Thread
                            thread.Start();
                            // Thread task
                            void doIt()
                            {
                                int threadNum = threadCount;
                                int rowNum = threadNum - 1;
                                // add row to datatable
                                
                                //int yourPosition = 0;
                                //dt.Rows.InsertAt(dr, yourPosition);
                                
                                //for (int i = 0; i < 100; i++)
                                int i = 0;
                                // add info to list
                                DataRow dr = ThreadData.NewRow();
                                dr[0] = "INFO";
                                dr[1] = "Running...";
                                dr[2] = rowNum;
                                //RowUUpdateListR.Add(dr);
                                queue.Enqueue(dr);
                                //Tuple<string, string, int> tuple = new Tuple<string, string, int>("INFO", "Running...", rowNum);
                                //this.Invoke(new Action(() => RowUUpdateList.Add(tuple)));
                                //ThreadData.Rows[rowNum][1] = "Running"; // change to put in list   
                                while (true)
                                {
                                    // index traffic-runner link attempt
                                    sndRow("Thread " + threadNum + " Check Index: " + i, rowNum);
                                    Thread.Sleep(500);
                                    // get random user agent from list
                                    sndRow("Getting random User Agent from list...", rowNum);
                                    Thread.Sleep(500);
                                    // get random web link from list
                                    sndRow("Getting random Web Link from list...", rowNum);
                                    Thread.Sleep(500);
                                    // get and check random proxy from list
                                    sndRow("Getting and checking random Proxy...", rowNum);
                                    string randomGotProxy = proxyGetter(rowNum);
                                    sndRow("Got random Proxy: " + randomGotProxy, rowNum);
                                    Thread.Sleep(500);
                                    // get random time if random..
                                    Random rnd = new Random();
                                    int sleepTime = rnd.Next(31000, 35000);
                                    int hitTime = sleepTime / 1000;
                                    // start browser session with goodies
                                    sndRow("Starting Browser Session - Link: "+"link"+" Proxy: "+ randomGotProxy +" Time: "+hitTime+" sec", rowNum);
                                    Thread.Sleep(sleepTime);
                                    // record total hit Attempt
                                    totalHits++;
                                    i++;
                                }
                            
                            
                            }

                            
                            // do count updates
                            threadCount++;
                            threadChange--;
                            runningThreads++;
                            Thread.Sleep(100);
                        }
                        this.Invoke(new Action(() => label8.Text = "Stop"));
                        
                    }
                    else if (threadCount > currentThreads)
                    {
                        this.Invoke(new Action(() => DebugBox("ThreadWatcher - Thread Count Change Detected...")));
                        this.Invoke(new Action(() => DebugBox("ThreadWatcher - Running Threads: " + threadCount)));
                        int threadChange = threadCount - currentThreads;
                        int curThreads = threadCount - 1;
                        this.Invoke(new Action(() => DebugBox("ThreadWatcher - Stopping " + threadChange + " Threads...")));
                        while (threadChange > 0)
                        {
                            //if (stoppingThreads == false)
                            //{
                            this.Invoke(new Action(() => label8.Text = "Removing.."));
                            //}
                            this.Invoke(new Action(() => DebugBox("ThreadWatcher - Running Threads: " + threadCount)));
                            this.Invoke(new Action(() => DebugBox("ThreadWatcher - ThreadChange " + threadChange)));
                            this.Invoke(new Action(() => DebugBox("ThreadWatcher - Stopping Thread " + curThreads + "...")));
                            DataRow dr = ThreadData.NewRow();
                            dr[0] = "INFO";
                            dr[1] = "Stopping...";
                            dr[2] = curThreads;
                            //RowUUpdateListR.Add(dr);
                            queue.Enqueue(dr);
                            //Tuple<string, string, int> tuple = new Tuple<string, string, int>("INFO", "Stopping...", curThreads);
                            //this.Invoke(new Action(() => RowUUpdateList.Add(tuple)));
                            // stop thread with index

                            string rid = string.Format("MyThread{0}", curThreads);
                            foreach (Thread thread in _threads)
                            {
                                if (thread.Name == rid)
                                {
                                    thread.Abort();
                                    rthread = thread;
                                }
                                
                            }
                            // remove thread from list
                            _threads.Remove(rthread);
                            //this.Invoke(new Action(() => remThreadRow(curThreads)));
                            // do count updates
                            threadCount--;
                            threadChange--;
                            curThreads--;
                            runningThreads--;
                            // remove thread row
                            //Tuple<string, string, int> tuple = new Tuple<string, string, int>("REM", "", threadCount);
                            //this.Invoke(new Action(() => RowUUpdateList.Add(tuple)));
                            this.Invoke(new Action(() => remThreadRowL(threadCount)));
                            Thread.Sleep(100);
                        }
                        this.Invoke(new Action(() => label8.Text = "Stop"));
                    }
                    else if (threadCount == currentThreads)
                    {
                        startingThreads = false;
                    }
                }
                if (threadsStarted == false)
                {
                    // stop threads
                    if (threadCount != 0)
                    {
                        int curThreads = threadCount - 1;
                        this.Invoke(new Action(() => DebugBox("ThreadWatcher - Stopping Threads...")));
                        while (threadCount > 0)
                        {
                            this.Invoke(new Action(() => DebugBox("ThreadWatcher - Running Threads: " + threadCount)));
                            this.Invoke(new Action(() => DebugBox("ThreadWatcher - Stopping Thread " + curThreads + "...")));
                            
                            // stop thread with index
                            string id = string.Format("MyThread{0}", curThreads);
                            foreach (Thread thread in _threads)
                            {
                                if (thread.Name == id)
                                    thread.Abort();
                                
                            }
                            //this.Invoke(new Action(() => remThreadRow(curThreads)));
                            // do count updates
                            threadCount--;
                            curThreads--;
                            runningThreads--;
                            // remove thread row
                            //Tuple<string, string, int> tuple = new Tuple<string, string, int>("REM", "", threadCount);
                            //this.Invoke(new Action(() => RowUUpdateList.Add(tuple)));
                            this.Invoke(new Action(() => remThreadRowL(threadCount)));
                            Thread.Sleep(100);
                        }
                    }
                    if (threadCount == 0)
                    {
                        //this.Invoke(new Action(() => DebugBox("ThreadWatcher - Thread Count: " + threadCount)));
                        if (runningThreads == 0)
                        {
                            //this.Invoke(new Action(() => DebugBox("ThreadWatcher - Running Threads: " + threadCount)));
                            if (isClosing == true)
                            {
                                //this.Invoke(new Action(() => DebugBox("ThreadWatcher - Threads Stopped...")));

                                threadsStopped = true;
                                break;
                            }
                            this.Invoke(new Action(() => label8.Text = "Start"));
                        }

                    }

                }

            }
            if (threadsStopped == true)
            {
                try
                {
                    this.Invoke(new Action(() => formClose()));

                }
                catch
                {
                    
                }
            }

        }
        // Api requester
        private string ApiRequest(string apiReq)
        {
            string somestring;
            try
            {
                WebClient wc = new WebClientWithTimeout();
                somestring = wc.DownloadString("http://www.example.com/api.php?r=" + apiReq);
                return somestring;
            }
            catch (WebException we)
            {
                // add some kind of error processing
                DebugBox("ApiRequest Error - " + we.ToString());
                return "0";
            }
        }
        // Row Updater
        private void sndRow(string rowMsg, int sndRowNum)
        {
            DataRow dr = ThreadData.NewRow();
            dr[0] = "MSG";
            dr[1] = rowMsg;
            dr[2] = sndRowNum;
            //RowUUpdateListR.Add(dr);
            queue.Enqueue(dr);
            //Tuple<string, string, int> tuple = new Tuple<string, string, int>("MSG", rowMsg, sndRowNum);
            //RowUUpdateList.Add(tuple);
            //try
            //{

            //    ThreadData.Rows[sndRowNum][2] = rowMsg;
            //}
            //catch
            //{
            //    int rowThreadNum = sndRowNum + 1;
            //   this.Invoke(new Action(() => DebugBox("ThreadWatcher - Thread: " + rowThreadNum + " 000-RowNotThere...")));
            //}
        }
        // Thread count Updater
        private void updateThreadCount()
        {
            label6.Text = currentThreads.ToString();
        }

        // Hit count Updater
        private void updateHitCount()
        {
            label3.Text = totalHits.ToString();
        }

        // Debug Box 
        private void DebugBox(string dBugMsg)
        {
            try
            {
                string TimeStamp = DateTime.Now.ToString("@-MM-dd-yyyy-hh:mm:ss-> ");
                richTextBox1.Focus();
                richTextBox1.AppendText(TimeStamp + dBugMsg + Environment.NewLine);
            }
            catch
            {
                // Most likley form closing
            }
        }
        // add thread to row list
        public void addThreadRowL(int index)
        {
            DataRow dr = ThreadData.NewRow();
            dr[0] = "ADD";
            dr[1] = "Starting Thread";
            dr[2] = index;
            //RowUUpdateListR.Add(dr);
            queue.Enqueue(dr);
            //Tuple<string, string, int> tuple = new Tuple<string, string, int>("ADD", "Starting Thread", index);
            //RowUUpdateList.Add(tuple);
        }
        // rem thread from row list
        public void remThreadRowL(int index)
        {
            DataRow dr = ThreadData.NewRow();
            dr[0] = "REM";
            dr[1] = "Removing Thread";
            dr[2] = index;
            //RowUUpdateListR.Add(dr);
            queue.Enqueue(dr);
            //Tuple<string, string, int> tuple = new Tuple<string, string, int>("REM", "Removing Thread", index);
            //RowUUpdateList.Add(tuple);
        }
        // Add Row in Datatable
        private void addThreadRow(int index)
        {
            //dataGridView1.Rows.Add("ItemName", "ItemCode", "Quantity");
            
            try
            {
                DataRow dr = ThreadData.NewRow();
                dr[0] = index;
                dr[1] = "Starting";
                dr[2] = "Thread " + index + " Starting...";
                ThreadData.Rows.Add(dr);
                //dataGridView1.Rows.Add(index, "Starting", msg);
            }
            catch
            {
                this.Invoke(new Action(() => DebugBox("Thread  Row " + index + ": Add Error...")));
            }
        }
        // Remove Row in Datatable
        private void remThreadRow(int index)
        {
            try
            {
                ThreadData.Rows.RemoveAt(index);
            }
            catch
            {
                this.Invoke(new Action(() => DebugBox("Thread  Row " + index + ": Remove Error...")));
            }
        }

        private void startThreads()
        {
            // Start All Threads
            if (txtProxyListloaded == false)
            {
                MessageBox.Show("Please Choose a Proxy File Before Continuing!", "No Proxy File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DebugBox("No Proxy File Chosen...");
                return;
            }
            else if (txtProxyListloaded == true)
            {
                DebugBox("Starting Threads...");
                threadsStarted = true;
                startingThreads = true;
                label8.Text = "Starting..";
            }
        }
        private void stopThreads()
        {
            // Stop All Threads
            if (startingThreads == true)
            {
                DebugBox("Threads Are Still Starting...");
            }
            else if (startingThreads == false)
            {
                DebugBox("Stopping Threads...");
                threadsStarted = false;
                label8.Text = "Stopping..";
            }
        }
        private void OnExit()
        {
            // Stop All Threads then exit
            stopThreads();
            isClosing = true;
        }
        private void formClose()
        {
            // Exit Application
            DebugBox("GoodBye!...");
            Thread.Sleep(100);
            this.Close();
        }
        
     

        // Buttons and thangz
        private void button1_Click(object sender, EventArgs e)
        {
            DebugBox("Test Button Clicked...");
            // Test Shiz
            //DebugBox("Saving Test Variable....");
            string junks = "junks";
            string[] arr = { junks, "crap", "shiz", "stuff" };

            string inString = string.Join(":", arr);
            
            //SavingPlugin.SaveVariable("conf", TypeCode.String, inString, cnfPassphrase);
            Thread.Sleep(500);
            string oString = SavingPlugin.GetVariable("conf", TypeCode.String, cnfPassphrase).ToString();
            string[] words = oString.Split(':');
            DebugBox(SavingPlugin.GetVariable("conf", TypeCode.String, cnfPassphrase).ToString());
            Thread.Sleep(100);
            DebugBox("Variable 1: " + words[0].ToString());
            Thread.Sleep(100);
            DebugBox("Variable 2: " + words[1].ToString());
            Thread.Sleep(100);
            DebugBox("Variable 3: " + words[2].ToString());
            Thread.Sleep(100);
            DebugBox("Variable 4: " + words[3].ToString());
            Thread.Sleep(1000);
            DebugBox("WOOOHHHHHHOOOOOOOO!!!!!!!!");
            // encryption/decryption  websocket  test
            string inMsgCmd = "ECHO"; // the command server needs to do 
            //string inMsgCmdArgs = "3-2-1"; // strings-bools-ints
            string inMsg0 = "Hello Server";
            string inMsg1 = "Decrypt this shiz";
            string inMsg2 = "Then send it back";
            //bool inBool0 = true;
            //bool inBool1 = false;
            //int inInt0 = 0;
            // put together command
            //string[] inpCmdA = { inMsgCmd, inMsgCmdArgs };
            //string inpCmd = string.Join(":", inpCmdA);
            DebugBox(inMsgCmd);
            Thread.Sleep(250);
            // put together everything else
            string[] inpMsgA = { inMsg0, inMsg1, inMsg2, };
            string inpMsg = string.Join(" ", inpMsgA);
            DebugBox(inpMsg);
            Thread.Sleep(250);
            // encrypt inpMsg
            DebugBox("Encrypting Message...");
            Thread.Sleep(100);
            string enMsg = StringCipher.Encrypt(inpMsg, wsPassphrase);
            DebugBox("Encrypted String: " + enMsg);
            Thread.Sleep(100);
            // join inpCmd with encrypted message
            string[] enpMsgA = { inMsgCmd, enMsg };
            string enpMsg = string.Join("@", enpMsgA);
            DebugBox(enpMsg);
            Thread.Sleep(250);
            // encrypt again with added command
            DebugBox("Encrypting Message to send...");
            Thread.Sleep(100);
            string enpMsgToSnd = StringCipher.Encrypt(enpMsg, wsPassphrase);
            DebugBox("Encrypted String: " + enpMsgToSnd);
            Thread.Sleep(100);
            // send Websocket message


            //string dMsg = StringCipher.Decrypt(enMsg, wsPassphrase);
            //DebugBox("Decrypted Message: " + dMsg);
            Thread.Sleep(100);
            //string sndMsg = "Hello Server";
            DebugBox("Sending webSocket message...");// + sndMsg);
            web.Send(enpMsgToSnd);
            DebugBox("Sent WebSocket message...");
            Thread.Sleep(100);

        }

        private void stopThreadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopThreads();
        }

        private void startThreadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startThreads();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnExit();
        }

        private void addThreadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Add Threads
            DebugBox("Add Threads Button Clicked...");
            
            DebugBox("Current Running Threads: " + currentThreads);
            // Test Shiz
            Add_Threads.Text = "Add Threads";
            DialogResult dialogResult = Add_Threads.ShowDialog(this);
            
            if (dialogResult == DialogResult.OK)
            {
                //get user/password values from dialog
                DebugBox("Adding " + this.Add_Threads.Threads + " threads...");
                
                currentThreads = currentThreads + Convert.ToInt32(this.Add_Threads.Threads);
                DebugBox("Current Runner Threads: " + currentThreads);
                updateThreadCount();


            }
        }

        private void removeThreadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Remove Threads
            DebugBox("Remove Threads Button Clicked...");

            DebugBox("Current Running Threads: " + currentThreads);
            // Test Shiz
            Add_Threads.Text = "Remove Threads";
            DialogResult dialogResult = Add_Threads.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                // Get thread value from dialog
                DebugBox("Removing " + this.Add_Threads.Threads + " threads...");

                currentThreads = currentThreads - Convert.ToInt32(this.Add_Threads.Threads);
                if (currentThreads < 0)
                {
                    currentThreads = 0;
                }
                DebugBox("Current Runner Threads: " + currentThreads);
                updateThreadCount();

            }
        }

        private void agentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Pick User Agent File
        }

        private void linksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // edit links or choose file .. not sure yet
        }

        private void proxysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugBox("Choosing Proxy File...");
            // Choose Proxy File
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\proxies",
                Title = "Choose Proxy File",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "Text files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtProxyListPath = openFileDialog1.FileName;
            
                lines = File.ReadAllLines(txtProxyListPath);
                list_lines = new List<string>(lines);
                //HS = new HashSet<string>();
                if (list_lines.Count > 0)
                {
                    DebugBox("Found and Loaded " + list_lines.Count + " Proxies");
                    txtProxyListloaded = true;
                }
                else
                {
                    MessageBox.Show("No Proxys Found - Empty File...", "Proxy File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void threadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Choose amount of threads
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // check or uncheck 
        }

        private void showShizToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // check or uncheck
            if ((showShizToolStripMenuItem).Checked == false)
            {
                //showShizToolStripMenuItem.Checked = false; //false;
                label1.Hide();
                richTextBox1.Hide();
            }
            else if ((showShizToolStripMenuItem).Checked == true)
            {
                //showShizToolStripMenuItem.Checked = true; //false;
                label1.Show();
                richTextBox1.Show();
            }
        }

        private void aboutTrafficManToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show About Form
        }

        private void aboutShizToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show Shiz Form
        }

        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            OnExit();
            //threadWatcher.Abort();
            // Cleanup
        }

        private void resetHitCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            totalHits = 0;
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugBox("Login Button Clicked...");
            // Test Login Shiz
            DialogResult dialogResult = _PasswordForm.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
            //get user/password values from dialog
                DebugBox(this._PasswordForm.UserName);
                DebugBox(this._PasswordForm.Password);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            // add threads by 1
            currentThreads = currentThreads + 1;
            updateThreadCount();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            // rem threads by 1
            currentThreads = currentThreads - 1;
            if (currentThreads < 0)
            {
                currentThreads = 0;
            }
            updateThreadCount();
        }

        private void label8_Click(object sender, EventArgs e)
        {
            // start or stop threads
            if (threadsStarted == true)
            {
                stopThreads();
            }
            else if (threadsStarted == false)
            {
                startThreads();
            }
        }

        // config saving test   -----   Trying new shiz everyday
        public class SavingPlugin
        {

            public static void SaveVariable(string savename, TypeCode tc, object value, string Encryption_password)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\" + savename + "." + tc.ToString();

                if (!File.Exists(path))
                {
                    var myfile = File.Create(path);
                    myfile.Close();
                    string val = value.ToString();
                    string encrypted = StringCipher.Encrypt(val, Encryption_password);
                    File.WriteAllText(path, encrypted);
                    File.SetAttributes(path, FileAttributes.Hidden);

                }
                else
                {
                    string txt = "";
                    try
                    {
                        txt = StringCipher.Decrypt(File.ReadAllText(path), Encryption_password);
                        File.SetAttributes(path, FileAttributes.Normal);
                        string val = value.ToString();
                        string encrypted = StringCipher.Encrypt(val, Encryption_password);
                        File.WriteAllText(path, encrypted);
                        File.WriteAllText(path, encrypted);
                        File.SetAttributes(path, FileAttributes.Hidden);
                    }
                    catch
                    {
                        MessageBox.Show("Incorrect password : " + Encryption_password + " for the variable : " + savename + "." + tc.ToString());
                    }

                }
            }

            public static object GetVariable(string savename, TypeCode tc, string Encryption_password)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\" + savename + "." + tc.ToString();
                File.SetAttributes(path, FileAttributes.Normal);
                string txt = "";

                try
                {
                    txt = StringCipher.Decrypt(File.ReadAllText(path), Encryption_password);
                    File.SetAttributes(path, FileAttributes.Hidden);
                    var value = Convert.ChangeType(txt, tc);
                    return value;
                }
                catch
                {
                    MessageBox.Show("Incorrect password : " + Encryption_password + " for the variable : " + savename + "." + tc.ToString());
                    return null;
                }


            }

            public static void DeleteVariable(string savename, TypeCode tc)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\" + savename + "." + tc.ToString();
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }

            public static bool Exists(string savename, TypeCode tc)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\" + savename + "." + tc.ToString();
                bool _true = true;

                try
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    File.SetAttributes(path, FileAttributes.Hidden);
                }
                catch
                {
                    _true = false;
                }

                return _true;
            }
        }

        public static class StringCipher
        {
            // This constant is used to determine the keysize of the encryption algorithm in bits.
            // We divide this by 8 within the code below to get the equivalent number of bytes.
            private const int Keysize = 256;

            // This constant determines the number of iterations for the password bytes generation function.
            private const int DerivationIterations = 1000;

            public static string Encrypt(string plainText, string passPhrase)
            {
                // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
                // so that the same Salt and IV values can be used when decrypting.  
                var saltStringBytes = Generate256BitsOfRandomEntropy();
                var ivStringBytes = Generate256BitsOfRandomEntropy();
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
                {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                                {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                    var cipherTextBytes = saltStringBytes;
                                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }

            public static string Decrypt(string cipherText, string passPhrase)
            {
                // Get the complete stream of bytes that represent:
                // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
                var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
                // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
                var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
                // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
                var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
                // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
                var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
                {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream(cipherTextBytes))
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                {
                                    var plainTextBytes = new byte[cipherTextBytes.Length];
                                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }

            private static byte[] Generate256BitsOfRandomEntropy()
            {
                var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    // Fill the array with cryptographically secure random bytes.
                    rngCsp.GetBytes(randomBytes);
                }
                return randomBytes;
            }
        }

        // ui stuff
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            TogMove = 1;
            MValX = e.X;
            MValY = e.Y;


        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            TogMove = 0;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (TogMove == 1)
            {
                this.SetDesktopLocation(MousePosition.X - MValX, MousePosition.Y - MValY);
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {
            // exit
            OnExit();
        }

        private void label10_Click(object sender, EventArgs e)
        {
            // maximize or size down
            this.WindowState = FormWindowState.Maximized;
        }

        private void label11_Click(object sender, EventArgs e)
        {
            // minimize
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
