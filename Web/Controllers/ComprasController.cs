﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers
{
    public class ComprasController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        //public IActionResult CreatePaymentLink(int id)
        //{
        //    var purchase = PurchaseService.CreatePayment(id);
        //    return Redirect(purchase.Link);
        //}
    }
}