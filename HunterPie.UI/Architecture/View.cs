using HunterPie.Core.Architecture;
using HunterPie.UI.Overlay;
using System;
using System.Windows.Controls;

namespace HunterPie.UI.Architecture
{
    public class View<TViewModel> : UserControl
        where TViewModel : Bindable
    {
        public TViewModel ViewModel => (TViewModel)DataContext;

        protected virtual TViewModel InitializeViewModel(params object[] args)
        {
            if (this is IWidgetWindow widget)
                try
                {
                    return (TViewModel)Activator.CreateInstance(typeof(TViewModel), args);
                } catch { };

            return Activator.CreateInstance<TViewModel>();
        }

        public View(params object[] args)
        {
            DataContext = InitializeViewModel(args);
        }
    }
}
