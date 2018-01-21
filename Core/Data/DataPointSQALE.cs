using System;
using Xunit;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Core.Data
{
    public class DataPointSQALE : DataPoint, IDataPoint<string>
    {
        public DataPointSQALE(string _x) : base(_x) { }

        public System.Type GetExpectedValueType()
        {
            return typeof(string);
        }

        public string GetParsedValue()
        {
            throw new NotImplementedException();
        }

        public new static string Name = "SQALE";
        public new static string Description = "SQALE: Single Char A-Ea-e";

        public override OperationResult Validate()
        {
            if (this.Value is null)
            {
                return new OperationResult(false, "Nothing to validate - null value given");
            }
            if (Regex.IsMatch(this.Value, "^[A-Ea-e]{1}$"))
            {
                return new OperationResult(true, "OK");
            }
            return new OperationResult(false, "Not a valid SQALE data point");
        }

        [Fact]
        public void DataPointSQALEValidatorTest()
        {
            List<KeyValuePair<bool, string>> tset = new List<KeyValuePair<bool, string>>(){
                new KeyValuePair<bool, string>(false,"a"),
                new KeyValuePair<bool, string>(true,"2"),
                new KeyValuePair<bool, string>(true,"-3342234")
            };

            foreach (KeyValuePair<bool, string> test in tset)
            {
                DataPointSQALE d = new DataPointSQALE(test.Value);
                bool result = d.Validate().IsSuccessful;
                Assert.Equal(result, test.Key);
            }
        }
    }
}