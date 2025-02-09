using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UnityClient : MonoBehaviour
{
    public struct NPCInfo
    {
        public string Name;//名字
        public int MaxHP;//血量
        public int Attack;//攻击力
        public float AttackFrequency;//攻速
    }

    public string serverIP = "192.168.226.128";
    public int serverPort = 25001;
    public NPCInfo MyNPCInfo = new()
    {
        Name = "UnityNPC",
        MaxHP = 20,
        Attack = 1,
        AttackFrequency = 1,
    };
    
    TcpClient client;
    NetworkStream stream;

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        client = new TcpClient(serverIP, serverPort);
        stream = client.GetStream();
        Debug.Log("成功连接到服务器");
    }

    void Update()
    {
        ReceiveMessage();
        if (Input.GetKeyDown(KeyCode.U))
        {
            SendMessage(MyNPCInfo);
        }
    }

    void SendMessage(NPCInfo npcInfo)
    {
        // 将NPCInfo实例转换为JSON格式
        string json = JsonUtility.ToJson(npcInfo);
        byte[] data = Encoding.UTF8.GetBytes(json);
        stream.Write(data, 0, data.Length);
    }

    void ReceiveMessage()
    {
        if (stream.DataAvailable)
        {
            byte[] responseData = new byte[1024];
            int bytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = Encoding.UTF8.GetString(responseData, 0, bytesRead);
            DecodeJSON(response);
        }
    }

    public void DecodeJSON(string json)
    {
        // 使用JsonUtility.FromJson<T>解码JSON数据
        NPCInfo npcInfo = JsonUtility.FromJson<NPCInfo>(json);
        Debug.Log("名字：" + npcInfo.Name + "，血量：" + npcInfo.MaxHP + "，攻击力：" + npcInfo.Attack + "，攻速：" + npcInfo.AttackFrequency);
    }

    void OnDestroy()
    {
        stream.Close();
        client.Close();
    }
}