﻿using System;
using static Core.Data.MetricModel;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Core.Data
{
    public class Metric : BaseEntity, IEntityBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ModelType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
        public MetricModel Model
        {
            get
            {
                return new MetricModel(Type);
            }
        }
    }
}
