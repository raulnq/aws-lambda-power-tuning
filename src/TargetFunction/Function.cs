using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TargetFunction;

public class Function
{

    public record RegisterTargetRequest(string Name);
    public record RegisterTargetResponse(Guid Id);

    private readonly AmazonDynamoDBClient dynamoClient;
    private readonly AmazonSQSClient sqsClient;
    private readonly DynamoDBContext dbContext;
    private readonly string? queueUrl;
    public Function()
    {
        dynamoClient = new AmazonDynamoDBClient();
        dbContext = new DynamoDBContext(dynamoClient);
        sqsClient = new AmazonSQSClient();
        queueUrl = Environment.GetEnvironmentVariable("QUEUEURL");

    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        context.Logger.Log(JsonSerializer.Serialize(input));

        var req = JsonSerializer.Deserialize<RegisterTargetRequest>(input.Body, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
        var task = new Target() { Id = Guid.NewGuid(), Name = req.Name };
        await dbContext.SaveAsync(task);
        var resp = JsonSerializer.Serialize(new RegisterTargetResponse(task.Id));

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = resp
        });

        return new APIGatewayProxyResponse
        {
            Body = resp,
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
