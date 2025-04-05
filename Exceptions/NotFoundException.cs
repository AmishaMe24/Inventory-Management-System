namespace InventoryManagementSystem.Exceptions
{

    public class NotFoundException : CustomException
    {
        public NotFoundException(string message) : base(StatusCodes.Status404NotFound, "Not Found", message) { }
    }
}