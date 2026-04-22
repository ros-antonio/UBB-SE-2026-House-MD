using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERManagementSystem.Core.Services
{
    public interface INavigationService
    {
        void Navigate(Type pageType);
        void Navigate(Type pageType, object parameter);
    }
}
