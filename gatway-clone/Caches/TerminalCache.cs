using gatway_clone.Utils.CacheBase;
using System.Text.Json.Serialization;

namespace gatway_clone.Caches
{
    public class TerminalCache
    {
    }
    public class TerminalObject : CacheData
    {
        public string MID { get; set; }
        public string TID { get; set; }
        public string MCC { get; set; }
        public string CurrencyCode { get; set; }
        public string BankCode { get; set; }
        public int Systrace { get; set; }
        public int BatchNo { get; set; }
        public string SerialNumber { get; set; }
        public string BeneficiaryMethod { get; set; }
        public int ReceivedDate { get; set; }
        public string WalletToAcc { get; set; }
        public string WalletPayAcc { get; set; }
        public string MOMerchantID { get; set; }
        public string MOTerminalID { get; set; }
        public TerminalStatus Status { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool HasTransaction() => TerminalInfos.Any(x => x.Value.HasTransaction);
        public bool IsDisable { get; set; }
        [JsonIgnore]
        public bool IsAutoSettlement { get; set; }
        [JsonIgnore]
        public bool IsForceSettlement { get; set; }
        [JsonIgnore]
        public string LMID { get; set; }
        public Dictionary<string, TerminalInfo> TerminalInfos { get; set; } = new();
        public string CurrentSystrace() => Systrace.ToString("D6");

        public string NextSystrace() => (++Systrace == 999999 ? Systrace = 1 : Systrace).ToString("D6");

        public string CurrentBatchNo(string networkIdentifier) => TerminalInfos[networkIdentifier].BatchNo.ToString("D6");

        public void IncreaseBatchNo(string networkIdentifier)
        {
            if (BatchNo++ == 999999)
            {
                BatchNo = 1;
            }
            TerminalInfos[networkIdentifier].BatchNo = BatchNo;
            if (TerminalInfos.Any(x => x.Value.BatchNo == TerminalInfos[networkIdentifier].BatchNo && x.Key != networkIdentifier))
            {
                IncreaseBatchNo(networkIdentifier);
            }
        }

     
        public override string GetPrimaryKey() => $"{MID}_{TID}_{BankCode}";

        public TerminalInfo this[string networkIdentifier] => TerminalInfos[networkIdentifier];
    }

    public enum TerminalStatus : int
    {
        READY = 0,
        PROCESSING_SALE = 1,
        PROCESSING_REVERSALSALE = 2,
        PROCESSING_VOID = 3,
        PROCESSING_SETTLEMENT = 4,
        SETTLEMENT_ERROR = 5,
        WAIT_COMPLETION_TRANSACTION = 6
    }

    public class TerminalInfo
    {
        public bool HasTransaction { get; set; }
        public int BatchNo { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransaction { get; set; }
    }
}
