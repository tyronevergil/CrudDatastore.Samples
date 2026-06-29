using System;
using CrudDatastore;

namespace CrudDatastore.MultiDbClientORM.Entities
{
    public class Identification : EntityBase
    {
        public int IdentificationId { get; set; }
        public int PersonId { get; set; }
        public int Type { get; set; }
        public string Number { get; set; }
    }
}
