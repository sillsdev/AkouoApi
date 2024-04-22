namespace AkouoApi.Utility;

public static class Utils
{
    public static bool BoolParse(string? value)
    {
        return value?.ToLower() switch
        {
            "true" => true,
            "false" => false,
            _ => false
        };
    }
}
