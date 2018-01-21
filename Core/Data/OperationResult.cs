using System;
namespace Core.Data
{
    public class OperationResult
    {
        public OperationResult(bool success, string message)
        {
            IsSuccessful = success;
            ValidationMessage = message;
        }

        public bool IsSuccessful { get; set; }
        public string ValidationMessage { get; set; }
    }
}
