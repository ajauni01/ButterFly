using Butterfly.Mobile.ViewModels;

namespace Butterfly.Mobile.Views;

public partial class SurveyPage : ContentPage
{
    public SurveyPage(SurveyViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
