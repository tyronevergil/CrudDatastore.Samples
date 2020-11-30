using System;
using System.Transactions;
using CrudDatastore.MultiDbClientORM.Entities;

namespace CrudDatastore.MultiDbClientORM
{
    public class MultiDbClientUnitOfWork : UnitOfWorkBase
    {
        public MultiDbClientUnitOfWork(string sqlClientConnectionString, string orcleClientConnectionString)
        {
            var dataStorePerson = new DataStore<Person>(new SqlClientCrudAdapter<Person>(sqlClientConnectionString, "People", p => p.PersonId));
            var dataStoreIdentification = new DataStore<Identification>(new OracleClientCrudAdapter<Identification>(orcleClientConnectionString, "Identifications", p => p.IdentificationId));

            this.Register(dataStorePerson)
                .Map(p => p.Identifications, (p, i) => p.PersonId == i.PersonId);
            this.Register(dataStoreIdentification);
        }

        public override void Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                base.Commit();
                scope.Complete();
            }
        }
    }
}
