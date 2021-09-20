use Academy
--insert into Account(Account.Account, Is_Active, Created_At) values ('00004', 0, '19-09-2021')
--insert into Transactions (Account_Id, Amount, Created_At) values (18, 0, '19-09-2021')

 --(case when t.Amount=0.00 then a.Is_Active=0 else a.Is_Active=1 end)
/*select a.Account,
t.Account_Id, a.Is_Active, t.Amount, a.Created_At
from Transactions t
right join Account a
on a.id=t.Account_Id*/
--select * from Account
--select * from Transactions
--select t.Amount from Transactions t where Id='00008'
--when Transactions.Amount > 0 then Account.Is_Active = 1
--else Account.Is_Active = 0 end
--update Transactions set Amount=Amount+200 where Account_Id = 16
delete Account where id = 29
select * from Transactions 
right join Account on Transactions.Account_Id=Account.Id


