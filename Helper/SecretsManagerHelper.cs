using System;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

public static class SecretsManagerHelper
{
    public static async Task<string> GetSecretAsync(string secretName, string region = "us-east-1")
    {
        IAmazonSecretsManager client = new AmazonSecretsManagerClient(
            RegionEndpoint.GetBySystemName(region)
        );

        var request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT",
        };

        try
        {
            var response = await client.GetSecretValueAsync(request);
            return response.SecretString;
        }
        catch (AmazonSecretsManagerException awsEx)
        {
            Console.Error.WriteLine(
                $"AWS Secrets Manager error while fetching '{secretName}' in region '{region}': {awsEx.Message}"
            );
            Console.Error.WriteLine(
                $"Status Code: {awsEx.StatusCode}, Error Code: {awsEx.ErrorCode}, Request ID: {awsEx.RequestId}"
            );
            throw;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Unexpected error while retrieving secret '{secretName}' from AWS: {ex.Message}"
            );
            Console.Error.WriteLine(ex.StackTrace);
            throw;
        }
    }
}
