namespace InventoryManagementSystem.Exceptions
{

    public class ConflictException : CustomException
    {
        public ConflictException(string message) : base(StatusCodes.Status409Conflict, "Conflict", message) { }
    }
}