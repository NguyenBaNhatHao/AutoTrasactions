using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using AutoTransactions.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

DotNetEnv.Env.Load();
HttpClient client = new HttpClient();
TransactionModel transactionModel = new TransactionModel();
var ApiKey = Environment.GetEnvironmentVariable("apiKey");
var walletId = Environment.GetEnvironmentVariable("walletId");
var StoreTron = Environment.GetEnvironmentVariable("StoreTron");
var TronEnergy = Environment.GetEnvironmentVariable("TronEnergy");


client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));
client.DefaultRequestHeaders.Add("X-API-Key",ApiKey);

await ProcessRepositoriesAsync(client, walletId, ApiKey, StoreTron, TronEnergy, transactionModel);

static async Task ProcessRepositoriesAsync(HttpClient client, string? walletId, string? ApiKey, string? StoreTron,string? TronEnergy,TransactionModel transactionModel)
{
    var feelimit = Environment.GetEnvironmentVariable("feelimit");
    var tokenIdentifier = Environment.GetEnvironmentVariable("tokenIdentifier");
    var ReceiveAddress = Environment.GetEnvironmentVariable("ReceiveAddress");

    var ServerBGO = Environment.GetEnvironmentVariable("ServerBGO");
    var Database = Environment.GetEnvironmentVariable("Database");
    var UserId = Environment.GetEnvironmentVariable("UserId");
    var Password = Environment.GetEnvironmentVariable("Password");

    HttpResponseMessage httpResponse = client.GetAsync("https://rest.cryptoapis.io/wallet-as-a-service/wallets/"+walletId+"/tron/nile/addresses?context=yourExampleString&limit=50&offset=0").GetAwaiter().GetResult();
    httpResponse.EnsureSuccessStatusCode(); 
    var responseString = await httpResponse.Content.ReadAsStringAsync();
    JObject jObject = JObject.Parse(responseString);
    var direction = jObject["data"]["items"];
    foreach(var item in direction){
        Console.WriteLine(item);
        if(item["fungibleTokens"].HasValues == true && item["fungibleTokens"][0]["amount"].Value<double>() > 0 && item["confirmedBalance"]["amount"].Value<double>() < 13){
            transactionModel.amount = item["fungibleTokens"][0]["amount"].Value<double>().ToString();
            transactionModel.recipient = item["address"].Value<string>();
            Console.WriteLine("Income Address: "+transactionModel.recipient);
            await SendTron(client,walletId,ApiKey,StoreTron,TronEnergy,transactionModel);
            await SendToken(client,walletId,ApiKey,ReceiveAddress,feelimit,tokenIdentifier,ServerBGO,Database,UserId,Password,transactionModel);
        }else if(item["fungibleTokens"].HasValues == true && item["confirmedBalance"]["amount"].Value<double>() >= 13 && item["fungibleTokens"][0]["amount"].Value<double>() > 0){
            transactionModel.amount = item["fungibleTokens"][0]["amount"].Value<double>().ToString();
            transactionModel.recipient = item["address"].Value<string>();
            Console.WriteLine("Income Address: "+transactionModel.recipient);
            await SendToken(client,walletId,ApiKey,ReceiveAddress,feelimit,tokenIdentifier,ServerBGO,Database,UserId,Password,transactionModel);
        }else{
            Console.WriteLine(item["label"]+" khong co USDT de chuyen");
            continue;
        }
    }
}

static async Task SendTron (HttpClient client,string? walletId,string? ApiKey, string? StoreTron, string? TronEnergy, TransactionModel transactionModel){
    SendCoin sendCoin = new SendCoin();
    DataSendCoin data = new DataSendCoin();
    ItemSendCoin item = new ItemSendCoin();

    item.amount = TronEnergy;
    item.recipientAddress = transactionModel.recipient;
    item.note="auto send coin";
    sendCoin.data = data;
    sendCoin.data.item = item;
    string dataJson = JsonConvert.SerializeObject(sendCoin);
    HttpContent c = new StringContent(dataJson, Encoding.UTF8, "application/json");
    HttpResponseMessage httpResponse = client.PostAsync("https://rest.cryptoapis.io/wallet-as-a-service/wallets/"+walletId+"/tron/nile/addresses/"+StoreTron+"/feeless-transaction-requests", c).GetAwaiter().GetResult();
    httpResponse.EnsureSuccessStatusCode(); 
    
    var responseString = await httpResponse.Content.ReadAsStringAsync();
    Console.WriteLine(responseString);
}

static async Task SendToken (HttpClient client, string? walletId, string? ApiKey,string? ReceiveAddress,string? feelimit,string? tokenIdentifier,string? ServerBGO, string? Database, string? UserId,string? Password,TransactionModel transactionModel){
    try
    {
        HttpRequestMessage req = new HttpRequestMessage();
    // var dic = new Dictionary<string, string>()
    // {
    //     { "currencycode", "USDT" },
    //     { "amount", transactionModel.amount },
    //     { "recipientAddress", transactionModel.recipient },
    //     { "note", "Auto chuyen tien" }
    // };
    SendToken sendToken = new SendToken();
    Data data = new Data();
    Item item = new Item();

    item.amount = transactionModel.amount;
    item.feeLimit = feelimit;
    item.tokenIdentifier = tokenIdentifier;
    item.amount = transactionModel.amount;
    item.recipientAddress = ReceiveAddress;
    item.note="auto send token";
    sendToken.data = data;
    sendToken.data.item = item;
    string dataJson = JsonConvert.SerializeObject(sendToken);
    HttpContent c = new StringContent(dataJson, Encoding.UTF8, "application/json");
    HttpResponseMessage httpResponse = client.PostAsync("https://rest.cryptoapis.io/wallet-as-a-service/wallets/"+walletId+"/tron/nile/addresses/"+transactionModel.recipient+"/feeless-token-transaction-requests", c).GetAwaiter().GetResult();
    httpResponse.EnsureSuccessStatusCode(); 
    var responseString = await httpResponse.Content.ReadAsStringAsync();
    // SqlConnection conn = new SqlConnection();
    // conn.ConnectionString = 
    // "Server="+ServerBGO+";"+
    // "Database="+Database+";"+
    // "User Id="+UserId+";"+
    // "Password="+Password+";"+
    // "Trust Server Certificate=true";
    // // conn.Open();
    // SqlCommand cmd = new SqlCommand("usp_USDTTransactions_Insert", conn);
    // cmd.CommandType = System.Data.CommandType.StoredProcedure;
    // if (cmd.Connection.State != System.Data.ConnectionState.Open) cmd.Connection.Open();
    
    Console.WriteLine(responseString);    
    }
    catch (System.Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}