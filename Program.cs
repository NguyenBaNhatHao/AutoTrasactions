using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using AutoTransactions.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

DotNetEnv.Env.Load();
HttpClient client = new HttpClient();
TransactionModel transactionModel = new TransactionModel();
var ApiKey = Environment.GetEnvironmentVariable("apiKey");
var walletId = Environment.GetEnvironmentVariable("walletId");
var StoreTron = Environment.GetEnvironmentVariable("StoreTron");
var DefautlAddress = Environment.GetEnvironmentVariable("DefautAddress");
var TronEnergy = Environment.GetEnvironmentVariable("TronEnergy");
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));
client.DefaultRequestHeaders.Add("X-API-Key",ApiKey);

await ProcessRepositoriesAsync(client, walletId, ApiKey, DefautlAddress, StoreTron, TronEnergy, transactionModel);

static async Task ProcessRepositoriesAsync(HttpClient client, string? walletId, string? ApiKey, string? DefautlAddress,string? StoreTron,string? TronEnergy,TransactionModel transactionModel)
{
    var feelimit = Environment.GetEnvironmentVariable("feelimit");
    var tokenIdentifier = Environment.GetEnvironmentVariable("tokenIdentifier");
    var ReceiveAddress = Environment.GetEnvironmentVariable("ReceiveAddress");
    HttpResponseMessage httpResponse = client.GetAsync("https://rest.cryptoapis.io/wallet-as-a-service/wallets/"+walletId+"/tron/nile/transactions").GetAwaiter().GetResult();
    httpResponse.EnsureSuccessStatusCode(); 
    var responseString = await httpResponse.Content.ReadAsStringAsync();
    JObject jObject = JObject.Parse(responseString);
    var direction = jObject["data"]["items"];
    foreach(var item in direction){
        if(item["direction"].Value<string>().Equals("incoming") && item["fungibleTokens"].HasValues == true){
            Console.WriteLine(direction);
            transactionModel.amount = item["fungibleTokens"][0]["amount"].Value<string>();
            transactionModel.recipient = item["fungibleTokens"][0]["recipient"].Value<string>();
            Console.WriteLine("Income Address: "+transactionModel.recipient);
            await SendTron(client,walletId,ApiKey,StoreTron,TronEnergy,transactionModel);
            await SendToken(client,walletId,ApiKey,ReceiveAddress,feelimit,tokenIdentifier,transactionModel);
        }else{
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

static async Task SendToken (HttpClient client, string? walletId, string? ApiKey,string? ReceiveAddress,string? feelimit,string? tokenIdentifier,TransactionModel transactionModel){
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
    Console.WriteLine(responseString);    
    }
    catch (System.Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}