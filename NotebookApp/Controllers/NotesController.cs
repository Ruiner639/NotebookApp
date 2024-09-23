using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotebookApp.Models;
using NotebookApp.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace NotebookApp.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly NoteService _noteService;
        private readonly ILogger<NotesController> _logger;

        public NotesController(NoteService noteService, ILogger<NotesController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID is null or empty");
                return Unauthorized();
            }

            var notes = _noteService.GetAllNotes()
                .Where(n => n.UserId == userId)
                .ToList();

            return View(notes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Note note)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID is null or empty during note creation");
                    return Unauthorized();
                }

                note.UserId = userId;
                _noteService.CreateNote(note);
                _logger.LogInformation("Note with title '{Title}' was created successfully.", note.Title);
                return RedirectToAction("Index");
            }

            _logger.LogError("ModelState is invalid. Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return View(note);
        }

        public IActionResult Edit(string id)
        {
            var note = _noteService.GetNoteById(id);
            if (note == null || note.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _logger.LogWarning("Note not found or user is not the owner.");
                return NotFound();
            }
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Note note)
        {
            if (ModelState.IsValid)
            {
                var existingNote = _noteService.GetNoteById(note.Id);
                if (existingNote == null || existingNote.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    _logger.LogWarning("Note not found or user is not the owner.");
                    return NotFound();
                }

                existingNote.Title = note.Title;
                existingNote.Content = note.Content;

                _noteService.UpdateNote(existingNote.Id, existingNote);
                _logger.LogInformation("Note with ID {Id} was updated successfully.", existingNote.Id);
                return RedirectToAction("Index");
            }
            else
            {
                _logger.LogError("ModelState is invalid. Errors: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            return View(note);
        }

        public IActionResult Delete(string id)
        {
            var note = _noteService.GetNoteById(id);
            if (note == null || note.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _logger.LogWarning("Note not found or user is not the owner.");
                return NotFound();
            }
            return View(note);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var note = _noteService.GetNoteById(id);
            if (note != null && note.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _noteService.DeleteNote(id);
                _logger.LogInformation("Note with ID {Id} was deleted successfully.", note.Id);
            }
            return RedirectToAction("Index");
        }
    }
}
