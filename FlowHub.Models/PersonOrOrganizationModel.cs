using LiteDB;

namespace FlowHub.Models;

public class PersonOrOrganizationModel
{
    public string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}