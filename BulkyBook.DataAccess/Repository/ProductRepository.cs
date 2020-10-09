using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System.Linq;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var objFromDb = _db.Products.FirstOrDefault(x => x.Id == product.Id);

            if (product.ImageUrl != null)
            {
                objFromDb.ImageUrl = product.ImageUrl;
            }

            objFromDb.Price = product.Price;
            objFromDb.Price50 = product.Price50;
            objFromDb.Price100 = product.Price100;
            objFromDb.ListPrice = product.ListPrice;
            objFromDb.Author = product.Author;
            objFromDb.ISBN = product.ISBN;
            objFromDb.Title = product.Title;
            objFromDb.Description = product.Description;
            objFromDb.CategoryId = product.CategoryId;
            objFromDb.CoverTypeId = product.CoverTypeId;
        }
    }
}