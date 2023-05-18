namespace SpendLess.Shared
{
    public class UserDto
    {

        public string? Username { get; set; } = "User";
        public string? Email { get; set; }
        public string? Password { get; set; }
        public double Balance { get; set; } = 0;
        public int Id { get; set; }

        /* public User(string? username, string? email, string? password)
         {
             this.username = username;
             this.emailAddress = email;
             this.password = password;
             this.balance = 0; 
         }*/
        /* public string? password
         {

             get
             {
                 return password;
             }

             set
             {
                 if (value == null)
                     value = String.Empty;

                 password = value.PasswordHash();
             }
         }
     }*/



    }
}