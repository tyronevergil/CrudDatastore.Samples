using System;
using System.Collections.Generic;
using CrudDatastore;

namespace CrudDatastore.SqlClient.Entities
{
    public class Person : EntityBase
    {
        public int PersonId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
