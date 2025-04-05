namespace InventoryManagementSystem.Exceptions
{

    public class CustomException : Exception
    {
        public int StatusCode { get; }
        public string Title { get; }

        public CustomException(int statusCode, string title, string message) : base(message)
        {
            StatusCode = statusCode;
            Title = title;
        }
    }
}