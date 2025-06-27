# HTTP-Endpoint-Monitor-with-AWS-Lambda
This project is a serverless application that uses AWS Lambda to monitor the health of HTTP endpoints. It fetches endpoint URLs from a DynamoDB table, performs health checks, and sends alerts via Amazon SNS if any endpoint fails. Built with .NET 8, it’s designed for reliability and easy deployment.
What It Does





Checks multiple HTTP endpoints simultaneously to ensure they’re up and running.



Stores endpoint URLs in a DynamoDB table for easy configuration.



Sends email or SMS notifications through SNS when an endpoint returns an error.

Requirements

Before starting, ensure you have:





.NET 8 SDK: Download from https://dotnet.microsoft.com/en-us/download/dotnet/8.0.



AWS CLI v2: Install from https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html.



Git: Get it from https://git-scm.com/downloads.



An AWS account with credentials configured (run aws configure).

Installation Steps





Get the Code: Clone your repository to your computer:

git clone https://github.com/raasaamit/http-endpoint-monitor.git
cd http-endpoint-monitor



Set Up Dependencies: Install the required libraries for the main code and tests:

cd src
dotnet restore
cd ../test
dotnet restore



Test the Code: Run the unit tests to verify everything works:

cd test
dotnet test



Create a DynamoDB Table: Set up a DynamoDB table to store the list of endpoints:

aws dynamodb create-table \
  --table-name endpoint-config \
  --attribute-definitions AttributeName=key,AttributeType=S \
  --key-schema AttributeName=key,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region us-east-1



Add Endpoints to Monitor: In the AWS Console, go to DynamoDB > endpoint-config > Items > Create item. Add:





key: endpoints (String)



value: StringSet (e.g., ["https://example.com/health", "https://api.example.com/status"]) Or use the CLI:

aws dynamodb put-item \
  --table-name endpoint-config \
  --item '{"key": {"S": "endpoints"}, "value": {"SS": ["https://example.com/health"]}}' \
  --region us-east-1



Set Up SNS Notifications: Create an SNS topic for alerts:

aws sns create-topic --name EndpointAlerts --region us-east-1

Subscribe your email:

aws sns subscribe \
  --topic-arn arn:aws:sns:us-east-1:your-account-id:EndpointAlerts \
  --protocol email \
  --notification-endpoint your-email@example.com \
  --region us-east-1

Update src/Function.cs with the Topic ARN (e.g., arn:aws:sns:us-east-1:your-account-id:EndpointAlerts).



Create an IAM Role: In the AWS Console, create a role named endpoint-monitor-role with these policies:





AWSLambdaExecute



AmazonDynamoDBFullAccess



AmazonSNSFullAccessNote the Role ARN (e.g., arn:aws:iam::your-account-id:role/endpoint-monitor-role).



Deploy the Lambda Function: Deploy the function using the .NET Lambda tools:

cd src
dotnet lambda deploy-function \
  --function-name endpoint-monitor \
  --function-role arn:aws:iam::your-account-id:role/endpoint-monitor-role \
  --region us-east-1



Schedule Regular Checks: Create a CloudWatch Events rule to run the function every 5 minutes:

aws events put-rule \
  --name EndpointCheckSchedule \
  --schedule-expression "rate(5 minutes)" \
  --state ENABLED \
  --region us-east-1
aws lambda add-permission \
  --function-name endpoint-monitor \
  --statement-id EndpointCheckSchedule \
  --action lambda:InvokeFunction \
  --principal events.amazonaws.com \
  --source-arn arn:aws:events:us-east-1:your-account-id:rule/EndpointCheckSchedule \
  --region us-east-1
aws events put-targets \
  --rule EndpointCheckSchedule \
  --targets "Id"="1","Arn"="arn:aws:lambda:us-east-1:your-account-id:function:endpoint-monitor" \
  --region us-east-1

Project Files





src/: Contains the Lambda function code (Function.cs) and project file (health-check.csproj).



test/: Includes unit tests (UnitTest.cs) and test project file (health-check.Tests.csproj).



aws-lambda-tools-defaults.json: Configures deployment settings.



README.md: This file.

Dependencies





Runtime:





.NET 8



Amazon.Lambda.Core 2.2.0



Amazon.Lambda.Serialization.Json 2.1.1



AWSSDK.DynamoDBv2 3.7.400



AWSSDK.SimpleNotificationService 3.7.300



Newtonsoft.Json 13.0.3



Testing:





xunit 2.9.0



Microsoft.NET.Test.Sdk 17.11.0



Amazon.Lambda.TestUtilities 2.0.0



FakeItEasy 8.3.0

Testing the Function





Manually trigger the function:

cd src
dotnet lambda invoke-function --function-name endpoint-monitor --region us-east-1



Check logs in CloudWatch (Log Groups > /aws/lambda/endpoint-monitor).



Verify notifications in your email for failed endpoints.

Cleanup

To avoid AWS charges, delete resources:

aws lambda delete-function --function-name endpoint-monitor --region us-east-1
aws dynamodb delete-table --table-name endpoint-config --region us-east-1
aws sns delete-topic --topic-arn arn:aws:sns:us-east-1:your-account-id:EndpointAlerts --region us-east-1
aws events delete-rule --name EndpointCheckSchedule --region us-east-1
aws iam detach-role-policy --role-name endpoint-monitor-role --policy-arn arn:aws:iam::aws:policy/AWSLambdaExecute
aws iam detach-role-policy --role-name endpoint-monitor-role --policy-arn arn:aws:iam::aws:policy/AmazonDynamoDBFullAccess
aws iam detach-role-policy --role-name endpoint-monitor-role --policy-arn arn:aws:iam::aws:policy/AmazonSNSFullAccess
aws iam delete-role --role-name endpoint-monitor-role

Contributing

Feel free to open issues or submit pull requests on GitHub.
