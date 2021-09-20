using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mime;
using System.Security.Principal;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var conString = @"Data source=.\SQLI; Initial catalog=Academy; Integrated security=true";

            while (true)
            {
                Console.Write("1. Creat Account\n2. Show Accountns\n3. Transfer\nChoice:");
                var choice = int.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        {
                            CreateAccount(conString, new NewAccount());
                        }
                        break;
                    case 2:
                        {
                            ShowAccounts(conString);
                        }
                        break;
                    case 3:
                        {
                            Console.Write("From account:");
                            var FromAcc = Console.ReadLine();
                            Console.Write("To account:");
                            var ToAcc = Console.ReadLine();
                            Console.Write("Amount:");
                            decimal.TryParse(Console.ReadLine(), out decimal amount);
                            Transfer(FromAcc, ToAcc, amount, conString);
                        }
                        break;
                    default:
                        {
                            Console.WriteLine("Wrong command!");
                        }
                        break;
                        
                }
                Console.WriteLine("Press any key...");
                Console.ReadLine();
                Console.Clear();

            }

           
        }

        private static void ShowAccounts(string conString)
        {
            var conn = new SqlConnection(conString);
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = "Select * from Account";
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"Account: {reader.GetValue(1)}");
            }
            reader.Close();
            conn.Close();
        }

        private static void CreateAccount(string conString, NewAccount newAccount)
        {
            Console.Write("Enter account number:");
            newAccount.Account = Console.ReadLine();
            newAccount.Created_At = DateTime.Now;
            Console.Write("Enter active status 1 or 0:");
            newAccount.Is_Active = int.Parse(Console.ReadLine());
           

            SqlConnection sqlConnection = new SqlConnection(conString);
            sqlConnection.Open();
            var command = sqlConnection.CreateCommand();
            command.CommandText = "insert into Account(Account, Created_At, Is_Active) " +
                $"values (@account, @createdAt, @isActive)";

            command.Parameters.AddWithValue("@account", newAccount.Account);
            command.Parameters.AddWithValue("@isActive", newAccount.Is_Active);
            command.Parameters.AddWithValue("@createdAt", newAccount.Created_At);
            var result = command.ExecuteNonQuery();
            if (result > 0) Console.WriteLine("Uspex");
            command.Parameters.Clear();
            sqlConnection.Close();
        }

        private static decimal GetAccBalance(string conString, string account)
        {
            var conn = new SqlConnection(conString);
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = "Select Amount from Transactions where Account_Id=@fromAcc";
            command.Parameters.AddWithValue("@fromAcc", account);
            var reader = command.ExecuteReader();
            var accBalance = 0m;
            while (reader.Read())
            {
                accBalance = !string.IsNullOrEmpty(reader.GetValue(0)?.ToString()) ? reader.GetDecimal(0) : 0;
            }
            reader.Close();
            command.Parameters.Clear();
            conn.Close();
            return accBalance;
        }

        private static int GetAccId(string account, string conString)
        {
            var conn = new SqlConnection(conString);
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = "Select Id from Account where Account=@account";
            command.Parameters.AddWithValue("@account", account);
            var accNumber = 0;
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                accNumber = reader.GetInt32(0);
            }
            reader.Close();
            command.Parameters.Clear();
            conn.Close();
            return accNumber;
        }

        private static void Transfer(string FromAcc, string ToAcc, decimal Amount, string conString)
        {
            if (string.IsNullOrEmpty(FromAcc) || string.IsNullOrEmpty(ToAcc) || Amount == 0)
            {
                Console.WriteLine("Something went wrong!");
                return;
            }

            var conn = new SqlConnection(conString);
            conn.Open();
            if (!(conn.State == ConnectionState.Open))
            {
                Console.WriteLine("Connection is not opened!");
                return;
            }

            SqlTransaction sqlTransaction = conn.BeginTransaction();
            var command = conn.CreateCommand();
            command.Transaction = sqlTransaction;
            try
            {
                var FromAccBalance = GetAccBalance(conString, FromAcc);
                if (FromAccBalance <= 0 || (FromAccBalance - Amount) < 0)
                {
                    throw new Exception("From account balance not enough amount!");
                }

                var fromAccId = GetAccId(FromAcc, conString);
                if (fromAccId == 0)
                {
                    throw new Exception("from account not found!");
                }

                command.CommandText = "update set Transactions set Amount = Amount - @amount where Account_Id = @accountId";
                command.Parameters.AddWithValue("@amount", Amount);
                command.Parameters.AddWithValue("@accountId", fromAccId);
                var result1 = command.ExecuteNonQuery();

                var ToAccId = GetAccId(ToAcc, conString);
                if (ToAccId == 0)
                {
                    throw new Exception("To account not found!");
                }

                command.Parameters.Clear();
                command.CommandText = "update set Transactions set Amount = Amount + @amount where Account_Id = @accountId";
                command.Parameters.AddWithValue("@amount", Amount);
                command.Parameters.AddWithValue("@accountId", ToAccId);
                var result2 = command.ExecuteNonQuery();

                if (result1 == 0 || result2 == 0)
                {
                    throw new Exception("Something went wrong!");
                }
                sqlTransaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sqlTransaction.Rollback();
            }
            finally
            {
                conn.Close();
            }

        }
    }

    class NewAccount
    {
        public string Account { get; set; }
        public int Is_Active { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
 
    }

    class Transactions
    {
        public decimal Amount { get; set; }
        public DateTime Created_At { get; set; }
    }
}
