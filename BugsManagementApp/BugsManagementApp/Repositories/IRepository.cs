using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugsManagementApp.Repositories
{
    public interface IRepository<T>
    {
        public Task Add(T item);
        public Task Delete(int itemID);
        public Task Update(int itemID, T item);
        public Task<T?> Get(int itemID);
        public Task<List<T>> GetAll();
    }

}
