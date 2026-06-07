namespace NextNet.Edge.Adapters;

/// <summary>
/// Edge runtime adapter for AWS Lambda@Edge.
/// Generates CloudFormation templates and Lambda entry points for CloudFront distributions.
/// </summary>
public class AwsLambdaEdgeAdapter : IEdgeRuntimeAdapter
{
    /// <summary>
    /// Template for a minimal CloudFormation template for Lambda@Edge.
    /// </summary>
    public const string CloudFormationTemplate = """
{
  "AWSTemplateFormatVersion": "2010-09-09",
  "Description": "NextNet Lambda@Edge for CloudFront",
  "Resources": {
    "EdgeFunction": {
      "Type": "AWS::CloudFront::Function",
      "Properties": {
        "Name": "{name}-edge",
        "AutoPublish": true,
        "FunctionCode": "",
        "FunctionConfig": {
          "Comment": "NextNet edge handler",
          "Runtime": "cloudfront-js-2.0"
        }
      }
    },
    "LambdaFunction": {
      "Type": "AWS::Lambda::Function",
      "Properties": {
        "FunctionName": "{name}-edge-lambda",
        "Runtime": "dotnet8",
        "Handler": "{entry}",
        "Role": { "Fn::GetAtt": ["EdgeFunctionRole", "Arn"] },
        "Code": {
          "S3Bucket": "{bucket}",
          "S3Key": "{key}"
        },
        "MemorySize": 128,
        "Timeout": 5
      }
    },
    "EdgeFunctionRole": {
      "Type": "AWS::IAM::Role",
      "Properties": {
        "AssumeRolePolicyDocument": {
          "Version": "2012-10-17",
          "Statement": [
            {
              "Effect": "Allow",
              "Principal": {
                "Service": ["lambda.amazonaws.com", "edgelambda.amazonaws.com"]
              },
              "Action": "sts:AssumeRole"
            }
          ]
        },
        "ManagedPolicyArns": [
          "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
        ]
      }
    }
  }
}
""";

    /// <inheritdoc />
    public string ProviderName => "AWS Lambda@Edge";

    /// <inheritdoc />
    public string ProviderId => "aws";

    /// <summary>
    /// The CloudFront distribution ID (optional).
    /// </summary>
    public string? DistributionId { get; set; }

    /// <summary>
    /// The origin path prefix (optional).
    /// </summary>
    public string? OriginPath { get; set; }

    /// <inheritdoc />
    public Task<IEdgeResponse> HandleRequestAsync(IEdgeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        cancellationToken.ThrowIfCancellationRequested();

        var responseBody = new MemoryStream();
        var headers = new Dictionary<string, string>
        {
            ["x-edge-provider"] = ProviderId,
            ["x-edge-runtime"] = "aws-lambda-edge"
        };

        var edgeResponse = new EdgeResponse(
            statusCode: 200,
            headers: headers,
            body: responseBody);

        return Task.FromResult<IEdgeResponse>(edgeResponse);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticAsset>> GetStaticAssetsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<StaticAsset>>(Array.Empty<StaticAsset>());
    }

    /// <summary>
    /// Generates a CloudFormation template for Lambda@Edge deployment.
    /// </summary>
    /// <param name="functionName">The Lambda function name.</param>
    /// <param name="handler">The .NET handler (assembly::namespace.type::method).</param>
    /// <param name="s3Bucket">The S3 bucket name containing the deployment package.</param>
    /// <param name="s3Key">The S3 key of the deployment package.</param>
    /// <returns>The CloudFormation template JSON string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public string GenerateCloudFormationTemplate(
        string functionName,
        string handler,
        string s3Bucket,
        string s3Key)
    {
        if (functionName == null) throw new ArgumentNullException(nameof(functionName));
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        if (s3Bucket == null) throw new ArgumentNullException(nameof(s3Bucket));
        if (s3Key == null) throw new ArgumentNullException(nameof(s3Key));

        return CloudFormationTemplate
            .Replace("{name}", functionName)
            .Replace("{entry}", handler)
            .Replace("{bucket}", s3Bucket)
            .Replace("{key}", s3Key);
    }
}
