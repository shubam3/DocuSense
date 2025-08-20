using DocuSense.Models.ViewModels;
using DocuSense.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocuSense.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IUserService _userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IDocumentService documentService,
            IUserService userService,
            ILogger<HomeController> logger)
        {
            _documentService = documentService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var documentCount = await _documentService.GetDocumentCountAsync(userId);
                        var recentDocuments = await _documentService.GetDocumentsByUserAsync(userId, new DTOs.DocumentSearchDto
                        {
                            PageNumber = 1,
                            PageSize = 5
                        });

                        var dashboardViewModel = new DashboardViewModel
                        {
                            TotalDocuments = documentCount,
                            RecentDocuments = recentDocuments.Take(5).ToList(),
                            UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User"
                        };

                        return View(dashboardViewModel);
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View("Error");
            }
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var documentCount = await _documentService.GetDocumentCountAsync(userId);
                var recentDocuments = await _documentService.GetDocumentsByUserAsync(userId, new DTOs.DocumentSearchDto
                {
                    PageNumber = 1,
                    PageSize = 10
                });

                var categories = await _documentService.GetDocumentCategoriesAsync(userId);

                var dashboardViewModel = new DashboardViewModel
                {
                    TotalDocuments = documentCount,
                    RecentDocuments = recentDocuments,
                    DocumentCategories = categories,
                    UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User"
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return View("Error");
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }

    public class DashboardViewModel
    {
        public int TotalDocuments { get; set; }
        public List<DTOs.DocumentDto> RecentDocuments { get; set; } = new List<DTOs.DocumentDto>();
        public List<string> DocumentCategories { get; set; } = new List<string>();
        public string UserRole { get; set; } = string.Empty;
    }
} 