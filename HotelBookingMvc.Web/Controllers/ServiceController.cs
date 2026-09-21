using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class ServiceController : Controller
{
    private readonly Service_Service _service;
    private readonly IWebHostEnvironment _environment;

    private readonly string[] _allowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public ServiceController(
        Service_Service service,
        IWebHostEnvironment environment)
    {
        _service = service;
        _environment = environment;
    }


 

    public async Task<IActionResult> Index()
    {
        var services = await _service.GetServicesAsync();

        ViewBag.ServiceImages = GetServiceImages();

        return View(services);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TblService service,
        IFormFile? Photo)
    {
        if (!ModelState.IsValid)
        {
            return View(service);
        }


        // Validate image
        if (Photo != null && Photo.Length > 0)
        {
            if (!IsValidImage(Photo))
            {
                ModelState.AddModelError(
                    "Photo",
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

                return View(service);
            }
        }


        bool result = await _service.CreateServiceAsync(service);

        if (!result)
        {
            TempData["Error"] = "Unable to create service.";

            return View(service);
        }


       
        if (Photo != null && Photo.Length > 0)
        {
            SaveServiceImage(service.ServiceId, Photo);
        }


        TempData["Success"] =
            "Service created successfully.";

        return RedirectToAction(nameof(Index));
    }




    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await _service.GetServiceByIdAsync(id);

        if (service == null)
        {
            return NotFound();
        }

        ViewBag.CurrentImage =
            GetServiceImage(id);

        return View(service);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        TblService service,
        IFormFile? Photo)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CurrentImage =
                GetServiceImage(service.ServiceId);

            return View(service);
        }


      
        if (Photo != null && Photo.Length > 0)
        {
            if (!IsValidImage(Photo))
            {
                ModelState.AddModelError(
                    "Photo",
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

                ViewBag.CurrentImage =
                    GetServiceImage(service.ServiceId);

                return View(service);
            }
        }


        bool result =
            await _service.UpdateServiceAsync(service);

        if (!result)
        {
            TempData["Error"] =
                "Unable to update service.";

            return View(service);
        }


        if (Photo != null && Photo.Length > 0)
        {
            DeleteServiceImages(service.ServiceId);

            SaveServiceImage(
                service.ServiceId,
                Photo);
        }


        TempData["Success"] =
            "Service updated successfully.";

        return RedirectToAction(nameof(Index));
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        bool result =
            await _service.DeleteServiceAsync(id);

        if (result)
        {
            DeleteServiceImages(id);

            TempData["Success"] =
                "Service deleted successfully.";
        }
        else
        {
            TempData["Error"] =
                "Unable to delete service.";
        }

        return RedirectToAction(nameof(Index));
    }


    private bool IsValidImage(IFormFile photo)
    {
        string extension =
            Path.GetExtension(photo.FileName)
                .ToLowerInvariant();

        return _allowedExtensions.Contains(extension);
    }




    private void SaveServiceImage(
        int serviceId,
        IFormFile photo)
    {
        string folder =
            Path.Combine(
                _environment.WebRootPath,
                "images",
                "services");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }


        string extension =
            Path.GetExtension(photo.FileName)
                .ToLowerInvariant();

        string fileName =
            $"{serviceId}{extension}";

        string filePath =
            Path.Combine(folder, fileName);


        using var stream =
            new FileStream(
                filePath,
                FileMode.Create);

        photo.CopyTo(stream);
    }


  

    private void DeleteServiceImages(int serviceId)
    {
        string folder =
            Path.Combine(
                _environment.WebRootPath,
                "images",
                "services");

        if (!Directory.Exists(folder))
            return;


        foreach (string extension in _allowedExtensions)
        {
            string filePath =
                Path.Combine(
                    folder,
                    $"{serviceId}{extension}");

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }


    private string? GetServiceImage(int serviceId)
    {
        string folder =
            Path.Combine(
                _environment.WebRootPath,
                "images",
                "services");

        if (!Directory.Exists(folder))
            return null;


        foreach (string extension in _allowedExtensions)
        {
            string filePath =
                Path.Combine(
                    folder,
                    $"{serviceId}{extension}");

            if (System.IO.File.Exists(filePath))
            {
                return $"/images/services/{serviceId}{extension}";
            }
        }

        return null;
    }


    
    private Dictionary<int, string> GetServiceImages()
    {
        var result =
            new Dictionary<int, string>();

        string folder =
            Path.Combine(
                _environment.WebRootPath,
                "images",
                "services");

        if (!Directory.Exists(folder))
            return result;


        foreach (string extension in _allowedExtensions)
        {
            string[] files =
                Directory.GetFiles(
                    folder,
                    $"*{extension}");

            foreach (string file in files)
            {
                string fileName =
                    Path.GetFileNameWithoutExtension(file);

                if (int.TryParse(
                    fileName,
                    out int serviceId))
                {
                    result[serviceId] =
                        $"/images/services/{fileName}{extension}";
                }
            }
        }

        return result;
    }
}