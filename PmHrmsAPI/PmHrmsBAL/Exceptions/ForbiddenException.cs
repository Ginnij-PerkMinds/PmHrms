namespace PmHrmsAPI.PmHrmsBAL.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
