using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using FilmsCatalog.Models.ViewModels;
using FilmsCatalog.Services;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace FilmsCatalog.Controllers
{
    [Authorize]
    public class FilmsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png"};

        private readonly ApplicationDbContext context;
        private readonly IUserPermissionsService userPermissions;
        private readonly IHostingEnvironment hostingEnvironment;

        public FilmsController(ApplicationDbContext context, IUserPermissionsService userPermissions, IHostingEnvironment hostingEnvironment)
        {
            this.context = context;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Films
        [AllowAnonymous]
        public async Task<IActionResult> Index(Int32? pageId)
        {
            Int32 pageSize = 10;
            Int32 pageNumber = (pageId ?? 1);
            return View(context.Films.ToPagedList(pageNumber, pageSize));
        }

        // GET: Films/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id, Int32? pageId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await context.Films
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            ViewBag.pageId = pageId;
            ViewBag.canEditFilm = userPermissions.CanEditFilm(film);

            return View(film);
        }

        // GET: Films/Create
        public IActionResult Create()
        {
            return View(new FilmCreateModel());
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FilmCreateModel model)
        {
            if (this.ModelState.IsValid)
            {
                var film = new Film
                {
                    Name = model.Name,
                    Description = model.Description,
                    ReleaseDate = model.ReleaseDate,
                    Producer = model.Producer,
                    CreatorId = userPermissions.GetIdUser()
                };

                if (model.File != null)
                {
                    var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Trim('"'));
                    var fileExt = Path.GetExtension(fileName);
                    if (!FilmsController.AllowedExtensions.Contains(fileExt))
                    {
                        this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
                    }
                    var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + fileExt);
                    film.PosterPath = $"/posters/{film.Id:N}{fileExt}";
                    using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                    {
                        await model.File.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    film.PosterPath = "/posters/noPoster.jpg";
                }

                this.context.Films.Add(film);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(model);

        }

        // GET: Films/Edit/5
        public async Task<IActionResult> Edit(Guid? id, Int32? pageId)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this.context.Films
                .SingleOrDefaultAsync(m => m.Id == id);
            Boolean flag = userPermissions.CanEditFilm(film);

            if (film == null || !flag)
            {
                return this.NotFound();
            }

            var model = new FilmEditModel
            {
                Name = film.Name,
                Description = film.Description,
                ReleaseDate = film.ReleaseDate,
                Producer = film.Producer
            };

            ViewBag.id = id;
            ViewBag.pageId = pageId;

            return this.View(model);

        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, Int32? pageId, FilmEditModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this.context.Films
                .SingleOrDefaultAsync(m => m.Id == id);

            if (film == null)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                film.Name = model.Name;
                film.Description = model.Description;
                film.ReleaseDate = model.ReleaseDate;
                film.Producer = model.Producer;

                if (model.File != null)
                {
                    var posterPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + Path.GetExtension(film.PosterPath));
                    System.IO.File.Delete(posterPath);
                    film.PosterPath = null;

                    var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Trim('"'));
                    var fileExt = Path.GetExtension(fileName);
                    var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + fileExt);
                    film.PosterPath = $"/posters/{film.Id:N}{fileExt}";
                    using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                    {
                        await model.File.CopyToAsync(fileStream);
                    }
                }

                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", "Films", new { id = id, pageId = pageId });
            }

            return this.View(model);

        }

        // POST: Films/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await context.Films
                .FirstOrDefaultAsync(m => m.Id == id);

            Boolean flag = userPermissions.CanEditFilm(film);

            if (film == null || !flag)
            {
                return this.NotFound();
            }

            if (film.PosterPath != "/posters/noPoster.jpg")
            {
                var posterPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + Path.GetExtension(film.PosterPath));
                System.IO.File.Delete(posterPath);
            }

            this.context.Films.Remove(film);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
