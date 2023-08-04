using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace DataLayer
{
    public class ContactRepositorySP : IContactRepository
    {
        private readonly IDbConnection db;

        public ContactRepositorySP(string strConnection)
        {
            this.db = new SqlConnection(strConnection);
        }
        public Contact Add(Contact contact)
        {
            throw new NotImplementedException();
        }

        public Contact Find(int id)
        {
            return this.db.Query<Contact>("GetContact", new { id },
                commandType: CommandType.StoredProcedure).SingleOrDefault();
        }

        public List<Contact> GetAll()
        {
            throw new NotImplementedException();
        }

        public Contact GetFullContact(int id)
        {
            using var results = this.db.QueryMultiple("GetContact", new { id },
                commandType: CommandType.StoredProcedure);

            var contact = results.Read<Contact>().SingleOrDefault();
            var addresses = results.Read<Address>().ToList();

            if (contact != null && addresses != null)
            {
                contact.Addresses.AddRange(addresses);
            }

            return contact;
        }

        public void Remove(int id)
        {
            this.db.Execute("DeleteContact", new { id }, commandType: CommandType.StoredProcedure);
        }

        public void Save(Contact contact)
        {
            using var txScope = new TransactionScope();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", value: contact.Id, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("@FirstName", contact.FirstName);
            parameters.Add("@LastName", contact.LastName);
            parameters.Add("@Company", contact.Company);
            parameters.Add("@Title", contact.Title);
            parameters.Add("@Email", contact.Email);
            this.db.Execute("SaveContact", parameters, commandType: CommandType.StoredProcedure);
            contact.Id = parameters.Get<int>("@Id");

            foreach (var addr in contact.Addresses.Where(a => !a.IsDeleted))
            {
                addr.ContactId = contact.Id;

                var addrParams = new DynamicParameters(new
                {
                    addr.ContactId,
                    addr.AddressType,
                    addr.StreetAddress,
                    addr.City,
                    addr.StateId,
                    addr.PostalCode
                }); 

                addrParams.Add("@Id", addr.Id, DbType.Int32, ParameterDirection.InputOutput);
                this.db.Execute("SaveAddress", addrParams, commandType: CommandType.StoredProcedure);
                addr.Id = addrParams.Get<int>("@Id");
            }

            foreach (var addr in contact.Addresses.Where(a => a.IsDeleted))
            {
                this.db.Execute("DeleteAddress", new { addr.Id }, commandType: CommandType.StoredProcedure);
            }

            txScope.Complete();
        }

        public Contact Update(Contact contact)
        {
            throw new NotImplementedException();
        }
    }
}
