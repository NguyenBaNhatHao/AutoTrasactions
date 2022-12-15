namespace AutoTransactions.Models{
    public class SendToken{
        public Data data {get;set;}
    }   
    public class Data {
        public Item item {get;set;}
    }
    public class Item{
        public string? amount {get;set;}
        public string recipientAddress {get;set;}
        public string? feeLimit {get;set;}
        public string? tokenIdentifier {get;set;}
        public string note {get;set;}
    }
}