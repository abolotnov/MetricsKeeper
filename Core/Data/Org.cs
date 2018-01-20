using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data
{
    public class Org : BaseEntity, IEntityBase
    {
        public string Name { get; set; }
        public List<Portfolio> Portfolios { get; set; }
        public string MetricDatabaseName {
            get{
                return String.Format("org{0}", this.Id);
            }
        }
    }

	public class OrgAccess : BaseEntity, IEntityBase
	{
		public int OrganizationId { get; set; }
		[ForeignKey("OrganizationId")]
		public Org Organization { get; set; }
		
        public string ApplicationUserId { get; set; }
        public bool AllowRead { get; set; }
		public bool AllowWrite { get; set; }
		public bool AllowAdministration { get; set; }
	}
}
