using System.ComponentModel.DataAnnotations;

namespace NotebookApp.Models
{
    public class NoteViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
    }
}
