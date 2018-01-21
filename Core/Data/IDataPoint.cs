using System;
namespace Core.Data
{
    public interface IDataPoint<T> : IDataPointListable
    {
        System.Type GetExpectedValueType();
        string GetDescription();
        OperationResult Validate();
        T GetParsedValue();
    }
}
