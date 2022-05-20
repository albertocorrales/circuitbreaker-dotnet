using CircuitBreaker.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CircuitBreaker.Test
{
    public class CircuitBreakerTests
    {
        [Fact]
        public async Task Fire_SuccessfulExecution_CircuitBreakerClosed()
        {
            //Arrange
            var storageMock = new Mock<ICircuitBreakerRepository>();
            var loggerMock = new Mock<ILogger>();
            var functionExecutionMock = new FunctionExecutionMock();
            var options = new CircuitBreakerOptions<ResponseMock>("123", () => functionExecutionMock.ExecuteProcessSuccess());

            //Act
            var circuitBreaker = new CircuitBreakerInstance<ResponseMock>(options, storageMock.Object, loggerMock.Object);
            var response = await circuitBreaker.Fire();

            //Assert
            response.Should().NotBeNull();
            response.ExecutionResult.Should().Be("Success");
            circuitBreaker.CircuitBreakerModel.Status.Should().Be(CircuitBreakerStatus.Closed);
        }

        [Fact]
        public async Task Fire_FailedExecutionLessThanThreshold_CircuitBreakerClosed()
        {
            //Arrange
            var storageMock = new Mock<ICircuitBreakerRepository>();
            var loggerMock = new Mock<ILogger>();
            var functionExecutionMock = new FunctionExecutionMock();
            var options = new CircuitBreakerOptions<ResponseMock>("circuitBreaker123", () => functionExecutionMock.ExecuteProcessFailed());

            //Act
            var circuitBreaker = new CircuitBreakerInstance<ResponseMock>(options, storageMock.Object, loggerMock.Object);
            Func<Task<ResponseMock>> act = async () => await circuitBreaker.Fire();

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("mock exception");
            circuitBreaker.CircuitBreakerModel.Status.Should().Be(CircuitBreakerStatus.Closed);
        }

        [Fact]
        public async Task Fire_FailedWithFallback_CircuitBreakerClosedAndCallFallback()
        {
            //Arrange
            var storageMock = new Mock<ICircuitBreakerRepository>();
            var loggerMock = new Mock<ILogger>();
            var functionExecutionMock = new FunctionExecutionMock();
            var options = new CircuitBreakerOptions<ResponseMock>(
                "123", 
                () => functionExecutionMock.ExecuteProcessFailed(), 
                Fallback: () => functionExecutionMock.ExecuteProcessFallback());

            //Act
            var circuitBreaker = new CircuitBreakerInstance<ResponseMock>(options, storageMock.Object, loggerMock.Object);
            var response = await circuitBreaker.Fire();

            //Assert
            response.Should().NotBeNull();
            response.ExecutionResult.Should().Be("Fallback");
            circuitBreaker.CircuitBreakerModel.Status.Should().Be(CircuitBreakerStatus.Closed);
        }

        [Fact]
        public async Task Fire_FailedExecutionMoreThanThreshold_CircuitBreakerOpen()
        {
            //Arrange
            var storageMock = new Mock<ICircuitBreakerRepository>();
            var loggerMock = new Mock<ILogger>();
            var functionExecutionMock = new FunctionExecutionMock();
            var options = new CircuitBreakerOptions<ResponseMock>("123", () => functionExecutionMock.ExecuteProcessFailed());

            //Act
            var circuitBreaker = new CircuitBreakerInstance<ResponseMock>(options, storageMock.Object, loggerMock.Object);
            for (int i = 0; i < 5; i++) 
            {
                Action act = async () => await circuitBreaker.Fire();
                act.Invoke();

                //Assert
                var expectedStatus = i < 4 ? CircuitBreakerStatus.Closed : CircuitBreakerStatus.Open;
                circuitBreaker.CircuitBreakerModel.Status.Should().Be(expectedStatus);
            }   
        }

        [Fact]
        public async Task Fire_WaitTimeoutAfterOpen_CircuitBreakerHalf()
        {
            //Arrange
            var storageMock = new Mock<ICircuitBreakerRepository>();
            var loggerMock = new Mock<ILogger>();
            var functionExecutionMock = new FunctionExecutionMock();
            var options = new CircuitBreakerOptions<ResponseMock>(
                "123", 
                () => functionExecutionMock.ExecuteProcessConditional(new List<int> { 1, 2, 3, 4, 5 }),
                timeout: 200);

            var circuitBreaker = new CircuitBreakerInstance<ResponseMock>(options, storageMock.Object, loggerMock.Object);
            for (int i = 0; i < 5; i++)
            {
                Action act = async () => await circuitBreaker.Fire();
                act.Invoke();
                var expectedStatus = i < 4 ? CircuitBreakerStatus.Closed : CircuitBreakerStatus.Open;
                circuitBreaker.CircuitBreakerModel.Status.Should().Be(expectedStatus);
            }

            //Act
            Task.Delay(500).Wait();
            var response = await circuitBreaker.Fire();

            //Assert
            response.Should().NotBeNull();
            response.ExecutionResult.Should().Be("Success");
            circuitBreaker.CircuitBreakerModel.Status.Should().Be(CircuitBreakerStatus.Half);
        }
    }
}
