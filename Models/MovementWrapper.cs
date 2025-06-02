namespace AkouoApi.Models;

public class MovementWrapper(string book_id)
{
    public string Book_id { get; set; } = book_id;
    public string Name { get; set; } = "";
    public string Obt_type { get { return OBTTypeEnum.movement.ToString(); } }
    public List<MovementInfo> Movements { get; set; } = [];
}
