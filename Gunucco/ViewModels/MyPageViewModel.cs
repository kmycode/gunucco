using Gunucco.Entities;
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
}
