using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderDetailsVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderDetailsVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id,includeProperties:"ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(x => x.OrderId == id,includeProperties:"Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id);

            if(stripeToken != null)
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),//in cent
                    Currency = "usd",
                    Description = $"Order ID: {orderHeader.Id}",
                    Source = stripeToken
                };

                //create charge service
                var service = new ChargeService();
                Charge charge = service.Create(options);

                //check for balance in acc
                if (charge.BalanceTransactionId == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.BalanceTransactionId;
                }

                //check for charge status
                if (charge.Status.ToLower() == "succeeded")
                {
                   orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                   orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Details),new { id = orderHeader.Id});
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);
            orderHeader.OrderStatus = SD.StatusInProcess;
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [HttpPost]
        public IActionResult ShipOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);

            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);

            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;

            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            //get login user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            //retrieve all data if user is admin and employee
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll
                                    (x => x.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            //seperate data show
            switch (status)
            {
                case "paymentPending":
                    orderHeaderList = orderHeaderList.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(x => x.OrderStatus == SD.StatusApproved ||
                                                                 x.OrderStatus == SD.StatusInProcess ||
                                                                 x.OrderStatus == SD.StatusPending);
                    break;

                case "completed":
                    orderHeaderList = orderHeaderList.Where(x => x.OrderStatus == SD.StatusShipped);
                    break;

                case "rejected":
                    orderHeaderList = orderHeaderList.Where(x => x.OrderStatus == SD.StatusRefunded ||
                                                                 x.OrderStatus == SD.StatusCancelled ||
                                                                 x.PaymentStatus == SD.PaymentStatusRejected);
                    break;

                default:
                    break;
            }

            return Json(new { data = orderHeaderList });
        }
        #endregion
    }
}
