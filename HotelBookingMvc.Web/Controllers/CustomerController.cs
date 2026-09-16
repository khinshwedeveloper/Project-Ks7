using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class CustomerController : Controller
{
    private readonly Customer_Service _customerService;

    public CustomerController(Customer_Service customerService)
    {
        _customerService = customerService;
    }

    // GET: /Customer
    public async Task<IActionResult> Index()
    {
        var customers = await _customerService.GetCustomersAsync();

        return View(customers);
    }

    // GET: /Customer/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Customer/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblCustomer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        var result =
            await _customerService.CreateCustomerAsync(customer);

        if (!result)
        {
            TempData["Error"] = "Unable to create customer.";

            return View(customer);
        }

        TempData["Success"] = "Customer created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Customer/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer =
            await _customerService.GetCustomerByIdAsync(id);

        if (customer == null)
        {
            TempData["Error"] = "Customer not found.";

            return RedirectToAction(nameof(Index));
        }

        return View(customer);
    }

    // POST: /Customer/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TblCustomer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        var result =
            await _customerService.UpdateCustomerAsync(customer);

        if (!result)
        {
            TempData["Error"] = "Unable to update customer.";

            return View(customer);
        }

        TempData["Success"] = "Customer updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Customer/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result =
            await _customerService.DeleteCustomerAsync(id);

        if (!result)
        {
            TempData["Error"] =
                "Customer could not be deleted.";
        }
        else
        {
            TempData["Success"] =
                "Customer deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}