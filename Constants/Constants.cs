namespace CapfortuneBE.Constants
{
    public class Constants
    {
        public static class EnquiryStatus
        {
            public const string PENDING = "Pending";
            public const string REPLIED = "Replied";
            public const string CLOSED = "Closed";
        }
        public static class  Messages
        {
            public const string ErrorMessage = "An unexpected error occurred. Please try again later.";
        }
        public static class Layers
        {
            public const string Controller = "Controller";
            public const string Service = "Service";
            public const string DataAccess = "DataAccess";
        }
    }
}
