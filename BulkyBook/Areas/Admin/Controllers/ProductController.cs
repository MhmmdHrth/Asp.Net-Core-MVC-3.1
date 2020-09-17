using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })

            };

            if (id == null)
            {
                //for create
                return View(productVM);
            }

            //for update
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null)
            {
                return NotFound();
            }
            return View(productVM.Product);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Upsert(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (product.Id == 0)
        //        {
        //            _unitOfWork.Product.Add(product);
        //        }
        //        else
        //        {
        //            _unitOfWork.Product.Update(product);
        //        }

        //        _unitOfWork.Save();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(product);
        //}

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Successfully deleting record" });
        }

        #endregion
    }
}
