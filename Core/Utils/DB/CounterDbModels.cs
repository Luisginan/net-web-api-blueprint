using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB
{
    [ExcludeFromCodeCoverage]

    [Table("ol_ms_counter", "ms_counter_id")]
    public class CounterDbModel
    {
        [Field("ms_counter_id")]
        public long Id { get; set; }

        [Field("ms_counter_table_name")]
        public string TableName { get; set; } = string.Empty;
        [Field("ms_counter_columns_name")]
        public string ColumnName { get; set; } = string.Empty;
        [Field("ms_counter_partner_id")]
        public string PartnerCode { get; set; } = string.Empty;
        [Field("ms_counter_reset")]
        public string Reset { get; set; } = string.Empty;
        [Field("ms_counter_reset_time")]
        public DateTime? ResetTime { get; set; }
        [Field("ms_counter_next_value")]
        public long NextValue { get; set; } = 0;
    }
}
