using HunterPie.Core.Logger;
using System.Linq;
using HunterPie.Domain.Sidebar;
using HunterPie.GUI.Parts.Sidebar.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using HunterPie.Core.Domain.Features;
using HunterPie.Core.Domain.Constants;

namespace HunterPie.GUI.Parts.Sidebar
{
    /// <summary>
    /// Interaction logic for SideBarContainer.xaml
    /// </summary>
    public partial class SideBarContainer : UserControl
    {
        private static readonly ObservableCollection<ISideBarElement> _elements = new ObservableCollection<ISideBarElement>();

        public ObservableCollection<ISideBarElement> Elements => _elements;

        private bool _isMouseInside;
        private bool _isMouseDown;
        private readonly Storyboard _selectSlideAnimation;

        public double ItemsHeight
        {
            get { return (double)GetValue(ItemsHeightProperty); }
            set { SetValue(ItemsHeightProperty, value); }
        }
        public static readonly DependencyProperty ItemsHeightProperty =
            DependencyProperty.Register("ItemsHeight", typeof(double), typeof(SideBarContainer), new PropertyMetadata(20.0));

        public Thickness SelectedButton
        {
            get { return (Thickness)GetValue(SelectedButtonProperty); }
            set { SetValue(SelectedButtonProperty, value); }
        }
        public static readonly DependencyProperty SelectedButtonProperty =
            DependencyProperty.Register("SelectedButton", typeof(Thickness), typeof(SideBarContainer));

        public SideBarContainer()
        {
            InitializeComponent();
            _selectSlideAnimation = FindResource("PART_SelectionAnimation") as Storyboard;
            DataContext = this;
        }

        protected override void OnInitialized(EventArgs e)
        {
            if (!FeatureFlagManager.IsEnabled(FeatureFlags.FEATURE_USER_ACCOUNT))
                PART_UserAccount.Visibility = Visibility.Collapsed;

            base.OnInitialized(e);
        }

        public static void SetMenu(ISideBar menu)
        {
            Add(menu.Menu);
        }

        public static void Add(params ISideBarElement[] elements)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (ISideBarElement element in elements)
                    _elements.Add(element);
            });
        }

        public static void Remove(ISideBarElement element)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _elements.Remove(element);
            });
        }

        private void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
        }

        private void OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Was a click!
            if (_isMouseDown && _isMouseInside) 
                OnClick(e.GetPosition(this));

            _isMouseDown = false;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseInside = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseInside = false;
            _isMouseDown = false;
        }
        
        private void OnClick(Point mousePosition)
        {
            int idx = (int)Math.Abs(mousePosition.Y / ItemsHeight);

            if (idx >= Elements.Count)
                return;

            ISideBarElement element = Elements[idx];

            if (!element.IsActivable || !element.IsEnabled)
                return;

            ((ThicknessAnimation)_selectSlideAnimation.Children[0]).To = new Thickness(0, idx * ItemsHeight, 0, 0);

            PART_Selection.BeginStoryboard(_selectSlideAnimation);
        }
    }
}
