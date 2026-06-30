using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

[HasTabField]
public class SerialManager : MonoBehaviour
{
    public static SerialManager Instance;

    [TabField]
    public string portName = "COM5";
    [TabField]
    public int baudRate = 115200;
    [TabField]
    public float BufferTimeout = 1f;

    private SerialPort port;
    private Thread readThread;
    private bool running;
    private float bufferTimer;
    private bool clearingBuffer = false;
    private readonly ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    public UnityEvent<string> OnMessageReceived;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        try
        {
            port = new SerialPort(portName, baudRate)
            {
                ReadTimeout = 500,
                NewLine = "\n"
            };

            port.Open();
            port.DiscardInBuffer();

            Debug.Log($"Serial Opened: {portName} @ {baudRate}");

            // Start buffer clearing phase
            clearingBuffer = true;
            bufferTimer = BufferTimeout;

            running = true;

            readThread = new Thread(ReadSerial);
            readThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Serial connection failed: " + e.Message);
        }
    }

    private void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            if (clearingBuffer)
                continue;

            Debug.Log("From Arduino: " + message);
            OnMessageReceived?.Invoke(message);
        }

        // handle buffer timeout countdown
        if (clearingBuffer)
        {
            bufferTimer -= Time.deltaTime;

            if (bufferTimer <= 0f)
            {
                clearingBuffer = false;
                port?.DiscardInBuffer(); // final clean
                Debug.Log("Serial buffer cleared, now listening.");
            }
        }
    }

    public void SendToSerial(string message)
    {
        if (port != null && port.IsOpen)
        {
            port.WriteLine(message);
            Debug.Log("Sent '"+message+"' to Arduino");
        }
    }
    private void ReadSerial()
    {
        byte[] buffer = new byte[1024];
        int bufferCount = 0;


        while (running)
        {
            try
            {
                if (clearingBuffer)
                {
                    port.DiscardInBuffer();
                    bufferCount = 0;
                    continue;
                }
                if (port != null && port.IsOpen && port.BytesToRead > 0)
                {
                    int b = port.ReadByte();

                    if (b == -1)
                        continue;

                    char c = (char)b;

                    if (c == '\n')
                    {
                        string line = System.Text.Encoding.ASCII
                            .GetString(buffer, 0, bufferCount)
                            .Trim('\r');

                        if (!string.IsNullOrEmpty(line))
                        {
                            messageQueue.Enqueue(line);
                        }

                        bufferCount = 0;
                    }
                    else
                    {
                        if (bufferCount < buffer.Length)
                        {
                            buffer[bufferCount++] = (byte)b;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Serial read error: " + e.Message);
            }
        }
    }

    private void OnApplicationQuit()
    {
        running = false;

        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }

        if (port != null && port.IsOpen)
        {
            port.Close();
        }
    }
}