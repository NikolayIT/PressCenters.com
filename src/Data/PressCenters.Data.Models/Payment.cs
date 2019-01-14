namespace PressCenters.Data.Models
{
    using System;

    using PressCenters.Data.Common.Models;

    public class Payment : BaseModel<int>
    {
        public PaymentType Type { get; set; }

        public string Info { get; set; }

        public DateTime? Validity { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int? NewsId { get; set; }

        public virtual News News { get; set; }
    }
}
