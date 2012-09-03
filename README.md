Dypo
====

A "mini" SQL ORM that supports mapping to dynamic or POCO objects using raw SQL or strongly typed lambda expressions.

**NOTICE**
This was created simply as a research project and will definitely have bugs/issues. It does work for simple cases, but be forewarned!


Examples
--------

#### Dynamic Mapping

	using (var db = Dypo.Db.Connect())
    {
    	var query = db.Select("select Id, Username, Created from account where Id = @0", 1);
        var account = query.First();
        
        Console.WriteLine("Username: " + account.Username);
    }


#### POCO Mapping

	class Account
    {
    	public int Id { get; set; }
        public string Username { get; set; }
        public DateTime Created { get; set; }
    }
    
    using (var db = Dypo.Db.Connect())
    {
    	var query = db.Select<Account>(a => a.Id == 1);
        var account = query.First();
        
        Console.WriteLine("Username: " + account.Username);
    }


License
-------
Apache 2.0