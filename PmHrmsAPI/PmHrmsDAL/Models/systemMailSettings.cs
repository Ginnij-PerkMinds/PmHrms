namespace PmHrmsAPI.PmHrmsDAL.Models
{
public class SystemMailSetting
{
    public int Id { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string Mail { get; set; }
    public string? DisplayName { get; set; }
    public string? UserName { get; set; }
    public string Password { get; set; }
    public DateTime UpdatedAt { get; set; }
}
}