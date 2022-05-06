using DevInSales.Core.Data.Context;
using DevInSales.EFCoreApi.Core.Interfaces;
using RegexExamples;

namespace DevInSales.Core.Entities
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        public UserService (DataContext context)
        {
            _context = context; 
        }

         public int CriarUser(User user)
        {
            var EmailValido = new EmailValidate();

            if (!EmailValido.IsValidEmail(user.Email) || _context.Users.Any(x=> x.Email == user.Email))
                return 0;

            if (user.BirthDate.AddYears(18) > DateTime.Now)
                return 0;

            if (user.Password.Length < 4 || IsValidPass(user.Password))
                return 0;
    
            _context.Users.Add(user);
            _context.SaveChanges();
            return user.Id;
        } 
       
          public bool IsValidPass(string password)
        {
            return password.Length == 0 || password.All(ch => ch == password[0]);
        }

         public User? ObterPorId(int id)
        {
            return _context.Users.Find(id);
        }

        private bool StringValida(string? text)
        {
            return !String.IsNullOrWhiteSpace(text);
        }

        public List<User> ObterUsers(string? name, string? DataMin, string? DataMax)
        {
           var result = _context.Users.Where(StringValida(name) && StringValida(DataMin) && StringValida(DataMax) ?
                        x => x.Name == name && x.BirthDate >= DateTime.Parse(DataMin) && x.BirthDate <= DateTime.Parse(DataMax) : 
                        StringValida(name) && StringValida(DataMin) ? x => x.Name == name && x.BirthDate >= DateTime.Parse(DataMin) :
                        StringValida(name) && StringValida(DataMax) ? x => x.Name == name && x.BirthDate <= DateTime.Parse(DataMax) :
                        StringValida(DataMin) && StringValida(DataMax) ? x => x.BirthDate >= DateTime.Parse(DataMin) && x.BirthDate <= DateTime.Parse(DataMax) :
                        StringValida(name) ? x => x.Name == name :
                        StringValida(DataMin) ? x => x.BirthDate >= DateTime.Parse(DataMin) :
                        StringValida(DataMax) ? x => x.BirthDate <= DateTime.Parse(DataMax) :
                       x => true).ToList();
           return result;
        }
        public void RemoverUser(int id){
            if (id >= 0){
                var user = _context.Users.FirstOrDefault(user => user.Id == id);
                if (user != null)
                    _context.Users.Remove(user); 
                    _context.SaveChanges();
            }
        }
    }
}