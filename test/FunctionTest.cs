using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using FakeItEasy;
using InTheClouds.Lambda.HealthCheck;
using Xunit;

namespace InTheClouds.Lambda.HealthCheck.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void Function_Can_Be_Constructed()
        {
            // Arrange
            var fakeDdbClient = A.Fake<IAmazonDynamoDB>();
            var fakeSnsClient = A.Fake<IAmazonSimpleNotificationService>();

            // Act
            var function = new Function(fakeDdbClient, fakeSnsClient);

            // Assert
            Assert.NotNull(function);
        }

        [Fact]
        public async Task Function_FunctionHandler_Can_Be_Invoked_With_Valid_Endpoint()
        {
            // Arrange
            const string endpoint = "http://example.com";

            var fakeDdbClient = A.Fake<IAmazonDynamoDB>();
            var fakeSnsClient = A.Fake<IAmazonSimpleNotificationService>();
            var function = new Function(fakeDdbClient, fakeSnsClient);

            var response = new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue>
                {
                    { "value", new AttributeValue(new List<string> { endpoint } ) }
                }
            };

            A.CallTo(() => fakeDdbClient.GetItemAsync(A<string>.Ignored, A<Dictionary<string, AttributeValue>>.Ignored, A<CancellationToken>.Ignored))
                .Returns(response);

            var context = new TestLambdaContext();

            // Act
            await function.FunctionHandler(context);

            // Assert
            A.CallTo(() => fakeSnsClient.PublishAsync(A<PublishRequest>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Function_FunctionHandler_Publishes_Notification_For_Bad_Endpoint()
        {
            // Arrange
            const string endpoint = "bad.endpoint";

            var fakeDdbClient = A.Fake<IAmazonDynamoDB>();
            var fakeSnsClient = A.Fake<IAmazonSimpleNotificationService>();
            var function = new Function(fakeDdbClient, fakeSnsClient);

            var response = new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue>
                {
                    { "value", new AttributeValue(new List<string> { endpoint } ) }
                }
            };

            A.CallTo(() => fakeDdbClient.GetItemAsync(A<string>.Ignored, A<Dictionary<string, AttributeValue>>.Ignored, A<CancellationToken>.Ignored))
                .Returns(response);

            var context = new TestLambdaContext();

            // Act
            await function.FunctionHandler(context);

            // Assert
            A.CallTo(() => fakeSnsClient.PublishAsync(A<PublishRequest>.That.Matches(r => r.Message.Contains(endpoint)), A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }
}
