using Dapper;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data;

namespace DataLayer
{
    public class ContactRepositoryMySql
    {
        private readonly IDbConnection db;

        public ContactRepositoryMySql(string strConnection)
        {
            this.db = new MySqlConnection(strConnection);
        }

        public List<Contact> GetAll()
        {
            return this.db.Query<Contact>("SELECT * FROM Contacts").ToList();
        }
    }

    public class ContactRepository : IContactRepository
    {
        private readonly IDbConnection db;

        public ContactRepository(string strConnection)
        {
            this.db = new SqlConnection(strConnection);
        }
        public Contact Add(Contact contact)
        {
            var sql = "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title) " +
                    "VALUES(@FirstName, @LastName, @Email, @Company, @Title);" +
                    "SELECT CAST(SCOPE_IDENTITY() AS int);";
            contact.Id = this.db.Query<int>(sql, contact).Single();
            return contact;
        }

        public Contact Find(int id)
        {
            return this.db.Query<Contact>("SELECT * FROM Contacts WHERE Id = @Id;", new { id }).SingleOrDefault();
        }

        public List<Contact> GetAll()
        {
            return this.db.Query<Contact>("SELECT * FROM Contacts;").ToList();
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
            this.db.Execute("DELETE FROM Contacts WHERE Id = @Id;", new { id });
        }

        public void Save(Contact contact)
        {
            throw new NotImplementedException();
        }

        public Contact Update(Contact contact)
        {
            string sql = "UPDATE contacts SET " +
                "FirstName = @FirstName, LastName = @LastName, " +
                "Email = @Email, Company = @Company, Title = @Title " +
                "WHERE Id = @Id;";
            this.db.Execute(sql, contact);
            return contact;
        }
    }
}
