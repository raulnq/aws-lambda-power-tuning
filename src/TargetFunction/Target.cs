using Amazon.DynamoDBv2.DataModel;

namespace TargetFunction;

[DynamoDBTable("targettable")]
public class Target
{
    [DynamoDBHashKey("id")]
    public Guid Id { get; set; }
    [DynamoDBProperty("name")]
    public string? Name { get; set; }
}
