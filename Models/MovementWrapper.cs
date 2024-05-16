namespace AkouoApi.Models;

public class MovementWrapper
{
    public MovementWrapper(string book_id)
    {
        Book_id = book_id;
    }

    public string Book_id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Obt_type { get { return OBTTypeEnum.movement.ToString(); } }
    public List<MovementInfo> Movements { get; set; } = new();
}
