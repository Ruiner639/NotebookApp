using MongoDB.Driver;
using NotebookApp.Models;

namespace NotebookApp.Services
{
    public class NoteService
    {
        private readonly IMongoCollection<Note> _notes;

        public NoteService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("NotebookAppDb");
            _notes = database.GetCollection<Note>("Notes");
        }

        public List<Note> GetAllNotes()
        {
            return _notes.Find(note => true).ToList();
        }

        public Note GetNoteById(string id)
        {
            return _notes.Find(note => note.Id == id).FirstOrDefault();
        }

        public void UpdateNote(string id, Note updatedNote)
        {
            var note = _notes.Find(n => n.Id == id).FirstOrDefault();
            if (note == null)
            {
                throw new Exception("Note not found");
            }

            note.Title = updatedNote.Title;
            note.Content = updatedNote.Content;

            _notes.ReplaceOne(n => n.Id == id, note);
        }

        public void DeleteNote(string id)
        {
            _notes.DeleteOne(note => note.Id == id);
        }

        public void CreateNote(Note note)
        {
            _notes.InsertOne(note);
        }
    }
}
