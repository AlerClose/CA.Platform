namespace CA.Platform.Application.Interfaces
{
    public interface IListQuery
    {
        int Skip { get; set; }
        
        int Take { get; set; }
    }
}