using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data
{
    public abstract class DataPoint : BaseEntity, IEntityBase
    {
        protected DataPoint(string _v){
            Value = _v;
        }
        
        public string Name { get; }
        public string Description { get; }
        public string Value { get; set; }

        public abstract OperationResult Validate();

        public string GetName()
        {
            return this.Name;
        }

        public string GetDescription()
        {
            return this.Description;
        }

        public string GetValue()
        {
            return this.Value;
        }

    }
}
