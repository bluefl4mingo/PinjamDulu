using System.Windows.Controls;

namespace PinjamDuluApp.Services
{
    public class NavigationService
    {
        private readonly Frame _frame;

        public NavigationService(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(Type pageType)
        {
            _frame.Navigate(Activator.CreateInstance(pageType));
        }

        public void NavigateTo(Type pageType, params object[] parameters) // the additional parameters is to pass data to other pages, if necessary (like in the case where we need to pass email and password from SignUpPage to FillUserInfoPage)
        {
            var page = Activator.CreateInstance(pageType, parameters) as Page;
            _frame.Navigate(page);
        }

        public void GoBack()
        {
            if (_frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }
    }
}
