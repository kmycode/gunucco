using Gunucco.Entities;
using Gunucco.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.ViewModels
{
    public abstract class ViewViewModelBase
    {
        public MessageViewModel Message { get; set; } = new MessageViewModel();
    }

    public class UserViewViewModel : ViewViewModelBase
    {
        public User User { get; set; }

        public IEnumerable<Book> Books { get; set; }
    }

    public class BookViewViewModel : ViewViewModelBase
    {
        public User User { get; set; }

        public Book Book { get; set; }

        public IEnumerable<TreeEntity<Chapter>> Chapters { get; set; }
    }

    public class ChapterViewViewModel : ViewViewModelBase
    {
        public Book Book { get; set; }

        public Chapter Chapter { get; set; }

        public Chapter NextChapter { get; set; }

        public Chapter PrevChapter { get; set; }

        public IEnumerable<ContentMediaPair> Contents { get; set; }
    }
}
