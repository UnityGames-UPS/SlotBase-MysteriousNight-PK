using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using DG.Tweening;
using System.Linq;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Unity.VisualScripting;


public class SocketIOManager : MonoBehaviour
{
    [SerializeField]
    private SlotBehaviour slotManager;
    [SerializeField] private UIManager uiManager;

    internal GameData initialData = null;
    internal UiData initUIData = null;
    internal Root resultData = null;
    internal Player playerdata = null;
    internal Root bonusData = null;
    internal Root GambleData = null;
    internal GambleResults gambleData = null;

    private SocketManager manager;
    // protected string nameSpace="game"; //BackendChanges
    protected string nameSpace = "playground";//BackendChanges
    private Socket gameSocket; //BackendChanges

    internal Message myMessage = null;

    internal double GambleLimit = 0;

    //[SerializeField]
    //private string SocketURI;

    protected string SocketURI = null;
    //protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
    protected string TestSocketURI = "http://localhost:5000/";
    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

    [SerializeField]
    private string testToken;

    internal bool isResultdone = false;

    protected string gameID = "SL-MYN";
    // protected string gameID = "";
    [SerializeField] internal JSFunctCalls JSManager;
    // protected string gameID = "";
    internal bool isLoading = true;
    internal bool SetInit = false;

    private bool isConnected = false; //Back2 Start
    private bool hasEverConnected = false;
    private const int MaxReconnectAttempts = 5;
    private const float ReconnectDelaySeconds = 2f;

    private float lastPongTime = 0f;
    private float pingInterval = 2f;
    private float pongTimeout = 3f;
    private bool waitingForPong = false;
    private int missedPongs = 0;
    private const int MaxMissedPongs = 5;
    private Coroutine PingRoutine; //Back2 end
    [SerializeField] private GameObject RaycastBlocker;

    internal int[,] Winmatrix = new int[3, 5]
  {
        { 2, 7, 7, 7, 2 },
        { 4, 7, 7, 7, 4 },
        { 5, 7, 7, 7, 5 }

  };

    private void Start()
    {
        SetInit = false;
        OpenSocket();

        //Debug.unityLogger.logEnabled = false;
    }

    void ReceiveAuthToken(string jsonData)
    {
        Debug.Log("Received data: " + jsonData);
        //Parse the JSON data
        var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
        SocketURI = data.socketURL;
        myAuth = data.cookie;
        nameSpace = data.nameSpace; //BackendChanges
    }

    string myAuth = null;

    private void OpenSocket()
    {
        //Create and setup SocketOptions
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

        //Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = testToken,
                gameId = gameID
            };
        };
        options.Auth = authFunction;
        // Proceed with connecting to the server
        SetupSocketManager(options);
#endif
    }

    private IEnumerator WaitForAuthToken(SocketOptions options)
    {
        // Wait until myAuth is not null
        while (myAuth == null)
        {
            yield return null;
        }

        // Once myAuth is set, configure the authFunction
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
         {
             return new
             {
                 token = myAuth,
             };
         };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
    }


    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
            //InitRequest("AUTH");
        }
        else
        {

        }
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        uiManager.ADfunction();
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
        // AliveRequest("YES I AM ALIVE");

    }

    private void AliveRequest()
    {
        InitData message = new InitData();
        SendDataWithNamespace("YES I AM ALIVE");
    }

    private void SendDataWithNamespace(string eventName, string json = null)
    {
        // Send the message
        if (gameSocket != null && gameSocket.IsOpen) //BackendChanges
        {
            if (json != null)
            {
                gameSocket.Emit(eventName, json);
                Debug.Log("JSON data sent: " + json);
            }
            else
            {
                gameSocket.Emit(eventName);
            }
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }

    private void SetupSocketManager(SocketOptions options)
    {
#if UNITY_EDITOR
        // Create and setup SocketManager for Testing
        this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        // Create and setup SocketManager
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif

        if (string.IsNullOrEmpty(nameSpace))
        {  //BackendChanges Start
            gameSocket = this.manager.Socket;
        }
        else
        {
            print("nameSpace: " + nameSpace);
            gameSocket = this.manager.GetSocket("/" + nameSpace);
        }
        // Set subscriptions
        gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnListenEvent);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived);
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
                                                                     // Start connecting to the server
        manager.Open();
    }

    // Connected event handler implementation
    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("✅ Connected to server.");

        if (hasEverConnected)
        {
            uiManager.CheckAndClosePopups();
        }

        isConnected = true;
        hasEverConnected = true;
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        SendPing();
    }
    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
    }
    private void SendPing()
    {
        ResetPingRoutine();
        PingRoutine = StartCoroutine(PingCheck());
    }
    void ResetPingRoutine()
    {
        if (PingRoutine != null)
        {
            StopCoroutine(PingRoutine);
        }
        PingRoutine = null;
    }

    private IEnumerator PingCheck()
    {
        while (true)
        {
            Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

            if (missedPongs == 0)
            {
                uiManager.CheckAndClosePopups();
            }

            // If waiting for pong, and timeout passed
            if (waitingForPong)
            {
                if (missedPongs == 2)
                {
                    uiManager.ReconnectionPopup();
                }
                missedPongs++;
                Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

                if (missedPongs >= MaxMissedPongs)
                {
                    Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
                    isConnected = false;
                    uiManager.DisconnectionPopup();
                    yield break;
                }
            }

            // Send next ping
            waitingForPong = true;
            lastPongTime = Time.time;
            Debug.Log("📤 Sending ping...");
            SendDataWithNamespace("ping");
            yield return new WaitForSeconds(pingInterval);
        }
    } //Back2 end
    private void OnDisconnected()
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        isConnected = false;
        uiManager.DisconnectionPopup();
        ResetPingRoutine();
    }
    private void OnPongReceived(string data) //Back2 Start
    {
        Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end

    private void OnError(Error err)
    {
        Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
    }

    private void OnListenEvent(string data)
    {
        Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
    }

    private void InitRequest(string eventName)
    {
        InitData message = new InitData();
        message.Data = new AuthData();
        message.Data.GameID = gameID;
        message.id = "Auth";
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        Debug.Log(json);
        // Send the message
        if (this.manager.Socket != null && this.manager.Socket.IsOpen)
        {
            this.manager.Socket.Emit(eventName, json);
            Debug.Log("JSON data sent: " + json);
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }

    internal void ReactNativeCallOnFailedToConnect() //BackendChanges
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
    }

    internal void CloseWebSocket()
    {
        CloseSocketMesssage("EXIT");

        //DOVirtual.DelayedCall(0.1f, () =>
        //{
        //    if (this.manager != null)
        //    {
        //        Debug.Log("Dispose my Socket");
        //        this.manager.Close();
        //    }
        //});
        SendDataWithNamespace("game:exit");
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit");
#endif

    }

    private void CloseSocketMesssage(string eventName)
    {
        // Construct message data

        if (gameSocket != null && gameSocket.IsOpen)
        {
            gameSocket.Emit(eventName);
            //Debug.Log("JSON data sent: " + json);
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }

    internal IEnumerator CloseSocket() //Back2 Start
    {
        RaycastBlocker.SetActive(true);
        ResetPingRoutine();

        Debug.Log("Closing Socket");

        manager?.Close();
        manager = null;

        Debug.Log("Waiting for socket to close");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
    } //Back2 end

    private void ParseResponse(string jsonObject)
    {
        Debug.Log(jsonObject);
        Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

        string id = myData.id;

        switch (id)
        {
            case "initData":
                {
                    initialData = myData.gameData;
                    initUIData = myData.uiData;
                    playerdata = myData.player;
                    // GambleLimit = myData.message.maxGambleBet;
                    if (!SetInit)
                    {
                        Debug.Log(jsonObject);
                        List<string> InitialReels = ConvertListOfListsToStrings(initialData.lines);
                        InitialReels = RemoveQuotes(InitialReels);
                        PopulateSlotSocket(InitialReels);
                        SetInit = true;
                    }
                    else
                    {
                        RefreshUI();
                    }
                    break;
                }
            case "ResultData":
                {
                    Debug.Log(jsonObject);
                    // myData.message.GameData.FinalResultReel = ConvertListOfListsToStrings(myData.message.GameData.ResultReel);
                    // myData.message.GameData.FinalsymbolsToEmit = TransformAndRemoveRecurring(myData.message.GameData.symbolsToEmit);
                    resultData = myData;
                    playerdata = myData.player;
                    isResultdone = true;
                    break;
                }

            case "bonusResult":
                {
                    Debug.Log(jsonObject);
                    // myData.message.GameData.FinalResultReel = ConvertListOfListsToStrings(myData.message.GameData.ResultReel);
                    // myData.message.GameData.FinalsymbolsToEmit = TransformAndRemoveRecurring(myData.message.GameData.symbolsToEmit);
                    bonusData = myData;
                    playerdata = myData.player;
                    isResultdone = true;
                    break;
                }
            case "gambleInit":
                {
                    Debug.Log(jsonObject);
                    // myMessage = myData.message;
                    GambleData = myData;
                    isResultdone = true;
                    break;
                }

            case "gambleDraw":
                {

                    GambleData = myData;
                    // slotManager.updateBalance();
                    // UpdateUiOnResult(myData);
                    isResultdone = true;
                    break;
                }
            case "gambleCollect":
                {
                    //Debug.Log(jsonObject);
                    //  PlayerData = myData.player;
                    GambleData = myData;
                    //UpdateUiOnResult(myData);
                    // slotManager.updateBalance(GambleData.player.balance,GambleData.payload.winAmount);
                    isResultdone = true;
                    break;
                }

            case "ExitUser":
                {
                    if (gameSocket != null) //BackendChanges
                    {
                        Debug.Log("Dispose my Socket");
                        this.manager.Close();
                    }
                    // Application.ExternalCall("window.parent.postMessage", "onExit", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif
                    break;
                }
        }
    }

    private void RefreshUI()
    {
        uiManager.InitialiseUIData(initUIData.paylines);
    }

    private void PopulateSlotSocket(List<string> slotPop)
    {
        slotManager.shuffleInitialMatrix();

        //for (int i = 0; i < slotPop.Count; i++)
        //{
        //    List<int> points = slotPop[i]?.Split(',')?.Select(Int32.Parse)?.ToList();
        //    slotManager.PopulateInitalSlots(i, points);
        //}

        // for (int i = 0; i < slotPop.Count; i++)
        // {
        //     slotManager.LayoutReset(i);
        // }

        slotManager.SetInitialUI();
        isLoading = false;
        //Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
        RaycastBlocker.SetActive(false);
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif

    }

    internal void AccumulateResult(int currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.type = "SPIN";
        message.payload = new Data();
        message.payload.betIndex = currBet;
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }

    internal void AccumulateTapBonusResult(int index)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.type = "BONUS";
        message.payload = new Data();
        message.payload.Event = "tap";
        message.payload.index = index;
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
        Debug.Log($"Send Tap Bonus Data " + json);

    }

    internal void OnGamble()
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.payload = new Data();
        message.type = "GAMBLE";
        Debug.Log(slotManager.BetCounter);
        message.payload.lastWinning = slotManager.BetCounter;
        message.payload.Event = "init";
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }

    internal void GambleDraw()
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.payload = new Data();
        message.type = "GAMBLE";
        Debug.Log(slotManager.BetCounter);
        message.payload.lastWinning = slotManager.BetCounter;
        message.payload.Event = "draw";
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
        Debug.Log($"Gamble Draw Sended json : " + json);
    }

    internal void GambleCollect()
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.payload = new Data();
        message.type = "GAMBLE";

        message.payload.lastWinning = slotManager.BetCounter;
        message.payload.Event = "collect";
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }

    internal void GambleCollectCall()
    {
        ExitData message = new ExitData();
        message.id = "GAMBLECOLLECT";
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("message", json);
    }

    internal void OnCollect()
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.payload = new Data();
        message.type = "GAMBLE";

        message.payload.lastWinning = slotManager.BetCounter;
        message.payload.Event = "collect";
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }

    private List<string> RemoveQuotes(List<string> stringList)
    {
        for (int i = 0; i < stringList.Count; i++)
        {
            stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
        }
        return stringList;
    }

    private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
    {
        List<string> resultList = new List<string>();

        foreach (List<int> innerList in listOfLists)
        {
            // Convert each integer in the inner list to string
            List<string> stringList = new List<string>();
            foreach (int number in innerList)
            {
                stringList.Add(number.ToString());
            }

            // Join the string representation of integers with ","
            string joinedString = string.Join(",", stringList.ToArray()).Trim();
            resultList.Add(joinedString);
        }

        return resultList;
    }

    private List<string> ConvertListOfListsToStrings(List<List<int>> inputList)
    {
        List<string> outputList = new List<string>();

        foreach (List<int> row in inputList)
        {
            string concatenatedString = string.Join(",", row);
            outputList.Add(concatenatedString);
        }

        return outputList;
    }

    private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
    {
        // Flattened list
        List<string> flattenedList = new List<string>();
        foreach (List<string> sublist in originalList)
        {
            flattenedList.AddRange(sublist);
        }

        // Remove recurring elements
        HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

        // Transformed list
        List<string> transformedList = new List<string>();
        foreach (string element in uniqueElements)
        {
            transformedList.Add(element.Replace(",", ""));
        }

        return transformedList;
    }
}

[Serializable]
public class HighCard
{
    public string suit { get; set; }
    public string value { get; set; }
}

[Serializable]
public class LowCard
{
    public string suit { get; set; }
    public string value { get; set; }
}

[Serializable]
public class BetData
{
    public double currentBet;
    public double currentLines = 9;
    public double spins = 1;
}

[Serializable]
public class GambleData
{
    public string GAMBLETYPE;
}



[Serializable]
public class AuthData
{
    public string GameID;
    public double TotalLines;
}

[Serializable]
public class MessageData
{
    // public BetData data;
    // public string id;

    public string type;
    public Data payload;
}
[Serializable]
public class Data
{
    public int betIndex;
    public string Event;
    public double lastWinning;
    public int index;

}

[Serializable]
public class InitData
{
    public AuthData Data;
    public string id;
}

[Serializable]
public class AbtLogo
{
    public string logoSprite { get; set; }
    public string link { get; set; }
}

[Serializable]
public class GameData
{
    public List<List<string>> Reel { get; set; }
    public List<List<int>> Lines { get; set; }
    //  public List<double> bets { get; set; }
    public bool canSwitchLines { get; set; }
    public List<int> LinesCount { get; set; }
    public List<int> autoSpin { get; set; }
    public List<List<string>> ResultReel { get; set; }
    public List<int> linesToEmit { get; set; }
    public List<List<string>> symbolsToEmit { get; set; }
    public double WinAmout { get; set; }
    public FreeSpins freeSpins { get; set; }
    public List<string> FinalsymbolsToEmit { get; set; }
    public List<string> FinalResultReel { get; set; }
    public double jackpot { get; set; }
    public bool isBonus { get; set; }
    public double BonusStopIndex { get; set; }
    public List<int> BonusResult { get; set; }


    public List<List<int>> lines { get; set; }
    public List<double> bets { get; set; }
    public List<int> spinBonus { get; set; }
}

[Serializable]
public class FreeSpins
{
    public int count { get; set; }
    public bool isNewAdded { get; set; }
}

[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
    public string nameSpace; //BackendChanges
}

[Serializable]
public class GambleResults
{
    public double currentWining;
    public double totalWinningAmount;

}

[Serializable]
public class RiskData
{
    public GambleData data;
    public string id;
}

[Serializable]
public class Message
{
    public GameData GameData { get; set; }
    public GambleResults GambleData { get; set; }
    public UiData UIData { get; set; }
    public Player PlayerData { get; set; }
    public List<string> BonusData { get; set; }

    public HighCard highCard { get; set; }
    public LowCard lowCard { get; set; }
    public List<ExCard> exCards { get; set; }
    public bool playerWon { get; set; }
    public double Balance { get; set; }
    public double currentWining { get; set; }
    public double maxGambleBet { get; set; }
}

[Serializable]
public class ExCard
{
    public string suit { get; set; }
    public string value { get; set; }
}

[Serializable]
public class Root
{
    // public string id { get; set; }
    // public Message message { get; set; }

    public string id { get; set; }
    public GameData gameData { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }

    public bool success { get; set; }
    public List<List<string>> matrix { get; set; }
    public Payload payload { get; set; }
    public Bonus bonus { get; set; }
}

[Serializable]
public class UiData
{
    // public Paylines paylines { get; set; }
    // public AbtLogo AbtLogo { get; set; }
    // public string ToULink { get; set; }
    // public string PopLink { get; set; }

    public Paylines paylines { get; set; }
}
[Serializable]
public class Bonus
{
    public bool isTriggered { get; set; }
    public List<object> result { get; set; }
    public int amount { get; set; }
}
[Serializable]
public class Payload
{
    public double winAmount { get; set; }
    public int payout { get; set; }
    public List<Win> wins { get; set; }

    public bool playerWon { get; set; }
    public Cards cards { get; set; }
}

[Serializable]
public class Win
{
    public int line { get; set; }
    public List<int> positions { get; set; }
    public double amount { get; set; }
}
[Serializable]
public class Cards
{
    public int dealerCard { get; set; }
    public int playerCard { get; set; }
}


[Serializable]
public class Paylines
{
    public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
    // public int ID { get; set; }
    // public string Name { get; set; }
    // [JsonProperty("multiplier")]
    // public object MultiplierObject { get; set; }

    // // This property will hold the properly deserialized list of lists of integers
    // [JsonIgnore]
    // public List<List<int>> multiplier { get; private set; }

    // // Custom deserialization method to handle the conversion
    // [OnDeserialized]
    // internal void OnDeserializedMethod(StreamingContext context)
    // {
    //     // Handle the case where multiplier is an object (empty in JSON)
    //     if (MultiplierObject is JObject)
    //     {
    //         multiplier = new List<List<int>>();
    //     }
    //     else
    //     {
    //         // Deserialize normally assuming it's an array of arrays
    //         multiplier = JsonConvert.DeserializeObject<List<List<int>>>(MultiplierObject.ToString());
    //     }
    // }
    public object defaultAmount { get; set; }
    public object symbolsCount { get; set; }
    public object increaseValue { get; set; }
    // public object description { get; set; }
    public int freeSpin { get; set; }

    public int id { get; set; }
    public string name { get; set; }
    public List<int> multiplier { get; set; }
    public string description { get; set; }

}


[Serializable]
public class Player
{
    public double balance { get; set; }
    public double haveWon { get; set; }
    public double currentWining { get; set; }
}

[Serializable]
public class ExitData
{
    public string id;
}