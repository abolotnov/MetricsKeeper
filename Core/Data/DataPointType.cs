using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Data
{
    public class DataPointType
    {
        public string FullName { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; } 

        public static List<DataPointType> GetAllTypes(){
            List<DataPointType> output = new List<DataPointType>();
            var type = typeof(IDataPointListable);
            var availableTypes = AppDomain.CurrentDomain.GetAssemblies()
                                          .SelectMany(s => s.GetTypes())
                                          .Where(w => w.IsAbstract == false)
                                          .Where(x => type.IsAssignableFrom(x));
            foreach (var item in availableTypes)
            {
                DataPointType t = new DataPointType();
                t.FullName = item.FullName;
                t.Name = item.GetField("Name").GetValue(null).ToString();
                t.Description = item.GetField("Description").GetValue(null).ToString();
                output.Add(t);
            }
            return output;
        }
    }
}
