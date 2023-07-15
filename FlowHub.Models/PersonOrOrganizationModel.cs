using LiteDB;

namespace FlowHub.Models;

public class PersonOrOrganizationModel
{
    [BsonId]
    public string Id { get; set; }
    public required string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}