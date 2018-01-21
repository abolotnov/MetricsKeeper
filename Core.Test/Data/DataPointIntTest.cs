using System;
using Xunit;
using System.Collections.Generic;
using Core.Data;
namespace Core.Test.Data
{
    public class DataPointIntTest
    {

        [Fact]
        public void DataPointIntValidatorValidValuesTest(){
            int[] testset = new int[] { 
                int.MinValue,
                int.MaxValue,
                0,
                2
            };
            foreach (int x in testset){
                DataPointInt d = new DataPointInt(x.ToString());
                Assert.True(d.Validate().IsSuccessful);
            }
        }

        [Fact]
        public void DataPointIntValidatorEmptyOrNullTest(){
            DataPointInt d = new DataPointInt("");
            Assert.False(d.Validate().IsSuccessful);

            DataPointInt d1 = new DataPointInt(null);
            Assert.False(d1.Validate().IsSuccessful);
        }

        [Fact]
        public void DataPointIntValidatorTooLargeOrTooSmallTest(){
            DataPointInt d = new DataPointInt("21474836471");
            Assert.False(d.Validate().IsSuccessful);

            DataPointInt d1 = new DataPointInt("-32147483647");
            Assert.False(d1.Validate().IsSuccessful);
        }
    }
}
