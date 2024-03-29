using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DoucmentManagmentSys.Repo;
using DoucmentManagmentSys.RoleManagment;
using Microsoft.AspNetCore.Identity;
using DoucmentManagmentSys.Models.Static;
using Microsoft.AspNetCore.Authorization;



namespace DoucmentManagmentSys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;



        private readonly DocumentRepository _DocsRepo;

        private readonly IRoleManagment _roleManagment;

        public SignInManager<IdentityUser> _signInManager { get; set; }

        public HomeController(ILogger<HomeController> logger, DocumentRepository repository, IRoleManagment roleManagment, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _DocsRepo = repository;
            _roleManagment = roleManagment;
            _signInManager = signInManager;


        }

        public IActionResult Index(string Message, string Messages)
        {

            ViewBag.Message = Message ?? "";
            ViewBag.Messages = Messages ?? "";
            TempData["Id"] = TempData["Id"] ?? "";

            return View(_DocsRepo.GetAll());
        }

        [HttpPost]
        [Authorize(Roles = "Uploader")]
        public async Task<IActionResult> UploadFile(IFormFile oFile)
        {
            MessageResult result = ServerFileManager.UploadtoServer(oFile);

            if (result.Status)
            {
                result = await SaveToDB();
            }

            return RedirectToAction("index", "Home", new { Message = result.Message });
        }



        public async Task<MessageResult> SaveToDB()
        {
            string strFolder = "./UploadedFiles/";
            MessageResult Result = _DocsRepo.AddRange(await ServerFileManager.FilesToDocs());

            if (Result.Status)
            {
                _DocsRepo.SaveChanges();
            }

            ViewBag.Message = Result.Message;
            ServerFileManager.CleanDirectory(strFolder);
            return Result;

        }

        public async Task<MessageResult> UpdateToDB(int id, string newName)
        {

            string strFolder = "./UploadedFiles/";
            MessageResult Result = _DocsRepo.Update(id, newName);
            if (Result.Status)
            {
                _DocsRepo.SaveChanges();
            }

            ServerFileManager.CleanDirectory(strFolder);
            return Result;


        }

        [HttpPost]
        public IActionResult DownloadFile(string name, int id)
        {
            string Message = "Download Started.";
            //Get name from database and then check



            Document document = _DocsRepo.Find([id, name]);
            if (document == null)
            {
                Message = "document not found";
                return RedirectToAction("index", "Home", new { Message });
            }
            else
            {

                return File(document.Content, FileTypes.GetContentType(document.FileName), document.FileName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFile(int id, string fileName)
        {
            Document document = _DocsRepo.Find([id, fileName]);
            if (document == null)
            {
                return Content("document not found");
            }
            else
            {
                _DocsRepo.Delete(document);
                _DocsRepo.SaveChanges();
                return RedirectToAction("index", "Home", new { Message = "File deleted successfully." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(IFormFile oFile, int id)
        {
            MessageResult result = ServerFileManager.UploadtoServer(oFile);

            if (result.Status)
            {
                result = await UpdateToDB(id, oFile.FileName);
            }
            ViewBag.Messages = result.Message;
            TempData["Id"] = id;

            return RedirectToAction("index", "Home", new { ViewBag.Messages });


            //return RedirectToAction("index", "Home", ViewBag.Message = "File updated successfully.");

        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmation(int id, string FileName)
        {
            ViewBag.id = id;
            ViewBag.FileName = FileName;
            return View("Confirmation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string search)
        {
            if (search == null || search == string.Empty)
            {
                return PartialView("_DocsListPartial", _DocsRepo.GetAll());
            }
            else
            {
                return PartialView("_DocsListPartial", _DocsRepo.Search(search));
            }


        }

        [HttpGet]
        public IActionResult GSearch(string search)
        {
            if (search == null || search == string.Empty)
            {
                return View("index", _DocsRepo.GetAll());
            }
            else
            {
                return View("index", _DocsRepo.Search(search));
            }


        }

        [HttpPost]
        [Authorize(Roles = "Finalizer ,Revisor")]
        public IActionResult Approve(int id, string Filename)
        {
            MessageResult result = new MessageResult("File not Approved.");
            Document Doc = _DocsRepo.Find([id, Filename]);
            if ((Doc.status == Document.Status.Under_Finalization && User.IsInRole("Finalizer")) || (Doc.status == Document.Status.Under_Revison && User.IsInRole("Revisor")))
            {
                Doc.Approve();
                _DocsRepo.SaveChanges();
                result.Status = true;
                result.Message = "File Approved successfully.";
                return RedirectToAction("SendMail", "Mail", new { Filename = Filename, actionTaken = "Approved", status = Doc.status });
            }

            return RedirectToAction("index", "Home", new { Message = result.Status });

        }
        [HttpPost]
        [Authorize(Roles = "Finalizer ,Revisor")]
        [ValidateAntiForgeryToken]
        public IActionResult RejectPopup(int id, string Filename)
        {
            return PartialView("_ReasonPopup", _DocsRepo.Find([id, Filename]));
        }



        [HttpPost]
        [Authorize(Roles = "Revisor ,Finalizer")]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string Filename, string reason)
        {
            MessageResult result = new MessageResult("File not Rejected.");
            Document Doc = _DocsRepo.Find([id, Filename]);
            if ((Doc.status == Document.Status.Under_Finalization && User.IsInRole("Finalizer")) || (Doc.status == Document.Status.Under_Revison && User.IsInRole("Revisor")))
            {
                //displayRejectPopup


                Doc.Reject(reason);
                _DocsRepo.SaveChanges();
                result.Status = true;
                result.Message = "File Rejected successfully.";
                return RedirectToAction("SendMail", "Mail", new { Filename = Filename, actionTaken = "Rejected", status = Doc.status, reason = reason });
            }

            return RedirectToAction("index", "Home", new { Message = result.Status });
        }



        public IActionResult Docs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }
}
