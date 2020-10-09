using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area(nameof(Customer))]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork,
                                IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            //get login user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //set instance
            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value
                                                           , includeProperties: "Product")
            };

            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            //ShoppingCartVM.OrderHeader.ApplicationUser 
            //    = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value,
            //                                                    includeProperties: "Company");


            //manipulate data
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price,
                                                        list.Product.Price50, list.Product.Price100);

                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);

                list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = String.Format($"{list.Product.Description.Substring(0, 99)}...");
                }
            }


            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            //retrieve cart
            var cart = _unitOfWork.ShoppingCart
                                    .GetFirstOrDefault(x => x.Id == cartId, includeProperties: "Product");

            cart.Count += 1;

            //update price if there archive some of amount
            cart.Price = SD.GetPriceBasedOnQuantity
                                (cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            //retrieve cart
            var cart = _unitOfWork.ShoppingCart
                                    .GetFirstOrDefault(x => x.Id == cartId, includeProperties: "Product");

            //if cart was last item, then we need to remove them at db
            if (cart.Count == 1)
            {
                var count = _unitOfWork.ShoppingCart.GetAll
                                        (x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count;

                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

                HttpContext.Session.SetObj(SD.ssShoppingCart, count - 1);
            }
            else
            {
                cart.Count -= 1;

                //update price if there archive some of amount
                cart.Price = SD.GetPriceBasedOnQuantity
                                    (cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

                _unitOfWork.Save();

            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            //retrieve cart
            var cart = _unitOfWork.ShoppingCart
                                    .GetFirstOrDefault(x => x.Id == cartId, includeProperties: "Product");


            var count = _unitOfWork.ShoppingCart.GetAll
                                    (x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count;

            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();

            HttpContext.Session.SetObj(SD.ssShoppingCart, count - 1);

            return RedirectToAction(nameof(Index));

        }
    }
}
