using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataLayer
{
    public class ContactRepositoryEx
    {
        private readonly IDbConnection db;

        public ContactRepositoryEx(string strConnection)
        {
            this.db = new SqlConnection(strConnection);
        }

        public async Task<List<Contact>> GetContactsAsync()
        {
            var result = await this.db.QueryAsync<Contact>("SELECT * FROM Contacts;");
            return result.ToList();
        }

        public List<Contact> GetAllContactsWithAddresses()
        {
            var sql = "SELECT * FROM Contacts C INNER JOIN Addresses A ON C.Id = A.ContactId;";
            var contactDict = new Dictionary<int, Contact>();

            return this.db.Query<Contact, Address, Contact>(sql, (contact, address) =>
            {
                if(!contactDict.TryGetValue(contact.Id, out Contact currentContact))
                {
                    currentContact = contact;
                    contactDict.Add(currentContact.Id, currentContact);
                }

                currentContact.Addresses.Add(address);
                return currentContact;
            }).Distinct().ToList();
        }

        public List<Contact> GetContactsById(params int[] ids)
        {
            return this.db.Query<Contact>("SELECT * FROM Contacts WHERE ID in @ids;", new { ids }).ToList();
        }

        public List<dynamic> GetDynamicContactsById(params int[] ids)
        {
            return this.db.Query("SELECT * FROM Contacts WHERE ID in @ids;", new { ids }).ToList();
        }

        public int BulkInsertContacts(List<Contact> contacts)
        {
            var sql =
              "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title) VALUES(@FirstName, @LastName, @Email, @Company, @Title); " +
              "SELECT CAST(SCOPE_IDENTITY() as int)";
            return this.db.Execute(sql, contacts);
        }

        public List<Address> GetAddressesByState(int stateId)
        {
            return this.db.Query<Address>("SELECT * FROM Addresses WHERE StateId = {=stateId}", new { stateId }).ToList();
        }
    }
}
