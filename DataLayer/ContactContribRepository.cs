using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using System.Transactions;

namespace DataLayer
{
    public class ContactContribRepository : IContactRepository
    {
        private readonly IDbConnection db;

        public ContactContribRepository(string strConnection)
        {
            this.db = new SqlConnection(strConnection);
        }
        public Contact Add(Contact contact)
        {
            contact.Id = (int)this.db.Insert(contact);
            return contact;
        }

        public Contact Find(int id)
        {
            return this.db.Get<Contact>(id);
        }

        public Address Add(Address address)
        {
            address.Id = (int)this.db.Insert(address);
            return address;
        }

        public Address Update(Address address)
        {
            this.db.Update(address);
            return address;
        }

        public List<Contact> GetAll()
        {
            return this.db.GetAll<Contact>().ToList();
        }

        public Contact GetFullContact(int id)
        {
            var sql = "SELECT * FROM Contacts WHERE Id = @Id; " +
                "SELECT * FROM Addresses WHERE ContactId = @Id;";
            using (var results = this.db.QueryMultiple(sql, new { id }))
            {
                var contact = results.Read<Contact>().SingleOrDefault();
                var addresses = results.Read<Address>().ToList();

                if (contact != null && addresses != null)
                {
                    contact.Addresses.AddRange(addresses);
                }

                return contact;
            }
        }

        public void Remove(int id)
        {
            this.db.Delete(new Contact { Id = id });
        }

        public Contact Update(Contact contact)
        {
            this.db.Update(contact);
            return contact;
        }

        public void Save(Contact contact)
        {
            using var txScope = new TransactionScope();

            if (contact.IsNew)
            {
                this.Add(contact);
            }
            else
            {
                this.Update(contact);
            }

            foreach (var address in contact.Addresses.Where(x => !x.IsDeleted))
            {
                address.ContactId = contact.Id;

                if (address.IsNew)
                {
                    this.Add(address);
                }
                else
                {
                    this.Update(address);
                }
            }

            foreach(var address in contact.Addresses.Where(x => x.IsDeleted))
            {
                this.db.Delete(new Address { Id = address.Id });
            }

            txScope.Complete();
        }
    }
}
