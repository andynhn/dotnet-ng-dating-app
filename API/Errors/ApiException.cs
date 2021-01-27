namespace API.Errors
{
    public class ApiException
    {
        // add constructor to help make it easier to use.
        // can also modify default values here (e.g. for message and details)
        public ApiException(int statusCode, string message = null, string details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}