using System;
using System.Data.Linq.Mapping;
using CrudDatastore;

namespace CrudDatastore.SqlClientDopper.Entities
{
    [Table(Name = "People")]
    public class Person : EntityBase
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int PersonId { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
