namespace AkouoApi.Models;

public class BaseModel
{
    public int Id { get; set; }
}
public class RecordEqualityComparer<T> : IEqualityComparer<T> where T : BaseModel
{
    public bool Equals(T? b1, T? b2)
    {
        return ReferenceEquals(b1, b2) ||
            (b1 is not null && b2 is not null && b1.Id == b2.Id);
    }

    public int GetHashCode(T b) => b.Id;
}
