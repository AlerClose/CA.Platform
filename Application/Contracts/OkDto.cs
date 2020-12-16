namespace CA.Platform.Application.Contracts
{
    public class OkDto
    {
        public string Message { get; }
        
        public bool Ok { get; }

        public OkDto(string message)
        {
            Message = message;
            Ok = false;
        }

        public OkDto()
        {
            Ok = true;
        }
    }
}