using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using FilmsCatalog.Models.ViewModels;
using FilmsCatalog.Services;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;

namespace FilmsCatalog.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            return View(await context.Films.ToListAsync());
        }

        // GET: Films/Details/5
        public async Task<IActionResult> Details(Guid? id)
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
            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!FilmsController.AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
            }

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

                var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + fileExt);
                film.PosterPath = $"/posters/{film.Id:N}{fileExt}";
                using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                this.context.Films.Add(film);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(model);

        }

        // GET: Films/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            return View(film);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description,ReleaseDate,Producer,UserId,PosterPath")] Film film)
        {
            if (id != film.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(film);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(film.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(film);
        }

        // GET: Films/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
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

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await context.Films
                .FirstOrDefaultAsync(m => m.Id == id);

            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            var posterPath = Path.Combine(this.hostingEnvironment.WebRootPath, "posters", film.Id.ToString("N") + Path.GetExtension(film.PosterPath));
            System.IO.File.Delete(posterPath);

            this.context.Films.Remove(film);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmExists(Guid id)
        {
            return context.Films.Any(e => e.Id == id);
        }
    }
}
