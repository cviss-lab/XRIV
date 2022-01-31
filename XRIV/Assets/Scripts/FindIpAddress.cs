using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class FindIpAddress : MonoBehaviour
{
    public static FindIpAddress instance;
    public string defaultIp;
    public string defaultPort;
    public TouchScreenKeyboard keyboard;
    ConfigSettings config = new ConfigSettings();
    public TMP_InputField InputIP;
    public TMP_InputField InputPort;


    void Awake()
    {
        instance = this;
        config.readTextFile();
        if (config.IpAddress == null)
        {
            config.IpAddress = defaultIp;
            config.port = defaultPort;
            config.writeTextFile();
        }
        PlayerPrefs.SetString("server_ip", config.IpAddress);
        PlayerPrefs.SetString("server_port", config.port);

        InputIP.text = config.IpAddress;
        InputPort.text = config.port;

    }

    public void OnEditText1()
    {
        config.IpAddress = InputIP.text;
        PlayerPrefs.SetString("server_ip", config.IpAddress);
        config.writeTextFile();
        InputIP.text = keyboard.text;

    }

    public void OpenSystemKeyboard1()
    {
        keyboard = TouchScreenKeyboard.Open(config.IpAddress, TouchScreenKeyboardType.Default, false, false, false, false);
        InputIP.ActivateInputField();
    }

    public void OnEditText2()
    {
        config.port = InputPort.text;
        PlayerPrefs.SetString("server_port", config.port);
        config.writeTextFile();
        InputPort.text = keyboard.text;

    }

    public void OpenSystemKeyboard2()
    {
        keyboard = TouchScreenKeyboard.Open(config.port, TouchScreenKeyboardType.Default, false, false, false, false);
        InputPort.ActivateInputField();
    }
}

public static class IPManager
{
    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}

[Serializable]
public class ConfigSettings
{
    public string IpAddress;
    public string port;

    public void writeTextFile()
    {
        string fPath = Path.Combine(Application.persistentDataPath, "config.json");
        try
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(fPath, json);
        }
        catch
        {

        }
    }
    public void readTextFile()
    {
        string fPath = Path.Combine(Application.persistentDataPath, "config.json");
        try
        {
            StreamReader reader = new StreamReader(fPath);
            string json = reader.ReadToEnd();
            ConfigSettings placeHolder = JsonUtility.FromJson<ConfigSettings>(json);
            IpAddress = placeHolder.IpAddress;
            port = placeHolder.port;
        }
        catch
        {

        }
    }
}