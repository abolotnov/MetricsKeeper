using System;
using Xunit;
using System.Collections.Generic;
using Core.Data;
namespace Core.Test.Data
{
    public class DataPointSQALETest
    {

        [Fact]
        public void DataPointSQALEValueNull(){
            DataPointSQALE d = new DataPointSQALE(null);
            Assert.False(d.Validate().IsSuccessful);

            DataPointSQALE d1 = new DataPointSQALE("");
            Assert.False(d1.Validate().IsSuccessful);
        }

        [Fact]
        public void DataPointSQALEValueTooLong(){
            DataPointSQALE d = new DataPointSQALE("AEV");
            Assert.False(d.Validate().IsSuccessful);
        }

        [Fact]
        public void DataPointSQALEValueOutOfIntendedRange(){
            DataPointSQALE d = new DataPointSQALE("T");
            Assert.False(d.Validate().IsSuccessful);
        }

        [Fact]
        public void DataPointSQALECorrectValue(){
            char[] validChars = "ABCDEabcde".ToCharArray();
            foreach (char x in validChars){
                DataPointSQALE testpoint = new DataPointSQALE(x.ToString());
                Assert.True(testpoint.Validate().IsSuccessful);
            }
        }
    }
}
