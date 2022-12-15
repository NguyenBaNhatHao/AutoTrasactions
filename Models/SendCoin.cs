namespace AutoTransactions.Models{
    public class SendCoin{
        public DataSendCoin data {get; set;}
    }
    public class DataSendCoin{
        public ItemSendCoin item {get; set;}
    }
    public class ItemSendCoin{
        public string amount {get;set;}
        public string note {get; set;}
        public string recipientAddress {get;set;}
    }
}