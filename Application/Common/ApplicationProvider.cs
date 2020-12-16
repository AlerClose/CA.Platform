namespace CA.Platform.Application.Common
{
    public class ApplicationProvider
    {
        private readonly string _appKey;
        
        public ApplicationProvider(string appKey)
        {
            _appKey = appKey;
        }

        public string GetAppKey()
        {
            return _appKey;
        }
    }
}