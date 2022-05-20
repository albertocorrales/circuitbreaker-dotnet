using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircuitBreaker.Test
{
    public class FunctionExecutionMock
    {
        private int _numberOfExecutions = 0;
        public Task<ResponseMock> ExecuteProcessSuccess()
        {
            var response = new ResponseMock();
            response.ExecutionResult = "Success";
            return Task.FromResult(response);
        }

        public Task<ResponseMock> ExecuteProcessConditional(List<int> iterationsToFail)
        {
            _numberOfExecutions++;
            return iterationsToFail.Contains(_numberOfExecutions) ? ExecuteProcessFailed() : ExecuteProcessSuccess();
        }

        public Task<ResponseMock> ExecuteProcessFailed()
        {
            throw new Exception("mock exception");
        }

        public Task<ResponseMock> ExecuteProcessFallback()
        {
            var response = new ResponseMock();
            response.ExecutionResult = "Fallback";
            return Task.FromResult(response);
        }
    }
}
