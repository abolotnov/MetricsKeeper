using System;
using Xunit;
using System.Collections.Generic;
namespace Core.Data
{
    public class DataPointInt : DataPoint, IDataPoint<int>
    {
        public DataPointInt(string _x) : base(_x) {}

        public System.Type GetExpectedValueType()
        {
            return typeof(int);
        }

        public int GetParsedValue()
        {
            throw new NotImplementedException();
        }

        public new static string Name = "Int";
        public new static string Description = "Simple integer value";

        public override OperationResult Validate()
        {
            if (this.Value is null){
                return new OperationResult(false, "Nothing to validate - null value given");
            }
            if (int.TryParse(this.Value, out int _value)){
                return new OperationResult(true, "OK");
            }
            return new OperationResult(false, "Could not conver to int");
        }

    }
}