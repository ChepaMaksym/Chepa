using System.Linq;
using Warehouse.Manager;
namespace Chepa.Bot.Db
{
    public class UserRepository
    {
        private readonly ChepaBotContext context = new ChepaBotContext();

        public UserRepository(ChepaBotContext context)
        {
            this.context = context;
        }
        public IQueryable<User> GetAll()
        {
            return context.Users;
        }
        public User GetUser(int id)
        {
            return context.Users.Find(id);
        }

        public void UpdateRights(string userName, Rights rights)
        {
            var existingUser = context.Users.FirstOrDefault(u => u.UserName == userName);
            if (existingUser != null)
            {
                existingUser.Rights = rights;
                context.SaveChanges();
            }
        }
        public void Update(User user)
        {
            var existingUser = context.Users.Find(user.UserId);

            if (existingUser != null)
            {
                existingUser.UserName = user.UserName;
                existingUser.ChatId = user.ChatId;
                existingUser.Rights = user.Rights;
                existingUser.IsSetBuyItem = user.IsSetBuyItem;//make
                existingUser.StoreId = user.StoreId;
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var userToDelete = context.Users.Find(id);
            if (userToDelete != null)
            {
                context.Users.Remove(userToDelete);
                context.SaveChanges();
            }
        }
    }
}
