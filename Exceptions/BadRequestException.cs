namespace InventoryManagementSystem.Exceptions
{
    public class BadRequestException : CustomException
    {
        public BadRequestException(string message) : base(StatusCodes.Status400BadRequest, "Bad Request", message) { }
    }
}