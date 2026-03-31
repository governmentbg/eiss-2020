namespace IOWebApplicationService.Infrastructure.Constants
{
    public static class DWConstants
    {
        public class DWTransfer
        {
            /// <summary>
            /// Брой редове на транзакция
            /// </summary>
            public const int TransferRowCounts = 50;

            //Общ брой редове, които да бъдат прехвърлени на 1 итерация на DWServiceJob-а
            public const int TotalRowCounts = 5000;
        }
    }
}
