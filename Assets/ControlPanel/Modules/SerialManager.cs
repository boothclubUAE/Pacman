using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

[HasTabField]
public class SerialManager : MonoBehaviour
{
    public static SerialManager Instance;

    [TabField] public string portName = "COM5";
    [TabField] public int baudRate = 115200;
    [TabField] public int bufferTimeout = 1;

    private IntPtr port = IntPtr.Zero;
    private Thread readThread;
    private bool running;

    private readonly ConcurrentQueue<string> messageQueue = new();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[Serial] Awake called. Waiting for ControlPanel initialization.");
    }

    private void Start()
    {
        // NOTE: We don't call OpenPort() immediately because ControlPanel 
        // needs to run its Start() and load settings from settings.txt first.
        // ControlPanel will explicitly call InitSerial() when ready.
    }

    public void InitSerial()
    {
        Debug.Log($"[Serial] Initializing connection using ControlPanel data: {portName} @ {baudRate}");

        // Safety cleanup if already open
        StopSerialThreadAndPort();

        try
        {
            port = WinSerial.Open(portName, baudRate);
            Debug.Log("[Serial] Opened successfully");
            WinSerial.Discard(port);

            StartCoroutine(BufferWarmupThenStart());
        }
        catch (Exception e)
        {
            Debug.LogError("[Serial] FAILED TO OPEN");
            Debug.LogException(e);
        }
    }

    private IEnumerator BufferWarmupThenStart()
    {
        float timer = 0f;

        while (timer < bufferTimeout)
        {
            timer += Time.deltaTime;
            try
            {
                if (port != IntPtr.Zero)
                    WinSerial.Discard(port);
            }
            catch { }

            yield return null;
        }

        running = true;
        readThread = new Thread(ReadSerial)
        {
            IsBackground = true
        };
        readThread.Start();

        Debug.Log("[Serial] Read thread started");
    }

    private void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            Debug.Log("Arduino -> " + message);
            if (message.Equals("START", StringComparison.OrdinalIgnoreCase))
                GameManager.Instance?.OnSTART();
            else if (message.Equals("END", StringComparison.OrdinalIgnoreCase))
            {
                //TODO
                return;
                if (GameUI.Instance.waitingForEndButton)
                {
                    GameUI.Instance?.OnEnd();
                    GameManager.Instance?.GameOver("GAME CANCELED");
                }
            }
            else if (message.Equals("LEFT", StringComparison.OrdinalIgnoreCase))
                GameManager.Instance?.SetPacmanDirection(Vector2.left);
            else if (message.Equals("RIGHT", StringComparison.OrdinalIgnoreCase))
                GameManager.Instance?.SetPacmanDirection(Vector2.right);
            else if (message.Equals("UP", StringComparison.OrdinalIgnoreCase))
                GameManager.Instance?.SetPacmanDirection(Vector2.up);
            else if (message.Equals("DOWN", StringComparison.OrdinalIgnoreCase))
                GameManager.Instance?.SetPacmanDirection(Vector2.down);
        }
    }

    private void Send(string message)
    {
        if (port == IntPtr.Zero)
        {
            Debug.LogError("[Serial] Port not open");
            return;
        }

        try
        {
            WinSerial.Write(port, message);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void ReadSerial()
    {
        byte[] buffer = new byte[256];
        string currentMessage = "";

        while (running)
        {
            try
            {
                if (port != IntPtr.Zero)
                {
                    int bytes = WinSerial.Read(port, buffer);
                    if (bytes > 0)
                    {
                        string received = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
                        currentMessage += received;

                        // Continuous check for keywords inside the accumulated string stream
                        bool foundKeyword = true;
                        while (foundKeyword)
                        {
                            foundKeyword = false;

                            // Check for START
                            if (currentMessage.Contains("START"))
                            {
                                messageQueue.Enqueue("START");
                                int index = currentMessage.IndexOf("START");
                                // Remove "START" from the buffer (5 characters)
                                currentMessage = currentMessage.Remove(index, 5);
                                foundKeyword = true;
                            }

                            // Check for END
                            if (currentMessage.Contains("END"))
                            {
                                messageQueue.Enqueue("END");
                                int index = currentMessage.IndexOf("END");
                                // Remove "END" from the buffer (3 characters)
                                currentMessage = currentMessage.Remove(index, 3);
                                foundKeyword = true;
                            }
                            if (currentMessage.Contains("LEFT"))
                            {
                                messageQueue.Enqueue("LEFT");
                                int index = currentMessage.IndexOf("LEFT");
                                currentMessage = currentMessage.Remove(index, 4);
                                foundKeyword = true;
                            }
                            if (currentMessage.Contains("RIGHT"))
                            {
                                messageQueue.Enqueue("RIGHT");
                                int index = currentMessage.IndexOf("RIGHT");
                                currentMessage = currentMessage.Remove(index, 4);
                                foundKeyword = true;
                            }
                            if (currentMessage.Contains("UP"))
                            {
                                messageQueue.Enqueue("UP");
                                int index = currentMessage.IndexOf("UP");
                                currentMessage = currentMessage.Remove(index, 2);
                                foundKeyword = true;
                            }
                            if (currentMessage.Contains("DOWN"))
                            {
                                messageQueue.Enqueue("DOWN");
                                int index = currentMessage.IndexOf("DOWN");
                                currentMessage = currentMessage.Remove(index, 4);
                                foundKeyword = true;
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Serial] Read error");
                Debug.LogException(e);
                Thread.Sleep(100);
            }
        }
    }

    public void StopSerialThreadAndPort()
    {
        running = false;
        IntPtr open = port;
        port = IntPtr.Zero;

        try
        {
            WinSerial.Close(open);
        }
        catch { }

        try
        {
            if (readThread != null && readThread.IsAlive)
                readThread.Join(700);
        }
        catch { }
    }

    private void OnApplicationQuit()
    {
        StopSerialThreadAndPort();
        Debug.Log("[Serial] Closed");
    }
}