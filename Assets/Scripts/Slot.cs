using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using System;
using UnityEngine.SceneManagement;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using System.IO;
using Assets.Tweens.Scripts.Common.Tweens;

public class Slot : MonoBehaviour
{
    #region 變數
    private string accountAddress;
    private string accountPrivateKey; //遊戲開始輸入私鑰
    public int TANCoins;
    public decimal BetTCoins;
    public int SlotResult;
    public int MineGasAmount = 60000; //輸入挖礦GasAmount預設1000000
    public int MineGasPrice = 1000000000; //輸入挖礦GasPrice預設1GWEI

    public Text AddressTAN;
    public Text CopyMainAddressText;
    public Text MainPKeyText;
    public Text TANAmount;
    public Text WINAmount;
    public Text TotalPrizeAmount;
    private decimal TANBalance;
    public decimal TotalPrize;
    public Text TransactionText;
    public GameObject EnterPkeyPanel;
    public GameObject LoadingPanel;
    public GameObject BlockBG;
    public GameObject NoEnoughTAN;
    public GameObject SpinLine01;
    public GameObject SpinLine02;
    public GameObject SpinLine03;
    public Sprite[] SpinLineSprit;
    public Sprite BlankSpinSprit;
    public float SpinCount = 100;
    public GameObject[] SpinLineIcon;
    public GameObject[] SpinLineIconWin;
    public Sprite[] IconSpin;
    public GameObject SpinSound;
    public GameObject RewardSound;
    public GameObject ButtonBack;
    public GameObject WINPanel;
    public GameObject BIGPanel;
    public GameObject MEGAPanel;

    public int StartSpin = 0;
    public int SpinType = 0;

    private decimal m_baseAmount = new decimal(0.01);
    private int m_constractMaxRatio = 10;

    struct BetInfo
    {
        public BetInfo(int min, int max, int ratio, int winPic)
        {
            rangeMin = min;
            rangeMax = max;
            this.ratio = ratio;
            winSpinPicNumber = winPic;
        }
        public int rangeMin;
        public int rangeMax;
        public int ratio;
        public int winSpinPicNumber;
    }
    private BetInfo[] m_betInfos = new BetInfo[]
    {
        new BetInfo(0,0,10,1),
        new BetInfo(1,3,4,2),
        new BetInfo(4,5,3,3),
        new BetInfo(6,10,2,4),
        new BetInfo(11,20,1,5),
    };
    #endregion

    #region 智能合約
    //-----以下為智能合約資料----
    private string _urlMain = "https://testnet-rpc.tangerine-network.io";  //TAN 測試鏈網路
    //private string _urlMain = "https://mainnet-rpc.tangerine-network.io";  //TAN 主鏈網路
    private DeployContractTransactionBuilder contractTransactionBuilder = new DeployContractTransactionBuilder();
    private static string ContractAddressTSlot = "0xdbf4f468bdFb597F61cA1147cB7F706F4B754a47"; //TAN 測試鏈合約地址
    private static string ABITSlot = @"[{""constant"":false,""inputs"":[],""name"":""play_game"",""outputs"":[],""payable"":true,""stateMutability"":""payable"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""rand"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_new_manager"",""type"":""address""}],""name"":""transferownership"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""withdraw_all_ETH"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_eth_wei"",""type"":""uint256""}],""name"":""withdraw_ETH"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""payable"":true,""stateMutability"":""payable"",""type"":""constructor""},{""constant"":true,""inputs"":[],""name"":""balance"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""maxNumber"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":"""",""type"":""uint256""}],""name"":""odds"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""oddsNumber"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""payment"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""randonNumber"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""}]";
    private Contract ContractTSlot;

    public Slot()
    {
        this.ContractTSlot = new Contract(null, ABITSlot, ContractAddressTSlot);
    }
    //-----以上為智能合約資料----
    #endregion

    #region 初始化
    void Start()
    {
        SpinType = 0;
        PlayerPrefs.SetInt("StartSpin", 0);
        SpinCount = 100;
        EnterPkeyPanel.gameObject.SetActive(true);
        SpinLine01.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        SpinLine02.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        SpinLine03.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
    }

    public void AfterEnterPkey()
    {
        LoadingPanel.gameObject.SetActive(true);
        ButtonBack.gameObject.SetActive(false);
        BlockBG.gameObject.SetActive(false);
        NoEnoughTAN.gameObject.SetActive(false);
        SpinSound.gameObject.SetActive(false);
        RewardSound.gameObject.SetActive(false);
        WINPanel.gameObject.SetActive(false);
        BIGPanel.gameObject.SetActive(false);
        MEGAPanel.gameObject.SetActive(false);
        SpinLine01.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        SpinLine02.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        SpinLine03.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        StartRandom();
        importAccountFromPrivateKey();
    }

    public void StartRandom()
    {
        SpinLine01.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        SpinLine02.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        SpinLine03.gameObject.GetComponent<Image>().sprite = BlankSpinSprit;
        for (int i = 0; i < 6; i++)
        {
            int randomSpin = UnityEngine.Random.Range(0, 9);
            SpinLineIcon[i].gameObject.SetActive(true);
            SpinLineIcon[i].gameObject.GetComponent<Image>().sprite = IconSpin[randomSpin];
        }

        for (int i = 0; i < 3; i++)
        {
            int WinrandomSpin = UnityEngine.Random.Range(0, 8);
            SpinLineIconWin[i].gameObject.SetActive(true);
            SpinLineIconWin[i].gameObject.GetComponent<Image>().sprite = IconSpin[WinrandomSpin];
            SpinLineIconWin[i].gameObject.GetComponent<ScaleSpring>().enabled = false;
        }
    }
    #endregion

    #region 私鑰類函數
    public void EnterPkeyStart(Text enterText)
    {
        accountPrivateKey = enterText.text;
    }

    public void ImportPkey()
    {
        InputField PasswordInput1 = GameObject.Find("InputField_Pkey").GetComponent<InputField>();
        accountPrivateKey = PasswordInput1.text;
        AfterEnterPkey();
    }

    public void CreatePkey()
    {
        EthECKey ecKey = EthECKey.GenerateKey();
        accountPrivateKey = ecKey.GetPrivateKey();
        CopyMainPrivateKey();
        AfterEnterPkey();
    }

    public void CopyMainPrivateKey()   //複製私鑰
    {
        TextEditor text2Editor = new TextEditor();
        text2Editor.text = accountPrivateKey;
        text2Editor.OnFocus();
        text2Editor.Copy();   //--將私鑰複製到剪貼版
        TransactionText.text = "私鑰已經複製到剪貼版！";
    }

    public void ReloadSceneGo()
    {
        LoadingPanel.gameObject.SetActive(true);
        StartCoroutine(ReloadSceneGoWait(2.0f));
    }

    public IEnumerator ReloadSceneGoWait(float waittimes)
    {
        yield return new WaitForSeconds(waittimes);
        SceneManager.LoadScene("FastSlot");
    }

    public void CopyMainAddress()   //複製地址
    {
        CopyMainAddressText.text = accountAddress;
        TextEditor text2Editor = new TextEditor();
        text2Editor.text = accountAddress;
        text2Editor.OnFocus();
        text2Editor.Copy();   //--將地址複製到剪貼版
        TransactionText.text = "地址已經複製到剪貼版！";
    }

    public void importAccountFromPrivateKey()
    {
        // 這裡是把我們的私鑰轉為公鑰地址的函數
        if (accountPrivateKey == null || accountPrivateKey == "")
        {
            TransactionText.text = "Private key not loaded";
            LoadingPanel.gameObject.SetActive(true);
            ButtonBack.gameObject.SetActive(true);
        }
        else
        {
            try
            {
                var address = Nethereum.Signer.EthECKey.GetPublicAddress(accountPrivateKey);
                // 把address轉換取得的地址字串轉給accountAdress
                accountAddress = address;
                AddressTAN.text = accountAddress;  //將取得的地址顯示在螢幕上
                Debug.Log("私鑰轉換成地址成功:" + address);
                ReloadBalance();
                ReloadPollBalance();
            }
            catch (Exception e)
            {
                Debug.Log("error" + e);
            }
        }
    }
    #endregion

    #region 執行玩SLOT的合約
    //----以下是執行玩SLOT的函數---(調用執行函數)
    public void PlaySlot(int betRatio)
    {
        decimal betAmount = new decimal(betRatio) * m_baseAmount;
        WINPanel.gameObject.SetActive(false);
        BIGPanel.gameObject.SetActive(false);
        MEGAPanel.gameObject.SetActive(false);
        if (TANBalance < betAmount)
        {
            TransactionText.text = "Not enough money to play SLOT！";
            NoEnoughTAN.gameObject.SetActive(true);
        }
        else
        {
            if (TotalPrize < betAmount * m_constractMaxRatio)
            {
                TransactionText.text = "Prize pool not enough money to play SLOT！";
                NoEnoughTAN.gameObject.SetActive(true);
            }
            else
            {
                LoadingPanel.gameObject.SetActive(true);
                NoEnoughTAN.gameObject.SetActive(false);
                StartCoroutine(PlaySlotGo(betAmount));
                BetTCoins = betAmount;
                WINAmount.text = "0";
                TANAmount.text = (TANBalance - BetTCoins).ToString("#0.00");
                BlockBG.gameObject.SetActive(true);
            }
        }
    }

    public IEnumerator PlaySlotGo(decimal BetAmount)
    {
        var s = Nethereum.Util.UnitConversion.Convert.ToWei(BetAmount, 18).ToString();
        Debug.Log("s" +s);
        var transactionInput = CreateBuyMaterialSlotInput(
            accountAddress,
            new HexBigInteger(MineGasAmount), //GasAmount
            new HexBigInteger(MineGasPrice), //GasPrice / GWEI
            new HexBigInteger(Nethereum.Util.UnitConversion.Convert.ToWei(BetAmount, 18)) //賭注金額 / TAN
        );
        Debug.Log("你已經開始玩Slot，等待系統確認...");
        TransactionText.text = "Play Slot " + BetAmount + "X...";
        var transactionSignedRequest = new TransactionSignedUnityRequest(_urlMain, accountPrivateKey, accountAddress);
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            Debug.Log("Transfered tx created: " + transactionSignedRequest.Result);
            checkPlaySlotTx(transactionSignedRequest.Result, (cb) => {
                Debug.Log("Slot 開啟！");
                TransactionText.text = "Slot 開啟！";
                LoadingPanel.gameObject.SetActive(false);
                SpinStart(BetAmount);
            });
        }
        else
        {
            Debug.Log("Error transfering: " + transactionSignedRequest.Exception.Message);
        }
    }

    public TransactionInput CreateBuyMaterialSlotInput(
        string addressFrom,
        HexBigInteger gas = null,
        HexBigInteger gasPrice = null,
        HexBigInteger valueAmount = null
    )
    {
        var function = GetBuyMaterialSlotGoFunction();
        return function.CreateTransactionInput(
            addressFrom, gas, gasPrice, valueAmount
        );
    }

    public Function GetBuyMaterialSlotGoFunction()  //----執行ABI
    {
        return ContractTSlot.GetFunction("play_game");
    }

    public void checkPlaySlotTx(string txHash, Action<bool> callback) // 這個函數是監聽交易是否成功的功能
    {
        StartCoroutine(CheckPlaySlotIsMined(
            _urlMain,
            txHash,
            (cb) => {
                Debug.Log("本交易已經完成");
                callback(true);
            }
        ));
    }

    public IEnumerator CheckPlaySlotIsMined(             //------監聽交易在區塊練是否確認成功
        string url, string txHash, System.Action<bool> callback
    )
    {
        var mined = false;
        var tries = 3600;
        while (!mined)
        {
            if (tries > 0)
            {
                tries = tries - 1;
            }
            else
            {
                mined = true;
                Debug.Log("Performing last try..");
            }
            Debug.Log("Checking receipt for: " + txHash);
            var receiptRequest = new EthGetTransactionReceiptUnityRequest(url);
            yield return receiptRequest.SendRequest(txHash);
            if (receiptRequest.Exception == null)
            {
                if (receiptRequest.Result != null)
                {
                    if (receiptRequest.Result.Logs.Count == 0)
                    {
                        Debug.LogError(receiptRequest.Result.ToString());
                        yield return new WaitForSeconds(0.5f);
                    }
                    else
                    {
                        string txLogs = receiptRequest.Result.Logs[0]["data"].ToString();
                        var txLogsHex = txLogs.RemoveHexPrefix();
                        string AddressGet = txLogsHex.Substring(24, 40);
                        string BetAmountGet = txLogsHex.Substring(64, 64);
                        string RandomNOGet = txLogsHex.Substring(128, 64);
                        SlotResult = int.Parse(RandomNOGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                        Debug.Log("Slot結果1:" + txLogsHex);
                        Debug.Log("Slot結果2:地址:0X" + AddressGet + "/投注" + BetTCoins + "TAN/開出號碼:" + SlotResult);
                    }
                    
                    var txType = "mined";
                    if (txType == "mined")
                    {
                        mined = true;
                        callback(mined);
                    }
                    else
                    {
                        mined = false;
                        callback(mined);
                    }
                }
            }
            else
            {
                Debug.Log("Error checking receipt: " + receiptRequest.Exception.Message);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    //----以下是執行玩SLOT的函數---(調用執行函數)
    #endregion

    #region 餘額查詢
    //--------------取得玩家地址TAN餘額---(查詢)
    public void ReloadBalance()
    {
        StartCoroutine(getAccountBalance(accountAddress, (balance) => {     //執行檢查主網ETH的餘額
            TANAmount.text = (balance).ToString("#0.00");
            TANBalance = balance;
            Debug.Log(balance);
            BlockBG.gameObject.SetActive(false);
            LoadingPanel.gameObject.SetActive(false);
        }));
    }

    public static IEnumerator getAccountBalance(string address, System.Action<decimal> callback)
    {
        //--取得主網ETH餘額
        var getBalanceRequest = new EthGetBalanceUnityRequest("https://testnet-rpc.tangerine-network.io");
        yield return getBalanceRequest.SendRequest(address, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
        if (getBalanceRequest.Exception == null)
        {
            var balance = getBalanceRequest.Result.Value;
            yield return new WaitForSeconds(1);
            callback(Nethereum.Util.UnitConversion.Convert.FromWei(balance, 18));
        }
        else
        {
            throw new System.InvalidOperationException("取得TAN餘額失敗");
        }
    }
    //--------------取得玩家地址TAN餘額---(查詢)
    //--------------取得合約地址TAN餘額---(查詢)
    public void ReloadPollBalance()
    {
        StartCoroutine(getAccountBalance(ContractAddressTSlot, (balance) => {     //執行檢查主網ETH的餘額
            TotalPrizeAmount.text = balance.ToString("#0.00");
            TotalPrize = balance;
            Debug.LogFormat("contract:{0},address:{1}",balance, ContractAddressTSlot);
        }));
    }
    //--------------取得合約地址TAN餘額---(查詢)
    #endregion

    #region Slot主要功能
    public void SpinStart(decimal BetAmount)
    {
        if (TANBalance < BetAmount)
        {
            NoEnoughTAN.gameObject.SetActive(true);
            TransactionText.text = "你身上的錢不夠玩Slot！";
        }
        else
        {
            SpinType = 1;
            PlayerPrefs.SetInt("StartSpin", 1);
        }
    }

    public IEnumerator SpinStartGO()
    {
        StartSpin = 0;
        PlayerPrefs.SetInt("StartSpin", 0);
        BlockBG.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        SpinCount = 0;
        SpinSound.gameObject.SetActive(true);
        RewardSound.gameObject.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            SpinLineIcon[i].gameObject.GetComponent<Image>().sprite = null;
            SpinLineIcon[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < 3; i++)
        {
            SpinLineIconWin[i].gameObject.GetComponent<Image>().sprite = null;
            SpinLineIconWin[i].gameObject.SetActive(false);
            SpinLineIconWin[i].gameObject.GetComponent<ScaleSpring>().enabled = false;
        }
    }

    public void Reward()
    {
        SpinSound.gameObject.SetActive(false);
        RewardSound.gameObject.SetActive(true);
        SpinCount = 100;
        SpinLine01.gameObject.GetComponent<Image>().sprite = BlankSpinSprit; //將長條模糊圖案換成空白底圖
        SpinLine02.gameObject.GetComponent<Image>().sprite = BlankSpinSprit; //將長條模糊圖案換成空白底圖
        SpinLine03.gameObject.GetComponent<Image>().sprite = BlankSpinSprit; //將長條模糊圖案換成空白底圖
        for (int i = 0; i < 6; i++)
        {
            SpinLineIcon[i].gameObject.SetActive(true); //將上下排單獨每格圖示激活
        }

        int UpperIcon1 = UnityEngine.Random.Range(0, 8); //亂數顯示上排圖示1
        int UpperIcon2 = UnityEngine.Random.Range(0, 8); //亂數顯示上排圖示2
        int UpperIcon3 = UnityEngine.Random.Range(0, 8); //亂數顯示上排圖示3
        if (UpperIcon1 == UpperIcon2 && UpperIcon2 == UpperIcon3) //如果上排三個圖示皆為同一數,需另作調整
        {
            if (UpperIcon1 == 8) //如果上排圖示皆為同一數並且為8(最大值)
            {
                UpperIcon2 = 0; //將上排中間圖示改為0
            }
            else
            {
                UpperIcon2 += 1; //否則將上排中間圖示+1
            }
        }
        SpinLineIcon[0].gameObject.GetComponent<Image>().sprite = IconSpin[UpperIcon1]; //圖示改為亂數對應圖案
        SpinLineIcon[1].gameObject.GetComponent<Image>().sprite = IconSpin[UpperIcon2]; //圖示改為亂數對應圖案
        SpinLineIcon[2].gameObject.GetComponent<Image>().sprite = IconSpin[UpperIcon3]; //圖示改為亂數對應圖案

        int LowerIcon1 = UnityEngine.Random.Range(0, 8); //亂數顯示下排圖示1
        int LowerIcon2 = UnityEngine.Random.Range(0, 8); //亂數顯示下排圖示2
        int LowerIcon3 = UnityEngine.Random.Range(0, 8); //亂數顯示下排圖示3
        if (LowerIcon1 == LowerIcon2 && LowerIcon2 == LowerIcon3) //如果下排三個圖示皆為同一數,需另作調整
        {
            if (LowerIcon1 == 8) //如果下排圖示皆為同一數並且為8(最大值)
            {
                LowerIcon2 = 0; //將下排中間圖示改為0
            }
            else
            {
                LowerIcon2 += 1; //否則將下排中間圖示+1
            }
        }
        SpinLineIcon[3].gameObject.GetComponent<Image>().sprite = IconSpin[LowerIcon1]; //圖示改為亂數對應圖案
        SpinLineIcon[4].gameObject.GetComponent<Image>().sprite = IconSpin[LowerIcon2]; //圖示改為亂數對應圖案
        SpinLineIcon[5].gameObject.GetComponent<Image>().sprite = IconSpin[LowerIcon3]; //圖示改為亂數對應圖案

        int WinrandomSpin = -1;
        foreach ( var rBet in m_betInfos)
        {
            if(SlotResult <= rBet.rangeMax && SlotResult >= rBet.rangeMin)
            {
                WinrandomSpin = rBet.winSpinPicNumber;
                WINAmount.text = (BetTCoins * rBet.ratio).ToString();
                TransactionText.text = "玩家贏了" + (BetTCoins * rBet.ratio).ToString() + "TAN！";
                MEGAPanel.gameObject.SetActive(true);
                break;
            }
        }

        if (WinrandomSpin == -1)
        {
            WinrandomSpin = 8;
            WINAmount.text = "0";
            TransactionText.text = "玩家輸了！";
        }

        if(WinrandomSpin == 8)  // 如果玩家輸了,中間排三個圖案需要打亂
        {
            int LoseIcon1 = UnityEngine.Random.Range(0, 8); //亂數顯示中間排圖示1
            int LoseIcon2 = UnityEngine.Random.Range(0, 8); //亂數顯示中間排圖示2
            int LoseIcon3 = UnityEngine.Random.Range(0, 8); //亂數顯示中間排圖示3
            if (LoseIcon1 == LoseIcon2 && LoseIcon2 == LoseIcon3) //如果中間排三個圖示皆為同一數,需另作調整
            {
                if (LoseIcon1 == 8) //如果中間排圖示皆為同一數並且為8(最大值)
                {
                    LoseIcon2 = 0; //將中間排中間圖示改為0
                }
                else
                {
                    LoseIcon2 += 1; //否則將中間排中間圖示+1
                }
            }
            SpinLineIconWin[0].gameObject.SetActive(true); //將中間排單獨每格圖示激活
            SpinLineIconWin[1].gameObject.SetActive(true); //將中間排單獨每格圖示激活
            SpinLineIconWin[2].gameObject.SetActive(true); //將中間排單獨每格圖示激活
            SpinLineIconWin[0].gameObject.GetComponent<Image>().sprite = IconSpin[LoseIcon1]; //圖示改為亂數對應圖案
            SpinLineIconWin[1].gameObject.GetComponent<Image>().sprite = IconSpin[LoseIcon2]; //圖示改為亂數對應圖案
            SpinLineIconWin[2].gameObject.GetComponent<Image>().sprite = IconSpin[LoseIcon3]; //圖示改為亂數對應圖案
        }
        else  // 如果玩家贏了,中間三個圖案顯示對應圖示
        {
            for (int i = 0; i < 3; i++)
            {
                SpinLineIconWin[i].gameObject.SetActive(true); //將中間排單獨每格圖示激活
                SpinLineIconWin[i].gameObject.GetComponent<Image>().sprite = IconSpin[WinrandomSpin]; //圖示改為中獎對應圖案
                SpinLineIconWin[i].gameObject.GetComponent<ScaleSpring>().enabled = true;
            }
        }
        ReloadBalance();  //Reload 玩家TAN餘額
        ReloadPollBalance();  //Reload 合約獎池TAN餘額

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion

    #region Update
    void Update()
    {
        SpinCount += Time.deltaTime;
        StartSpin = PlayerPrefs.GetInt("StartSpin");
        if (SpinCount < 3)
        {
            int randomSpin1 = UnityEngine.Random.Range(0, 9);
            int randomSpin2 = UnityEngine.Random.Range(0, 9);
            int randomSpin3 = UnityEngine.Random.Range(0, 9);
            SpinLine01.gameObject.GetComponent<Image>().sprite = SpinLineSprit[randomSpin1];
            SpinLine02.gameObject.GetComponent<Image>().sprite = SpinLineSprit[randomSpin2];
            SpinLine03.gameObject.GetComponent<Image>().sprite = SpinLineSprit[randomSpin3];
        }
        else if (SpinCount < 10 && SpinCount > 3)
        {
            if(SpinType == 1)
            {
                Reward();
            }
            else if (SpinType == 10)
            {
                Reward();
            }
        }

        if (StartSpin >= 1)
        {
            StartCoroutine(SpinStartGO());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    #endregion
}
