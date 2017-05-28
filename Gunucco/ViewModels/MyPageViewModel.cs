using Gunucco.Entities;
using Gunucco.Entities.Helpers;
using Gunucco.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.ViewModels
{
    public abstract class MyPageViewModelBase
    {
        public AuthorizationData AuthData { get; set; }

        public MessageViewModel Message { get; set; } = new MessageViewModel();
    }

    public class MyPageTopViewModel : MyPageViewModelBase
    {
        public IEnumerable<Book> Books { get; set; }
    }

    public class MyPageBookViewModel : MyPageViewModelBase
    {
        public Book Book { get; set; }

        public IEnumerable<TreeEntity<Chapter>> Chapters { get; set; }
    }

    public class MyPageChapterViewModel : MyPageViewModelBase
    {
        public Book Book { get; set; }

        public Chapter Chapter { get; set; }

        public IEnumerable<ContentMediaPair> Contents { get; set; }
    }
}
